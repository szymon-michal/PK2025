using Microsoft.EntityFrameworkCore;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Entities;
using ConnectionPlatform.Core.Interfaces;
using ConnectionPlatform.Infrastructure.Data;

namespace ConnectionPlatform.Infrastructure.Services;

public class CodeService : ICodeService
{
    private readonly ApplicationDbContext _context;

    public CodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RepositoryDto> CreateRepositoryAsync(int userId, CreateRepositoryDto createRepoDto)
    {
        var repository = new Repository
        {
            UserId = userId,
            Name = createRepoDto.Name,
            Description = createRepoDto.Description
        };

        _context.Repositories.Add(repository);
        await _context.SaveChangesAsync();

        var visibility = Enum.TryParse<RepositoryVisibility>(createRepoDto.Visibility, true, out var vis) 
            ? vis : RepositoryVisibility.Private;

        var metadata = new RepositoryMetadata
        {
            RepositoryId = repository.Id,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        };

        _context.RepositoryMetadata.Add(metadata);
        await _context.SaveChangesAsync();

        return new RepositoryDto
        {
            Id = repository.Id,
            UserId = repository.UserId,
            Name = repository.Name,
            Description = repository.Description,
            Visibility = visibility.ToString(),
            CreatedAt = metadata.CreatedAt,
            TotalFiles = 0,
            TotalFolders = 0,
            TotalSize = 0
        };
    }

    public async Task<RepositoryDto?> GetRepositoryAsync(int repositoryId)
    {
        var repository = await _context.Repositories
            .Include(r => r.Metadata)
            .FirstOrDefaultAsync(r => r.Id == repositoryId);

        if (repository == null) return null;

        return new RepositoryDto
        {
            Id = repository.Id,
            UserId = repository.UserId,
            Name = repository.Name,
            Description = repository.Description,
            Visibility = repository.Metadata?.Visibility.ToString() ?? "Private",
            CreatedAt = repository.Metadata?.CreatedAt ?? DateTime.UtcNow,
            LastModified = repository.Metadata?.LastModified,
            TotalFiles = repository.Metadata?.TotalFiles ?? 0,
            TotalFolders = repository.Metadata?.TotalFolders ?? 0,
            TotalSize = repository.Metadata?.TotalSize ?? 0
        };
    }

    public async Task<bool> DeleteRepositoryAsync(int repositoryId, int userId)
    {
        var repository = await _context.Repositories
            .FirstOrDefaultAsync(r => r.Id == repositoryId && r.UserId == userId);

        if (repository == null) return false;

        _context.Repositories.Remove(repository);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CodeTreeDto> CreateFolderAsync(int userId, CreateFolderDto createFolderDto)
    {
        var repository = await _context.Repositories
            .FirstOrDefaultAsync(r => r.Id == createFolderDto.RepositoryId && r.UserId == userId);

        if (repository == null)
            throw new UnauthorizedAccessException("Repository not found or access denied");

        var entry = new RepoEntry
        {
            Name = createFolderDto.Name,
            RepositoryId = createFolderDto.RepositoryId,
            ParentId = createFolderDto.ParentId
        };

        _context.RepoEntries.Add(entry);
        await _context.SaveChangesAsync();

        var entryData = new RepoEntryData
        {
            EntryId = entry.Id,
            IsDirectory = true,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _context.RepoEntryData.Add(entryData);
        await _context.SaveChangesAsync();

        // Update repository metadata
        await UpdateRepositoryMetadataAsync(createFolderDto.RepositoryId);

        return new CodeTreeDto
        {
            Id = entry.Id,
            Name = entry.Name,
            IsDirectory = true,
            Size = 0,
            LastModified = entryData.LastModified,
            Children = new List<CodeTreeDto>()
        };
    }

    public async Task<CodeTreeDto?> GetFolderAsync(int folderId)
    {
        var entry = await _context.RepoEntries
            .Include(e => e.Data)
            .Include(e => e.Children)
            .ThenInclude(c => c.Data)
            .FirstOrDefaultAsync(e => e.Id == folderId);

        if (entry?.Data?.IsDirectory != true) return null;

        return new CodeTreeDto
        {
            Id = entry.Id,
            Name = entry.Name,
            IsDirectory = true,
            Size = entry.Data.Size,
            LastModified = entry.Data.LastModified,
            Children = entry.Children.Select(c => new CodeTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                IsDirectory = c.Data?.IsDirectory ?? false,
                Extension = c.Data?.Extension?.ToString(),
                Size = c.Data?.Size ?? 0,
                LastModified = c.Data?.LastModified ?? DateTime.UtcNow
            }).ToList()
        };
    }

    public async Task<bool> DeleteFolderAsync(int folderId, int userId)
    {
        var entry = await _context.RepoEntries
            .Include(e => e.Repository)
            .FirstOrDefaultAsync(e => e.Id == folderId && e.Repository.UserId == userId);

        if (entry == null) return false;

        _context.RepoEntries.Remove(entry);
        await _context.SaveChangesAsync();

        // Update repository metadata
        await UpdateRepositoryMetadataAsync(entry.RepositoryId);
        return true;
    }

    public async Task<CodeTreeDto> CreateFileAsync(int userId, CreateFileDto createFileDto)
    {
        var repository = await _context.Repositories
            .FirstOrDefaultAsync(r => r.Id == createFileDto.RepositoryId && r.UserId == userId);

        if (repository == null)
            throw new UnauthorizedAccessException("Repository not found or access denied");

        var entry = new RepoEntry
        {
            Name = createFileDto.Name,
            RepositoryId = createFileDto.RepositoryId,
            ParentId = createFileDto.ParentId
        };

        _context.RepoEntries.Add(entry);
        await _context.SaveChangesAsync();

        var extension = Enum.TryParse<FileExtension>(createFileDto.Extension, true, out var ext) ? ext : FileExtension.txt;
        var lines = createFileDto.Content.Split('\n').Length;

        var entryData = new RepoEntryData
        {
            EntryId = entry.Id,
            IsDirectory = false,
            Extension = extension,
            Content = createFileDto.Content,
            NumberOfLines = lines,
            Size = System.Text.Encoding.UTF8.GetByteCount(createFileDto.Content),
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _context.RepoEntryData.Add(entryData);
        await _context.SaveChangesAsync();

        // Update repository metadata
        await UpdateRepositoryMetadataAsync(createFileDto.RepositoryId);

        return new CodeTreeDto
        {
            Id = entry.Id,
            Name = entry.Name,
            IsDirectory = false,
            Extension = extension.ToString(),
            Size = entryData.Size,
            LastModified = entryData.LastModified,
            Children = new List<CodeTreeDto>()
        };
    }

    public async Task<CodeTreeDto?> GetFileAsync(int fileId)
    {
        var entry = await _context.RepoEntries
            .Include(e => e.Data)
            .FirstOrDefaultAsync(e => e.Id == fileId);

        if (entry?.Data?.IsDirectory != false) return null;

        return new CodeTreeDto
        {
            Id = entry.Id,
            Name = entry.Name,
            IsDirectory = false,
            Extension = entry.Data.Extension?.ToString(),
            Size = entry.Data.Size,
            LastModified = entry.Data.LastModified,
            Children = new List<CodeTreeDto>()
        };
    }

    public async Task<bool> UpdateFileContentAsync(int fileId, int userId, string content)
    {
        var entry = await _context.RepoEntries
            .Include(e => e.Data)
            .Include(e => e.Repository)
            .FirstOrDefaultAsync(e => e.Id == fileId && e.Repository.UserId == userId);

        if (entry?.Data?.IsDirectory != false) return false;

        entry.Data.Content = content;
        entry.Data.NumberOfLines = content.Split('\n').Length;
        entry.Data.Size = System.Text.Encoding.UTF8.GetByteCount(content);
        entry.Data.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update repository metadata
        await UpdateRepositoryMetadataAsync(entry.RepositoryId);
        return true;
    }

    public async Task<bool> DeleteFileAsync(int fileId, int userId)
    {
        var entry = await _context.RepoEntries
            .Include(e => e.Repository)
            .FirstOrDefaultAsync(e => e.Id == fileId && e.Repository.UserId == userId);

        if (entry == null) return false;

        _context.RepoEntries.Remove(entry);
        await _context.SaveChangesAsync();

        // Update repository metadata
        await UpdateRepositoryMetadataAsync(entry.RepositoryId);
        return true;
    }

    public async Task<List<CodeTreeDto>> GetRepositoryTreeAsync(int repositoryId, int? folderId = null)
    {
        var entries = await _context.RepoEntries
            .Include(e => e.Data)
            .Where(e => e.RepositoryId == repositoryId && e.ParentId == folderId)
            .ToListAsync();

        var result = new List<CodeTreeDto>();
        
        foreach (var entry in entries)
        {
            var treeDto = new CodeTreeDto
            {
                Id = entry.Id,
                Name = entry.Name,
                IsDirectory = entry.Data?.IsDirectory ?? false,
                Extension = entry.Data?.Extension?.ToString(),
                Size = entry.Data?.Size ?? 0,
                LastModified = entry.Data?.LastModified ?? DateTime.UtcNow,
                Children = new List<CodeTreeDto>()
            };

            // If it's a directory, recursively get its children
            if (entry.Data?.IsDirectory == true)
            {
                treeDto.Children = await GetRepositoryTreeAsync(repositoryId, entry.Id);
            }

            result.Add(treeDto);
        }

        return result;
    }

    public async Task<CodeTreeDto> CreateSnippetAsync(int userId, CreateFileDto createSnippetDto)
    {
        // For simplicity, snippets are treated as files in a special "snippets" repository
        var snippetRepo = await GetOrCreateSnippetRepository(userId);
        
        createSnippetDto.RepositoryId = snippetRepo.Id;
        return await CreateFileAsync(userId, createSnippetDto);
    }

    public async Task<CodeTreeDto?> GetSnippetAsync(int snippetId)
    {
        return await GetFileAsync(snippetId);
    }

    private async Task<Repository> GetOrCreateSnippetRepository(int userId)
    {
        var snippetRepo = await _context.Repositories
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Name == "Code Snippets");

        if (snippetRepo == null)
        {
            snippetRepo = new Repository
            {
                UserId = userId,
                Name = "Code Snippets",
                Description = "Personal code snippets collection"
            };

            _context.Repositories.Add(snippetRepo);
            await _context.SaveChangesAsync();

            var metadata = new RepositoryMetadata
            {
                RepositoryId = snippetRepo.Id,
                Visibility = RepositoryVisibility.Private,
                CreatedAt = DateTime.UtcNow
            };

            _context.RepositoryMetadata.Add(metadata);
            await _context.SaveChangesAsync();
        }

        return snippetRepo;
    }

    private async Task UpdateRepositoryMetadataAsync(int repositoryId)
    {
        var metadata = await _context.RepositoryMetadata
            .FirstOrDefaultAsync(m => m.RepositoryId == repositoryId);

        if (metadata == null) return;

        var entries = await _context.RepoEntries
            .Include(e => e.Data)
            .Where(e => e.RepositoryId == repositoryId)
            .ToListAsync();

        metadata.TotalFiles = entries.Count(e => e.Data?.IsDirectory == false);
        metadata.TotalFolders = entries.Count(e => e.Data?.IsDirectory == true);
        metadata.TotalSize = entries.Where(e => e.Data?.IsDirectory == false).Sum(e => e.Data?.Size ?? 0);
        metadata.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}