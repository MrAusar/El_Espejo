using System;
using System.Collections.Generic;

[Serializable]
public class BehaviorQuestionData
{
    public List<BehaviorQuestion> questions =
        new List<BehaviorQuestion>();
}

[Serializable]
public class BehaviorQuestion
{
    public string questionID;

    // Vacío = disponible para todas las competencias.
    public string topicID;

    // Frequency, Context o Consistency.
    public string questionType;

    public string questionText;

    // Opciones para preguntas de selección.
    public List<BehaviorOption> options =
        new List<BehaviorOption>();

    // Filtrado con resultados reales.
    public List<string> requiredLevelStatuses =
        new List<string>();

    public List<string> perceptionAlignments =
        new List<string>();

    public bool isActive = true;
}

[Serializable]
public class BehaviorOption
{
    public string optionID;
    public string optionText;
}