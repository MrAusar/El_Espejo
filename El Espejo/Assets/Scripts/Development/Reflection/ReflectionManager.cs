using System;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionManager : MonoBehaviour
{
    [Header("References")]
    public PersonalDevelopmentPlanManager planManager;

    [Header("Question Bank")]
    public ReflectionQuestionData questionData;


    [Header("Reflection Progress")]
    private List<ReflectionQuestion> currentQuestions = new List<ReflectionQuestion>();

    private int currentQuestionIndex = -1;

    private string currentTopicID;
    private string currentReflectionStage;

    public enum ReflectionStage
    {
        Awareness,
        Evidence,
        Projection
    }

    [Header("Stage Progress")]
    public ReflectionStage currentStage = ReflectionStage.Awareness;

    private bool isWaitingForStageTransition;

    public List<ReflectionQuestion> GetQuestionsForTopic( string topicID, string reflectionStage)
    {
        List<ReflectionQuestion> availableQuestions = new List<ReflectionQuestion>();

        if (planManager == null ||
            planManager.CurrentPlan == null ||
            questionData == null)
        {
            Debug.LogWarning( "ReflectionManager: faltan referencias o datos." );

            return availableQuestions;
        }

        DevelopmentTopicSummary topicResult = planManager.CurrentPlan.topicResults.Find( topic => topic.topicID == topicID );

        if (topicResult == null)
        {
            Debug.LogWarning( "No se encontraron resultados para la competencia: " + topicID  );

            return availableQuestions;
        }

        foreach (ReflectionQuestion question in questionData.questions)
        {
            if (!question.isActive)
                continue;

            if (question.topicID != topicID)
                continue;

            if (question.reflectionStage != reflectionStage)
                continue;

            bool matchesRequiredLevel =
                question.requiredLevelStatuses.Count == 0 ||
                question.requiredLevelStatuses.Contains(
                    topicResult.requiredLevelStatus
                );

            bool matchesPerception =
                question.perceptionAlignments.Count == 0 ||
                question.perceptionAlignments.Contains(
                    topicResult.perceptionAlignment
                );

            if (matchesRequiredLevel && matchesPerception)
            {
                availableQuestions.Add(question);
            }
        }

        return availableQuestions;

    }

    public bool StartReflection(
        string topicID,
        string reflectionStage)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.reflectionData == null)
        {
            Debug.LogWarning("No hay un plan de reflexión disponible.");
            return false;
        }

        currentQuestions = GetPendingQuestionsForTopic(topicID,reflectionStage);

        if (currentQuestions.Count == 0)
        {
            Debug.LogWarning("No hay preguntas disponibles para esta etapa.");

            return false;
        }

        currentTopicID = topicID;
        currentReflectionStage = reflectionStage;
        currentQuestionIndex = 0;

        Debug.Log( "Reflexión iniciada. Preguntas: " + currentQuestions.Count );

        return true;
    }

    public ReflectionQuestion GetCurrentQuestion()
    {
        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= currentQuestions.Count)
        {
            return null;
        }

        return currentQuestions[currentQuestionIndex];
    }

    public bool SaveCurrentAnswer(string answer)
    {
        ReflectionQuestion currentQuestion = GetCurrentQuestion();

        if (currentQuestion == null)
        {
            Debug.LogWarning("No hay una pregunta actual.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            Debug.LogWarning("La respuesta no puede estar vacía.");
            return false;
        }

        ReflectionResponse existingResponse =
            planManager.CurrentPlan.reflectionData.responses.Find(
                response =>
                    response.questionID == currentQuestion.questionID &&
                    response.topicID == currentTopicID &&
                    response.reflectionStage == currentReflectionStage
            );

        if (existingResponse != null)
        {
            existingResponse.answer = answer.Trim();
        }
        else
        {
            ReflectionResponse newResponse = new ReflectionResponse
                {
                    topicID = currentTopicID,
                    questionID = currentQuestion.questionID,
                    reflectionStage = currentReflectionStage,
                    answer = answer.Trim()
                };

            planManager.CurrentPlan.reflectionData.responses.Add(newResponse);
        }

        Debug.Log("Respuesta guardada: " + currentQuestion.questionID);

        return true;
    }

    public bool MoveToNextQuestion()
    {
        if (currentQuestionIndex < 0 || currentQuestionIndex >= currentQuestions.Count)
        {
            return false;
        }

        currentQuestionIndex++;

        if (currentQuestionIndex >= currentQuestions.Count)
        {
            Debug.Log("Etapa de reflexión completada.");
            return false;
        }

        Debug.Log("Siguiente pregunta: " + GetCurrentQuestion().questionID );

        return true;
    }

    public bool IsReflectionCompleted()
    {
        return currentQuestions.Count > 0 && currentQuestionIndex >= currentQuestions.Count;
    }

    public bool StartStage(string topicID, ReflectionStage stage)
    {
        string stageName = stage.ToString();

        bool started = StartReflection(topicID, stageName);

        if (!started)
            return false;

        currentStage = stage;
        isWaitingForStageTransition = false;

        Debug.Log("Etapa iniciada: " + GetStageDisplayName());

        return true;
    }

    public bool CompleteCurrentStage()
    {
        if (!IsReflectionCompleted())
        {
            Debug.LogWarning( "Todavía hay preguntas pendientes.");

            return false;
        }

        isWaitingForStageTransition = true;

        Debug.Log( "Etapa terminada: " + GetStageDisplayName());

        return true;
    }

    public bool ContinueToNextStage()
    {
        if (!isWaitingForStageTransition)
        {
            Debug.LogWarning( "No se puede avanzar: la etapa actual no ha terminado.");

            return false;
        }

        if (!HasMoreStages())
        {
            Debug.Log("El proceso de reflexión ha finalizado.");
            return false;
        }

        ReflectionStage nextStage = (ReflectionStage)((int)currentStage + 1);

        if (!StartStage(currentTopicID, nextStage))
        {
            Debug.LogWarning( "No hay preguntas disponibles para la siguiente etapa.");

            return false;
        }

        return true;
    }


    public bool HasMoreStages()
    {
        return currentStage < ReflectionStage.Projection;
    }

    public string GetStageDisplayName()
    {
        switch (currentStage)
        {
            case ReflectionStage.Awareness:
                return "Conciencia";

            case ReflectionStage.Evidence:
                return "Evidencia";

            case ReflectionStage.Projection:
                return "Proyección";

            default:
                return "Desconocida";
        }
    }

    public bool IsWaitingForStageTransition()
    {
        return isWaitingForStageTransition;
    }


    public List<ReflectionQuestion> GetPendingQuestionsForTopic(
        string topicID,
        string reflectionStage)
    {
        List<ReflectionQuestion> availableQuestions = GetQuestionsForTopic(topicID, reflectionStage);

        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.reflectionData == null)
        {
            return new List<ReflectionQuestion>();
        }

        List<ReflectionQuestion> pendingQuestions = new List<ReflectionQuestion>();

        foreach (ReflectionQuestion question in availableQuestions)
        {
            ReflectionResponse existingResponse =
                planManager.CurrentPlan.reflectionData.responses.Find(
                    response =>
                        response.topicID == topicID &&
                        response.questionID == question.questionID &&
                        response.reflectionStage == reflectionStage
                );

            if (existingResponse == null)
            {
                pendingQuestions.Add(question);
            }
        }

        return pendingQuestions;
    }


    public bool IsStageCompleted(
        string topicID,
        string reflectionStage)
    {
        List<ReflectionQuestion> availableQuestions = GetQuestionsForTopic(topicID, reflectionStage);

        if (availableQuestions.Count == 0)
            return false;

        List<ReflectionQuestion> pendingQuestions = GetPendingQuestionsForTopic(topicID, reflectionStage);

        return pendingQuestions.Count == 0;
    }

    public bool HasQuestionsForStage(
        string topicID,
        string reflectionStage)
    {
        return GetQuestionsForTopic(
            topicID,
            reflectionStage
        ).Count > 0;
    }

    public bool TryResumeReflection(string topicID)
    {
        ReflectionStage[] stages = (ReflectionStage[])Enum.GetValues( typeof(ReflectionStage) );

        foreach (ReflectionStage stage in stages)
        {
            string stageName = stage.ToString();

            // Verificar que la etapa tenga preguntas.
            if (!HasQuestionsForStage(topicID, stageName))
            {
                Debug.LogWarning($"La etapa {stageName} no tiene preguntas configuradas." );

                return false;
            }

            // Si está completada, buscar la siguiente.
            if (IsStageCompleted(topicID, stageName))
            {
                continue;
            }

            // Encontramos la primera etapa pendiente.
            bool started = StartStage(topicID, stage);

            if (started)
            {
                Debug.Log( $"Reflexión reanudada en: {stageName}"  );
            }

            return started;
        }

        Debug.Log( $"Todas las etapas de reflexión están completadas para {topicID}.");

        return false;
    }

    public bool TryGetCurrentStageProgress(
    out int questionNumber,
    out int totalQuestions)
    {
        questionNumber = 0;
        totalQuestions = 0;

        if (currentQuestions == null ||
            currentQuestions.Count == 0 ||
            currentQuestionIndex < 0 ||
            currentQuestionIndex >= currentQuestions.Count)
        {
            return false;
        }

        List<ReflectionQuestion> allQuestions =
            GetQuestionsForTopic(
                currentTopicID,
                currentReflectionStage
            );

        totalQuestions = allQuestions.Count;

        int previouslyAnswered =
            totalQuestions - currentQuestions.Count;

        questionNumber =
            previouslyAnswered + currentQuestionIndex + 1;

        return totalQuestions > 0;
    }

    public bool IsTopicReflectionCompleted(string topicID)
    {
        if (string.IsNullOrWhiteSpace(topicID))
            return false;

        ReflectionStage[] stages = (ReflectionStage[])Enum.GetValues(typeof(ReflectionStage));

        foreach (ReflectionStage stage in stages)
        {
            string stageName = stage.ToString();

            if (!HasQuestionsForStage(topicID, stageName))
            {
                return false;
            }

            if (!IsStageCompleted(topicID, stageName))
            {
                return false;
            }
        }

        return true;
    }

    public void DiagnoseTopicReflectionCompletion(string topicID)
    {
        Debug.Log($"=== DIAGNÓSTICO REFLEXIÓN: {topicID} ===");

        ReflectionStage[] stages = (ReflectionStage[])Enum.GetValues(typeof(ReflectionStage));

        foreach (ReflectionStage stage in stages)
        {
            string stageName = stage.ToString();

            List<ReflectionQuestion> available = GetQuestionsForTopic(topicID, stageName);

            List<ReflectionQuestion> pending = GetPendingQuestionsForTopic(topicID, stageName);

            Debug.Log(
                $"Etapa: {stageName} | " +
                $"Configuradas: {available.Count} | " +
                $"Pendientes: {pending.Count} | " +
                $"Completada: {IsStageCompleted(topicID, stageName)}"
            );

            foreach (ReflectionQuestion question in pending)
            {
                Debug.LogWarning($"Pregunta pendiente: {question.questionID}");
            }
        }

        Debug.Log($"REFLEXIÓN COMPLETA: " + IsTopicReflectionCompleted(topicID));
    }

    //PRUEBAS
    [ContextMenu("Test / Resume Reflection Stages")]
    private void TestResumeReflectionStages()
    {
        if (planManager == null || planManager.CurrentPlan == null)
        {
            Debug.LogWarning("No hay un plan disponible.");
            return;
        }

        Debug.Log($"Intentando reanudar reflexión: {testTopicID}");

        bool resumed = TryResumeReflection(testTopicID);

        if (!resumed)
        {
            Debug.LogWarning(
                "No se pudo reanudar la reflexión. " +
                "Comprueba si todas las etapas están completas " +
                "o si faltan preguntas configuradas."
            );

            return;
        }

        ReflectionQuestion question = GetCurrentQuestion();

        Debug.Log(
            $"Etapa actual: {currentStage} | " +
            $"Pregunta actual: {question.questionID}"
        );
    }
    [ContextMenu("Test / Diagnose Reflection Stage Status")]
    private void TestReflectionStageStatus()
    {
        ReflectionStage[] stages =
            (ReflectionStage[])Enum.GetValues(typeof(ReflectionStage));

        foreach (ReflectionStage stage in stages)
        {
            string stageName = stage.ToString();

            bool hasQuestions = HasQuestionsForStage(
                testTopicID,
                stageName
            );

            bool isCompleted = IsStageCompleted(
                testTopicID,
                stageName
            );

            string status;

            if (!hasQuestions)
            {
                status = "SIN PREGUNTAS CONFIGURADAS";
            }
            else if (isCompleted)
            {
                status = "COMPLETADA";
            }
            else
            {
                status = "PENDIENTE";
            }

            Debug.Log(
                $"Etapa: {stageName} | Estado: {status}"
            );
        }
    }

    [ContextMenu("Test / Resume Reflection")]
    private void TestResumeReflection()
    {
        Debug.Log("=== PRUEBA DE RECUPERACIÓN ===");

        // Consultar respuestas existentes.
        int previousResponses =
            planManager.CurrentPlan.reflectionData.responses.Count;

        Debug.Log(
            "Respuestas antes de iniciar: " + previousResponses
        );

        // Iniciar o recuperar la etapa.
        bool started = StartStage(
            testTopicID,
            ReflectionStage.Awareness
        );

        if (!started)
        {
            Debug.Log(
                "No se inició la etapa. Puede que todas las preguntas estén respondidas."
            );
            return;
        }

        ReflectionQuestion currentQuestion =
            GetCurrentQuestion();

        Debug.Log(
            "Primera pregunta pendiente: " +
            currentQuestion.questionID
        );

        Debug.Log(
            "Texto: " + currentQuestion.questionText
        );

        // Comprobar que no tenga respuesta previa.
        ReflectionResponse existing =
            planManager.CurrentPlan.reflectionData.responses.Find(
                response =>
                    response.topicID == testTopicID &&
                    response.questionID == currentQuestion.questionID &&
                    response.reflectionStage == "Awareness"
            );

        Debug.Log(
            "¿Ya estaba respondida?: " + (existing != null)
        );
    }


    [ContextMenu("Test / Diagnose Reflection Questions")]
    private void DiagnoseReflectionQuestions()
    {
        Debug.Log("=== DIAGNÓSTICO DE REFLEXIÓN ===");

        Debug.Log("Topic ID: " + testTopicID);
        Debug.Log("Stage: " + testReflectionStage);

        List<ReflectionQuestion> available =
            GetQuestionsForTopic(
                testTopicID,
                testReflectionStage
            );

        Debug.Log("Preguntas disponibles: " + available.Count);

        List<ReflectionQuestion> pending =
            GetPendingQuestionsForTopic(
                testTopicID,
                testReflectionStage
            );

        Debug.Log("Preguntas pendientes: " + pending.Count);

        if (planManager != null &&
            planManager.CurrentPlan != null &&
            planManager.CurrentPlan.reflectionData != null)
        {
            Debug.Log(
                "Respuestas almacenadas: " +
                planManager.CurrentPlan.reflectionData.responses.Count
            );
        }

        foreach (ReflectionQuestion question in available)
        {
            Debug.Log(
                "Pregunta: " + question.questionID +
                " | Activa: " + question.isActive
            );
        }
    }


    [ContextMenu("Test / Pending Reflection Questions")]
    private void TestPendingQuestions()
    {
        // Consultar preguntas pendientes inicialmente.
        List<ReflectionQuestion> pendingBefore =
            GetPendingQuestionsForTopic(
                testTopicID,
                testReflectionStage
            );

        Debug.Log(
            "Preguntas pendientes inicialmente: "
            + pendingBefore.Count
        );

        if (pendingBefore.Count == 0)
        {
            Debug.LogWarning(
                "No hay preguntas disponibles para realizar la prueba."
            );
            return;
        }

        // Iniciar la etapa y responder la primera pregunta.
        if (!StartReflection(testTopicID, testReflectionStage))
            return;

        ReflectionQuestion firstQuestion = GetCurrentQuestion();

        if (!SaveCurrentAnswer("Respuesta temporal para la prueba."))
            return;

        Debug.Log(
            "Pregunta respondida: " + firstQuestion.questionID
        );

        // Consultar nuevamente las preguntas pendientes.
        List<ReflectionQuestion> pendingAfter =
            GetPendingQuestionsForTopic(
                testTopicID,
                testReflectionStage
            );

        Debug.Log(
            "Preguntas pendientes después de responder: "
            + pendingAfter.Count
        );

        foreach (ReflectionQuestion question in pendingAfter)
        {
            Debug.Log(
                "Pendiente: " + question.questionID
            );
        }
    }

    [ContextMenu("Test / Reflection Stage Navigation")]
    private void TestStageNavigation()
    {
        // Iniciar Conciencia
        if (!StartStage(testTopicID, ReflectionStage.Awareness))
            return;

        Debug.Log("ETAPA 1: " + GetStageDisplayName());

        Debug.Log(
            "Pregunta: " + GetCurrentQuestion().questionText
        );

        SaveCurrentAnswer("Respuesta de prueba de Conciencia.");
        MoveToNextQuestion();

        // Completar Conciencia
        if (!CompleteCurrentStage())
            return;

        Debug.Log(
            "Esperando transición: " +
            IsWaitingForStageTransition()
        );

        // Continuar a Evidencia
        if (!ContinueToNextStage())
            return;

        Debug.Log("ETAPA 2: " + GetStageDisplayName());

        Debug.Log(
            "Pregunta: " + GetCurrentQuestion().questionText
        );

        SaveCurrentAnswer("Respuesta de prueba de Evidencia.");
        MoveToNextQuestion();

        if (!CompleteCurrentStage())
            return;

        // Continuar a Proyección
        if (!ContinueToNextStage())
            return;

        Debug.Log("ETAPA 3: " + GetStageDisplayName());

        Debug.Log(
            "Pregunta: " + GetCurrentQuestion().questionText
        );

        SaveCurrentAnswer("Respuesta de prueba de Proyección.");
        MoveToNextQuestion();

        if (!CompleteCurrentStage())
            return;

        // Comprobar finalización
        Debug.Log(
            "¿Hay más etapas?: " + HasMoreStages()
        );

        Debug.Log(
            "Respuestas almacenadas: " +
            planManager.CurrentPlan.reflectionData.responses.Count
        );
    }


    [Header("Test Settings")]
    public string testTopicID;
    public string testReflectionStage = "Awareness";

    [ContextMenu("Test / Filter Reflection Questions")]
    private void TestQuestionFilter()
    {
        List<ReflectionQuestion> results =
            GetQuestionsForTopic(
                testTopicID,
                testReflectionStage
            );

        Debug.Log(
            "Preguntas encontradas: " + results.Count
        );

        foreach (ReflectionQuestion question in results)
        {
            Debug.Log(
                question.questionID + " | " +
                question.questionText
            );
        }
    }


    [ContextMenu("Test / Show Available Topic IDs")]
    private void ShowAvailableTopicIDs()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null)
        {
            Debug.LogWarning("No hay un plan disponible.");
            return;
        }

        foreach (DevelopmentTopicSummary topic
                 in planManager.CurrentPlan.topicResults)
        {
            Debug.Log(
                "Topic ID: " + topic.topicID +
                " | Nombre: " + topic.topicTitle +
                " | Required Status: " + topic.requiredLevelStatus +
                " | Perception: " + topic.perceptionAlignment
            );
        }
    }

    [ContextMenu("Test / Complete Reflection Flow")]
    private void TestReflectionFlow()
    {
        // 1. Iniciar reflexión
        bool started = StartReflection(
            testTopicID,
            testReflectionStage
        );

        if (!started)
        {
            Debug.LogWarning("No se pudo iniciar la reflexión.");
            return;
        }

        // 2. Obtener primera pregunta
        ReflectionQuestion firstQuestion =
            GetCurrentQuestion();

        Debug.Log(
            "Primera pregunta: " + firstQuestion.questionText
        );

        // 3. Guardar respuesta
        bool saved = SaveCurrentAnswer(
            "Esta es una respuesta de prueba para comprobar el sistema."
        );

        if (!saved)
            return;

        // 4. Avanzar
        bool moved = MoveToNextQuestion();

        if (moved)
        {
            ReflectionQuestion nextQuestion =
                GetCurrentQuestion();

            Debug.Log(
                "Segunda pregunta: " + nextQuestion.questionText
            );

            SaveCurrentAnswer(
                "Esta es mi segunda respuesta de prueba."
            );

            // Avanzar hasta terminar
            MoveToNextQuestion();
        }

        // 5. Comprobar finalización
        Debug.Log(
            "¿Etapa completada?: " + IsReflectionCompleted()
        );

        // 6. Mostrar respuestas guardadas
        Debug.Log(
            "Respuestas almacenadas: " +
            planManager.CurrentPlan.reflectionData.responses.Count
        );
    }

}
