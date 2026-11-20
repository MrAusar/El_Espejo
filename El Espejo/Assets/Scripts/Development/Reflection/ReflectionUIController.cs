using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ReflectionManager;

public class ReflectionUIController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ReflectionManager reflectionManager;

    [Header("UI References")]
    [SerializeField] private GameObject reflectionPanel;

    [SerializeField] private TMP_Text topicTitleText;
    [SerializeField] private TMP_Text stageTitleText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;

    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private Button continueButton;

    [Header("Testing")]
    [SerializeField] private string testTopicID = "P01";

    [Header("Transition UI")]
    [SerializeField] private GameObject transitionPanel;

    [SerializeField] private TMP_Text transitionTitleText;
    [SerializeField] private TMP_Text transitionDescriptionText;
    [SerializeField] private Button transitionContinueButton;

    private string activeTopicID;

    [Header("Next Experience")]
    [SerializeField]
    private InterpretationUIController interpretationUIController;


    [ContextMenu("Test / Show Reflection Question")]

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (transitionContinueButton != null)
        {
            transitionContinueButton.onClick.AddListener(OnTransitionContinueClicked);
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        if (transitionContinueButton != null)
        {
            transitionContinueButton.onClick.RemoveListener(OnTransitionContinueClicked);
        }
    }

    private void TestShowReflectionQuestion()
    {
        if (reflectionManager == null)
        {
            Debug.LogWarning("ReflectionManager no asignado.");
            return;
        }

        bool started = reflectionManager.TryResumeReflection(testTopicID);

        if (!started)
        {
            Debug.LogWarning("No se pudo iniciar la reflexión.");

            return;
        }

        activeTopicID = testTopicID;

        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        ReflectionQuestion question = reflectionManager.GetCurrentQuestion();
        Debug.Log(
            $"[REFLECTION UI] " +
            $"Topic: {question.topicID} | " +
            $"Stage: {question.reflectionStage} | " +
            $"Question ID: {question.questionID} | " +
            $"Texto: '{question.questionText}'");

        if (question == null)
        {
            Debug.LogWarning( "No hay una pregunta activa." );

            return;
        }

        reflectionPanel.SetActive(true);
        transitionPanel.SetActive(false);

        topicTitleText.text = question.topicID;

        stageTitleText.text = reflectionManager.GetStageDisplayName();

        questionText.text = question.questionText;

        if (reflectionManager.TryGetCurrentStageProgress( out int questionNumber,out int totalQuestions))
        {
            progressText.text =  $"Pregunta {questionNumber} de {totalQuestions}";
        }
        else
        {
            progressText.text = "";
        }

        feedbackText.text = "";

        answerInputField.text = "";

        continueButton.interactable = true;

        Debug.Log($"Mostrando pregunta: {question.questionID}");
    }

    private void ShowStageTransition()
    {
        reflectionPanel.SetActive(false);
        transitionPanel.SetActive(true);

        ReflectionStage stage = reflectionManager.currentStage;

        switch (stage)
        {
            case ReflectionStage.Awareness:

                transitionTitleText.text =
                    "Has completado Conciencia";

                transitionDescriptionText.text =
                    "Ahora pasaremos a Evidencia.\n\n" +
                    "Reflexionarás sobre situaciones concretas " +
                    "que pueden ayudarte a comprender mejor " +
                    "tus resultados.";

                transitionContinueButton.interactable = true;
                break;

            case ReflectionStage.Evidence:

                transitionTitleText.text =
                    "Has completado Evidencia";

                transitionDescriptionText.text =
                    "Ahora pasaremos a Proyección.\n\n" +
                    "Es momento de pensar qué te gustaría " +
                    "desarrollar y qué cambios quieres " +
                    "conseguir.";

                transitionContinueButton.interactable = true;
                break;


            case ReflectionStage.Projection:

                reflectionManager.DiagnoseTopicReflectionCompletion(activeTopicID);

                bool topicCompleted = reflectionManager.IsTopicReflectionCompleted(activeTopicID);

                if (topicCompleted)
                {
                    transitionTitleText.text = "¡Reflexión completada!";

                    transitionDescriptionText.text =
                        "Has completado las tres etapas de reflexión.\n\n" +
                        "Ahora explorarás las posibles explicaciones " +
                        "detrás de tus resultados y de las diferencias " +
                        "entre ambas perspectivas.";

                    transitionContinueButton.interactable = true;

                    TMP_Text buttonText = transitionContinueButton.GetComponentInChildren<TMP_Text>();

                    if (buttonText != null)
                    {
                        buttonText.text = "Continuar a Interpretación";
                    }
                }
                else
                {
                    transitionTitleText.text = "Reflexión incompleta";

                    transitionDescriptionText.text =
                        "Todavía existen preguntas pendientes " +
                        "o etapas sin configurar.";

                    transitionContinueButton.interactable = false;
                }

                break;
            }
        }

    private void OnTransitionContinueClicked()
    {
        if (reflectionManager == null)
            return;

        if (!reflectionManager.IsWaitingForStageTransition())
            return;

        // Caso especial: terminó la última etapa.
        if (reflectionManager.currentStage == ReflectionStage.Projection)
        {
            if (!reflectionManager.IsTopicReflectionCompleted(activeTopicID))
            {
                Debug.LogWarning("La reflexión todavía no está completa.");

                return;
            }

            if (interpretationUIController == null)
            {
                Debug.LogWarning("InterpretationUIController no asignado.");

                return;
            }

            // Abrir la interpretación de la misma competencia.
            interpretationUIController.ShowInterpretation(activeTopicID);

            reflectionPanel.SetActive(false);
            transitionPanel.SetActive(false);

            Debug.Log($"Reflexión finalizada. Abriendo interpretación: " + activeTopicID);

            return;
        }

        // Conciencia -> Evidencia -> Proyección.
        bool started = reflectionManager.ContinueToNextStage();

        if (!started)
        {
            transitionDescriptionText.text =
                "No se pudo iniciar la siguiente etapa. " +
                "Comprueba que existan preguntas pendientes.";

            return;
        }

        transitionPanel.SetActive(false);
        reflectionPanel.SetActive(true);

        ShowCurrentQuestion();
    }

    public bool OpenTopic(string topicID)
    {
        if (reflectionManager == null ||
            interpretationUIController == null ||
            string.IsNullOrWhiteSpace(topicID))
        {
            Debug.LogWarning("Faltan referencias para abrir la competencia.");
            return false;
        }

        activeTopicID = topicID;

        // Si la reflexión ya terminó, abrir interpretación.
        if (reflectionManager.IsTopicReflectionCompleted(topicID))
        {
            reflectionPanel.SetActive(false);
            transitionPanel.SetActive(false);

            interpretationUIController.ShowInterpretation(topicID);

            Debug.Log($"Abriendo interpretación de {topicID}");

            return true;
        }

        // Reanudar la primera etapa que tenga preguntas pendientes.
        bool started = reflectionManager.TryResumeReflection(topicID);

        if (!started)
        {
            Debug.LogWarning( $"No se pudo iniciar {topicID}. " + "Revisa las preguntas y etapas configuradas.");

            return false;
        }

        transitionPanel.SetActive(false);
        reflectionPanel.SetActive(true);

        ShowCurrentQuestion();

        Debug.Log($"Reflexión iniciada desde el juego: {topicID}");

        return true;
    }

    //BOTONES

    private void OnContinueClicked()
    {
        if (reflectionManager == null || answerInputField == null)
        {
            Debug.LogWarning("Faltan referencias para guardar la reflexión.");
            return;
        }

        ReflectionQuestion currentQuestion = reflectionManager.GetCurrentQuestion();

        if (currentQuestion == null)
        {
            Debug.LogWarning("No hay una pregunta activa.");
            return;
        }

        string answer = answerInputField.text.Trim();

        // Validar que la respuesta no esté vacía.
        if (string.IsNullOrWhiteSpace(answer))
        {
            feedbackText.text = "Por favor, escribe tu reflexión antes de continuar.";

            return;
        }

        // Guardar la respuesta.
        bool saved = reflectionManager.SaveCurrentAnswer(answer);

        if (!saved)
        {
            feedbackText.text = "No se pudo guardar tu respuesta.";

            return;
        }

        Debug.Log( $"Respuesta guardada: {currentQuestion.questionID}" );

        // Intentar avanzar a la siguiente pregunta.
        bool hasNextQuestion = reflectionManager.MoveToNextQuestion();

        if (hasNextQuestion)
        {
            ShowCurrentQuestion();
            return;
        }

        // Ya no quedan preguntas en la etapa.
        bool completed = reflectionManager.CompleteCurrentStage();

        if (completed)
        {
            Debug.Log(
                "Etapa completada: " +
                reflectionManager.GetStageDisplayName()
            );

            ShowStageTransition();
        }
        else
        {
            feedbackText.text = "No se pudo completar la etapa.";
        }
    }
}