using CUSoCalPhoneAssignments.Web.Models;
using CUSoCalPhoneAssignments.Web.Repositories;
using CUSoCalPhoneAssignments.Web.Services;
using Xunit;

namespace CUSoCalPhoneAssignments.Tests;

public sealed class AssignmentServiceTests
{
    private static (AssignmentService Service, FakeRepository Repository) Setup()
    {
        var repository = new FakeRepository();
        return (new AssignmentService(repository, TimeProvider.System), repository);
    }

    [Fact]
    public void Dashboard_ShowsInitialAssignmentsAndAvailableCount()
    {
        var (service, _) = Setup();
        var dashboard = service.GetDashboard();
        Assert.Equal(3, dashboard.Rows.Count);
        Assert.Equal(2, dashboard.AssignedCount);
        Assert.Equal(4, dashboard.TotalNumbers);
        Assert.Equal("714-555-1001", dashboard.Rows.Single(r => r.MemberId == 1).CurrentPhone);
        Assert.Null(dashboard.Rows.Single(r => r.MemberId == 3).CurrentPhone);
    }

    [Fact]
    public void Assign_UnassignedEmployee_SucceedsAndRecordsAudit()
    {
        var (service, repository) = Setup();
        var result = service.Assign(3, 3, "Test reviewer");
        Assert.True(result.Success);
        Assert.Equal(3, repository.Load().Assignments.Single(a => a.TeamMemberId == 3).PhoneNumberId);
        Assert.Equal("Assigned", repository.Load().AuditHistory.Single().Action);
    }

    [Fact]
    public void ChangeAssignment_ReleasesPreviousPhone()
    {
        var (service, _) = Setup();
        Assert.True(service.Assign(1, 3, "Test reviewer").Success);
        var dashboard = service.GetDashboard();
        Assert.Equal(3, dashboard.Rows.Single(r => r.MemberId == 1).CurrentPhoneId);
        Assert.Contains(dashboard.Rows.Single(r => r.MemberId == 3).Choices, p => p.Id == 1);
    }

    [Fact]
    public void Assign_DuplicatePhone_IsRejectedWithoutMutation()
    {
        var (service, repository) = Setup();
        var result = service.Assign(3, 1, "Test reviewer");
        Assert.Equal(AssignmentError.AlreadyAssigned, result.Error);
        Assert.Equal(2, repository.Load().Assignments.Count);
        Assert.Empty(repository.Load().AuditHistory);
    }

    [Theory]
    [InlineData(999, 3, AssignmentError.InvalidMember)]
    [InlineData(3, 999, AssignmentError.InvalidPhone)]
    public void Assign_InvalidIdentifiers_ReturnsValidationError(int member, int phone, AssignmentError expected)
    {
        var (service, repository) = Setup();
        Assert.Equal(expected, service.Assign(member, phone, "Test reviewer").Error);
        Assert.Empty(repository.Load().AuditHistory);
    }

    [Fact]
    public void Remove_ExistingAssignment_ReleasesPhoneAndRecordsAudit()
    {
        var (service, repository) = Setup();
        var result = service.Remove(2, "Test reviewer");
        Assert.True(result.Success);
        Assert.DoesNotContain(repository.Load().Assignments, a => a.TeamMemberId == 2);
        Assert.Contains(service.GetDashboard().Rows.Single(r => r.MemberId == 3).Choices, p => p.Id == 2);
        Assert.Equal("Removed", repository.Load().AuditHistory.Single().Action);
    }

    [Fact]
    public void Remove_UnassignedMember_ReturnsValidationError()
    {
        var (service, repository) = Setup();
        Assert.Equal(AssignmentError.NoAssignment, service.Remove(3, "Test reviewer").Error);
        Assert.Equal(2, repository.Load().Assignments.Count);
    }

    [Fact]
    public void Remove_UnknownMember_ReturnsValidationError()
    {
        var (service, _) = Setup();
        Assert.Equal(AssignmentError.InvalidMember, service.Remove(999, "Test reviewer").Error);
    }

    [Fact]
    public void Assign_SamePhoneAgain_IsIdempotentAndDoesNotDuplicateAudit()
    {
        var (service, repository) = Setup();
        Assert.True(service.Assign(1, 1, "Test reviewer").Success);
        Assert.Equal(2, repository.Load().Assignments.Count);
        Assert.Empty(repository.Load().AuditHistory);
    }

    [Fact]
    public void SaveFailure_DoesNotCommitMutation()
    {
        var (service, repository) = Setup();
        repository.FailOnSave = true;
        Assert.Throws<IOException>(() => service.Assign(3, 3, "Test reviewer"));
        Assert.Equal(2, repository.Load().Assignments.Count);
        Assert.Empty(repository.Load().AuditHistory);
    }

    private sealed class FakeRepository : IAssignmentRepository
    {
        private AssignmentData _data = new()
        {
            TeamMembers = new() { new(1, "John", "Smith"), new(2, "Jane", "Doe"), new(3, "Michael", "Johnson") },
            PhoneNumbers = new() { new(1, "714-555-1001"), new(2, "714-555-1002"), new(3, "714-555-1003"), new(4, "714-555-1004") },
            Assignments = new() { new(1, 1), new(2, 2) }
        };
        public bool FailOnSave { get; set; }
        public AssignmentData Load() => _data.Copy();
        public void Save(AssignmentData data)
        {
            if (FailOnSave) throw new IOException("Simulated disk failure");
            _data = data.Copy();
        }
    }
}
