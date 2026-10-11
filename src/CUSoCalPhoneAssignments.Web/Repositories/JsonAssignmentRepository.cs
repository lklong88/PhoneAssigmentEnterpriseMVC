using System.Text.Json;
using CUSoCalPhoneAssignments.Web.Models;

namespace CUSoCalPhoneAssignments.Web.Repositories;

// Suitable for one application instance. Use a transactional DB for multiple instances.
public sealed class JsonAssignmentRepository : IAssignmentRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly string _filePath;
    private AssignmentData _current;

    public JsonAssignmentRepository(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var directory = configuration["DATA_DIRECTORY"];
        if (string.IsNullOrWhiteSpace(directory))
            directory = Path.Combine(environment.ContentRootPath, "Data");

        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "assignments.json");
        var seedPath = Path.Combine(environment.ContentRootPath, "Data", "seed.json");
        if (!File.Exists(_filePath)) File.Copy(seedPath, _filePath);
        _current = JsonSerializer.Deserialize<AssignmentData>(File.ReadAllText(_filePath), JsonOptions)
            ?? throw new InvalidDataException("Assignment data is empty.");
        ValidateInitialData(_current);
    }

    public AssignmentData Load() => _current.Copy();

    public void Save(AssignmentData data)
    {
        var tempPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, JsonOptions));
            File.Move(tempPath, _filePath, overwrite: true);
            _current = data.Copy(); // Commit memory only after durable file replacement.
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    private static void ValidateInitialData(AssignmentData data)
    {
        if (data.TeamMembers.Select(m => m.Id).Distinct().Count() != data.TeamMembers.Count ||
            data.PhoneNumbers.Select(n => n.Id).Distinct().Count() != data.PhoneNumbers.Count ||
            data.Assignments.Select(a => a.TeamMemberId).Distinct().Count() != data.Assignments.Count ||
            data.Assignments.Select(a => a.PhoneNumberId).Distinct().Count() != data.Assignments.Count ||
            data.Assignments.Any(a => !data.TeamMembers.Any(m => m.Id == a.TeamMemberId) ||
                                      !data.PhoneNumbers.Any(n => n.Id == a.PhoneNumberId)))
            throw new InvalidDataException("Assignment data violates uniqueness or referential integrity.");
    }
}
