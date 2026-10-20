
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssessmentUI : MonoBehaviour
{
    [Header("Sistema de evaluación")]
    [SerializeField] private AssessmentNavigation navigation;

    [Header("Textos")]
    [SerializeField] private TMP_Text topicText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Progreso")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text completionPercentageText;

    [Header("Respuestas")]
    [SerializeField] private Toggle[] scoreToggles;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_InputField evidenceInput;

    [Header("Navegación")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;


    private void Start()
    {
        // Conectar los botones.
        previousButton.onClick.AddListener(OnPreviousClicked);
        nextButton.onClick.AddListener(OnNextClicked);

        // Conectar las cinco opciones de puntuación.
        for (int i = 0; i < scoreToggles.Length; i++)
        {
            int score = i + 1;
            scoreToggles[i].onValueChanged.AddListener(
                isOn => OnScoreChanged(score, isOn)
            );
        }

        // Iniciar una evaluación de prueba para el gerente.
        bool started = navigation.StartNavigation(AssessmentManager_script.ManagerRole, "ASSESSMENT_STORAGE_TEST_01");

        if (!started)
        {
            ShowFeedback("No se pudo iniciar la evaluación. Revisa las referencias.");
            return;
        }

        RefreshUI();
    }


    private void OnScoreChanged(int score, bool isOn)
    {
        // Solo reaccionar cuando una opción queda seleccionada.
        if (!isOn)
            return;

        navigation.SetCurrentAnswer(score, evidenceInput.text);

        scoreLabel.text = $"Calificación seleccionada: {score}";

        ShowFeedback("");
    }


    private void OnNextClicked()
    {
        SaveCurrentInput();

        bool isLastQuestion =
            navigation.CurrentQuestionNumber == navigation.TotalQuestions;

        if (isLastQuestion)
        {
            if (navigation.TrySubmitCurrentAssessment())
            {
                ShowFeedback("¡Evaluación completada y enviada correctamente!");

                nextButton.interactable = false;
                previousButton.interactable = false;
            }
            else
            {
                ShowFeedback("No se pudo finalizar. Verifica la puntuación, " + "la evidencia y que las 65 preguntas estén respondidas.");
            }

            return;
        }

        if (navigation.NextQuestion())
        {
            RefreshUI();
        }
        else
        {
            ShowFeedback( "No se pudo avanzar. Verifica la puntuación y la evidencia.");
        }
    }


    private void OnPreviousClicked()
    {
        SaveCurrentInput();

        if (navigation.PreviousQuestion())
        {
            RefreshUI();
        }
        else
        {
            ShowFeedback("No se puede retroceder más o la respuesta actual no es válida.");
        }
    }

    private void SaveCurrentInput()
    {
        int selectedScore = 0;

        for (int i = 0; i < scoreToggles.Length; i++)
        {
            if (scoreToggles[i].isOn)
            {
                selectedScore = i + 1;
                break;
            }
        }

        navigation.SetCurrentAnswer(selectedScore,evidenceInput.text);
    }


    private void RefreshUI()
    {
        AssesmentTopic topic = navigation.CurrentTopic;
        AssesmentQuestion question = navigation.CurrentQuestion;

        if (topic == null || question == null)
        {
            ShowFeedback("No se encontró la pregunta actual.");
            return;
        }

        topicText.text = topic.title;
        questionText.text = question.text;

        progressText.text = $"Pregunta {navigation.CurrentQuestionNumber} de {navigation.TotalQuestions}";

        progressBar.minValue = 0;
        progressBar.maxValue = navigation.TotalQuestions;
        progressBar.value = navigation.CurrentQuestionNumber;

        float percentage = navigation.GetCompletionPercentage();

        completionPercentageText.text = $"{percentage:0.#}% completado";

        // Primero restaurar la evidencia de la pregunta actual.
        evidenceInput.SetTextWithoutNotify(navigation.CurrentEvidence);

        // Restaurar las puntuaciones sin disparar OnScoreChanged.
        for (int i = 0; i < scoreToggles.Length; i++)
        {
            scoreToggles[i].SetIsOnWithoutNotify(
                navigation.CurrentScore == i + 1
            );
        }

        // Actualizar el texto de la puntuación.
        scoreLabel.text = navigation.CurrentScore > 0
            ? $"Calificación seleccionada: {navigation.CurrentScore}"
            : "Selecciona una calificación del 1 al 5";

        previousButton.interactable = navigation.CurrentQuestionNumber > 1;

        nextButton.interactable = true;

        bool isLastQuestion =
    navigation.CurrentQuestionNumber == navigation.TotalQuestions;

        nextButtonText.text = isLastQuestion
            ? "Finalizar evaluación"
            : "Siguiente";

        ShowFeedback("");
    }


    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }
}
