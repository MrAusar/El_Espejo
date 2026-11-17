
using System;
using System.Collections.Generic;

[Serializable]
public class ChallengeTemplateData
{
    public List<ChallengeTemplate> templates = new List<ChallengeTemplate>();
}

[Serializable]
public class ChallengeTemplate
{
    public string templateID;
    public string topicID;

    public string title;
    public string objective;
    public string action;
    public string context;
    public string frequency;
    public string evidence;
    public string successIndicator;
    public int suggestedDurationWeeks;
}
