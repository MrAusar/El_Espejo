using System;
using System.Collections.Generic;
using UnityEngine;

public class DevelopmentPlanBuilder
{
    public bool TryBuildAssessmentSummary(AssessmentManager_script assessmentManager,PersonalDevelopmentPlanData planData)
    {
        if (assessmentManager == null || planData == null)
        {
            Debug.LogWarning( "No se pudo construir el resumen: faltan referencias.");

            return false;
        }

        if (!assessmentManager.TryCompareManagerAndBoss(out List<AssessmentManager_script.TopicComparisonResult> comparisons))
        {
            Debug.LogWarning( "No se pudieron obtener los resultados de evaluación." );

            return false;
        }

        planData.topicResults.Clear();

        foreach (AssessmentManager_script.TopicComparisonResult comparison in comparisons)
        {
            DevelopmentTopicSummary summary = new DevelopmentTopicSummary
                {
                    topicID = comparison.topicID,
                    topicTitle = comparison.topicTitle,

                    managerAverage = comparison.managerAverage,
                    bossAverage = comparison.bossAverage,
                    requiredLevel = comparison.requiredLevel,

                    managerDifference = comparison.managerDifference,
                    bossDifference = comparison.bossDifference,
                    perceptionDifference = comparison.perceptionDifference,

                    perceptionAlignment = comparison.perceptionAlignment.ToString(),

                    requiredLevelStatus = comparison.requiredLevelStatus.ToString()
                };

            planData.topicResults.Add(summary);
        }

        planData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        Debug.Log( "Resumen de evaluación generado correctamente. Competencias: "  + planData.topicResults.Count );

        return true;
    }
}
