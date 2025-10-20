'use client';
import { useEffect, useState } from 'react';
export default function ClientOnly({ children }: { children: React.ReactNode }) {
  const [m, setM] = useState(false);
  useEffect(() => setM(true), []);
  if (!m) return null;
  return <>{children}</>;
}
