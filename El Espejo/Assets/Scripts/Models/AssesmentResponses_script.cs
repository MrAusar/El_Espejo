using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AssesmentResponses
{
    public string assesmentID; //Identifica la evaluación a la que pertenecen las respuestas
    public string participantRole; //Distingue al gerente y al jefe
    public List<QuestionResponses> responses = new List<QuestionResponses>(); //Guarda las respuestas a las preguntas
    public bool isSubmitted; //Checa si el participante ya envió su evaluación
}

[Serializable]
public class QuestionResponses
{
    public string questionID; //Identifica la pregunta respondida
    public int score; //Guarda la puntuacion del 1 al 5
    public string evidence; //Registra comentarios acerca de la pregunta
}
