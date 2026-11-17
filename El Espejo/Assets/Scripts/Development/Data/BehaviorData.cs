
using System;
using System.Collections.Generic;

[Serializable]
public class BehaviorData
{
    public string assessmentID;

    public List<BehaviorResponse> responses = new List<BehaviorResponse>();
}

[Serializable]
public class BehaviorResponse
{
    public string topicID;
    public string questionID;

    public string questionType;

    public int frequencyScore;

    public string selectedOptionID;

    public string writtenResponse;
}
