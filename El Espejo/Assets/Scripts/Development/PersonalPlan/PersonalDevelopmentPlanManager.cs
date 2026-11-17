using UnityEngine;

public class PersonalDevelopmentPlanManager : MonoBehaviour
{
    public AssessmentManager_script assessmentManager;

    private DevelopmentPlanBuilder planBuilder;

    public PersonalDevelopmentPlanData CurrentPlan { get; private set; }

    private void Awake()
    {
        planBuilder = new DevelopmentPlanBuilder();
        CreateNewPlan();
    }

    public void CreateNewPlan()
    {
        CurrentPlan = new PersonalDevelopmentPlanData();

        CurrentPlan.assessmentID = System.Guid.NewGuid().ToString();

        CurrentPlan.reflectionData = new ReflectionData
        {
            assessmentID = CurrentPlan.assessmentID
        };

        CurrentPlan.interpretationData = new InterpretationData
        {
            assessmentID = CurrentPlan.assessmentID
        };

        CurrentPlan.prioritizationData = new PrioritizationData
        {
            assessmentID = CurrentPlan.assessmentID
        };

        CurrentPlan.behaviorData = new BehaviorData
        {
            assessmentID = CurrentPlan.assessmentID
        };

        CurrentPlan.challengeData = new DevelopmentChallengeData
        {
            assessmentID = CurrentPlan.assessmentID
        };

        Debug.Log("Nuevo Plan Personal de Desarrollo creado.");
    }

    public bool BuildAssessmentSummary()
    {
        if (assessmentManager == null)
        {
            Debug.LogWarning("Falta asignar AssessmentManager.");
            return false;
        }

        return planBuilder.TryBuildAssessmentSummary(assessmentManager, CurrentPlan);
    }


//PRUEBAS

    [ContextMenu("Test / Build Assessment Summary")]
    private void TestBuildAssessmentSummary()
    {
        bool success = BuildAssessmentSummary();

        if (!success)
        {
            Debug.LogWarning("No se pudo generar el resumen.");
            return;
        }

        Debug.Log(
            "Competencias guardadas en el plan: " + CurrentPlan.topicResults.Count);

        foreach (DevelopmentTopicSummary topic
                 in CurrentPlan.topicResults)
        {
            Debug.Log(
                topic.topicTitle
                + " | Gerente: " + topic.managerAverage
                + " | Jefe: " + topic.bossAverage
                + " | Requerido: " + topic.requiredLevel
            );
        }
    }

}
