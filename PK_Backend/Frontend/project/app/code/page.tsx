'use client';
import { useAuth } from '@/contexts/auth-context';
import RepoManager from '@/components/code/RepoManger';
import ClientOnly from '../_components/ClientOnly';

export default function CodePage() {
  const { user } = useAuth();
  if (!user) return null;

  return (
    <ClientOnly>
      <div className="p-6 max-w-5xl mx-auto">
        <h1 className="text-3xl font-bold mb-4">Code</h1>
        <RepoManager me={user.id} />
      </div>
    </ClientOnly>
  );
}
