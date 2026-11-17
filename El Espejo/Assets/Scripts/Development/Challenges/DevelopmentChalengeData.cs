
using System;
using System.Collections.Generic;

[Serializable]
public class DevelopmentChallengeData
{
    public string assessmentID;

    public List<DevelopmentChallenge> challenges = new List<DevelopmentChallenge>();
}

[Serializable]
public class DevelopmentChallenge
{
    public string challengeID;
    public string sourceTemplateID;
    public string topicID;

    public string title;
    public string objective;
    public string action;
    public string context;
    public string frequency;
    public string evidence;
    public string successIndicator;

    public int durationWeeks;

    public string status;
}
