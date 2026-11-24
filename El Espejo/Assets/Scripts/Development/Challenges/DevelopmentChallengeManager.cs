using System;
using System.Collections.Generic;
using UnityEngine;

public class DevelopmentChallengeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    [SerializeField]
    private PrioritizationManager prioritizationManager;

    [Header("Challenge Templates")]
    public ChallengeTemplateData templateData = new ChallengeTemplateData();


    public List<ChallengeTemplate> GetTemplatesForTopic( string topicID)
    {
        List<ChallengeTemplate> results = new List<ChallengeTemplate>();

        if (templateData == null ||
            templateData.templates == null)
            return results;

        foreach (ChallengeTemplate template
            in templateData.templates)
        {
            if (template != null &&
                template.topicID == topicID)
            {
                results.Add(template);
            }
        }

        return results;
    }


    public DevelopmentChallenge GetChallengeForTopic(string topicID)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.challengeData == null ||
            planManager.CurrentPlan.challengeData.challenges == null)
            return null;

        return planManager.CurrentPlan.challengeData.challenges.Find( challenge => challenge.topicID == topicID);
    }


    public bool GenerateChallengeProposal(string topicID)
    {
        if (planManager == null ||
            planManager.CurrentPlan == null ||
            planManager.CurrentPlan.challengeData == null ||
            prioritizationManager == null)
            return false;

        // Únicamente generar retos para prioridades.
        List<string> priorities = prioritizationManager.GetSelectedPriorities();

        if (!priorities.Contains(topicID))
        {
            Debug.LogWarning(  $"{topicID} no es una competencia prioritaria.");
            return false;
        }

        // Evitar sobrescribir un reto existente.
        if (GetChallengeForTopic(topicID) != null)
        {
            Debug.LogWarning( $"Ya existe un reto para {topicID}.");
            return false;
        }

        List<ChallengeTemplate> templates = GetTemplatesForTopic(topicID);

        if (templates.Count == 0)
        {
            Debug.LogWarning( $"No existen plantillas para {topicID}." );
            return false;
        }

        // Primera propuesta del banco.
        ChallengeTemplate selectedTemplate = templates[0];

        DevelopmentChallenge challenge = new DevelopmentChallenge
            {
                challengeID = Guid.NewGuid().ToString(),
                sourceTemplateID = selectedTemplate.templateID,
                topicID = topicID,

                title = selectedTemplate.title,
                objective = selectedTemplate.objective,
                action = selectedTemplate.action,
                context = selectedTemplate.context,
                frequency = selectedTemplate.frequency,
                evidence = selectedTemplate.evidence,
                successIndicator = selectedTemplate.successIndicator,

                durationWeeks = selectedTemplate.suggestedDurationWeeks,

                status = "Proposed"
            };

        DevelopmentChallengeData data = planManager.CurrentPlan.challengeData;

        if (data.challenges == null)
            data.challenges = new List<DevelopmentChallenge>();

        data.challenges.Add(challenge);

        Debug.Log($"Reto propuesto para {topicID}: {challenge.title}");

        return true;
    }
}
