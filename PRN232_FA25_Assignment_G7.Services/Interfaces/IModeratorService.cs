using PRN232_FA25_Assignment_G7.Services.DTOs.Moderator;
using PRN232_FA25_Assignment_G7.Services.DTOs.Examiner;
using PRN232_FA25_Assignment_G7.Services.DTOs;

namespace PRN232_FA25_Assignment_G7.Services.Interfaces;

public interface IModeratorService
{
    Task<ModeratorDashboardResponse> GetDashboardAsync(Guid moderatorId);
    Task<PagedResult<ComplaintSummaryDto>> GetComplaintsAsync(ModeratorQuery filter);
    Task<ComplaintDetailDto> GetComplaintAsync(Guid id);
    Task DecideComplaintAsync(Guid id, DecisionDto dto, Guid moderatorId);
    Task<PagedResult<ZeroScoreSubmissionDto>> GetZeroScoreSubmissionsAsync(ModeratorQuery filter);
    Task<ZeroScoreDetailDto> GetZeroScoreDetailAsync(Guid id);
    Task VerifyZeroScoreAsync(Guid id, VerifyZeroScoreRequest request, Guid moderatorId);
}

public class ModeratorQuery
{
    public string? Status { get; set; } // Pending, Resolved, All
    public Guid? ExamId { get; set; }
    public string? StudentCode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}