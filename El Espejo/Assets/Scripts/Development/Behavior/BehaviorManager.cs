using System.Collections.Generic;
using UnityEngine;

public class BehaviorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    [Header("Behavior Questions")]
    public BehaviorQuestionData questionData =new BehaviorQuestionData();

    public List<BehaviorQuestion> GetQuestionsForTopic(string topicID)
    {
        List<BehaviorQuestion> availableQuestions =
            new List<BehaviorQuestion>();

        if (planManager == null ||
            planManager.CurrentPlan == null ||
            questionData == null ||
            questionData.questions == null)
        {
            Debug.LogWarning(
                "No están disponibles los datos de comportamiento."
            );

            return availableQuestions;
        }

        DevelopmentTopicSummary topicResult =
            planManager.CurrentPlan.topicResults.Find(
                topic => topic.topicID == topicID
            );

        if (topicResult == null)
        {
            Debug.LogWarning(
                $"No existen resultados para {topicID}."
            );

            return availableQuestions;
        }

        foreach (BehaviorQuestion question in questionData.questions)
        {
            if (question == null || !question.isActive)
                continue;

            // Filtrar por competencia.
            if (!string.IsNullOrEmpty(question.topicID) &&
                question.topicID != topicID)
                continue;

            // Filtrar por nivel requerido.
            if (question.requiredLevelStatuses != null &&
                question.requiredLevelStatuses.Count > 0 &&
                !question.requiredLevelStatuses.Contains(
                    topicResult.requiredLevelStatus))
                continue;

            // Filtrar por diferencia de percepción.
            if (question.perceptionAlignments != null &&
                question.perceptionAlignments.Count > 0 &&
                !question.perceptionAlignments.Contains(
                    topicResult.perceptionAlignment))
                continue;

            availableQuestions.Add(question);
        }

        return availableQuestions;
    }

    public bool SaveBehaviorResponse(
    string topicID,
    string questionID,
    int frequencyScore,
    string selectedOptionID,
    string writtenResponse = "")
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.behaviorData == null)
            return false;

        BehaviorQuestion question = GetQuestionsForTopic(topicID).Find( item => item.questionID == questionID);

        if (question == null)
            return false;

        // Validación según el tipo de pregunta.
        if (question.questionType == "Frequency")
        {
            if (frequencyScore < 1 || frequencyScore > 5)
                return false;
        }
        else if (question.questionType == "Context" ||
                 question.questionType == "Consistency")
        {
            if (question.options == null ||
                !question.options.Exists(
                    option => option != null &&
                              option.optionID == selectedOptionID))
            {
                return false;
            }
        }
        else
        {
            Debug.LogWarning( $"Tipo de pregunta no reconocido: {question.questionType}");
            return false;
        }

        BehaviorData data = planManager.CurrentPlan.behaviorData;

        if (data.responses == null)
            data.responses = new List<BehaviorResponse>();

        BehaviorResponse response = data.responses.Find( item => item.topicID == topicID && item.questionID == questionID);

        if (response == null)
        {
            response = new BehaviorResponse
            {
                topicID = topicID,
                questionID = questionID
            };

            data.responses.Add(response);
        }

        response.questionType = question.questionType;

        response.frequencyScore = question.questionType == "Frequency"
                ? frequencyScore
                : 0;

        response.selectedOptionID = question.questionType == "Frequency"
                ? ""
                : selectedOptionID;

        response.writtenResponse = writtenResponse?.Trim() ?? "";

        return true;
    }

    public BehaviorResponse GetSavedResponse(
    string topicID,
    string questionID)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.behaviorData == null ||
            planManager.CurrentPlan.behaviorData.responses == null)
            return null;

        return planManager.CurrentPlan.behaviorData.responses.Find(
            item => item.topicID == topicID &&
                    item.questionID == questionID
        );
    }

    public bool IsTopicBehaviorCompleted(string topicID)
    {
        List<BehaviorQuestion> questions =
            GetQuestionsForTopic(topicID);

        if (questions.Count == 0)
            return false;

        foreach (BehaviorQuestion question in questions)
        {
            BehaviorResponse response =
                GetSavedResponse(topicID, question.questionID);

            if (response == null ||
                response.questionType != question.questionType)
                return false;

            if (question.questionType == "Frequency")
            {
                if (response.frequencyScore < 1 ||
                    response.frequencyScore > 5)
                    return false;
            }
            else if (question.questionType == "Context" ||
                     question.questionType == "Consistency")
            {
                if (question.options == null ||
                    !question.options.Exists(
                        option => option != null &&
                                  option.optionID ==
                                  response.selectedOptionID))
                    return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}