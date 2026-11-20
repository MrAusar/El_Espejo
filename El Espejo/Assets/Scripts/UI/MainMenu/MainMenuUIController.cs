using UnityEngine;
using UnityEngine.UI;
public class MainMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject assessmentPanel;
    [SerializeField] private GameObject comparisonPanel;

    [Header("Buttons")]
    [SerializeField] private Button startAssessmentButton;

    private void Awake()
    {
        if (startAssessmentButton != null)
        {
            startAssessmentButton.onClick.AddListener(OpenAssessment);
        }
    }

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);

        assessmentPanel.SetActive(false);
        comparisonPanel.SetActive(false);
    }

    private void OpenAssessment()
    {
        mainMenuPanel.SetActive(false);
        comparisonPanel.SetActive(false);

        assessmentPanel.SetActive(true);

        Debug.Log("Abriendo pantalla de evaluación.");
    }

    private void OnDestroy()
    {
        if (startAssessmentButton != null)
        {
            startAssessmentButton.onClick.RemoveListener(OpenAssessment);
        }
    }
}