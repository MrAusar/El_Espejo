using System.Collections.Generic;
using UnityEngine;

public class PrioritizationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    public List<DevelopmentTopicSummary> GetAvailableTopics()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.topicResults == null)
        {
            return new List<DevelopmentTopicSummary>();
        }

        return new List<DevelopmentTopicSummary>(planManager.CurrentPlan.topicResults);
    }

    public List<string> GetSelectedPriorities()
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.prioritizationData == null)
        {
            return new List<string>();
        }

        List<string> selected = planManager.CurrentPlan.prioritizationData.selectedTopicIDs;

        return selected != null
            ? new List<string>(selected)
            : new List<string>();
    }

    public bool SetTopicPriority(string topicID, bool selected)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.prioritizationData == null)
        {
            return false;
        }

        PrioritizationData data =
            planManager.CurrentPlan.prioritizationData;

        if (data.selectedTopicIDs == null)
            data.selectedTopicIDs = new List<string>();

        // Solo permitir competencias que existan
        // en los resultados del assessment.
        bool topicExists = GetAvailableTopics().Exists( topic => topic.topicID == topicID );

        if (!topicExists)
            return false;

        bool alreadySelected =
            data.selectedTopicIDs.Contains(topicID);

        if (!selected)
        {
            data.selectedTopicIDs.Remove(topicID);
            return true;
        }

        // No duplicar competencias.
        if (alreadySelected)
            return true;

        // Máximo tres competencias.
        if (data.selectedTopicIDs.Count >= PrioritizationData.MaxPriorities)
        {
            Debug.LogWarning( "No puedes seleccionar más de 3 prioridades.");
            return false;
        }

        data.selectedTopicIDs.Add(topicID);

        return true;
    }

    public bool HasValidPriorities()
    {
        List<string> selected = GetSelectedPriorities();

        if (selected.Count < 1 || selected.Count > PrioritizationData.MaxPriorities)
            return false;

        List<DevelopmentTopicSummary> available = GetAvailableTopics();

        HashSet<string> uniqueIDs = new HashSet<string>();

        foreach (string topicID in selected)
        {
            if (!uniqueIDs.Add(topicID))
                return false;

            if (!available.Exists(
                topic => topic.topicID == topicID))
                return false;
        }

        return true;
    }
}
