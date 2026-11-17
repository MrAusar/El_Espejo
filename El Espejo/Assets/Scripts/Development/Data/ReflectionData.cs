
using System;
using System.Collections.Generic;

[Serializable]
public class ReflectionData
{
    public string assessmentID;
    public List<ReflectionResponse> responses = new List<ReflectionResponse>();
}

[Serializable]
public class ReflectionResponse
{
    public string topicID;
    public string questionID;
    public string reflectionStage;
    public string answer;
}