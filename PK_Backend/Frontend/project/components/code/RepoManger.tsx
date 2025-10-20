'use client';
import { useState } from 'react';
import { api } from '@/lib/api-client';
import type { Repo, TreeNode } from '@/lib/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

function Tree({ nodes }: { nodes: TreeNode[] }) {
  return (
    <ul className="ml-4 list-disc">
      {nodes.map(n => (
        <li key={n.id}>
          <span className={n.isDirectory ? 'font-medium' : ''}>{n.name}</span>
          {n.children && n.children.length > 0 && <Tree nodes={n.children} />}
        </li>
      ))}
    </ul>
  );
}

export default function RepoManager({ me }: { me: number }) {
  const [repo, setRepo] = useState<Repo | null>(null);
  const [repoIdInput, setRepoIdInput] = useState('');
  const [name, setName] = useState('my-repo');
  const [description, setDescription] = useState('');
  const [tree, setTree] = useState<TreeNode[] | null>(null);
  const [msg, setMsg] = useState<string | null>(null);

  async function createRepo() {
    setMsg(null);
    try {
      const r = await api.createRepo(me, name, description);
      setRepo(r);
      const t = await api.getTree(r.id);
      setTree(t);
      setMsg(`Repo utworzone (id ${r.id}).`);
    } catch (e: any) { setMsg(String(e)); }
  }

  async function openRepo() {
    setMsg(null);
    try {
      const id = Number(repoIdInput);
      const r = await api.getRepo(id);
      setRepo(r);
      setTree(await api.getTree(id));
    } catch (e: any) { setMsg(String(e)); }
  }

  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader><CardTitle>Create Repository</CardTitle></CardHeader>
        <CardContent className="space-y-3">
          <Input value={name} onChange={e => setName(e.target.value)} placeholder="name" />
          <Input value={description} onChange={e => setDescription(e.target.value)} placeholder="description" />
          <Button onClick={createRepo}>Create</Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Open Repository</CardTitle></CardHeader>
        <CardContent className="space-y-3">
          <Input value={repoIdInput} onChange={e => setRepoIdInput(e.target.value)} placeholder="repository id" />
          <Button onClick={openRepo}>Open</Button>
        </CardContent>
      </Card>

      <div className="md:col-span-2 space-y-4">
        {msg && <div className="text-sm text-muted-foreground">{msg}</div>}
        {repo && <div className="text-sm">Opened repo: <b>{repo.name}</b> (id {repo.id})</div>}
        {tree ? (
          <Card>
            <CardHeader><CardTitle>Repository Tree</CardTitle></CardHeader>
            <CardContent><Tree nodes={tree} /></CardContent>
          </Card>
        ) : <div className="text-muted-foreground">Brak danych drzewa.</div>}
      </div>
    </div>
  );
}
