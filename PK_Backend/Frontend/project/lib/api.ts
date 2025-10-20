const API_BASE_URL = 'http://localhost:8080';

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  nick: string;
  bio?: string;
  age?: number;
  isActive: boolean;
  createdAt: string;
  interests: Interest[];
  skills: Skill[];
}

export interface Interest {
  id: number;
  name: string;
}

export interface Skill {
  id: number;
  name: string;
  level?: 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert';
}

export interface Conversation {
  id: number;
  user1Id: number;
  user2Id: number;
  lastMessage?: Message;
  unreadCount: number;
  createdAt: string;
}

export interface Message {
  id: number;
  conversationId: number;
  senderId: number;
  receiverId: number;
  messageType: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}

export interface Repository {
  id: number;
  userId: number;
  name: string;
  description?: string;
  metadata?: {
    totalFiles: number;
    visibility: 'public' | 'private';
  };
}

export interface FriendRequest {
  id: number;
  senderId: number;
  receiverId: number;
  message?: string;
  status: 'pending' | 'accepted' | 'rejected';
  sentAt: string;
}

export interface MatchSuggestion {
  userId: number;
  nick: string;
  score: number;
}

class ApiClient {
  private baseUrl = API_BASE_URL;

  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, value.toString());
        }
      });
    }
    
    const response = await fetch(url.toString());
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  async post<T>(endpoint: string, data?: any, params?: Record<string, any>): Promise<T> {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, value.toString());
        }
      });
    }
    
    const response = await fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: data ? JSON.stringify(data) : undefined,
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return response.json();
  }

  async patch<T>(endpoint: string, data?: any, params?: Record<string, any>): Promise<T> {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, value.toString());
        }
      });
    }
    
    const response = await fetch(url.toString(), {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: data ? JSON.stringify(data) : undefined,
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return response.json();
  }

  async delete<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, value.toString());
        }
      });
    }
    
    const response = await fetch(url.toString(), {
      method: 'DELETE',
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return response.json();
  }

  // Health check
  async checkHealth() {
    return this.get<{ status: string; timestamp: string }>('/health');
  }

  // User endpoints
  async createUser(userData: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    nick: string;
    bio?: string;
    age?: number;
  }) {
    return this.post<User>('/api/users', userData);
  }

  async getUser(id: number) {
    return this.get<User>(`/api/users/${id}`);
  }

  async updateUser(id: number, data: Partial<User>) {
    return this.patch<User>(`/api/users/${id}`, data);
  }

  async searchUsers(params: { skill?: string; interest?: string; category?: string }) {
    return this.get<User[]>('/api/users/search', params);
  }

  // Friends endpoints
  async sendFriendRequest(senderId: number, receiverId: number, message?: string) {
    return this.post<{ id: number }>('/api/friend-requests', {
      senderId,
      receiverId,
      message
    });
  }

  // Match endpoints
  async getMatchSuggestions(userId: number, limit = 20) {
    return this.get<MatchSuggestion[]>('/api/matches/suggestions', { userId, limit });
  }

  async getCompatibilityScore(user1Id: number, user2Id: number) {
    return this.get<{ user1Id: number; user2Id: number; score: number }>('/api/matches/compatibility', {
      user1Id,
      user2Id
    });
  }

  // Conversations endpoints
  async createDirectConversation(userId: number, otherUserId: number) {
    return this.post<Conversation>(`/api/conversations/direct/${otherUserId}`, undefined, { userId });
  }

  async getConversations(userId: number) {
    return this.get<Conversation[]>('/api/conversations', { userId });
  }

  async sendMessage(conversationId: number, senderId: number, data: {
    receiverId: number;
    messageType: string;
    content: string;
  }) {
    return this.post<Message>(`/api/conversations/${conversationId}/messages`, data, { senderId });
  }

  async getMessages(conversationId: number, userId: number, cursor = 0, limit = 20) {
    return this.get<Message[]>(`/api/conversations/${conversationId}/messages`, {
      userId,
      cursor,
      limit
    });
  }

  // Repositories endpoints
  async createRepository(userId: number, data: { name: string; description?: string }) {
    return this.post<Repository>('/api/codes/repos', data, { userId });
  }

  async getRepository(id: number) {
    return this.get<Repository>(`/api/codes/repos/${id}`);
  }

  async deleteRepository(id: number, userId: number) {
    return this.delete(`/api/codes/repos/${id}`, { userId });
  }
}

export const api = new ApiClient();