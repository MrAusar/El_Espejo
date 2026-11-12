
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AssessmentManager_script;

public class CompetencyChartRowUI : MonoBehaviour
{
    [Header("Competency")]
    [SerializeField] private TMP_Text competencyNameText;

    [Header("Manager Bar")]
    [SerializeField] private Slider managerSlider;
    [SerializeField] private TMP_Text managerValueText;

    [Header("Boss Bar")]
    [SerializeField] private Slider bossSlider;
    [SerializeField] private TMP_Text bossValueText;

    [Header("Required Level Bar")]
    [SerializeField] private Slider requiredSlider;
    [SerializeField] private TMP_Text requiredValueText;

    [Header("Bar Labels")]
    [SerializeField] private TMP_Text managerLabelText;
    [SerializeField] private TMP_Text bossLabelText;
    [SerializeField] private TMP_Text requiredLabelText;

    public void Setup(TopicComparisonResult result)
    {
        if (result == null)
        {
            Debug.LogError("CompetencyChartRowUI: resultado nulo.");
            return;
        }

        competencyNameText.text = result.topicTitle;

        managerLabelText.text = "Gerente";
        bossLabelText.text = "Jefe";
        requiredLabelText.text = "Requerido";

        UpdateBar(managerSlider, managerValueText, result.managerAverage);
        UpdateBar(bossSlider, bossValueText, result.bossAverage);
        UpdateBar(requiredSlider, requiredValueText, result.requiredLevel);
    }

    private void UpdateBar(
    Slider slider,
    TMP_Text valueText,
    float value)
    {
        if (slider == null || valueText == null)
        {
            Debug.LogError( "CompetencyChartRowUI: falta una referencia." );
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 5f;
        slider.value = Mathf.Clamp(value, 0f, 5f);

        valueText.text = value.ToString("F1");
    }
}
