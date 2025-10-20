'use client';
import { useEffect, useState } from 'react';
import { useAuth } from '@/contexts/auth-context';
import { api } from '@/lib/api-client';
import type { Conversation } from '@/lib/types';
import FriendsBar from '@/components/messages/FriendsBar';
import ConversationsList from '@/components/messages/ConversationsList';
import ClientOnly from '../_components/ClientOnly';

export default function MessagesPage() {
  const { user } = useAuth();
  const [convs, setConvs] = useState<Conversation[] | null>(null);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    if (!user?.id) return;
    api.getConversations(user.id).then(setConvs).catch(e => setErr(String(e)));
  }, [user?.id]);

  if (!user) return null;

  return (
    <ClientOnly>
      <div className="p-6 max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-4">Messages</h1>
        {err && <div className="text-red-600 mb-4">{err}</div>}
        {!convs ? <div>Loading…</div> : (
          <>
            <FriendsBar me={user.id} conversations={convs} />
            <ConversationsList me={user.id} conversations={convs} />
          </>
        )}
      </div>
    </ClientOnly>
  );
}
