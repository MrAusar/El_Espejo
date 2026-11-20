using System;
using System.Collections.Generic;

[Serializable]
public class InterpretationOptionData
{
    public List<InterpretationOption> options = new List<InterpretationOption>();
}

[Serializable]
public class InterpretationOption
{
    public string optionID;

    public string optionText;

    // Vacío significa que aplica a cualquier competencia.
    public string topicID;

    // Filtros según los resultados.
    public List<string> requiredLevelStatuses = new List<string>();

    public List<string> perceptionAlignments = new List<string>();

    // Comportamientos especiales.
    public bool isExclusive;

    public bool requiresCustomExplanation;

    public bool isActive = true;
}