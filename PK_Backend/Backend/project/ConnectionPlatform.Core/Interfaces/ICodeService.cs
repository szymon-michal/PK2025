namespace ConnectionPlatform.Core.Interfaces;

using ConnectionPlatform.Core.DTOs;

public interface ICodeService
{
    // Repository management
    Task<RepositoryDto> CreateRepositoryAsync(int userId, CreateRepositoryDto createRepoDto);
    Task<RepositoryDto?> GetRepositoryAsync(int repositoryId);
    Task<bool> DeleteRepositoryAsync(int repositoryId, int userId);
    
    // Folder management
    Task<CodeTreeDto> CreateFolderAsync(int userId, CreateFolderDto createFolderDto);
    Task<CodeTreeDto?> GetFolderAsync(int folderId);
    Task<bool> DeleteFolderAsync(int folderId, int userId);
    
    // File management
    Task<CodeTreeDto> CreateFileAsync(int userId, CreateFileDto createFileDto);
    Task<CodeTreeDto?> GetFileAsync(int fileId);
    Task<bool> UpdateFileContentAsync(int fileId, int userId, string content);
    Task<bool> DeleteFileAsync(int fileId, int userId);
    
    // Tree structure
    Task<List<CodeTreeDto>> GetRepositoryTreeAsync(int repositoryId, int? folderId = null);
    
    // Code snippets (simplified implementation)
    Task<CodeTreeDto> CreateSnippetAsync(int userId, CreateFileDto createSnippetDto);
    Task<CodeTreeDto?> GetSnippetAsync(int snippetId);
}