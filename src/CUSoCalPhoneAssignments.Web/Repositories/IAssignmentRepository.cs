using CUSoCalPhoneAssignments.Web.Models;

namespace CUSoCalPhoneAssignments.Web.Repositories;

public interface IAssignmentRepository
{
    AssignmentData Load();
    void Save(AssignmentData data);
}
