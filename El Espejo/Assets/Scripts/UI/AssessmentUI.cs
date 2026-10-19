
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

    [Header("Respuestas")]
    [SerializeField] private Toggle[] scoreToggles;
    [SerializeField] private TMP_InputField evidenceInput;

    [Header("Navegación")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

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
        bool started = navigation.StartNavigation(AssessmentManager_script.ManagerRole,"ASSESSMENT_LOCAL_TEST");

        if (!started)
        {
            ShowFeedback("No se pudo iniciar la evaluación. Revisa las referencias.");
            return;
        }

        RefreshUI();
    }

    private void OnScoreChanged(int score, bool isOn)
    {
        if (!isOn)
            return;

        navigation.SetCurrentAnswer(score, evidenceInput.text);
        ShowFeedback("");
    }

    private void OnNextClicked()
    {
        SaveCurrentInput();

        // Si estamos en la última pregunta, guardarla sin avanzar.
        if (navigation.CurrentQuestionNumber == navigation.TotalQuestions)
        {
            if (navigation.SaveCurrentAnswer())
            {
                ShowFeedback("Última respuesta guardada. Ya puedes finalizar la evaluación en el siguiente paso.");
            }
            else
            {
                ShowFeedback("Selecciona una calificación y escribe evidencia de al menos 20 caracteres.");
            }

            return;
        }

        if (navigation.NextQuestion())
        {
            RefreshUI();
        }
        else
        {
            ShowFeedback("No se pudo avanzar. Revisa la calificación y la evidencia.");
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

        // Recuperar la respuesta guardada de esta pregunta.
        for (int i = 0; i < scoreToggles.Length; i++)
        {
            scoreToggles[i].isOn =
                navigation.CurrentScore == i + 1;
        }

        evidenceInput.text = navigation.CurrentEvidence;

        previousButton.interactable = navigation.CurrentQuestionNumber > 1;

        nextButton.interactable = true;

        ShowFeedback("");
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }
}
