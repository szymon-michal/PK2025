// lib/types.ts
export type User = {
  id: string;
  name: string;
  email: string;
  // dodaj inne pola, które zwraca backend
};

export type NewUser = {
  name: string;
  email: string;
  password: string;
};

export type Conversation = {
  id: number;
  user1Id: number;
  user2Id: number;
  lastMessage?: Message | null;
  unreadCount?: number;
};

export type Message = {
  id: number; conversationId: number; senderId: number; receiverId: number;
  messageType: 'text'; content: string; sentAt: string; isRead: boolean;
};

export type Repo = { id: number; userId: number; name: string; description?: string };
export type TreeNode = { id: number; name: string; isDirectory: boolean; children?: TreeNode[] };