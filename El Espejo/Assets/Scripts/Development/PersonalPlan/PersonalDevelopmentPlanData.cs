using System;
using System.Collections.Generic;

[Serializable]
public class PersonalDevelopmentPlanData
{
    public string assessmentID;

    public string creationDate;

    public ReflectionData reflectionData;

    public InterpretationData interpretationData;

    public PrioritizationData prioritizationData;

    public BehaviorData behaviorData;

    public DevelopmentChallengeData challengeData;

    public bool isGenerated;

    public List<DevelopmentTopicSummary> topicResults = new List<DevelopmentTopicSummary>();
}
