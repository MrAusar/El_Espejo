using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersonalDevelopmentPlanUIController : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    [Header("Panel")]
    [SerializeField]
    private GameObject planPanel;

    [Header("Results")]
    [SerializeField]
    private TMP_Text resultsContentText;

    [Header("Priorities")]
    [SerializeField]
    private TMP_Text prioritiesContentText;

    [Header("Feedback")]
    [SerializeField]
    private TMP_Text feedbackText;

    [Header("Reflections")]
    [SerializeField]
    private TMP_Text reflectionContentText;

    [Header("Interpretations")]
    [SerializeField]
    private InterpretationManager interpretationManager;

    [SerializeField]
    private TMP_Text interpretationContentText;

    [Header("Behavior")]
    [SerializeField]
    private BehaviorManager behaviorManager;

    [SerializeField]
    private TMP_Text behaviorContentText;

    [Header("Development Resources")]
    [SerializeField]
    private TMP_Text resourcesContentText;

    [Header("Plan Status")]
    [SerializeField]
    private ReflectionManager reflectionManager;

    [SerializeField]
    private InterpretationManager planInterpretationManager;

    [SerializeField]
    private BehaviorManager planBehaviorManager;

    [SerializeField]
    private PrioritizationManager prioritizationManager;

    [SerializeField]
    private TMP_Text planStatusText;

    [Header("Save")]
    [SerializeField]
    private Button savePlanButton;

    private void Awake()
    {
        if (savePlanButton != null)
        {
            savePlanButton.onClick.AddListener(OnSavePlanClicked);
        }
    }
    private void OnDestroy()
    {
        if (savePlanButton != null)
        {
            savePlanButton.onClick.RemoveListener( OnSavePlanClicked);
        }
    }
    public bool OpenPlan()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planPanel == null ||
            resultsContentText == null ||
            prioritiesContentText == null)
        {
            Debug.LogWarning("Faltan referencias del Plan Personal de Desarrollo.");

            return false;
        }

        // Reconstruir el resumen desde las evaluaciones.
        planManager.BuildAssessmentSummary();

        PersonalDevelopmentPlanData plan = planManager.CurrentPlan;

        if (plan.topicResults == null ||
            plan.topicResults.Count == 0)
        {
            Debug.LogWarning("No existen resultados para generar el plan.");

            return false;
        }

        ShowResults(plan);
        ShowPriorities(plan);
        ShowReflections(plan);

        ShowInterpretations(plan);
        ShowBehavior(plan);

        UpdatePlanStatus(plan);

        ShowDevelopmentResources();

        if (feedbackText != null)
            feedbackText.text = "";

        planPanel.SetActive(true);

        Debug.Log("Plan Personal de Desarrollo abierto correctamente.");

        return true;
    }


    private void ShowResults(PersonalDevelopmentPlanData plan)
    {
        StringBuilder text = new StringBuilder();

        text.AppendLine("RESULTADOS DE MIS COMPETENCIAS");
        text.AppendLine();

        foreach (DevelopmentTopicSummary topic in plan.topicResults)
        {
            text.AppendLine($"{topic.topicID} - {topic.topicTitle}");

            text.AppendLine($"Autoevaluación: {topic.managerAverage:F1}");

            text.AppendLine($"Evaluación del jefe: {topic.bossAverage:F1}");

            text.AppendLine($"Nivel requerido: {topic.requiredLevel:F1}");

            text.AppendLine();
        }

        resultsContentText.text = text.ToString();
    }
    private void ShowPriorities(PersonalDevelopmentPlanData plan)
    {
        StringBuilder text = new StringBuilder();

        text.AppendLine("MIS COMPETENCIAS PRIORITARIAS");
        text.AppendLine();

        if (plan.prioritizationData == null ||
            plan.prioritizationData.selectedTopicIDs == null ||
            plan.prioritizationData.selectedTopicIDs.Count == 0)
        {
            text.AppendLine("Todavía no has seleccionado prioridades.");

            prioritiesContentText.text = text.ToString();
            return;
        }

        foreach (string topicID in
            plan.prioritizationData.selectedTopicIDs)
        {
            DevelopmentTopicSummary topic = plan.topicResults.Find(item => item.topicID == topicID);

            if (topic != null)
            {
                text.AppendLine($"• {topic.topicTitle} ({topic.topicID})");
            }
        }

        prioritiesContentText.text = text.ToString();
    }

    private void ShowReflections(PersonalDevelopmentPlanData plan)
    {
        if (reflectionContentText == null)
        {
            Debug.LogWarning( "ReflectionContentText no asignado.");
            return;
        }

        StringBuilder text = new StringBuilder();

        text.AppendLine("MIS REFLEXIONES");
        text.AppendLine();

        if (plan.reflectionData == null ||
            plan.reflectionData.responses == null ||
            plan.reflectionData.responses.Count == 0)
        {
            reflectionContentText.text ="Todavía no has registrado reflexiones.";

            return;
        }

        foreach (DevelopmentTopicSummary topic in plan.topicResults)
        {
            bool hasResponses = false;

            foreach (ReflectionResponse response
                in plan.reflectionData.responses)
            {
                if (response.topicID != topic.topicID)
                    continue;

                if (!hasResponses)
                {
                    text.AppendLine($"{topic.topicID} - {topic.topicTitle}");

                    hasResponses = true;
                }

                string stageName = response.reflectionStage;

                switch (response.reflectionStage)
                {
                    case "Awareness":
                        stageName = "Conciencia";
                        break;

                    case "Evidence":
                        stageName = "Evidencia";
                        break;

                    case "Projection":
                        stageName = "Proyección";
                        break;
                }

                text.AppendLine($"[{stageName}]");
                text.AppendLine(response.answer);
                text.AppendLine();
            }

            if (hasResponses)
            {
                text.AppendLine("------------------------");
                text.AppendLine();
            }
        }

        reflectionContentText.text = text.ToString();
    }

    private void ShowInterpretations(
    PersonalDevelopmentPlanData plan)
    {
        if (interpretationContentText == null)
            return;

        StringBuilder text = new StringBuilder();

        if (plan.interpretationData == null ||
            plan.interpretationData.interpretations == null ||
            plan.interpretationData.interpretations.Count == 0)
        {
            interpretationContentText.text =
                "Todavía no has registrado interpretaciones.";
            return;
        }

        foreach (DevelopmentTopicSummary topic in plan.topicResults)
        {
            TopicInterpretation interpretation =
                plan.interpretationData.interpretations.Find(
                    item => item.topicID == topic.topicID
                );

            if (interpretation == null ||
                interpretation.selectedOptionIDs == null ||
                interpretation.selectedOptionIDs.Count == 0)
                continue;

            text.AppendLine(
                $"{topic.topicID} - {topic.topicTitle}"
            );

            List<InterpretationOption> availableOptions =
                interpretationManager != null
                    ? interpretationManager.GetOptionsForTopic(
                        topic.topicID)
                    : new List<InterpretationOption>();

            foreach (string optionID in
                interpretation.selectedOptionIDs)
            {
                InterpretationOption option =
                    availableOptions.Find(
                        item => item.optionID == optionID
                    );

                text.AppendLine(
                    option != null
                        ? $"• {option.optionText}"
                        : $"• Opción {optionID}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                interpretation.customExplanation))
            {
                text.AppendLine();
                text.AppendLine("Explicación personal:");
                text.AppendLine(
                    interpretation.customExplanation
                );
            }

            text.AppendLine();
            text.AppendLine("------------------------");
            text.AppendLine();
        }

        interpretationContentText.text = text.ToString();
    }

    private void ShowBehavior(
    PersonalDevelopmentPlanData plan)
    {
        if (behaviorContentText == null)
            return;

        StringBuilder text = new StringBuilder();

        if (plan.behaviorData == null ||
            plan.behaviorData.responses == null ||
            plan.behaviorData.responses.Count == 0)
        {
            behaviorContentText.text =
                "Todavía no has registrado respuestas de comportamiento.";
            return;
        }

        foreach (DevelopmentTopicSummary topic in plan.topicResults)
        {
            List<BehaviorResponse> responses =
                plan.behaviorData.responses.FindAll(
                    item => item.topicID == topic.topicID
                );

            if (responses.Count == 0)
                continue;

            text.AppendLine(
                $"{topic.topicID} - {topic.topicTitle}"
            );

            List<BehaviorQuestion> questions =
                behaviorManager != null
                    ? behaviorManager.GetQuestionsForTopic(
                        topic.topicID)
                    : new List<BehaviorQuestion>();

            foreach (BehaviorResponse response in responses)
            {
                BehaviorQuestion question =
                    questions.Find(
                        item => item.questionID ==
                                response.questionID
                    );

                if (question != null)
                    text.AppendLine(question.questionText);

                if (response.questionType == "Frequency")
                {
                    text.AppendLine(
                        $"Frecuencia: {response.frequencyScore}/5"
                    );
                }
                else
                {
                    BehaviorOption selectedOption =
                        question?.options?.Find(
                            item => item.optionID ==
                                    response.selectedOptionID
                        );

                    text.AppendLine(
                        selectedOption != null
                            ? selectedOption.optionText
                            : $"Opción: {response.selectedOptionID}"
                    );
                }

                if (!string.IsNullOrWhiteSpace(
                    response.writtenResponse))
                {
                    text.AppendLine(
                        $"Comentario: {response.writtenResponse}"
                    );
                }

                text.AppendLine();
            }

            text.AppendLine("------------------------");
            text.AppendLine();
        }

        behaviorContentText.text = text.ToString();
    }

    private void ShowDevelopmentResources()
    {
        if (resourcesContentText == null)
            return;

        resourcesContentText.text =
            "RECURSOS DE DESARROLLO\n\n" +
            "Próximamente encontrarás aquí guías y retos " +
            "de desarrollo relacionados con tus competencias " +
            "prioritarias.\n\n" +
            "Podrás consultar estos materiales mediante " +
            "enlaces a documentos PDF externos.";
    }

    private void UpdatePlanStatus(PersonalDevelopmentPlanData plan)
    {
        if (planStatusText == null ||
            reflectionManager == null ||
            interpretationManager == null ||
            behaviorManager == null ||
            prioritizationManager == null)
        {
            Debug.LogWarning( "Faltan referencias para calcular el estado del PPD.");
            return;
        }

        bool hasResults =
            plan.topicResults != null &&
            plan.topicResults.Count == 13;

        bool validPriorities = prioritizationManager.HasValidPriorities();

        bool completed = hasResults && validPriorities;

        List<string> priorities =prioritizationManager.GetSelectedPriorities();

        int completedPriorities = 0;

        foreach (string topicID in priorities)
        {
            bool reflectionComplete = reflectionManager.IsTopicReflectionCompleted(topicID);

            bool interpretationComplete = interpretationManager.IsTopicInterpretationCompleted(topicID);

            bool behaviorComplete =behaviorManager.IsTopicBehaviorCompleted( topicID);

            if (reflectionComplete &&
                interpretationComplete &&
                behaviorComplete)
            {
                completedPriorities++;
            }
            else
            {
                completed = false;
            }
        }

        planStatusText.text = completed
                ? "Estado del plan: Listo para revisión"
                : $"Estado del plan: En construcción " +
                  $"({completedPriorities}/{priorities.Count} " +
                  "prioridades completadas)";
    }

    private void OnSavePlanClicked()
    {
        if (planManager == null)
        {
            Debug.LogWarning("PersonalDevelopmentPlanManager no asignado.");

            return;
        }

        bool saved = planManager.SaveCurrentPlan();

        if (feedbackText != null)
        {
            feedbackText.text = saved
                ? "Tu progreso se guardó correctamente."
                : "No se pudo guardar tu progreso.";
        }
    }
}