using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrioritizationUIController : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField]
    private PrioritizationManager prioritizationManager;

    [Header("Panel")]
    [SerializeField]
    private GameObject prioritizationPanel;

    [Header("UI References")]
    [SerializeField]
    private Transform topicsContent;

    [SerializeField]
    private Toggle topicPrefab;

    [SerializeField]
    private TMP_Text selectedCountText;

    [SerializeField]
    private TMP_Text feedbackText;

    [SerializeField]
    private Button confirmButton;

    private readonly Dictionary<string, Toggle> topicToggles =new Dictionary<string, Toggle>();

    public event Action OnPrioritiesConfirmed;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmPriorities);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmPriorities);
        }
    }

    public bool OpenPrioritization()
    {
        if (prioritizationManager == null ||
            prioritizationPanel == null ||
            topicsContent == null ||
            topicPrefab == null)
        {
            Debug.LogWarning( "Faltan referencias de Priorización.");
            return false;
        }

        List<DevelopmentTopicSummary> topics = prioritizationManager.GetAvailableTopics();

        if (topics.Count == 0)
        {
            Debug.LogWarning( "No hay competencias disponibles." );
            return false;
        }

        ClearTopics();

        foreach (DevelopmentTopicSummary topic in topics)
        {
            Toggle newToggle =Instantiate(topicPrefab, topicsContent);

            newToggle.gameObject.SetActive(true);

            TMP_Text label = null;

            TMP_Text[] labels =newToggle.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in labels)
            {
                if (text.gameObject.name.Contains("Label"))
                {
                    label = text;
                    break;
                }
            }

            if (label != null)
            {
                label.text =
                    $"{topic.topicID} - {topic.topicTitle}\n" +
                    $"Gerente: {topic.managerAverage:F1} | " +
                    $"Jefe: {topic.bossAverage:F1} | " +
                    $"Requerido: {topic.requiredLevel:F1}";
            }
            else
            {
                Debug.LogWarning( "No se encontró Label en PriorityTopicItem." );
            }

            string topicID = topic.topicID;

            newToggle.SetIsOnWithoutNotify(false);

            topicToggles.Add(topicID, newToggle);

            newToggle.onValueChanged.AddListener(isOn => OnTopicSelected(topicID, isOn));
        }

        feedbackText.text = "";
        prioritizationPanel.SetActive(true);

        RefreshSelectionUI();

        return true;
    }

    private void OnTopicSelected(string topicID, bool isOn)
    {
        bool success = prioritizationManager.SetTopicPriority( topicID, isOn);

        if (!success)
        {
            feedbackText.text = "Solo puedes seleccionar hasta tres " + "competencias prioritarias.";
        }
        else
        {
            feedbackText.text = "";
        }

        RefreshSelectionUI();
    }

    private void RefreshSelectionUI()
    {
        List<string> selected = prioritizationManager.GetSelectedPriorities();

        foreach (KeyValuePair<string, Toggle> entry
            in topicToggles)
        {
            bool isSelected = selected.Contains(entry.Key);

            entry.Value.SetIsOnWithoutNotify(isSelected);

            // Permitir desmarcar las seleccionadas.
            // Bloquear las demás cuando llegamos a 3.
            entry.Value.interactable = isSelected || selected.Count < PrioritizationData.MaxPriorities;
        }

        selectedCountText.text = $"{selected.Count} de " + $"{PrioritizationData.MaxPriorities} seleccionadas";

        confirmButton.interactable = prioritizationManager.HasValidPriorities();
    }

    private void ConfirmPriorities()
    {
        if (!prioritizationManager.HasValidPriorities())
        {
            feedbackText.text = "Selecciona al menos una competencia válida.";
            return;
        }

        Debug.Log("Prioridades confirmadas: " + string.Join(", ", prioritizationManager.GetSelectedPriorities()));

        OnPrioritiesConfirmed?.Invoke();
    }

    private void ClearTopics()
    {
        foreach (Toggle toggle in topicToggles.Values)
        {
            if (toggle != null)
                Destroy(toggle.gameObject);
        }

        topicToggles.Clear();
    }
}
