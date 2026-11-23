
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BehaviorUIController : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private BehaviorManager behaviorManager;

    [Header("Panel")]
    [SerializeField] private GameObject behaviorPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text topicTitleText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Answer Controls")]
    [SerializeField] private GameObject frequencyGroup;
    [SerializeField] private TMP_Dropdown frequencyDropdown;

    [SerializeField] private GameObject selectionGroup;
    [SerializeField] private TMP_Dropdown optionDropdown;

    [SerializeField] private TMP_InputField additionalResponseInput;
    [SerializeField] private Button continueButton;

    private List<BehaviorQuestion> currentQuestions =
        new List<BehaviorQuestion>();

    private string currentTopicID;
    private int currentQuestionIndex;

    public event Action<string> OnBehaviorCompleted;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    public bool OpenTopic(string topicID)
    {
        if (behaviorManager == null ||
            behaviorPanel == null ||
            string.IsNullOrWhiteSpace(topicID))
        {
            Debug.LogWarning(
                "Faltan referencias para abrir Comportamiento."
            );
            return false;
        }

        List<BehaviorQuestion> questions =
            behaviorManager.GetQuestionsForTopic(topicID);

        if (questions.Count == 0)
        {
            Debug.LogWarning(
                $"No hay preguntas de comportamiento para {topicID}."
            );
            return false;
        }

        currentTopicID = topicID;
        currentQuestions = questions;
        currentQuestionIndex = 0;

        // Buscar la primera pregunta que todavía no tiene respuesta.
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            BehaviorQuestion question = currentQuestions[i];

            BehaviorResponse response =
                behaviorManager.GetSavedResponse(
                    topicID, question.questionID
                );

            if (response == null)
            {
                currentQuestionIndex = i;
                break;
            }

            currentQuestionIndex = i + 1;
        }

        // Todas tienen respuesta: permitir revisarlas.
        if (currentQuestionIndex >= currentQuestions.Count)
            currentQuestionIndex = 0;

        behaviorPanel.SetActive(true);
        ShowCurrentQuestion();

        return true;
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= currentQuestions.Count)
            return;

        BehaviorQuestion question = currentQuestions[currentQuestionIndex];

        Debug.Log(
        $"[BEHAVIOR UI] " +
        $"Question ID: {question.questionID} | " +
        $"Type: '{question.questionType}' | " +
        $"Options: {question.options?.Count ?? 0}");

        topicTitleText.text = $"Competencia {currentTopicID}";

        questionText.text = question.questionText;

        progressText.text =
            $"Pregunta {currentQuestionIndex + 1} " +
            $"de {currentQuestions.Count}";

        feedbackText.text = "";

        bool isFrequency = question.questionType == "Frequency";

        frequencyGroup.SetActive(isFrequency);
        selectionGroup.SetActive(!isFrequency);

        frequencyDropdown.SetValueWithoutNotify(0);

        optionDropdown.ClearOptions();

        if (!isFrequency)
        {
            List<string> labels = new List<string>
            {
                "Selecciona una respuesta..."
            };

            if (question.options != null)
            {
                foreach (BehaviorOption option in question.options)
                    labels.Add(option.optionText);
            }

            optionDropdown.AddOptions(labels);
            optionDropdown.SetValueWithoutNotify(0);
            optionDropdown.RefreshShownValue();
        }

        additionalResponseInput.SetTextWithoutNotify("");

        // Restaurar una respuesta previa, si existe.
        BehaviorResponse saved = behaviorManager.GetSavedResponse(currentTopicID, question.questionID );

        if (saved != null)
        {
            if (isFrequency)
            {
                frequencyDropdown.SetValueWithoutNotify(saved.frequencyScore );
                frequencyDropdown.RefreshShownValue();
            }
            else if (question.options != null)
            {
                int index = question.options.FindIndex(option => option.optionID == saved.selectedOptionID);

                optionDropdown.SetValueWithoutNotify(index >= 0 ? index + 1 : 0);

                optionDropdown.RefreshShownValue();
            }

            additionalResponseInput.SetTextWithoutNotify(saved.writtenResponse ?? "");
        }
    }

    private void OnContinueClicked()
    {
        if (currentQuestions.Count == 0)
            return;

        BehaviorQuestion question = currentQuestions[currentQuestionIndex];

        int frequencyScore = 0;
        string selectedOptionID = "";

        if (question.questionType == "Frequency")
        {
            frequencyScore = frequencyDropdown.value;

            if (frequencyScore < 1 || frequencyScore > 5)
            {
                feedbackText.text = "Selecciona una frecuencia del 1 al 5.";
                return;
            }
        }
        else
        {
            int selectedIndex = optionDropdown.value - 1;

            if (question.options == null ||
                selectedIndex < 0 ||
                selectedIndex >= question.options.Count)
            {
                feedbackText.text = "Selecciona una opción antes de continuar.";
                return;
            }

            selectedOptionID = question.options[selectedIndex].optionID;
        }

        bool saved = behaviorManager.SaveBehaviorResponse(
            currentTopicID,
            question.questionID,
            frequencyScore,
            selectedOptionID,
            additionalResponseInput.text
        );

        if (!saved)
        {
            feedbackText.text = "No se pudo guardar la respuesta.";
            return;
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < currentQuestions.Count)
        {
            ShowCurrentQuestion();
            return;
        }

        if (!behaviorManager.IsTopicBehaviorCompleted(
            currentTopicID))
        {
            feedbackText.text = "Todavía existen respuestas pendientes.";
            currentQuestionIndex = 0;
            return;
        }

        Debug.Log( $"Comportamiento completado: {currentTopicID}");

        OnBehaviorCompleted?.Invoke(currentTopicID);
    }
}
