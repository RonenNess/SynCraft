namespace SynCraft.Models;

public enum StepCategory
{
    Development,
    Documentation,
    Security,
    Publishing,
    Tests
}

public enum StepState
{
    NotStarted,
    InProgress,
    InRisk,
    Done,
    Cancelled
}

public enum ProcessStatus
{
    Active,
    Completed
}

public enum MilestoneColor
{
    Green,
    Blue,
    Red
}
