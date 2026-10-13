using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AssesmentData //Contiene los 13 temas ha evaluar
{
    public List<AssesmentTopic> topics = new List<AssesmentTopic>();
}

[Serializable]
public class AssesmentTopic //Representa la competencia, el nivel requerido y las preguntas
{
    public string id;
    public string title;
    public float requiredLevel;
    public List<AssesmentQuestion> question = new List<AssesmentQuestion>();
}
[Serializable]
public class AssesmentQuestion //Representa las preguntas individualmente
{
    public string id;
    public string text;
}

