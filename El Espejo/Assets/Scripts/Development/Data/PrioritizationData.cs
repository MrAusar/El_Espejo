
using System;
using System.Collections.Generic;

[Serializable]
public class PrioritizationData
{
    public string assessmentID;

    public List<string> selectedTopicIDs = new List<string>();

    public const int MaxPriorities = 3;
}
