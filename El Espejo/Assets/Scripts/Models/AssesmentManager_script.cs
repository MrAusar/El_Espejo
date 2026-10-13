using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Properties;

public class AssesmentManager_script : MonoBehaviour
{
    public AssesmentData assesmentData;

    public AssesmentResponses managerResponses;
    public AssesmentResponses bossResponses;

    public const string ManagerRole = "Manager";
    public const string BossRole = "Boss";


    public void InitialAssesment(string assesmentID) // Prepara los registros de los participantes
    {
        if (assesmentData == null || assesmentData.topics == null)
        {
            Debug.LogError("Assesment data no asignado");
            return;
        }

        managerResponses = CreateResponses(assesmentID, ManagerRole);

        bossResponses = CreateResponses(assesmentID, BossRole);
    }

    private AssesmentResponses CreateResponses(string assesmentID, string role)
    {
        return new AssesmentResponses
        {
            assesmentID = assesmentID,
            participantRole = role,
            responses = new List<QuestionResponses>(),
            isSubmitted = false
        };
    }

    public bool RegisteredAnswer(string role, string questionID, int score, string evidence = "") // Guarda las respuestas de las preguntas
    {
        AssesmentResponses participant = GetResponses(role);

        if(participant == null || participant.isSubmitted)
        {
            return false;
        }
        if (score < 1 || score > 5)
        {
            return false;
        }
        if (!QuestionExist(questionID))
        {
            return false;
        }
        QuestionResponses answer = participant.responses.Find(r => r.questionID == questionID);

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

    public int GetAnsweredCount(string role) //Sabe cuantas preguntas tiene el participante
    {
        AssesmentResponses participant = GetResponses(role);

        return participant == null
            ? 0
            : participant.responses.Count;
    }

    public int GetTotalQuestionCount() //Sabe cuantas preguntas existen en los temas
    {
        int total = 0;

        if (assesmentData == null || assesmentData.topics == null)
        {
            return 0;
        }

        foreach (AssesmentTopic topic in assesmentData.topics)
        {
            if(topic.question != null)
            {
                total += topic.question.Count;
            }
        }

        return total;
    }

    public bool TrySubmitAssesment(string role) //Evita que se suba una encuesta si no ha respondido tadas las preguntas
    {
        AssesmentResponses participant = GetResponses(role);

        if (participant == null || participant.isSubmitted)
        {
            return false;
        }

        int total = GetTotalQuestionCount();

        if (total == 0 || participant.responses.Count != total)
        {
            return false;
        }

        participant.isSubmitted = true;

        return true;
    }

    private AssesmentResponses GetResponses(string role)//Da el registro correcto del participante
    {
        if (role == ManagerRole)
            return managerResponses;

        if (role == BossRole)
            return bossResponses;

        return null;
    }

    public bool QuestionExist(string questionID)
    {
        if (assesmentData == null || assesmentData.topics == null)
            return false;

        foreach (AssesmentTopic topic in assesmentData.topics)
        {
            if (topic.question == null)
                continue;

            foreach (AssesmentQuestion question in topic.question)
            {
                if (question.id == questionID)
                    return true;
            }
        }

        return false;
    }
}
