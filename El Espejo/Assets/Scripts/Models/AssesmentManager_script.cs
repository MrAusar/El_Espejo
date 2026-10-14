using System.Collections.Generic;
using UnityEngine;

public class AssessmentManager_script : MonoBehaviour
{
    [SerializeField] private AssessmentCsvLoader csvLoader;

    public AssesmentData assessmentData { get; private set; }

    public AssesmentResponses managerResponses;
    public AssesmentResponses bossResponses;

    public const string ManagerRole = "Manager";
    public const string BossRole = "Boss";

    public bool IsInitialized { get; private set; }

    public void InitializeAssessment(string assessmentID)
    {
        IsInitialized = false;

        if (csvLoader == null)
        {
            Debug.LogError("No se ha asignado AssessmentCsvLoader.");
            return;
        }

        assessmentData = csvLoader.LoadAssessment();

        if (assessmentData == null)
            return;

        managerResponses = CreateResponses(
            assessmentID, ManagerRole);

        bossResponses = CreateResponses(
            assessmentID, BossRole);

        IsInitialized = true;

        Debug.Log("AssessmentManager inicializado correctamente.");
    }

    private AssesmentResponses CreateResponses(
        string assessmentID, string role)
    {
        return new AssesmentResponses
        {
            assesmentID = assessmentID,
            participantRole = role,
            responses = new List<QuestionResponses>(),
            isSubmitted = false
        };
    }

    public bool RegisterAnswer(
        string role,
        string questionID,
        int score,
        string evidence = "")
    {
        if (!IsInitialized)
            return false;

        AssesmentResponses participant = GetResponses(role);

        if (participant == null || participant.isSubmitted)
            return false;

        if (score < 1 || score > 5)
        {
            Debug.LogWarning("La puntuación debe estar entre 1 y 5.");
            return false;
        }

        if (!QuestionExists(questionID))
        {
            Debug.LogWarning("Pregunta desconocida: " + questionID);
            return false;
        }

        QuestionResponses answer = participant.responses.Find(
            r => r.questionID == questionID);

        if (answer == null)
        {
            answer = new QuestionResponses
            {
                questionID = questionID
            };

            participant.responses.Add(answer);
        }

        answer.score = score;
        answer.evidence = evidence;

        return true;
    }

    public int GetAnsweredCount(string role)
    {
        AssesmentResponses participant = GetResponses(role);

        return participant == null
            ? 0
            : participant.responses.Count;
    }

    public int GetTotalQuestionCount()
    {
        if (assessmentData == null)
            return 0;

        int total = 0;

        foreach (AssesmentTopic topic in assessmentData.topics)
            total += topic.question.Count;

        return total;
    }

    public bool TrySubmitAssessment(string role)
    {
        if (!IsInitialized)
            return false;

        AssesmentResponses participant = GetResponses(role);

        if (participant == null || participant.isSubmitted)
            return false;

        int total = GetTotalQuestionCount();

        if (total == 0 || participant.responses.Count != total)
        {
            Debug.LogWarning(
                "No se puede enviar: faltan respuestas.");

            return false;
        }

        participant.isSubmitted = true;

        Debug.Log("Evaluación enviada para: " + role);

        return true;
    }

    private AssesmentResponses GetResponses(string role)
    {
        if (role == ManagerRole)
            return managerResponses;

        if (role == BossRole)
            return bossResponses;

        return null;
    }

    private bool QuestionExists(string questionID)
    {
        if (assessmentData == null)
            return false;

        foreach (AssesmentTopic topic in assessmentData.topics)
        {
            foreach (AssesmentQuestion question in topic.question)
            {
                if (question.id == questionID)
                    return true;
            }
        }

        return false;
    }

    [ContextMenu("Test Assessment Loading")]
    private void TestAssessmentLoading()
    {
        InitializeAssessment("ASSESSMENT_TEST_001");
    }
}