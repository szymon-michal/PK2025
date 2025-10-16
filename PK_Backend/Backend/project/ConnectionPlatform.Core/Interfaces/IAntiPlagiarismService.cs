namespace ConnectionPlatform.Core.Interfaces;

/// <summary>
/// Interface for anti-plagiarism detection services.
/// This is a placeholder for future implementation.
/// </summary>
public interface IAntiPlagiarismService
{
    /// <summary>
    /// Analyzes code for potential plagiarism using fingerprinting techniques.
    /// </summary>
    Task<CodeAnalysisResult> AnalyzeCodeAsync(int entryId, string content);
    
    /// <summary>
    /// Compares two code snippets and returns similarity score.
    /// </summary>
    Task<SimilarityResult> CompareCodesAsync(int code1Id, int code2Id);
    
    /// <summary>
    /// Gets all high similarity matches for a given code entry.
    /// </summary>
    Task<List<SimilarityResult>> GetSimilarCodesAsync(int entryId, double threshold = 0.7);
}

/// <summary>
/// Interface for code fingerprinting service.
/// This is a placeholder for future implementation.
/// </summary>
public interface ICodeFingerprintService
{
    /// <summary>
    /// Generates a unique fingerprint for the given code content.
    /// </summary>
    string GenerateFingerprint(string content, string language);
    
    /// <summary>
    /// Normalizes code by removing comments, formatting, and variable names.
    /// </summary>
    string NormalizeCode(string content, string language);
    
    /// <summary>
    /// Tokenizes code into a sequence of meaningful tokens.
    /// </summary>
    List<string> TokenizeCode(string content, string language);
}

public class CodeAnalysisResult
{
    public int EntryId { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public List<string> Tokens { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class SimilarityResult
{
    public int Code1Id { get; set; }
    public int Code2Id { get; set; }
    public double SimilarityScore { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
}