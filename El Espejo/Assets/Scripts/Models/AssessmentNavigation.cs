
using UnityEngine;

public class AssessmentNavigation : MonoBehaviour
{
    [SerializeField] private AssessmentManager_script assessmentManager;

    private string currentRole;

    private int currentTopicIndex;
    private int currentQuestionIndex;

    public bool IsNavigationStarted { get; private set; }


    public int CurrentScore { get; private set; }
    public string CurrentEvidence { get; private set; } = "";

    public bool IsCurrentQuestionAnswered
    {
        get
        {
            if (assessmentManager == null || !IsNavigationStarted)
                return false;

            AssesmentResponses participant =
                currentRole == AssessmentManager_script.ManagerRole
                    ? assessmentManager.managerResponses
                    : assessmentManager.bossResponses;

            if (participant == null)
                return false;

            return participant.responses.Exists(
                response =>
                    response.questionID == CurrentQuestion.id &&
                    response.score >= 1 &&
                    response.score <= 5 &&
                    !string.IsNullOrWhiteSpace(response.evidence) &&
                    response.evidence.Trim().Length >= 20
            );
        }
    }


    public void SetCurrentAnswer(int score, string evidence)
    {
        CurrentScore = score;
        CurrentEvidence = evidence ?? "";
    }

    public bool SaveCurrentAnswer()
    {
        if (!IsNavigationStarted)
            return false;

        bool saved = assessmentManager.RegisterAnswer(
            currentRole,
            CurrentQuestion.id,
            CurrentScore,
            CurrentEvidence
        );

        if (!saved)
        {
            Debug.LogWarning(
                "No se guardó la respuesta. Verifica la puntuación " +
                "y que la evidencia tenga al menos 20 caracteres."
            );
        }

        return saved;
    }

    private void LoadCurrentAnswer()
    {
        CurrentScore = 0;
        CurrentEvidence = "";

        if (!IsNavigationStarted)
            return;

        AssesmentResponses participant =
            currentRole == AssessmentManager_script.ManagerRole
                ? assessmentManager.managerResponses
                : assessmentManager.bossResponses;

        if (participant == null)
            return;

        QuestionResponses existing = participant.responses.Find(
            response => response.questionID == CurrentQuestion.id
        );

        if (existing != null)
        {
            CurrentScore = existing.score;
            CurrentEvidence = existing.evidence ?? "";
        }
    }


    public AssesmentTopic CurrentTopic
    {
        get
        {
            if (!IsNavigationStarted)
                return null;

            return assessmentManager.assessmentData
                .topics[currentTopicIndex];
        }
    }

    public AssesmentQuestion CurrentQuestion
    {
        get
        {
            if (CurrentTopic == null)
                return null;

            return CurrentTopic.question[currentQuestionIndex];
        }
    }

    public int CurrentQuestionNumber
    {
        get
        {
            if (!IsNavigationStarted)
                return 0;

            int number = 0;

            for (int i = 0; i < currentTopicIndex; i++)
            {
                number += assessmentManager.assessmentData
                    .topics[i].question.Count;
            }

            return number + currentQuestionIndex + 1;
        }
    }

    public int TotalQuestions
    {
        get
        {
            return assessmentManager == null
                ? 0
                : assessmentManager.GetTotalQuestionCount();
        }
    }

    public bool StartNavigation(string role, string assessmentID)
    {
        if (assessmentManager == null)
        {
            Debug.LogError("No se ha asignado AssessmentManager.");
            return false;
        }

        if (role != AssessmentManager_script.ManagerRole &&
            role != AssessmentManager_script.BossRole)
        {
            Debug.LogError("El rol indicado no es válido.");
            return false;
        }

        if (!assessmentManager.IsInitialized)
        {
            assessmentManager.InitializeAssessment(assessmentID);
        }

        if (!assessmentManager.IsInitialized)
            return false;

        currentRole = role;
        currentTopicIndex = 0;
        currentQuestionIndex = 0;

        IsNavigationStarted = true;
        LoadCurrentAnswer();
        ShowCurrentQuestion();

        return true;
    }

    public bool NextQuestion()
    {
        if (!IsNavigationStarted)
            return false;

        if (!SaveCurrentAnswer())
            return false;

        // Avanzar dentro del tema actual.
        if (currentQuestionIndex + 1 <
            CurrentTopic.question.Count)
        {
            currentQuestionIndex++;
            LoadCurrentAnswer();
            ShowCurrentQuestion();
            return true;
        }

        // Si terminamos el tema, pasar al siguiente.
        if (currentTopicIndex + 1 <
            assessmentManager.assessmentData.topics.Count)
        {
            currentTopicIndex++;
            currentQuestionIndex = 0;

            LoadCurrentAnswer();
            ShowCurrentQuestion();
            return true;
        }

        Debug.Log("Se alcanzó la última pregunta.");
        return false;
    }

    public bool PreviousQuestion()
    {
        if (!IsNavigationStarted)
            return false;

        if (!SaveCurrentAnswer())
            return false;

        // Retroceder dentro del tema actual.
        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;
            LoadCurrentAnswer();
            ShowCurrentQuestion();
            return true;
        }

        // Si estamos al inicio del tema, volver al anterior.
        if (currentTopicIndex > 0)
        {
            currentTopicIndex--;

            currentQuestionIndex = CurrentTopic.question.Count - 1;

            LoadCurrentAnswer();
            ShowCurrentQuestion();
            return true;
        }

        Debug.Log("Ya estamos en la primera pregunta.");
        return false;
    }

    public void ShowCurrentQuestion()
    {
        if (!IsNavigationStarted)
            return;

        Debug.Log(
            "Rol: " + currentRole +
            " | Tema: " + CurrentTopic.title +
            " | Pregunta " + CurrentQuestionNumber +
            "/" + TotalQuestions +
            " | " + CurrentQuestion.text
        );
    }


    [ContextMenu("Test Navigation")]
    private void TestNavigation()
    {
        bool started = StartNavigation(
            AssessmentManager_script.ManagerRole,
            "ASSESSMENT_NAVIGATION_TEST"
        );

        if (!started)
        {
            Debug.LogError("No se pudo iniciar la navegación.");
            return;
        }

        bool testPassed = true;

        // Comprobar la primera pregunta.
        if (CurrentQuestionNumber != 1)
            testPassed = false;

        // Avanzar hasta la pregunta 6.
        for (int i = 0; i < 5; i++)
        {
            if (!NextQuestion())
            {
                testPassed = false;
                break;
            }
        }

        Debug.Log(
            "Prueba de avance - Pregunta actual: " +
            CurrentQuestionNumber
        );

        // La pregunta 6 debe pertenecer al segundo tema.
        if (CurrentQuestionNumber != 6 ||
            currentQuestionIndex != 0 ||
            currentTopicIndex != 1)
        {
            testPassed = false;
        }

        // Retroceder a la pregunta 5.
        if (!PreviousQuestion() ||
            CurrentQuestionNumber != 5)
        {
            testPassed = false;
        }

        // Avanzar hasta la última pregunta.
        while (NextQuestion())
        {
        }

        Debug.Log(
            "Última pregunta alcanzada: " +
            CurrentQuestionNumber + "/" + TotalQuestions
        );

        // No debe poder avanzar más allá de la pregunta 65.
        if (CurrentQuestionNumber != TotalQuestions ||
            NextQuestion())
        {
            testPassed = false;
        }

        if (testPassed)
        {
            Debug.Log(
                "PRUEBA COMPLETADA: navegación correcta."
            );
        }
        else
        {
            Debug.LogError(
                "PRUEBA FALLIDA: revisa la navegación."
            );
        }
    }


    [ContextMenu("Test Navigation And Answers")]
    private void TestNavigationAndAnswers()
    {
        bool started = StartNavigation(
            AssessmentManager_script.ManagerRole,
            "ASSESSMENT_NAVIGATION_ANSWERS_TEST"
        );

        if (!started)
        {
            Debug.LogError("No se pudo iniciar la navegación.");
            return;
        }

        bool testPassed = true;

        // PRUEBA 1: Responder la primera pregunta.
        SetCurrentAnswer(
            4,
            "Organicé las tareas del equipo y revisé los resultados."
        );

        bool savedFirst = SaveCurrentAnswer();

        Debug.Log("Prueba 1 - Primera respuesta guardada: " + savedFirst);

        // PRUEBA 2: Avanzar a la segunda pregunta.
        bool advanced = NextQuestion();

        Debug.Log("Prueba 2 - Avance permitido: " + advanced);

        // PRUEBA 3: Responder la segunda pregunta.
        SetCurrentAnswer(
            2,
            "Necesito mejorar la comunicación con mi equipo."
        );

        bool savedSecond = SaveCurrentAnswer();

        // PRUEBA 4: Regresar a la primera pregunta.
        bool wentBack = PreviousQuestion();

        bool firstAnswerRestored =
            CurrentQuestionNumber == 1 &&
            CurrentScore == 4 &&
            CurrentEvidence ==
                "Organicé las tareas del equipo y revisé los resultados.";

        Debug.Log(
            "Prueba 3 - Primera respuesta recuperada: " +
            firstAnswerRestored
        );

        // PRUEBA 5: Volver a la segunda pregunta.
        bool advancedAgain = NextQuestion();

        bool secondAnswerRestored =
            CurrentQuestionNumber == 2 &&
            CurrentScore == 2 &&
            CurrentEvidence ==
                "Necesito mejorar la comunicación con mi equipo.";

        Debug.Log(
            "Prueba 4 - Segunda respuesta recuperada: " +
            secondAnswerRestored
        );

        testPassed =
            savedFirst &&
            advanced &&
            savedSecond &&
            wentBack &&
            firstAnswerRestored &&
            advancedAgain &&
            secondAnswerRestored;

        if (testPassed)
        {
            Debug.Log(
                "PRUEBA COMPLETADA: navegación y respuestas correctas."
            );
        }
        else
        {
            Debug.LogError(
                "PRUEBA FALLIDA: revisa el guardado y la recuperación."
            );
        }
    }

}