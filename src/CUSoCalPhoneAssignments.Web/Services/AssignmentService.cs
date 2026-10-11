using CUSoCalPhoneAssignments.Web.Models;
using CUSoCalPhoneAssignments.Web.Repositories;

namespace CUSoCalPhoneAssignments.Web.Services;

public sealed class AssignmentService
{
    private readonly object _gate = new();
    private readonly IAssignmentRepository _repository;
    private readonly TimeProvider _clock;

    public AssignmentService(IAssignmentRepository repository, TimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public DashboardViewModel GetDashboard()
    {
        lock (_gate)
        {
            var data = _repository.Load();
            var rows = data.TeamMembers.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).Select(member =>
            {
                var phoneId = data.Assignments.FirstOrDefault(a => a.TeamMemberId == member.Id)?.PhoneNumberId;
                var phone = data.PhoneNumbers.FirstOrDefault(p => p.Id == phoneId)?.Number;
                var choices = data.PhoneNumbers.Where(p => p.Id == phoneId ||
                    !data.Assignments.Any(a => a.PhoneNumberId == p.Id))
                    .OrderBy(p => p.Number).ToList();
                return new AssignmentRow(member.Id, $"{member.FirstName} {member.LastName}", phoneId, phone, choices);
            }).ToList();
            return new DashboardViewModel(rows, data.PhoneNumbers.Count, data.Assignments.Count,
                data.AuditHistory.OrderByDescending(a => a.TimestampUtc).Take(8).ToList());
        }
    }

    public OperationResult Assign(int memberId, int phoneId, string actor)
    {
        lock (_gate)
        {
            var data = _repository.Load();
            if (!data.TeamMembers.Any(m => m.Id == memberId))
                return OperationResult.Fail(AssignmentError.InvalidMember, "Team member not found.");
            if (!data.PhoneNumbers.Any(p => p.Id == phoneId))
                return OperationResult.Fail(AssignmentError.InvalidPhone, "Select a valid phone number.");
            if (data.Assignments.Any(a => a.PhoneNumberId == phoneId && a.TeamMemberId != memberId))
                return OperationResult.Fail(AssignmentError.AlreadyAssigned, "That phone number is already assigned to another team member.");

            var previous = data.Assignments.FirstOrDefault(a => a.TeamMemberId == memberId)?.PhoneNumberId;
            if (previous == phoneId) return OperationResult.Ok("Assignment is already up to date.");
            data.Assignments.RemoveAll(a => a.TeamMemberId == memberId);
            data.Assignments.Add(new Assignment(memberId, phoneId));
            data.AuditHistory.Add(new AuditEntry(_clock.GetUtcNow(), previous is null ? "Assigned" : "Changed",
                memberId, previous, phoneId, actor));
            _repository.Save(data);
            return OperationResult.Ok("Phone-number assignment saved.");
        }
    }

    public OperationResult Remove(int memberId, string actor)
    {
        lock (_gate)
        {
            var data = _repository.Load();
            if (!data.TeamMembers.Any(m => m.Id == memberId))
                return OperationResult.Fail(AssignmentError.InvalidMember, "Team member not found.");
            var current = data.Assignments.FirstOrDefault(a => a.TeamMemberId == memberId);
            if (current is null)
                return OperationResult.Fail(AssignmentError.NoAssignment, "This team member has no assignment to remove.");
            data.Assignments.RemoveAll(a => a.TeamMemberId == memberId);
            data.AuditHistory.Add(new AuditEntry(_clock.GetUtcNow(), "Removed", memberId,
                current.PhoneNumberId, null, actor));
            _repository.Save(data);
            return OperationResult.Ok("Phone-number assignment removed.");
        }
    }
}
