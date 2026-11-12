
using TMPro;
using UnityEngine;
using static AssessmentManager_script;
using System;
using UnityEngine.UI;

public class CompetencyRowUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text competencyNameText;
    [SerializeField] private TMP_Text managerAverageText;
    [SerializeField] private TMP_Text bossAverageText;
    [SerializeField] private TMP_Text requiredLevelText;

    [SerializeField] private TMP_Text perceptionDifferenceText;

    [SerializeField] private TMP_Text perceptionAlignmentText;
    [SerializeField] private TMP_Text requiredLevelStatusText;

    [SerializeField] private TMP_Text interpretationText;

    [Header("Evidence Button")]
    [SerializeField] private Button viewEvidenceButton;

    public event Action<TopicComparisonResult> OnEvidenceRequested;

    public void Setup(TopicComparisonResult result)
    {
        if (result == null)
        {
            Debug.LogError("CompetencyRowUI: resultado nulo.");
            return;
        }

        competencyNameText.text = result.topicTitle;

        managerAverageText.text = "Autoevaluación: " + result.managerAverage.ToString("F1") + " / 5";

        bossAverageText.text = "Evaluación del jefe: " + result.bossAverage.ToString("F1") + " / 5";

        requiredLevelText.text = "Nivel requerido: " + result.requiredLevel.ToString("F1") + " / 5";

        float difference = result.perceptionDifference;

        string differenceLabel = difference > 0 ? "+" : "";

        perceptionDifferenceText.text = "Diferencia: " + differenceLabel + difference.ToString("F1");

        // Clasificación de percepción.

        switch (result.perceptionAlignment)
        {
            case AssessmentManager_script.PerceptionAlignment.BossRatesHigher:
                perceptionAlignmentText.text = "Tu jefe percibe un nivel de desempeño superior al que tú reconoces en ti mismo.";
                perceptionAlignmentText.color = new Color32(90, 155, 220, 255);
                break;

            case AssessmentManager_script.PerceptionAlignment.Aligned:
                perceptionAlignmentText.text = "Percepción: Tu percepción de tu desempeño coincide con la de tu jefe.";
                perceptionAlignmentText.color = new Color32(80, 180, 120, 255);
                break;

            case AssessmentManager_script.PerceptionAlignment.ManagerRatesHigher:
                perceptionAlignmentText.text = "Percepción: Consideras que tu desempeño es superior al nivel que percibe tu jefe.";
                perceptionAlignmentText.color = new Color32(220, 165, 70, 255);
                break;
        }


        // Clasificación del nivel requerido

        switch (result.requiredLevelStatus)
        {
            case AssessmentManager_script.RequiredLevelStatus.SharedOpportunity:
                requiredLevelStatusText.text = "Nivel requerido: Ambas evaluaciones indican que esta competencia aún no alcanza el nivel esperado.";
                requiredLevelStatusText.color = new Color32(210, 100, 100, 255);
                break;

            case AssessmentManager_script.RequiredLevelStatus.ManagerOpportunity:
                requiredLevelStatusText.text = "Nivel requerido: Tu autoevaluación indica que necesitas fortalecer esta competencia para alcanzar el nivel esperado.";
                requiredLevelStatusText.color = new Color32(220, 165, 70, 255);
                break;

            case AssessmentManager_script.RequiredLevelStatus.BossOpportunity:
                requiredLevelStatusText.text = "Nivel requerido: La evaluación de tu jefe indica que necesitas fortalecer esta competencia para alcanzar el nivel esperado.";
                requiredLevelStatusText.color = new Color32(220, 165, 70, 255);
                break;

            case AssessmentManager_script.RequiredLevelStatus.BothMeetRequiredLevel:
                requiredLevelStatusText.text = "Nivel requerido: Ambas evaluaciones indican que alcanzas el nivel esperado en esta competencia.";
                requiredLevelStatusText.color = new Color32(80, 180, 120, 255);
                break;
        }


        UpdateInterpretation(result);

        viewEvidenceButton.onClick.RemoveAllListeners();

        viewEvidenceButton.onClick.AddListener(() =>
        {
            OnEvidenceRequested?.Invoke(result);
        });
    }


    private void UpdateInterpretation(TopicComparisonResult result)
    {
        switch (result.requiredLevelStatus)
        {
            case AssessmentManager_script.RequiredLevelStatus.BothMeetRequiredLevel:

                interpretationText.text =
                    "Fortaleza compartida: ambas perspectivas " +
                    "reconocen que alcanzas el nivel esperado.";

                interpretationText.color =
                    new Color32(80, 180, 120, 255);

                break;

            case AssessmentManager_script.RequiredLevelStatus.SharedOpportunity:

                interpretationText.text =
                    "Oportunidad compartida: ambas perspectivas " +
                    "identifican que esta competencia necesita fortalecerse.";

                interpretationText.color =
                    new Color32(210, 100, 100, 255);

                break;

            case AssessmentManager_script.RequiredLevelStatus.ManagerOpportunity:

                interpretationText.text =
                    "Diferencia de percepción: tu autoevaluación " +
                    "indica que necesitas fortalecer esta competencia, " +
                    "aunque tu jefe considera que alcanzas el nivel esperado.";

                interpretationText.color =
                    new Color32(220, 165, 70, 255);

                break;

            case AssessmentManager_script.RequiredLevelStatus.BossOpportunity:

                interpretationText.text =
                    "Diferencia de percepción: consideras que alcanzas " +
                    "el nivel esperado, pero tu jefe identifica una " +
                    "oportunidad de desarrollo.";

                interpretationText.color =
                    new Color32(220, 165, 70, 255);

                break;
        }
    }

}