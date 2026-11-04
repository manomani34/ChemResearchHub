namespace ChemResearchHub.Domain.Enums.Meetings;

public enum MeetingTimerEventType
{
    MeetingStarted = 1,
    MeetingPaused = 2,
    MeetingResumed = 3,
    MeetingEnded = 4,

    SectionStarted = 10,
    SectionPaused = 11,
    SectionResumed = 12,
    SectionExtended = 13,
    SectionOvertimeStarted = 14,
    SectionCompleted = 15,
    SectionSkipped = 16,

    ParkingLotCreated = 20,
    DecisionCreated = 30,
    ActionItemCreated = 40
}