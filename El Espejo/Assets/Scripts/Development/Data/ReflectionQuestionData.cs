using System;
using System.Collections.Generic;

[Serializable]
public class ReflectionQuestionData
{
    public List<ReflectionQuestion> questions = new List<ReflectionQuestion>();
}

[Serializable]
public class ReflectionQuestion
{
    public string questionID;
    public string topicID;

    public string reflectionStage;

    public string questionText;

    public List<string> requiredLevelStatuses = new List<string>();

    public List<string> perceptionAlignments = new List<string>();

    public bool isActive = true;
}
