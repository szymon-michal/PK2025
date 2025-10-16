using Microsoft.AspNetCore.Mvc;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodesController : ControllerBase
{
    private readonly ICodeService _codeService;

    public CodesController(ICodeService codeService)
    {
        _codeService = codeService;
    }

    // Repository endpoints
    [HttpPost("repos")]
    public async Task<ActionResult<RepositoryDto>> CreateRepository(
        [FromQuery] int userId, 
        [FromBody] CreateRepositoryDto createRepoDto)
    {
        var repository = await _codeService.CreateRepositoryAsync(userId, createRepoDto);
        return CreatedAtAction(nameof(GetRepository), new { id = repository.Id }, repository);
    }

    [HttpGet("repos/{id}")]
    public async Task<ActionResult<RepositoryDto>> GetRepository(int id)
    {
        var repository = await _codeService.GetRepositoryAsync(id);
        if (repository == null)
            return NotFound();

        return Ok(repository);
    }

    [HttpDelete("repos/{id}")]
    public async Task<ActionResult> DeleteRepository(int id, [FromQuery] int userId)
    {
        var success = await _codeService.DeleteRepositoryAsync(id, userId);
        if (!success)
            return NotFound();

        return Ok(new { message = "Repository deleted successfully" });
    }

    // Folder endpoints
    [HttpPost("folders")]
    public async Task<ActionResult<CodeTreeDto>> CreateFolder(
        [FromQuery] int userId, 
        [FromBody] CreateFolderDto createFolderDto)
    {
        try
        {
            var folder = await _codeService.CreateFolderAsync(userId, createFolderDto);
            return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, folder);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("folders/{id}")]
    public async Task<ActionResult<CodeTreeDto>> GetFolder(int id)
    {
        var folder = await _codeService.GetFolderAsync(id);
        if (folder == null)
            return NotFound();

        return Ok(folder);
    }

    [HttpDelete("folders/{id}")]
    public async Task<ActionResult> DeleteFolder(int id, [FromQuery] int userId)
    {
        var success = await _codeService.DeleteFolderAsync(id, userId);
        if (!success)
            return NotFound();

        return Ok(new { message = "Folder deleted successfully" });
    }

    // File endpoints
    [HttpPost("files")]
    public async Task<ActionResult<CodeTreeDto>> CreateFile(
        [FromQuery] int userId, 
        [FromBody] CreateFileDto createFileDto)
    {
        try
        {
            var file = await _codeService.CreateFileAsync(userId, createFileDto);
            return CreatedAtAction(nameof(GetFile), new { id = file.Id }, file);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("files/{id}")]
    public async Task<ActionResult<CodeTreeDto>> GetFile(int id)
    {
        var file = await _codeService.GetFileAsync(id);
        if (file == null)
            return NotFound();

        return Ok(file);
    }

    [HttpPut("files/{id}/content")]
    public async Task<ActionResult> UpdateFileContent(
        int id, 
        [FromQuery] int userId, 
        [FromBody] UpdateFileContentDto updateDto)
    {
        var success = await _codeService.UpdateFileContentAsync(id, userId, updateDto.Content);
        if (!success)
            return NotFound();

        return Ok(new { message = "File content updated successfully" });
    }

    [HttpDelete("files/{id}")]
    public async Task<ActionResult> DeleteFile(int id, [FromQuery] int userId)
    {
        var success = await _codeService.DeleteFileAsync(id, userId);
        if (!success)
            return NotFound();

        return Ok(new { message = "File deleted successfully" });
    }

    // Snippet endpoints
    [HttpPost("snippets")]
    public async Task<ActionResult<CodeTreeDto>> CreateSnippet(
        [FromQuery] int userId, 
        [FromBody] CreateFileDto createSnippetDto)
    {
        var snippet = await _codeService.CreateSnippetAsync(userId, createSnippetDto);
        return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippet);
    }

    [HttpGet("snippets/{id}")]
    public async Task<ActionResult<CodeTreeDto>> GetSnippet(int id)
    {
        var snippet = await _codeService.GetSnippetAsync(id);
        if (snippet == null)
            return NotFound();

        return Ok(snippet);
    }

    // Tree structure endpoint
    [HttpGet("{repoId}/tree")]
    public async Task<ActionResult<List<CodeTreeDto>>> GetRepositoryTree(
        int repoId, 
        [FromQuery] int? folderId = null)
    {
        var tree = await _codeService.GetRepositoryTreeAsync(repoId, folderId);
        return Ok(tree);
    }
}

public class UpdateFileContentDto
{
    public string Content { get; set; } = string.Empty;
}