using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Text;
using ConnectionPlatform.Infrastructure.Data;
using ConnectionPlatform.Core.DTOs;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(int userId)
    {
        await using var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        // pending friend requests
        using var cmdPending = conn.CreateCommand();
        cmdPending.CommandText = "SELECT COUNT(*) FROM friend_requests WHERE receiver_id = @userId AND status = 'pending'";
        var p1 = cmdPending.CreateParameter(); p1.ParameterName = "@userId"; p1.Value = userId; cmdPending.Parameters.Add(p1);
        var pendingObj = await cmdPending.ExecuteScalarAsync();
        var pending = Convert.ToInt32(pendingObj);

        // unread messages
        using var cmdUnread = conn.CreateCommand();
        cmdUnread.CommandText = "SELECT COUNT(*) FROM messages WHERE receiver_id = @userId AND is_read = false";
        var p2 = cmdUnread.CreateParameter(); p2.ParameterName = "@userId"; p2.Value = userId; cmdUnread.Parameters.Add(p2);
        var unreadObj = await cmdUnread.ExecuteScalarAsync();
        var unread = Convert.ToInt32(unreadObj);

        // repositories list
        var repos = new List<RepoSummaryDto>();
        using (var cmdRepos = conn.CreateCommand())
        {
            cmdRepos.CommandText = "SELECT id, name, description FROM repositories WHERE user_id = @userId";
            var p3 = cmdRepos.CreateParameter(); p3.ParameterName = "@userId"; p3.Value = userId; cmdRepos.Parameters.Add(p3);
            using var rdr = await cmdRepos.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                repos.Add(new RepoSummaryDto
                {
                    Id = Convert.ToInt32(rdr["id"]),
                    Name = rdr["name"]?.ToString() ?? string.Empty,
                    Description = rdr["description"]?.ToString() ?? string.Empty
                });
            }
        }

        // gather friends
        var friendIds = new List<int>();
        using (var cmdFriends = conn.CreateCommand())
        {
            cmdFriends.CommandText = "SELECT unnest(friends) AS friend_id FROM user_friends WHERE user_id = @userId";
            var p4 = cmdFriends.CreateParameter(); p4.ParameterName = "@userId"; p4.Value = userId; cmdFriends.Parameters.Add(p4);
            using var rdr = await cmdFriends.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                if (!rdr.IsDBNull(0)) friendIds.Add(Convert.ToInt32(rdr.GetInt32(0)));
            }
        }

        // for each friend compute similarity from ComparisonResults (avg per friend), then average across friends
        double sum = 0; int countWithScore = 0;
        foreach (var f in friendIds)
        {
            using var cmdScore = conn.CreateCommand();
            cmdScore.CommandText = @"
                SELECT AVG(""SimilarityScore"") 
                FROM ""ComparisonResults"" 
                WHERE (""User1"" = @u AND ""User2"" = @f) OR (""User1"" = @f AND ""User2"" = @u)";
            var pu = cmdScore.CreateParameter(); pu.ParameterName = "@u"; pu.Value = userId; cmdScore.Parameters.Add(pu);
            var pf = cmdScore.CreateParameter(); pf.ParameterName = "@f"; pf.Value = f; cmdScore.Parameters.Add(pf);
            var res = await cmdScore.ExecuteScalarAsync();
            if (res != null && res != DBNull.Value)
            {
                var v = Convert.ToDouble(res);
                sum += v;
                countWithScore++;
            }
        }

        double avgCompatibility = countWithScore > 0 ? sum / countWithScore : 0.0;
        double percentCompatibility = avgCompatibility * 100.0;

        var dto = new DashboardDto
        {
            UserId = userId,
            PendingFriendRequests = pending,
            UnreadMessages = unread,
            Repositories = repos,
            AverageCompatibilityToFriends = Math.Round(avgCompatibility, 4),
            PercentCompatibilityToFriends = Math.Round(percentCompatibility, 2)
        };

        return Ok(dto);
    }

    [HttpPost("{userId}/photo")]
    public async Task<ActionResult> UploadProfilePhoto(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType?.ToLower())) return BadRequest("Invalid image type");

        byte[] data;
        await using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            data = ms.ToArray();
        }

        await using var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        // upsert into user_profile_photo
        using var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = "SELECT id FROM user_profile_photo WHERE user_id = @userId";
        var pId = cmdCheck.CreateParameter(); pId.ParameterName = "@userId"; pId.Value = userId; cmdCheck.Parameters.Add(pId);
        var exists = await cmdCheck.ExecuteScalarAsync();

        if (exists != null && exists != DBNull.Value)
        {
            using var cmdUpd = conn.CreateCommand();
            cmdUpd.CommandText = "UPDATE user_profile_photo SET file_name = @name, file_data = @data, type = @type, uploaded_at = now() WHERE user_id = @userId";
            var pn = cmdUpd.CreateParameter(); pn.ParameterName = "@name"; pn.Value = file.FileName; cmdUpd.Parameters.Add(pn);
            var pd = cmdUpd.CreateParameter(); pd.ParameterName = "@data"; pd.Value = data; cmdUpd.Parameters.Add(pd);
            var pt = cmdUpd.CreateParameter(); pt.ParameterName = "@type"; pt.Value = file.ContentType; cmdUpd.Parameters.Add(pt);
            var pu = cmdUpd.CreateParameter(); pu.ParameterName = "@userId"; pu.Value = userId; cmdUpd.Parameters.Add(pu);
            await cmdUpd.ExecuteNonQueryAsync();
        }
        else
        {
            using var cmdIns = conn.CreateCommand();
            cmdIns.CommandText = "INSERT INTO user_profile_photo (user_id, file_name, file_data, type) VALUES (@userId, @name, @data, @type)";
            var pu = cmdIns.CreateParameter(); pu.ParameterName = "@userId"; pu.Value = userId; cmdIns.Parameters.Add(pu);
            var pn = cmdIns.CreateParameter(); pn.ParameterName = "@name"; pn.Value = file.FileName; cmdIns.Parameters.Add(pn);
            var pd = cmdIns.CreateParameter(); pd.ParameterName = "@data"; pd.Value = data; cmdIns.Parameters.Add(pd);
            var pt = cmdIns.CreateParameter(); pt.ParameterName = "@type"; pt.Value = file.ContentType; cmdIns.Parameters.Add(pt);
            await cmdIns.ExecuteNonQueryAsync();
        }

        return Ok(new { message = "Uploaded" });
    }

    [HttpGet("{userId}/photo")]
    public async Task<ActionResult> DownloadProfilePhoto(int userId)
    {
        await using var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT file_name, file_data, type FROM user_profile_photo WHERE user_id = @userId";
        var p = cmd.CreateParameter(); p.ParameterName = "@userId"; p.Value = userId; cmd.Parameters.Add(p);
        using var rdr = await cmd.ExecuteReaderAsync();
        if (!await rdr.ReadAsync()) return NotFound();

        var name = rdr["file_name"]?.ToString() ?? "avatar";
        var type = rdr["type"]?.ToString() ?? "application/octet-stream";
        var data = (byte[])(rdr["file_data"]);

        return File(data, type, name);
    }
}
