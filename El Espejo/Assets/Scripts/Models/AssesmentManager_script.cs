using System.Collections.Generic;
using UnityEngine;
using static AssessmentManager_script;

public class AssessmentManager_script : MonoBehaviour
{
    [SerializeField] private AssessmentCsvLoader csvLoader;

    public AssesmentData assessmentData { get; private set; }

    public AssesmentResponses managerResponses;
    public AssesmentResponses bossResponses;

    public const string ManagerRole = "Manager";
    public const string BossRole = "Boss";

    public bool IsInitialized { get; private set; }


    public enum PerceptionAlignment
    {
        BossRatesHigher,
        Aligned,
        ManagerRatesHigher
    }

    public enum RequiredLevelStatus
    {
        SharedOpportunity,
        ManagerOpportunity,
        BossOpportunity,
        BothMeetRequiredLevel
    }

    [System.Serializable]
    public class TopicAssessmentResult
    {
        public string topicID;
        public string topicTitle;
        public float average;
        public float requiredLevel;
        public float difference;
    }

    [System.Serializable]
    public class TopicComparisonResult
    {
        public string topicID;
        public string topicTitle;

        public float managerAverage;
        public float bossAverage;
        public float requiredLevel;

        // Promedio menos nivel requerido.
        public float managerDifference;
        public float bossDifference;

        // Autoevaluación menos evaluación del jefe.
        public float perceptionDifference;

        public PerceptionAlignment perceptionAlignment;
        public RequiredLevelStatus requiredLevelStatus;
    }

    public void InitializeAssessment(string assessmentID)
    {
        IsInitialized = false;

        if (csvLoader == null)
        {
            Debug.LogError("No se ha asignado AssessmentCsvLoader.");
            return;
        }

        assessmentData = csvLoader.LoadAssessment();

        if (assessmentData == null)
            return;

        managerResponses = AssessmentLocalStorage.Load(assessmentID,ManagerRole);

        bossResponses = AssessmentLocalStorage.Load(assessmentID,BossRole);

        // Crear una evaluación nueva si no hay datos guardados válidos.
        if (managerResponses == null || managerResponses.assesmentID != assessmentID || managerResponses.participantRole != ManagerRole)
        {
            managerResponses = new AssesmentResponses
            {
                assesmentID = assessmentID,
                participantRole = ManagerRole
            };
        }

        if (bossResponses == null ||
            bossResponses.assesmentID != assessmentID ||
            bossResponses.participantRole != BossRole)
        {
            bossResponses = new AssesmentResponses
            {
                assesmentID = assessmentID,
                participantRole = BossRole
            };
        }

        // Protegernos ante una lista de respuestas inexistente.
        if (managerResponses.responses == null)
            managerResponses.responses = new List<QuestionResponses>();

        if (bossResponses.responses == null)
            bossResponses.responses = new List<QuestionResponses>();

        IsInitialized = true;

        Debug.Log("AssessmentManager inicializado correctamente.");
    }

    private AssesmentResponses CreateResponses(string assessmentID, string role)
    {
        return new AssesmentResponses
        {
            assesmentID = assessmentID,
            participantRole = role,
            responses = new List<QuestionResponses>(),
            isSubmitted = false
        };
    }


    public bool RegisterAnswer(string role, string questionID, int score, string evidence = "")
    {
        if (!IsInitialized)
            return false;

        AssesmentResponses participant = GetResponses(role);

        if (participant == null || participant.isSubmitted)
            return false;

        if (score < 1 || score > 5)
        {
            Debug.LogWarning("La puntuación debe estar entre 1 y 5.");
            return false;
        }

        if (!QuestionExists(questionID))
        {
            Debug.LogWarning("Pregunta desconocida: " + questionID);
            return false;
        }

        // La evidencia es obligatoria y debe tener al menos 20 caracteres.
        string cleanEvidence = evidence == null ? "" : evidence.Trim();

        if (cleanEvidence.Length < 20)
        {
            Debug.LogWarning("La evidencia debe tener al menos 20 caracteres, sin contar espacios al inicio o al final.");
            return false;
        }

        // Actualiza la respuesta si ya existe; de lo contrario, la crea.
        QuestionResponses answer = participant.responses.Find(r => r.questionID == questionID);

        if (answer == null)
        {
            answer = new QuestionResponses
            {
                questionID = questionID
            };

            participant.responses.Add(answer);
        }

        answer.score = score;
        answer.evidence = cleanEvidence;

        if (!AssessmentLocalStorage.Save(participant))
        {
            Debug.LogWarning("La respuesta se registró en memoria, " + "pero no se pudo guardar en disco.");
        }

        return true;
    }


    public int GetAnsweredCount(string role)
    {
        AssesmentResponses participant = GetResponses(role);

        return participant == null
            ? 0
            : participant.responses.Count;
    }

    public float GetProgressPercentage(string role)
    {
        int totalQuestions = GetTotalQuestionCount();

        if (totalQuestions == 0)
            return 0f;

        int answeredQuestions = GetAnsweredCount(role);

        return (float)answeredQuestions / totalQuestions * 100f;
    }

    public bool TryGetTopicComparison(
        string role,
        string topicID,
        out float average,
        out float requiredLevel,
        out float difference)
    {
        average = 0f;
        requiredLevel = 0f;
        difference = 0f;


        if (!IsInitialized || assessmentData == null)
            return false;

        AssesmentResponses participant = GetResponses(role);

        if (participant == null)
            return false;

        // Buscar la competencia solicitada.
        AssesmentTopic selectedTopic = assessmentData.topics.Find(
            topic => topic.id == topicID
        );

        if (selectedTopic == null ||
            selectedTopic.question == null ||
            selectedTopic.question.Count == 0)
        {
            Debug.LogWarning("No se encontró la competencia: " + topicID);
            return false;
        }

        float totalScore = 0f;

        // Todas las preguntas de la competencia deben tener respuesta.
        foreach (AssesmentQuestion question in selectedTopic.question)
        {
            QuestionResponses response = participant.responses.Find(
                r => r.questionID == question.id
            );

            if (response == null ||
                response.score < 1 ||
                response.score > 5)
            {
                return false;
            }

            totalScore += response.score;
        }

        average = totalScore / selectedTopic.question.Count;
        requiredLevel = selectedTopic.requiredLevel;
        difference = average - requiredLevel;


        return true;
    }


    public bool TryGetAllTopicResults(
        string role,
        out List<TopicAssessmentResult> results)
    {
        results = new List<TopicAssessmentResult>();

        if (!IsInitialized || assessmentData == null)
            return false;

        foreach (AssesmentTopic topic in assessmentData.topics)
        {
            bool calculated = TryGetTopicComparison(
                role,
                topic.id,
                out float average,
                out float requiredLevel,
                out float difference
            );

            // No devolver resultados parciales si faltan respuestas.
            if (!calculated)
            {
                results.Clear();
                Debug.LogWarning(
                    "Faltan respuestas para calcular la competencia: " +
                    topic.title
                );
                return false;
            }

            TopicAssessmentResult result = new TopicAssessmentResult
            {
                topicID = topic.id,
                topicTitle = topic.title,
                average = average,
                requiredLevel = requiredLevel,
                difference = difference
            };

            results.Add(result);
        }

        return true;
    }


    public bool TryCompareManagerAndBoss(
        out List<TopicComparisonResult> comparisons)
    {
        comparisons = new List<TopicComparisonResult>();

        if (!IsInitialized)
            return false;


        if (!AreBothAssessmentsSubmitted())
        {
            Debug.LogWarning("No se puede comparar: ambas evaluaciones " + "deben estar finalizadas y enviadas.");

            return false;
        }


        // Obtener resultados completos de ambos participantes.
        if (!TryGetAllTopicResults(ManagerRole,out List<TopicAssessmentResult> managerResults))
        {
            Debug.LogWarning("No se puede comparar: faltan respuestas del gerente.");
            return false;
        }

        if (!TryGetAllTopicResults(BossRole, out List<TopicAssessmentResult> bossResults))
        {
            Debug.LogWarning("No se puede comparar: faltan respuestas del jefe.");
            return false;
        }

        foreach (TopicAssessmentResult managerResult in managerResults)
        {
            TopicAssessmentResult bossResult = bossResults.Find(result => result.topicID == managerResult.topicID);

            if (bossResult == null)
            {
                comparisons.Clear();

                Debug.LogWarning("No se encontró la competencia correspondiente: " +managerResult.topicID);

                return false;
            }

            TopicComparisonResult comparison = new TopicComparisonResult
            {
                topicID = managerResult.topicID,
                topicTitle = managerResult.topicTitle,

                managerAverage = managerResult.average,
                bossAverage = bossResult.average,
                requiredLevel = managerResult.requiredLevel,

                managerDifference = managerResult.average - managerResult.requiredLevel,

                bossDifference = bossResult.average - bossResult.requiredLevel,

                perceptionDifference = managerResult.average - bossResult.average

            };

            ClassifyComparison(comparison);

            comparisons.Add(comparison);
        }

        return true;
    }

    public int GetTotalQuestionCount()
    {
        if (assessmentData == null)
            return 0;

        int total = 0;

        foreach (AssesmentTopic topic in assessmentData.topics)
            total += topic.question.Count;

        return total;
    }


    public bool TrySubmitAssessment(string role)
    {
        if (!IsInitialized)
            return false;

        AssesmentResponses participant = GetResponses(role);

        if (participant == null || participant.isSubmitted)
        {
            Debug.LogWarning(
                "No se puede enviar: participante inexistente o evaluación ya enviada."
            );
            return false;
        }

        int totalQuestions = GetTotalQuestionCount();

        if (totalQuestions == 0 ||
            participant.responses.Count != totalQuestions)
        {
            Debug.LogWarning("No se puede enviar: faltan respuestas.");
            return false;
        }

        foreach (QuestionResponses response in participant.responses)
        {
            if (!QuestionExists(response.questionID))
            {
                Debug.LogWarning("No se puede enviar: pregunta desconocida " +response.questionID );
                return false;
            }

            if (response.score < 1 || response.score > 5)
            {
                Debug.LogWarning("No se puede enviar: puntuación inválida en " +response.questionID);
                return false;
            }

            string evidence = response.evidence == null
                ? ""
                : response.evidence.Trim();

            if (evidence.Length < 20)
            {
                Debug.LogWarning(
                    "No se puede enviar: evidencia insuficiente en " +
                    response.questionID
                );
                return false;
            }
        }

        participant.isSubmitted = true;

        if (!AssessmentLocalStorage.Save(participant))
        {
            participant.isSubmitted = false;

            Debug.LogError("No se pudo guardar la evaluación enviada. " + "La evaluación no se marcará como completada.");

            return false;
        }

        Debug.Log("Evaluación enviada correctamente para: " + role);

        return true;
    }

    private AssesmentResponses GetResponses(string role)
    {
        if (role == ManagerRole)
            return managerResponses;

        if (role == BossRole)
            return bossResponses;

        return null;
    }

    private bool QuestionExists(string questionID)
    {
        if (assessmentData == null)
            return false;

        foreach (AssesmentTopic topic in assessmentData.topics)
        {
            foreach (AssesmentQuestion question in topic.question)
            {
                if (question.id == questionID)
                    return true;
            }
        }

        return false;
    }
    public bool AreBothAssessmentsSubmitted()
    {
        if (!IsInitialized)
            return false;

        return managerResponses != null
            && bossResponses != null
            && managerResponses.isSubmitted
            && bossResponses.isSubmitted;
    }

    private void ClassifyComparison(TopicComparisonResult comparison)
    {
        // Dimensión 1: alineación de percepción.


        const float threshold = 0.4f;
        const float tolerance = 0.0001f;

        if (comparison.perceptionDifference < -threshold - tolerance)
        {
            comparison.perceptionAlignment = PerceptionAlignment.BossRatesHigher;
        }
        else if (comparison.perceptionDifference > threshold + tolerance)
        {
            comparison.perceptionAlignment = PerceptionAlignment.ManagerRatesHigher;
        }
        else
        {
            comparison.perceptionAlignment = PerceptionAlignment.Aligned;
        }


        // Dimensión 2: nivel requerido.
        bool managerBelow = comparison.managerAverage < comparison.requiredLevel;

        bool bossBelow = comparison.bossAverage < comparison.requiredLevel;

        if (managerBelow && bossBelow)
        {
            comparison.requiredLevelStatus = RequiredLevelStatus.SharedOpportunity;
        }
        else if (managerBelow)
        {
            comparison.requiredLevelStatus = RequiredLevelStatus.ManagerOpportunity;
        }
        else if (bossBelow)
        {
            comparison.requiredLevelStatus = RequiredLevelStatus.BossOpportunity;
        }
        else
        {
            comparison.requiredLevelStatus = RequiredLevelStatus.BothMeetRequiredLevel;
        }
    }

    //TEST PART

    [ContextMenu("Test Assessment Loading")]
    private void TestAssessmentLoading()
    {
        InitializeAssessment("ASSESSMENT_TEST_001");
    }


    [ContextMenu("Test Answer Registration")]
    private void TestAnswerRegistration()
    {
        // Reinicia la evaluación para que la prueba sea repetible.
        InitializeAssessment("ASSESSMENT_TEST_001");

        if (!IsInitialized)
        {
            Debug.LogError("No se pudo inicializar la evaluación.");
            return;
        }

        string questionID = assessmentData.topics[0].question[0].id;

        Debug.Log("Probando pregunta: " + questionID);

        // PRUEBA 1: Registrar una respuesta válida del gerente.
        bool test1 = RegisterAnswer(
            ManagerRole,
            questionID,
            4,
            "Organicé las tareas del equipo y revisé los resultados."
        );

        // PRUEBA 2: Actualizar la misma respuesta sin duplicarla.
        bool test2 = RegisterAnswer(
            ManagerRole,
            questionID,
            5,
            "Organicé las tareas y mejoré el seguimiento semanal."
        );

        // PRUEBA 3: Registrar una respuesta independiente del jefe.
        bool test3 = RegisterAnswer(
            BossRole,
            questionID,
            2,
            "Necesita mejorar la comunicación y el seguimiento."
        );

        // PRUEBA 4: Intentar registrar evidencia demasiado corta.
        bool test4 = RegisterAnswer(
            ManagerRole,
            questionID,
            3,
            "Muy poco"
        );

        // Comprobar los resultados.
        bool managerCorrect =
            managerResponses.responses.Count == 1 &&
            managerResponses.responses[0].score == 5 &&
            managerResponses.responses[0].evidence ==
                "Organicé las tareas y mejoré el seguimiento semanal.";

        bool bossCorrect =
            bossResponses.responses.Count == 1 &&
            bossResponses.responses[0].score == 2;

        bool invalidEvidenceRejected = !test4;

        Debug.Log("Prueba 1 - Respuesta válida: " + test1);
        Debug.Log("Prueba 2 - Actualización válida: " + test2);
        Debug.Log("Prueba 3 - Respuesta del jefe: " + test3);
        Debug.Log("Prueba 4 - Evidencia corta rechazada: " + invalidEvidenceRejected);

        Debug.Log(
            "Respuestas del gerente: " +
            GetAnsweredCount(ManagerRole)
        );

        Debug.Log(
            "Respuestas del jefe: " +
            GetAnsweredCount(BossRole)
        );

        if (test1 && test2 && test3 &&
            managerCorrect && bossCorrect && invalidEvidenceRejected)
        {
            Debug.Log("PRUEBA COMPLETADA: todas las comprobaciones pasaron.");
        }
        else
        {
            Debug.LogError("PRUEBA FALLIDA: revisa los resultados.");
        }
    }

    [ContextMenu("Test Assessment Progress")]
    private void TestAssessmentProgress()
    {
        if (!IsInitialized)
        {
            InitializeAssessment("ASSESSMENT_TEST_001");
        }

        if (!IsInitialized)
            return;

        // Registrar una respuesta de prueba para cada participante.
        string questionID = assessmentData.topics[0].question[0].id;

        RegisterAnswer(ManagerRole, questionID, 4, "Prueba de progreso");
        RegisterAnswer(BossRole, questionID, 3, "Prueba de progreso");

        int managerAnswered = GetAnsweredCount(ManagerRole);
        float managerProgress = GetProgressPercentage(ManagerRole);

        int bossAnswered = GetAnsweredCount(BossRole);
        float bossProgress = GetProgressPercentage(BossRole);

        Debug.Log("Gerente: " + managerAnswered + "/65 preguntas. Progreso: " + managerProgress.ToString("F1") + "%");

        Debug.Log("Jefe: " + bossAnswered + "/65 preguntas. Progreso: " + bossProgress.ToString("F1") + "%");
    }


    [ContextMenu("Test Assessment Submission")]
    private void TestAssessmentSubmission()
    {
        // Reiniciar la evaluación para que el test sea repetible.
        InitializeAssessment("ASSESSMENT_SUBMISSION_TEST");

        if (!IsInitialized)
        {
            Debug.LogError("No se pudo inicializar la evaluación.");
            return;
        }

        // PRUEBA 1: Intentar enviar una evaluación incompleta.
        bool incompleteRejected =
            !TrySubmitAssessment(ManagerRole);

        Debug.Log(
            "Prueba 1 - Evaluación incompleta rechazada: " +
            incompleteRejected
        );

        // PRUEBA 2: Registrar las 65 respuestas del gerente.
        bool allAnswersRegistered = true;

        foreach (AssesmentTopic topic in assessmentData.topics)
        {
            foreach (AssesmentQuestion question in topic.question)
            {
                bool registered = RegisterAnswer(
                    ManagerRole,
                    question.id,
                    4,
                    "Ejemplo de evidencia válida para esta pregunta."
                );

                if (!registered)
                    allAnswersRegistered = false;
            }
        }

        Debug.Log(
            "Respuestas registradas correctamente: " +
            allAnswersRegistered
        );

        // PRUEBA 3: Enviar la evaluación completa.
        bool completeAccepted =
            TrySubmitAssessment(ManagerRole);

        Debug.Log(
            "Prueba 2 - Evaluación completa aceptada: " +
            completeAccepted
        );

        // PRUEBA 4: Intentar modificar una evaluación enviada.
        string firstQuestionID =
            assessmentData.topics[0].question[0].id;

        bool modificationRejected = !RegisterAnswer(
            ManagerRole,
            firstQuestionID,
            3,
            "Intento de modificar una evaluación ya enviada."
        );

        Debug.Log(
            "Prueba 3 - Modificación posterior rechazada: " +
            modificationRejected
        );

        // PRUEBA 5: Intentar enviarla de nuevo.
        bool secondSubmissionRejected =
            !TrySubmitAssessment(ManagerRole);

        Debug.Log(
            "Prueba 4 - Segundo envío rechazado: " +
            secondSubmissionRejected
        );

        // RESULTADO FINAL.
        bool testPassed =
            incompleteRejected &&
            allAnswersRegistered &&
            completeAccepted &&
            modificationRejected &&
            secondSubmissionRejected &&
            GetAnsweredCount(ManagerRole) == 65 &&
            managerResponses.isSubmitted;

        if (testPassed)
        {
            Debug.Log("PRUEBA COMPLETADA: todas las comprobaciones pasaron.");
        }
        else
        {
            Debug.LogError("PRUEBA FALLIDA: revisa los resultados anteriores.");
        }
    }


    [ContextMenu("Test Topic Comparison")]
    private void TestTopicComparison()
    {
        InitializeAssessment("ASSESSMENT_COMPARISON_TEST");

        if (!IsInitialized)
        {
            Debug.LogError("No se pudo inicializar la evaluación.");
            return;
        }

        // Usaremos la primera competencia y sus cinco preguntas.
        AssesmentTopic topic = assessmentData.topics[0];

        if (topic.question.Count != 5)
        {
            Debug.LogError(
                "La competencia de prueba no tiene exactamente 5 preguntas."
            );
            return;
        }

        // Puntuaciones de prueba: 3, 4, 3, 2 y 4.
        int[] testScores = { 3, 4, 3, 2, 4 };

        bool allRegistered = true;

        for (int i = 0; i < topic.question.Count; i++)
        {
            bool registered = RegisterAnswer(
                ManagerRole,
                topic.question[i].id,
                testScores[i],
                "Ejemplo de evidencia válida para esta pregunta."
            );

            if (!registered)
                allRegistered = false;
        }

        if (!allRegistered)
        {
            Debug.LogError("No se pudieron registrar todas las respuestas.");
            return;
        }

        bool calculated = TryGetTopicComparison(
            ManagerRole,
            topic.id,
            out float average,
            out float requiredLevel,
            out float difference
        );

        if (!calculated)
        {
            Debug.LogError("No se pudo calcular la comparación.");
            return;
        }

        Debug.Log("Competencia: " + topic.title);
        Debug.Log("Promedio obtenido: " + average.ToString("F1"));
        Debug.Log("Nivel requerido: " + requiredLevel.ToString("F1"));
        Debug.Log("Diferencia: " + difference.ToString("F1"));

        bool testPassed =
            Mathf.Abs(average - 3.2f) < 0.001f &&
            Mathf.Abs(difference - (average - requiredLevel)) < 0.001f;

        if (testPassed)
        {
            Debug.Log(
                "PRUEBA COMPLETADA: promedio y comparación correctos."
            );
        }
        else
        {
            Debug.LogError("PRUEBA FALLIDA: revisa los cálculos.");
        }
    }


    [ContextMenu("Test All Topic Results")]
    private void TestAllTopicResults()
    {
        InitializeAssessment("ASSESSMENT_ALL_RESULTS_TEST");

        if (!IsInitialized)
        {
            Debug.LogError("No se pudo inicializar la evaluación.");
            return;
        }

        // Registrar las 65 respuestas ficticias del gerente.
        bool allRegistered = true;

        for (int topicIndex = 0;
             topicIndex < assessmentData.topics.Count;
             topicIndex++)
        {
            AssesmentTopic topic = assessmentData.topics[topicIndex];

            for (int questionIndex = 0;
                 questionIndex < topic.question.Count;
                 questionIndex++)
            {
                int score = (topicIndex + questionIndex) % 5 + 1;

                bool registered = RegisterAnswer(
                    ManagerRole,
                    topic.question[questionIndex].id,
                    score,
                    "Ejemplo de evidencia válida para realizar la prueba."
                );

                if (!registered)
                    allRegistered = false;
            }
        }

        if (!allRegistered)
        {
            Debug.LogError("No se pudieron registrar todas las respuestas.");
            return;
        }

        // Calcular los resultados de las 13 competencias.
        bool calculated = TryGetAllTopicResults(
            ManagerRole,
            out List<TopicAssessmentResult> results
        );

        if (!calculated)
        {
            Debug.LogError("No se pudieron calcular todos los resultados.");
            return;
        }

        foreach (TopicAssessmentResult result in results)
        {
            Debug.Log(
                result.topicTitle +
                " | Promedio: " + result.average.ToString("F1") +
                " | Requerido: " + result.requiredLevel.ToString("F1") +
                " | Diferencia: " + result.difference.ToString("F1")
            );
        }

        bool testPassed =
            results.Count == 13 &&
            GetAnsweredCount(ManagerRole) == 65;

        if (testPassed)
        {
            Debug.Log(
                "PRUEBA COMPLETADA: se calcularon las 13 competencias."
            );
        }
        else
        {
            Debug.LogError("PRUEBA FALLIDA: revisa los resultados.");
        }
    }


    [ContextMenu("Test Manager Boss Comparison")]
    private void TestManagerBossComparison()
    {
        InitializeAssessment("ASSESSMENT_COMPARISON_TEST");

        if (!IsInitialized)
        {
            Debug.LogError("No se pudo inicializar la evaluación.");
            return;
        }

        bool allRegistered = true;

        foreach (AssesmentTopic topic in assessmentData.topics)
        {
            foreach (AssesmentQuestion question in topic.question)
            {
                bool managerRegistered = RegisterAnswer(
                    ManagerRole,
                    question.id,
                    4,
                    "Ejemplo de evidencia válida del gerente."
                );

                bool bossRegistered = RegisterAnswer(
                    BossRole,
                    question.id,
                    3,
                    "Ejemplo de evidencia válida del jefe."
                );

                if (!managerRegistered || !bossRegistered)
                    allRegistered = false;
            }
        }

        if (!allRegistered)
        {
            Debug.LogError("No se pudieron registrar todas las respuestas.");
            return;
        }

        bool calculated = TryCompareManagerAndBoss(
            out List<TopicComparisonResult> comparisons
        );

        if (!calculated)
        {
            Debug.LogError("No se pudo realizar la comparación.");
            return;
        }

        foreach (TopicComparisonResult result in comparisons)
        {
            Debug.Log(
                result.topicTitle +
                " | Gerente: " + result.managerAverage.ToString("F1") +
                " | Jefe: " + result.bossAverage.ToString("F1") +
                " | Requerido: " + result.requiredLevel.ToString("F1") +
                " | Brecha gerente: " + result.managerDifference.ToString("F1") +
                " | Brecha jefe: " + result.bossDifference.ToString("F1") +
                " | Diferencia de percepción: " +
                result.perceptionDifference.ToString("F1")
            );
        }

        bool testPassed = comparisons.Count == 13;

        foreach (TopicComparisonResult result in comparisons)
        {
            if (Mathf.Abs(result.managerAverage - 4f) > 0.001f ||
                Mathf.Abs(result.bossAverage - 3f) > 0.001f ||
                Mathf.Abs(result.perceptionDifference - 1f) > 0.001f)
            {
                testPassed = false;
            }
        }

        if (testPassed)
        {
            Debug.Log(
                "PRUEBA COMPLETADA: las 13 comparaciones son correctas."
            );
        }
        else
        {
            Debug.LogError("PRUEBA FALLIDA: revisa los resultados.");
        }
    }
}