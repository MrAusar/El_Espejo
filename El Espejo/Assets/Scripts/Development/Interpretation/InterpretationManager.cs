using System.Collections.Generic;
using UnityEngine;

public class InterpretationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    [Header("Interpretation Options")]
    public InterpretationOptionData optionData = new InterpretationOptionData();

    [Header("Testing")]
    [SerializeField] private string testTopicID = "P01";


    public List<InterpretationOption> GetOptionsForTopic(string topicID)
    {
        List<InterpretationOption> availableOptions =
            new List<InterpretationOption>();

        if (planManager == null ||
            planManager.CurrentPlan == null ||
            optionData == null ||
            optionData.options == null)
        {
            Debug.LogWarning(
                "No están disponibles los datos de interpretación."
            );

            return availableOptions;
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

            return availableOptions;
        }

        foreach (InterpretationOption option in optionData.options)
        {
            if (option == null || !option.isActive)
                continue;

            // Filtro de competencia.
            if (!string.IsNullOrEmpty(option.topicID) &&
                option.topicID != topicID)
                continue;

            // Filtro del nivel requerido.
            if (option.requiredLevelStatuses != null &&
                option.requiredLevelStatuses.Count > 0 &&
                !option.requiredLevelStatuses.Contains(
                    topicResult.requiredLevelStatus))
                continue;

            // Filtro de percepción.
            if (option.perceptionAlignments != null &&
                option.perceptionAlignments.Count > 0 &&
                !option.perceptionAlignments.Contains(
                    topicResult.perceptionAlignment))
                continue;

            availableOptions.Add(option);
        }

        return availableOptions;
    }

    public bool SetOptionSelected(
        string topicID,
        string optionID,
        bool isSelected)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.interpretationData == null)
        {
            Debug.LogWarning("No hay datos de interpretación.");
            return false;
        }

        List<InterpretationOption> availableOptions = GetOptionsForTopic(topicID);

        InterpretationOption selectedOption = availableOptions.Find( option => option.optionID == optionID);

        if (selectedOption == null)
        {
            Debug.LogWarning($"La opción {optionID} no está disponible para {topicID}.");
            return false;
        }

        InterpretationData data = planManager.CurrentPlan.interpretationData;

        if (data.interpretations == null)
            data.interpretations = new List<TopicInterpretation>();

        TopicInterpretation interpretation = data.interpretations.Find( item => item.topicID == topicID);

        if (interpretation == null)
        {
            if (!isSelected)
                return true;

            interpretation = new TopicInterpretation
            {
                topicID = topicID
            };

            data.interpretations.Add(interpretation);
        }

        if (interpretation.selectedOptionIDs == null)
        {
            interpretation.selectedOptionIDs = new List<string>();
        }

        if (!isSelected)
        {
            interpretation.selectedOptionIDs.Remove(optionID);

            if (selectedOption.requiresCustomExplanation)
            {
                interpretation.customExplanation = "";
            }

            return true;
        }

        // Si seleccionamos una opción exclusiva,
        // eliminamos cualquier selección anterior.

        if (selectedOption.isExclusive)
        {
            interpretation.selectedOptionIDs.Clear();
            interpretation.selectedOptionIDs.Add(optionID);

            // Una opción exclusiva descarta las demás
            // explicaciones, incluyendo el texto personalizado.
            interpretation.customExplanation = "";

            return true;
        }

        // Si seleccionamos una opción normal,
        // eliminamos las opciones exclusivas previas.
        interpretation.selectedOptionIDs.RemoveAll(
            id => availableOptions.Exists(
                option => option.optionID == id &&
                          option.isExclusive)
        );

        // Evitar guardar opciones duplicadas.
        if (!interpretation.selectedOptionIDs.Contains(optionID))
        {
            interpretation.selectedOptionIDs.Add(optionID);
        }

        return true;
    }

    public bool SetCustomExplanation(
        string topicID,
        string explanation)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.interpretationData == null)
        {
            Debug.LogWarning( "No hay datos de interpretación disponibles.");

            return false;
        }

        InterpretationData data = planManager.CurrentPlan.interpretationData;

        if (data.interpretations == null)
            return false;

        TopicInterpretation interpretation = data.interpretations.Find( item => item.topicID == topicID );

        if (interpretation == null ||
            interpretation.selectedOptionIDs == null)
        {
            return false;
        }

        List<InterpretationOption> availableOptions = GetOptionsForTopic(topicID);

        // Comprobar que haya una opción seleccionada
        // que requiera explicación personalizada.
        bool requiresExplanation =
            availableOptions.Exists(option =>
                option.requiresCustomExplanation &&
                interpretation.selectedOptionIDs.Contains(option.optionID )
            );

        if (!requiresExplanation)
        {
            Debug.LogWarning( "No hay una opción seleccionada que requiera texto.");

            return false;
        }

        interpretation.customExplanation = explanation?.Trim() ?? "";

        return true;
    }

    public bool IsTopicInterpretationCompleted(string topicID)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.interpretationData == null)
        {
            return false;
        }

        InterpretationData data = planManager.CurrentPlan.interpretationData;

        if (data.interpretations == null)
            return false;

        TopicInterpretation interpretation = data.interpretations.Find( item => item.topicID == topicID);

        if (interpretation == null ||
            interpretation.selectedOptionIDs == null ||
            interpretation.selectedOptionIDs.Count == 0)
        {
            return false;
        }

        List<InterpretationOption> availableOptions = GetOptionsForTopic(topicID);

        bool hasExclusiveOption = false;
        bool requiresExplanation = false;

        HashSet<string> uniqueIDs = new HashSet<string>();

        foreach (string optionID in interpretation.selectedOptionIDs)
        {
            // No permitir opciones duplicadas.
            if (!uniqueIDs.Add(optionID))
                return false;

            InterpretationOption option = availableOptions.Find( item => item.optionID == optionID );

            // La opción debe seguir siendo válida.
            if (option == null)
                return false;

            if (option.isExclusive)
                hasExclusiveOption = true;

            if (option.requiresCustomExplanation)
                requiresExplanation = true;
        }

        // Una opción exclusiva no puede combinarse
        // con otras opciones.
        if (hasExclusiveOption &&
            interpretation.selectedOptionIDs.Count > 1)
        {
            return false;
        }

        // Verificar que exista una explicación escrita.
        if (requiresExplanation &&
            string.IsNullOrWhiteSpace(
                interpretation.customExplanation))
        {
            return false;
        }

        return true;
    }

    public TopicInterpretation GetTopicInterpretation(string topicID)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.interpretationData == null)
        {
            return null;
        }

        InterpretationData data = planManager.CurrentPlan.interpretationData;

        if (data.interpretations == null)
            return null;

        return data.interpretations.Find( item => item.topicID == topicID);
    }

    [ContextMenu("Test / Read Assessment Results")]
    private void TestReadAssessmentResults()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null)
        {
            Debug.LogWarning(
                "PersonalDevelopmentPlanManager no disponible."
            );
            return;
        }

        // Construir el resumen usando las evaluaciones existentes.
        planManager.BuildAssessmentSummary();

        List<DevelopmentTopicSummary> results =
            planManager.CurrentPlan.topicResults;

        Debug.Log(
            $"Competencias recibidas: {results.Count}"
        );

        foreach (DevelopmentTopicSummary topic in results)
        {
            Debug.Log(
                $"{topic.topicID} - {topic.topicTitle}\n" +
                $"Gerente: {topic.managerAverage:F1}\n" +
                $"Jefe: {topic.bossAverage:F1}\n" +
                $"Requerido: {topic.requiredLevel:F1}\n" +
                $"Percepción: {topic.perceptionAlignment}\n" +
                $"Estado: {topic.requiredLevelStatus}"
            );
        }
    }

    [ContextMenu("Test / Filter All Assessment Topics")]
    private void TestFilterAllAssessmentTopics()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null)
        {
            Debug.LogWarning("No existe un plan disponible.");
            return;
        }

        List<DevelopmentTopicSummary> results =
            planManager.CurrentPlan.topicResults;

        if (results == null || results.Count == 0)
        {
            Debug.LogWarning(
                "No hay resultados. Construye primero el resumen."
            );
            return;
        }

        foreach (DevelopmentTopicSummary topic in results)
        {
            List<InterpretationOption> availableOptions =
                GetOptionsForTopic(topic.topicID);

            Debug.Log(
                $"Competencia: {topic.topicID}\n" +
                $"Percepción: {topic.perceptionAlignment}\n" +
                $"Estado: {topic.requiredLevelStatus}\n" +
                $"Opciones disponibles: {availableOptions.Count}"
            );

            foreach (InterpretationOption option in availableOptions)
            {
                Debug.Log(
                    $"  {option.optionID}: {option.optionText}"
                );
            }
        }
    }
}