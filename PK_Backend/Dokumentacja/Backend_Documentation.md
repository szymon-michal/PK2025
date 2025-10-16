# Backend Documentation

## Overview

The Connection Platform Backend is a REST API built with C#/.NET 8.0 that facilitates user connections through advanced match-making algorithms based on skills, interests, and academic categories. The system includes real-time messaging, code repository management, and placeholder interfaces for future anti-plagiarism and communication features.

## Architecture

The backend follows Clean Architecture principles with three main layers:

### Core Layer (`ConnectionPlatform.Core`)
- **Entities**: Domain models representing business objects
- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Service contracts and abstractions

### Infrastructure Layer (`ConnectionPlatform.Infrastructure`)
- **Data**: Entity Framework Core DbContext and configurations
- **Services**: Concrete implementations of business logic

### API Layer (`ConnectionPlatform.API`)
- **Controllers**: REST API endpoints
- **Hubs**: SignalR real-time communication
- **Configuration**: Dependency injection and middleware setup

## Entity Summary

### User Management
- **User**: Core user entity with profile information
- **UserProfilePhoto**: User avatar/profile image storage
- **Skill**: Predefined skills with categories (Programming, Design, etc.)
- **UserSkill**: Many-to-many relationship with skill levels
- **Interest**: User interests for matching
- **Category**: Academic/professional categories
- **UserInterests**: Array-based storage of user's selected interests

### Social Features
- **FriendRequest**: Friend connection requests with status tracking
- **UserFriends**: Array-based storage of friend relationships
- **UserBlocked**: User blocking functionality

### Communication
- **Conversation**: Direct messaging between users
- **Message**: Individual messages with type support (text, images, files)

### Code Management
- **Repository**: Code project containers
- **RepositoryMetadata**: Repository statistics and settings
- **RepoEntry**: File/folder entries in hierarchical structure
- **RepoEntryData**: File content and metadata

## Core Endpoints

### User Management
```
POST   /api/users                    # Create new user
GET    /api/users/{id}               # Get user profile
PATCH  /api/users/{id}               # Update user profile
POST   /api/users/{id}/skills        # Add skill to user
DELETE /api/users/{id}/skills/{skillId} # Remove user skill
GET    /api/users/search             # Search users by attributes
```

### Match-Making
```
GET    /api/matches/suggestions      # Get compatible user suggestions
GET    /api/matches/compatibility    # Calculate compatibility score
```

### Conversations & Messages
```
POST   /api/conversations/direct/{otherUserId}  # Create conversation
GET    /api/conversations                      # List user conversations
POST   /api/conversations/{id}/messages        # Send message
GET    /api/conversations/{id}/messages        # Get conversation messages
PATCH  /api/messages/{id}                      # Edit message
DELETE /api/messages/{id}                      # Delete message
POST   /api/messages/{id}/read                 # Mark as read
```

### Code Repository Management
```
POST   /api/codes/repos              # Create repository
GET    /api/codes/repos/{id}         # Get repository details
DELETE /api/codes/repos/{id}         # Delete repository

POST   /api/codes/folders            # Create folder
GET    /api/codes/folders/{id}       # Get folder with children
DELETE /api/codes/folders/{id}       # Delete folder

POST   /api/codes/files              # Create file
GET    /api/codes/files/{id}         # Get file details
PUT    /api/codes/files/{id}/content # Update file content
DELETE /api/codes/files/{id}         # Delete file

POST   /api/codes/snippets           # Create code snippet
GET    /api/codes/snippets/{id}      # Get snippet

GET    /api/codes/{repoId}/tree      # Get repository tree structure
```

## Real-time Features (SignalR)

**Hub Endpoint**: `/chathub`

### Active Methods
- `JoinConversation(conversationId)` - Join conversation room
- `LeaveConversation(conversationId)` - Leave conversation room  
- `SendMessage(conversationId, senderId, message, messageType)` - Send real-time message
- `SendTyping(conversationId, senderId, isTyping)` - Typing indicators
- `SendReadReceipt(conversationId, messageId, userId)` - Read receipts

### Events Emitted
- `ReceiveMessage` - New message received
- `UserTyping` - Typing status updates
- `MessageRead` - Message read confirmations

## Match-Making Algorithm

The system uses a weighted compatibility scoring algorithm:

### Factors (weighted averages):
- **Skill Similarity (40%)**: Jaccard similarity of user skills
- **Interest Similarity (40%)**: Jaccard similarity of user interests  
- **Age Compatibility (20%)**: Inverse relationship to age difference

### Scoring Method:
```csharp
finalScore = (skillSimilarity * 0.4) + (interestSimilarity * 0.4) + (ageCompatibility * 0.2)
```

### Matching Process:
1. Exclude friends, blocked users, and self
2. Calculate compatibility scores for all potential matches
3. Return top matches sorted by score with pagination

## Placeholder Interfaces & Stubs

### Anti-Plagiarism System
```csharp
public interface IAntiPlagiarismService
{
    Task<CodeAnalysisResult> AnalyzeCodeAsync(int entryId, string content);
    Task<SimilarityResult> CompareCodesAsync(int code1Id, int code2Id);
    Task<List<SimilarityResult>> GetSimilarCodesAsync(int entryId, double threshold = 0.7);
}

public interface ICodeFingerprintService  
{
    string GenerateFingerprint(string content, string language);
    string NormalizeCode(string content, string language);
    List<string> TokenizeCode(string content, string language);
}
```

### Voice/Video Communication
```csharp
public interface ICommunicationService
{
    Task<VoiceCallResult> StartVoiceCallAsync(int callerId, int receiverId);
    Task<VideoCallResult> StartVideoCallAsync(int callerId, int receiverId);
    Task<bool> EndCallAsync(string callId);
    Task<CallStatus> GetCallStatusAsync(string callId);
}
```

### SignalR Communication Stubs
```csharp
// In ChatHub class - ready for implementation
public async Task StartVoiceCall(string conversationId, string callerId) { /* TODO */ }
public async Task StartVideoCall(string conversationId, string callerId) { /* TODO */ }
```

## Database Integration

- **ORM**: Entity Framework Core with PostgreSQL provider
- **Connection**: Configurable via environment variables or appsettings
- **Migrations**: Auto-created database schema on application startup
- **Configuration**: Fluent API configurations for complex relationships

## Error Handling

The API implements comprehensive error handling:
- **400 Bad Request**: Invalid parameters or business logic violations
- **401 Unauthorized**: Authentication required (placeholder)
- **403 Forbidden**: Access denied to resources
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Unhandled server errors

## Performance Considerations

- **Pagination**: Implemented for list endpoints (matches, messages)
- **Indexing**: Database indexes on frequently queried fields
- **Eager Loading**: Strategic use of `Include()` to minimize queries
- **Caching**: Ready for Redis implementation (interfaces provided)

## Security Notes

- **No Authentication**: Per requirements, no JWT or auth system implemented
- **Input Validation**: Basic validation on DTOs and parameters
- **SQL Injection Protection**: Entity Framework parameterized queries
- **XSS Protection**: Encoded responses for user content