'use client';
import type { Conversation } from '@/lib/types';

export default function FriendsBar({ me, conversations }: { me: number; conversations: Conversation[] }) {
  const noMsgs = conversations.filter(c => !c.lastMessage);
  if (!noMsgs.length) return null;

  const friendId = (c: Conversation) => (c.user1Id === me ? c.user2Id : c.user1Id);

  return (
    <div className="mb-6">
      <h2 className="text-lg font-semibold mb-3">Friends</h2>
      <div className="flex gap-3 overflow-x-auto pb-1">
        {noMsgs.map(c => (
          <div key={c.id} className="shrink-0 w-20 text-center">
            <div className="mx-auto h-12 w-12 rounded-full bg-muted flex items-center justify-center">{friendId(c)}</div>
            <div className="text-xs text-muted-foreground mt-1">ID {friendId(c)}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
