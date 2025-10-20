'use client';
import { useEffect, useState } from 'react';
import { useAuth } from '@/contexts/auth-context';
import { api } from '@/lib/api-client';
import type { MatchSuggestion } from '@/lib/types';
import SwipeDeck from '@/components/matches/SwipeDeck';
import ClientOnly from '../_components/ClientOnly';

export default function MatchesPage() {
  const { user } = useAuth();
  const [data, setData] = useState<MatchSuggestion[] | null>(null);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    if (!user?.id) return;
    api.getSuggestions(user.id, 20).then(setData).catch(e => setErr(String(e)));
  }, [user?.id]);

  if (!user) return null;
  return (
    <ClientOnly>
      <div className="p-6 max-w-3xl mx-auto">
        <h1 className="text-3xl font-bold mb-4">Matches</h1>
        {err && <div className="text-red-600 mb-4">{err}</div>}
        {!data ? <div>Loading…</div> : <SwipeDeck me={user.id} suggestions={data} />}
      </div>
    </ClientOnly>
  );
}
