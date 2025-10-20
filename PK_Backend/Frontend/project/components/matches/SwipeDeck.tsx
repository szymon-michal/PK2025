'use client';
import { useMemo, useState } from 'react';
import SwipeCard from './SwipeCard';
import { api } from '@/lib/api-client';
import type { MatchSuggestion } from '@/lib/types';
import { Alert, AlertDescription } from '@/components/ui/alert';

export default function SwipeDeck({
  me,
  suggestions,
}: { me: number; suggestions: MatchSuggestion[] }) {
  const [idx, setIdx] = useState(0);
  const [info, setInfo] = useState<string | null>(null);

  const current = useMemo(() => suggestions[idx], [suggestions, idx]);

  async function handleSwipe(dir: 'left' | 'right') {
    setInfo(null);
    if (!current) return;
    if (dir === 'right') {
      try {
        // próbujemy od razu utworzyć direct conversation (jeśli backend wymaga „mutual like”, 403 oznacza „czekaj na drugą stronę”)
        await api.createDirectConversation(current.userId, me);
        setInfo(`It's a match! Rozmowa z @${current.nick ?? current.userId} utworzona.`);
      } catch (e: any) {
        // 403 / 400 -> traktujemy jako pending lub już istnieje
        setInfo('Like zapisany. Czekamy na wzajemność lub rozmowa już istnieje.');
      }
    }
    setIdx((i) => i + 1);
  }

  if (!current) {
    return <div className="text-center text-muted-foreground">Brak dalszych sugestii.</div>;
  }

  return (
    <div className="space-y-4">
      {info && (
        <Alert><AlertDescription>{info}</AlertDescription></Alert>
      )}
      <SwipeCard nick={current.nick} score={current.score} onSwipe={handleSwipe} />
      <div className="text-center text-sm text-muted-foreground">
        {idx + 1} / {suggestions.length}
      </div>
    </div>
  );
}
