using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentNavigationUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private PersonalDevelopmentPlanManager planManager;

    [SerializeField]
    private ReflectionUIController reflectionUI;

    [Header("Panels")]
    [SerializeField]
    private GameObject navigationPanel;

    [SerializeField]
    private GameObject reflectionPanel;

    [SerializeField]
    private GameObject transitionPanel;

    [SerializeField]
    private GameObject interpretationPanel;

    [Header("UI")]
    [SerializeField]
    private TMP_Dropdown topicDropdown;

    [SerializeField]
    private TMP_Text feedbackText;

    [SerializeField]
    private Button refreshButton;

    [SerializeField]
    private Button openButton;

    private readonly List<string> topicIDs = new List<string>();

    [Header("Previous Experience")]
    [SerializeField]
    private GameObject comparisonPanel;

    [Header("Interpretation")]
    [SerializeField]
    private InterpretationUIController interpretationUI;
    private void Awake()
    {
        refreshButton.onClick.AddListener(RefreshTopics);
        openButton.onClick.AddListener(OpenSelectedTopic);
    }

    private void Start()
    {
        navigationPanel.SetActive(false);

        if (interpretationUI != null)
        {
            interpretationUI.OnInterpretationCompleted += HandleInterpretationCompleted;
        }
    }
    
    public void RefreshTopics()
    {
        topicIDs.Clear();
        topicDropdown.ClearOptions();

        // Utilizar las evaluaciones existentes.
        planManager.BuildAssessmentSummary();

        if (planManager.CurrentPlan == null ||
            planManager.CurrentPlan.topicResults == null)
        {
            feedbackText.text =
                "No se encontraron resultados disponibles.";

            openButton.interactable = false;
            return;
        }

        List<string> labels = new List<string>();

        foreach (DevelopmentTopicSummary topic
            in planManager.CurrentPlan.topicResults)
        {
            topicIDs.Add(topic.topicID);

            labels.Add(
                $"{topic.topicID} - {topic.topicTitle}"
            );
        }

        topicDropdown.AddOptions(labels);

        openButton.interactable = topicIDs.Count > 0;

        feedbackText.text = $"{topicIDs.Count} competencias disponibles.";
    }


    public void OpenSelectedTopic()
    {
        int index = topicDropdown.value;

        if (index < 0 || index >= topicIDs.Count)
            return;

        string topicID = topicIDs[index];

        bool opened = reflectionUI.OpenTopic(topicID);

        if (!opened)
        {
            feedbackText.text =
                $"No se pudo abrir {topicID}. " +
                "Revisa la consola.";
            return;
        }

        navigationPanel.SetActive(false);
    }


    public void ReturnToNavigation()
    {
        reflectionPanel.SetActive(false);
        transitionPanel.SetActive(false);
        interpretationPanel.SetActive(false);

        navigationPanel.SetActive(true);
    }


    private void OnDestroy()
    {
        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshTopics);

        if (openButton != null)
            openButton.onClick.RemoveListener(OpenSelectedTopic);

        if (interpretationUI != null)
        {
            interpretationUI.OnInterpretationCompleted -= HandleInterpretationCompleted;
        }
    }

    public void OpenDevelopmentNavigation()
    {
        if (planManager == null || planManager.assessmentManager == null)
        {
            Debug.LogWarning( "No hay un AssessmentManager configurado.");
            return;
        }

        // El desarrollo necesita las dos perspectivas.
        if (!planManager.assessmentManager
            .AreBothAssessmentsSubmitted())
        {
            Debug.LogWarning("Las dos evaluaciones deben estar enviadas.");

            return;
        }

        // Actualizar los resultados de las competencias.
        RefreshTopics();

        if (topicIDs.Count == 0)
        {
            Debug.LogWarning( "No hay competencias disponibles para desarrollo.");

            return;
        }

        if (comparisonPanel != null)
        {
            comparisonPanel.SetActive(false);
        }

        ReturnToNavigation();

        Debug.Log("Navegación de desarrollo abierta correctamente.");
    }

    private void HandleInterpretationCompleted(string topicID)
    {
        Debug.Log( $"Interpretación completada: {topicID}" );

        ReturnToNavigation();

        feedbackText.text = $"Interpretación de {topicID} completada correctamente.";
    }
}