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
} 