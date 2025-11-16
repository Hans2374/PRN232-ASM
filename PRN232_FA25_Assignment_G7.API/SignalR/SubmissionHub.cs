using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PRN232_FA25_Assignment_G7.API.SignalR;

[Authorize]
public class SubmissionHub : Hub
{
    public async Task SubmissionUploaded(Guid submissionId, string studentCode, string examName)
    {
        await Clients.All.SendAsync("SubmissionUploaded", new
        {
            SubmissionId = submissionId,
            StudentCode = studentCode,
            ExamName = examName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SubmissionGraded(Guid submissionId, string studentCode, decimal score)
    {
        await Clients.All.SendAsync("SubmissionGraded", new
        {
            SubmissionId = submissionId,
            StudentCode = studentCode,
            Score = score,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task ViolationFlagged(Guid submissionId, string studentCode, string violationType, bool isZeroScore)
    {
        await Clients.All.SendAsync("ViolationFlagged", new
        {
            SubmissionId = submissionId,
            StudentCode = studentCode,
            ViolationType = violationType,
            IsZeroScore = isZeroScore,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
