# Connection Platform Backend

A REST backend in C#/.NET for a browser-based platform that helps users form new connections via match-making based on skills, interests, and academic categories.

## Features

### Core Functionality
- **User Profiles & Categories**: Complete user management with skills, interests, and categories
- **Match-Making**: Advanced algorithm comparing skills, interests, and categories for compatibility scoring
- **Real-time Text Chat**: SignalR-powered messaging with conversation management
- **Code Repository Management**: Nested folder structure for code organization
- **Anti-plagiarism Stubs**: Clean interfaces ready for future implementation
- **Voice/Video Stubs**: Method signatures prepared for communication features

### Technical Stack
- **Framework**: .NET 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Real-time**: SignalR for chat functionality
- **Architecture**: Clean architecture with separate Core, Infrastructure, and API layers
- **Containerization**: Docker support for easy deployment

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+ running on localhost:5432
- Docker (optional, for containerized deployment)

## Local Development Setup

### 1. Database Setup
Ensure PostgreSQL is running on `localhost:5432` with a database named `connectionplatform`. The application will automatically create tables on startup.

Default connection string assumes:
- Host: localhost
- Port: 5432  
- Database: connectionplatform
- Username: postgres
- Password: postgres

### 2. Clone and Restore
```bash
git clone <repository-url>
cd connection-platform
dotnet restore
```

### 3. Build and Run
```bash
cd ConnectionPlatform.API
dotnet run
```

The API will be available at `http://localhost:5000`

### 4. Test the API
Health check endpoint: `GET http://localhost:5000/health`

## Docker Deployment

### Build Docker Image
```bash
docker build -t connection-platform-api .
```

### Run with Docker Compose

Create a `docker-compose.yml` file:

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - DATABASE_URL=Host=host.docker.internal;Port=5432;Database=connectionplatform;Username=postgres;Password=postgres
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - postgres
    
  # Note: This assumes you have an external PostgreSQL instance
  # If you need a containerized PostgreSQL, add:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: connectionplatform
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Run with:
```bash
docker-compose up -d
```

## Environment Variables

The application supports configuration via environment variables:

- `DATABASE_URL`: PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)
- `ASPNETCORE_URLS`: URL bindings (default: http://+:8080 in Docker)

## API Documentation

### Users
- `POST /api/users` - Create user
- `GET /api/users/{id}` - Get user info
- `PATCH /api/users/{id}` - Update profile
- `POST /api/users/{id}/skills` - Add skill
- `DELETE /api/users/{id}/skills/{skillId}` - Remove skill
- `GET /api/users/search?skill=&interest=&category=` - Search users

### Match-Making
- `GET /api/matches/suggestions?userId={id}&skip=&take=` - Get match suggestions
- `GET /api/matches/compatibility?userId1={id1}&userId2={id2}` - Get compatibility score

### Conversations & Messages
- `POST /api/conversations/direct/{otherUserId}?userId={id}` - Create conversation
- `GET /api/conversations?userId={id}` - List conversations
- `POST /api/conversations/{id}/messages?senderId={id}` - Send message
- `GET /api/conversations/{id}/messages?userId={id}&cursor=&limit=` - Get messages
- `PATCH /api/messages/{id}?userId={id}` - Edit message
- `DELETE /api/messages/{id}?userId={id}` - Delete message

### Code Management
- `POST /api/codes/repos?userId={id}` - Create repository
- `GET /api/codes/repos/{id}` - Get repository
- `DELETE /api/codes/repos/{id}?userId={id}` - Delete repository
- `POST /api/codes/folders?userId={id}` - Create folder
- `GET /api/codes/folders/{id}` - Get folder
- `DELETE /api/codes/folders/{id}?userId={id}` - Delete folder
- `POST /api/codes/files?userId={id}` - Create file
- `GET /api/codes/files/{id}` - Get file
- `PUT /api/codes/files/{id}/content?userId={id}` - Update file content
- `POST /api/codes/snippets?userId={id}` - Create snippet
- `GET /api/codes/snippets/{id}` - Get snippet
- `GET /api/codes/{repoId}/tree?folderId=` - Get repository tree

### Real-time Chat (SignalR)
WebSocket endpoint: `/chathub`

Methods:
- `JoinConversation(conversationId)`
- `SendMessage(conversationId, senderId, message, messageType)`
- `SendTyping(conversationId, senderId, isTyping)`
- `SendReadReceipt(conversationId, messageId, userId)`
- `StartVoiceCall(conversationId, callerId)` - Stub
- `StartVideoCall(conversationId, callerId)` - Stub

## Project Structure

```
ConnectionPlatform/
├── ConnectionPlatform.API/          # Web API layer
│   ├── Controllers/                 # REST API controllers
│   ├── Hubs/                       # SignalR hubs
│   └── Program.cs                  # Application entry point
├── ConnectionPlatform.Core/         # Domain layer
│   ├── Entities/                   # Domain entities
│   ├── DTOs/                       # Data transfer objects
│   └── Interfaces/                 # Service interfaces
├── ConnectionPlatform.Infrastructure/ # Data access layer
│   ├── Data/                       # DbContext and configurations
│   └── Services/                   # Service implementations
├── Dockerfile                      # Docker build instructions
└── README.md                       # This file
```

## Development Notes

- No authentication implemented (as per requirements)
- Database schema is automatically created on application startup
- All endpoints require userId as query parameter for user context
- SignalR hub provides real-time messaging capabilities
- Clean architecture allows for easy testing and maintenance
- Anti-plagiarism and voice/video features are stubbed with interfaces