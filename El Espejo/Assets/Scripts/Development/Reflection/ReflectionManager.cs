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

        currentQuestions = GetQuestionsForTopic(
            topicID,
            reflectionStage
        );

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
        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= currentQuestions.Count)
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


    //PRUEBAS

    [Header("Test Settings")]
    public string testTopicID;
    public string testReflectionStage = "Conciencia";

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


}
