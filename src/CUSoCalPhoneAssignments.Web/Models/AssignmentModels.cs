namespace CUSoCalPhoneAssignments.Web.Models;

public sealed record TeamMember(int Id, string FirstName, string LastName);
public sealed record PhoneNumber(int Id, string Number);
public sealed record Assignment(int TeamMemberId, int PhoneNumberId);
public sealed record AuditEntry(DateTimeOffset TimestampUtc, string Action, int TeamMemberId,
    int? PreviousPhoneNumberId, int? NewPhoneNumberId, string Actor);

public sealed class AssignmentData
{
    public List<TeamMember> TeamMembers { get; set; } = new();
    public List<PhoneNumber> PhoneNumbers { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
    public List<AuditEntry> AuditHistory { get; set; } = new();

    public AssignmentData Copy() => new()
    {
        TeamMembers = new(TeamMembers), PhoneNumbers = new(PhoneNumbers),
        Assignments = new(Assignments), AuditHistory = new(AuditHistory)
    };
}

public sealed record AssignmentRow(int MemberId, string FullName, int? CurrentPhoneId,
    string? CurrentPhone, IReadOnlyList<PhoneNumber> Choices);
public sealed record DashboardViewModel(IReadOnlyList<AssignmentRow> Rows, int TotalNumbers,
    int AssignedCount, IReadOnlyList<AuditEntry> RecentActivity);

public enum AssignmentError { None, InvalidMember, InvalidPhone, AlreadyAssigned, NoAssignment, StorageFailure }
public sealed record OperationResult(AssignmentError Error, string Message)
{
    public bool Success => Error == AssignmentError.None;
    public static OperationResult Ok(string message) => new(AssignmentError.None, message);
    public static OperationResult Fail(AssignmentError error, string message) => new(error, message);
}
