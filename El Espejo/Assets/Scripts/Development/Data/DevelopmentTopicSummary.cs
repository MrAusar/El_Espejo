using System;

[Serializable]
public class DevelopmentTopicSummary
{
    public string topicID;
    public string topicTitle;

    public float managerAverage;
    public float bossAverage;
    public float requiredLevel;

    public float managerDifference;
    public float bossDifference;
    public float perceptionDifference;

    public string perceptionAlignment;
    public string requiredLevelStatus;
}
