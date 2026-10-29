
using TMPro;
using UnityEngine;

public class ComparisonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AssessmentManager_script assessmentManager;
    [SerializeField] private GameObject comparisonPanel;
    [SerializeField] private GameObject assesmentPanel;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        if (comparisonPanel != null)
        {
            comparisonPanel.SetActive(false);
        }
    }

    public void ShowComparison()
    {
        if (assessmentManager == null || comparisonPanel == null || statusText == null)
        {
            Debug.LogError("ComparisonUI: faltan referencias en el Inspector.");
            return;
        }

        if (!assessmentManager.TryCompareManagerAndBoss(out var comparisons))
        {
            statusText.text = "La comparación estará disponible cuando " + "ambas evaluaciones estén finalizadas.";

            comparisonPanel.SetActive(true);
            assesmentPanel.SetActive(false);
            return;
        }

        statusText.text = "Comparación disponible. Competencias analizadas: " + comparisons.Count;

        comparisonPanel.SetActive(true);
        assesmentPanel.SetActive(false);
    }


    public void HideComparison()
    {
        if (comparisonPanel != null)
        {
            comparisonPanel.SetActive(false);
            assesmentPanel.SetActive(true);
        }
    }

}
