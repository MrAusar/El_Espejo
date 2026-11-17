
using System;
using System.Collections.Generic;

[Serializable]
public class InterpretationData
{
    public string assessmentID;
    public List<TopicInterpretation> interpretations = new List<TopicInterpretation>();
}

[Serializable]
public class TopicInterpretation
{
    public string topicID;
    public List<string> selectedOptionIDs = new List<string>();

    public string customExplanation;
}
