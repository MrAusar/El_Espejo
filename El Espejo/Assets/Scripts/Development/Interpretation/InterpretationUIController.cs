using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterpretationUIController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private InterpretationManager interpretationManager;

    [Header("Panel")]
    [SerializeField]
    private GameObject interpretationPanel;

    [Header("Text References")]
    [SerializeField]
    private TMP_Text topicTitleText;

    [SerializeField]
    private TMP_Text interpretationQuestionText;

    [SerializeField]
    private TMP_Text feedbackText;

    [Header("Dynamic Options")]
    [SerializeField]
    private Transform optionsContent;

    [SerializeField]
    private Toggle optionPrefab;

    [Header("Additional UI")]
    [SerializeField]
    private TMP_InputField customExplanationInput;

    [SerializeField]
    private Button saveInterpretationButton;

    [Header("Testing")]
    [SerializeField]
    private string testTopicID = "P01";

    private string currentTopicID;

    private readonly List<Toggle> generatedToggles = new List<Toggle>();

    private readonly Dictionary<string, Toggle> optionToggles = new Dictionary<string, Toggle>();

    public event System.Action<string> OnInterpretationCompleted;

    [ContextMenu("Test / Show Interpretation")]
    private void TestShowInterpretation()
    {
        ShowInterpretation(testTopicID);
    }

    private void Awake()
    {
        if (customExplanationInput != null)
        {
            customExplanationInput.onValueChanged.AddListener(
                OnCustomExplanationChanged
            );
        }

        if (saveInterpretationButton != null)
        {
            saveInterpretationButton.onClick.AddListener(OnSaveInterpretationClicked);
        }
    }

    private void OnDestroy()
    {
        if (customExplanationInput != null)
        {
            customExplanationInput.onValueChanged.RemoveListener(OnCustomExplanationChanged );
        }

        if (saveInterpretationButton != null)
        {
            saveInterpretationButton.onClick.RemoveListener(OnSaveInterpretationClicked);
        }
    }
    public void ShowInterpretation(string topicID)
    {
        if (interpretationManager == null ||
            interpretationPanel == null ||
            optionsContent == null ||
            optionPrefab == null)
        {
            Debug.LogWarning( "Faltan referencias para mostrar la interpretación.");

            return;
        }

        List<InterpretationOption> options = interpretationManager.GetOptionsForTopic(topicID);

        ClearOptions();

        if (options.Count == 0)
        {
            Debug.LogWarning( $"No hay opciones disponibles para {topicID}.");

            return;
        }

        currentTopicID = topicID;

        interpretationPanel.SetActive(true);

        topicTitleText.text = $"Competencia {topicID}";

        interpretationQuestionText.text =
            "¿Qué situaciones podrían explicar " +
            "las diferencias entre tu percepción " +
            "y la de tu jefe?";

        feedbackText.text = "";
        saveInterpretationButton.interactable = true;

        customExplanationInput.gameObject.SetActive(false);

        foreach (InterpretationOption option in options)
        {
            Toggle newToggle = Instantiate(optionPrefab, optionsContent);

            newToggle.gameObject.SetActive(true);

            TMP_Text[] labels = newToggle.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text label in labels)
            {
                if (label.gameObject.name.Contains("Label"))
                {
                    label.text = option.optionText;
                    break;
                }
            }

            newToggle.SetIsOnWithoutNotify(false);

            generatedToggles.Add(newToggle);

            string optionID = option.optionID;

            optionToggles.Add(optionID, newToggle);

            newToggle.onValueChanged.AddListener(isOn => OnOptionChanged(optionID, isOn) );
        }

        RefreshToggleStates();
        RefreshCustomExplanationUI();

        Debug.Log(
            $"Interpretación abierta: {topicID} | " +
            $"Opciones generadas: {generatedToggles.Count}"
        );
    }


    private void ClearOptions()
    {
        foreach (Toggle toggle in generatedToggles)
        {
            if (toggle != null)
            {
                Destroy(toggle.gameObject);
            }
        }

        generatedToggles.Clear();
        optionToggles.Clear();
    }

    private void OnOptionChanged(string optionID, bool isOn)
    {
        if (interpretationManager == null ||
            string.IsNullOrEmpty(currentTopicID))
        {
            return;
        }

        bool saved = interpretationManager.SetOptionSelected(currentTopicID, optionID, isOn);

        if (!saved)
        {
            feedbackText.text = "No se pudo actualizar la selección.";
            RefreshToggleStates();
            return;
        }

        feedbackText.text = "";

        RefreshToggleStates();
        RefreshCustomExplanationUI();

        Debug.Log(
            $"Competencia: {currentTopicID} | " +
            $"Opción: {optionID} | Seleccionada: {isOn}"
        );
    }

    private void RefreshToggleStates()
    {
        TopicInterpretation interpretation = interpretationManager.GetTopicInterpretation(currentTopicID);

        foreach (KeyValuePair<string, Toggle> entry in optionToggles)
        {
            bool isSelected =
                interpretation != null &&
                interpretation.selectedOptionIDs != null &&
                interpretation.selectedOptionIDs.Contains(entry.Key);

            entry.Value.SetIsOnWithoutNotify(isSelected);
        }
    }

    private void OnCustomExplanationChanged(string text)
    {
        if (interpretationManager == null ||
            string.IsNullOrEmpty(currentTopicID))
        {
            return;
        }

        bool saved = interpretationManager.SetCustomExplanation(currentTopicID, text);

        if (!saved)
        {
            feedbackText.text = "No se pudo actualizar la explicación.";
            return;
        }

        feedbackText.text = "";
    }

    private void RefreshCustomExplanationUI()
    {
        TopicInterpretation interpretation = interpretationManager.GetTopicInterpretation( currentTopicID );

        List<InterpretationOption> options = interpretationManager.GetOptionsForTopic( currentTopicID );

        bool requiresExplanation = false;

        if (interpretation != null &&
            interpretation.selectedOptionIDs != null)
        {
            foreach (InterpretationOption option in options)
            {
                if (option.requiresCustomExplanation &&
                    interpretation.selectedOptionIDs.Contains(
                        option.optionID))
                {
                    requiresExplanation = true;
                    break;
                }
            }
        }

        customExplanationInput.gameObject.SetActive(
            requiresExplanation
        );

        string savedText =
            requiresExplanation && interpretation != null
                ? interpretation.customExplanation
                : "";

        customExplanationInput.SetTextWithoutNotify(
            savedText ?? ""
        );
    }

    private void OnSaveInterpretationClicked()
    {
        if (interpretationManager == null ||
            string.IsNullOrEmpty(currentTopicID) ||
            feedbackText == null)
        {
            Debug.LogWarning("No hay una interpretación activa.");
            return;
        }

        bool isCompleted = interpretationManager.IsTopicInterpretationCompleted(currentTopicID);

        if (!isCompleted)
        {
            feedbackText.text =
                "Selecciona al menos una explicación válida. " +
                "Si elegiste 'Otra explicación', escribe tu respuesta.";

            Debug.LogWarning( $"Interpretación incompleta: {currentTopicID}" );

            return;
        }

        feedbackText.text = "¡Interpretación guardada correctamente!";

        OnInterpretationCompleted?.Invoke(currentTopicID);

        Debug.Log($"Interpretación completada: {currentTopicID}");
    }
}