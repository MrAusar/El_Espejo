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

        TryLoadSavedPlan();
    }

    public void CreateNewPlan()
    {
        CurrentPlan = new PersonalDevelopmentPlanData();

        CurrentPlan.assessmentID = GetLocalAssessmentID();

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

    private const string LocalAssessmentIDKey = "EspejoGerente_CurrentAssessmentID";

    private string GetLocalAssessmentID()
    {
        string assessmentID = PlayerPrefs.GetString(LocalAssessmentIDKey, "");

        if (string.IsNullOrWhiteSpace(assessmentID))
        {
            assessmentID = System.Guid.NewGuid().ToString();

            PlayerPrefs.SetString(LocalAssessmentIDKey, assessmentID);

            PlayerPrefs.Save();
        }

        return assessmentID;
    }

    public bool TryLoadSavedPlan()
    {
        string assessmentID = GetLocalAssessmentID();

        PersonalDevelopmentPlanData savedPlan =
            PersonalDevelopmentPlanStorage.Load(assessmentID);

        if (savedPlan == null)
        {
            Debug.Log("No existe un PPD guardado todavía.");
            return false;
        }

        CurrentPlan = savedPlan;

        Debug.Log("PPD recuperado correctamente.");

        return true;
    }
    public bool SaveCurrentPlan()
    {
        if (CurrentPlan == null)
        {
            Debug.LogWarning(
                "No existe un PPD para guardar."
            );

            return false;
        }

        return PersonalDevelopmentPlanStorage.Save(
            CurrentPlan
        );
    }
}
