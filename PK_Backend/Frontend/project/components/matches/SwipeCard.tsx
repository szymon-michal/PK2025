'use client';
import { motion, PanInfo } from 'framer-motion';

type Props = {
  nick?: string;
  score: number;
  onSwipe: (dir: 'left' | 'right') => void;
};

export default function SwipeCard({ nick = 'User', score, onSwipe }: Props) {
  const handleDragEnd = (_: any, info: PanInfo) => {
    const x = info.offset.x;
    if (x > 120) onSwipe('right');
    else if (x < -120) onSwipe('left');
  };

  return (
    <motion.div
      className="bg-white rounded-2xl shadow p-6 w-full max-w-md mx-auto cursor-grab select-none"
      drag="x"
      dragConstraints={{ left: 0, right: 0 }}
      dragElastic={0.8}
      onDragEnd={handleDragEnd}
      whileDrag={{ rotate: 5, scale: 1.02 }}
    >
      <div className="text-2xl font-semibold mb-2">{nick}</div>
      <div className="text-muted-foreground">Match score: {(score * 100).toFixed(0)}%</div>
      <div className="mt-6 h-48 rounded-xl bg-gradient-to-br from-blue-50 to-indigo-100" />
      <div className="mt-6 grid grid-cols-2 gap-3">
        <button onClick={() => onSwipe('left')} className="rounded-xl border py-2">Nope</button>
        <button onClick={() => onSwipe('right')} className="rounded-xl bg-primary text-primary-foreground py-2">Like</button>
      </div>
    </motion.div>
  );
}
