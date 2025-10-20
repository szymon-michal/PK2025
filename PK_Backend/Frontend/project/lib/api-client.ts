const BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? '';

async function handleError(r: Response): Promise<never> {
  const ct = r.headers.get('content-type') ?? '';
  if (ct.includes('application/json')) {
    const data = await r.json().catch(() => null);
    const msg = (data && (data.message || JSON.stringify(data))) || '';
    throw new Error(`${r.status} ${r.statusText}: ${msg}`);
  }
  const text = await r.text().catch(() => `HTTP ${r.status}`);
  throw new Error(`${r.status} ${r.statusText}: ${text}`);
}

async function j<T>(r: Response): Promise<T> {
  if (r.ok) {
    const ct = r.headers.get('content-type') ?? '';
    if (!ct) return (undefined as unknown) as T;
    return r.json();
  } else {
    return handleError(r); // rzuci Error z czytelnym komunikatem
  }
}

export const api = {
  // MATCHES
  getSuggestions(userId: number, limit = 20) {
    return fetch(`${BASE}/matches/suggestions?userId=${userId}&limit=${limit}`).then(j);
  }, // GET /api/matches/suggestions  :contentReference[oaicite:1]{index=1}

  createDirectConversation(otherUserId: number, userId: number) {
    return fetch(`${BASE}/conversations/direct/${otherUserId}?userId=${userId}`, { method: 'POST' }).then(j);
  }, // POST /api/conversations/direct/{otherUserId}?userId=  :contentReference[oaicite:2]{index=2}

  // MESSAGES
  getConversations(userId: number) {
    return fetch(`${BASE}/conversations?userId=${userId}`).then(j);
  }, // GET /api/conversations  :contentReference[oaicite:3]{index=3}

  getMessages(conversationId: number, userId: number, cursor = 0, limit = 20) {
    const qs = new URLSearchParams({ userId: String(userId), cursor: String(cursor), limit: String(limit) });
    return fetch(`${BASE}/conversations/${conversationId}/messages?${qs}`).then(j);
  }, // GET /api/conversations/{id}/messages  :contentReference[oaicite:4]{index=4}

  sendMessage(conversationId: number, senderId: number, receiverId: number, content: string) {
    return fetch(`${BASE}/conversations/${conversationId}/messages?senderId=${senderId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ receiverId, messageType: 'text', content }),
    }).then(j);
  }, // POST /api/conversations/{id}/messages?senderId=  :contentReference[oaicite:5]{index=5}

  // CODES
  createRepo(userId: number, name: string, description?: string) {
    return fetch(`${BASE}/codes/repos?userId=${userId}`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, description }),
    }).then(j);
  }, // POST /api/codes/repos?userId=  :contentReference[oaicite:6]{index=6}

  getRepo(repoId: number) { return fetch(`${BASE}/codes/repos/${repoId}`).then(j); }, // GET /api/codes/repos/{id}  :contentReference[oaicite:7]{index=7}
  deleteRepo(repoId: number, userId: number) { return fetch(`${BASE}/codes/repos/${repoId}?userId=${userId}`, { method:'DELETE' }).then(j); }, // :contentReference[oaicite:8]{index=8}
  getTree(repoId: number, folderId?: number) {
    const qs = folderId ? `?folderId=${folderId}` : '';
    return fetch(`${BASE}/codes/${repoId}/tree${qs}`).then(j);
  }, // GET /api/codes/{repoId}/tree  :contentReference[oaicite:9]{index=9}

  createFolder(userId: number, repositoryId: number, name: string, parentId: number | null) {
    return fetch(`${BASE}/codes/folders?userId=${userId}`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ repositoryId, name, parentId }),
    }).then(j);
  }, // POST /api/codes/folders?userId=  :contentReference[oaicite:10]{index=10}

  createFile(userId: number, repositoryId: number, name: string, parentId: number | null, content: string, extension: string) {
    return fetch(`${BASE}/codes/files?userId=${userId}`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ repositoryId, name, parentId, content, extension }),
    }).then(j);
  }, // POST /api/codes/files?userId=  :contentReference[oaicite:11]{index=11}

  updateFileContent(userId: number, fileId: number, content: string) {
    return fetch(`${BASE}/codes/files/${fileId}/content?userId=${userId}`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ content }),
    }).then(j);
  }, // PUT /api/codes/files/{id}/content?userId=  :contentReference[oaicite:12]{index=12}

  // Dashboard
  getDashboard(userId: number) {
    return fetch(`${BASE}/dashboard/${userId}`).then(j);
  },

  uploadProfilePhoto(userId: number, file: File) {
    const fd = new FormData();
    fd.append('file', file);
    return fetch(`${BASE}/dashboard/${userId}/photo`, {
      method: 'POST',
      body: fd
    }).then(j);
  },

  downloadProfilePhoto(userId: number) {
    return fetch(`${BASE}/dashboard/${userId}/photo`).then(async r => {
      if (!r.ok) throw new Error(await r.text().catch(() => `HTTP ${r.status}`));
      return r.blob();
    });
  },

  // USERS
  createUser(payload: unknown) {
    return fetch(`${BASE}/users`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }).then(j);
  },
};
