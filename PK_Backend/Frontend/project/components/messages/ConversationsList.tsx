'use client';
import type { Conversation } from '@/lib/types';

export default function ConversationsList({ me, conversations }: { me: number; conversations: Conversation[] }) {
  const withMsgs = conversations.filter(c => c.lastMessage);

  const friendId = (c: Conversation) => (c.user1Id === me ? c.user2Id : c.user1Id);

  return (
    <div className="space-y-3">
      {withMsgs.map(c => (
        <div key={c.id} className="rounded-xl border p-4 flex items-center justify-between">
          <div>
            <div className="font-medium">User {friendId(c)}</div>
            <div className="text-sm text-muted-foreground">{c.lastMessage?.content}</div>
          </div>
          <div className="text-right text-xs text-muted-foreground">{c.unreadCount ?? 0} unread</div>
        </div>
      ))}
      {!withMsgs.length && <div className="text-muted-foreground">Brak rozmów.</div>}
    </div>
  );
}
