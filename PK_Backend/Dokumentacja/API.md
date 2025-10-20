# Connection Platform API — Dokumentacja

**Base URL (domyślnie):** `http://localhost:8080`
**Wersjonowanie:** brak (ścieżka bazowa: `/api/...`)
**Autoryzacja:** obecnie **brak globalnego mechanizmu auth**. Część endpointów weryfikuje „tożsamość” przez **parametry zapytań** (`userId`, `senderId`). Jeśli użytkownik nie spełnia warunków, kontrolery zwracają **403 Forbid** (często **bez treści** w body).

> ℹ️ W wielu akcjach identyfikator użytkownika przekazuje się jako **query param** (np. `?userId=1` lub `?senderId=1`), a nie w nagłówku/autoryzacji.

---

## Konwencje

* **Identyfikatory**: `int` lub `long` zależnie od encji (np. wiadomości i konwersacje często mają `long`).
* **Czasy**: `timestamp with time zone` po stronie bazy; w API typowo ISO-8601.
* **Statusy błędów**:

  * `400 Bad Request` – z informacyjnym `{ "message": "..." }`
  * `403 Forbid` – zwykle **puste body** (komunikat bywa tylko w logach)
  * `404 Not Found` – puste lub `{ "message": "Not Found" }`
  * `500 Internal Server Error` – błąd serwera (np. naruszenie FK/unikatowości)

---

## Zdrowie usługi

### GET `/health`

Szybki check żywotności procesu.

**Przykład:**

```bash
curl -s http://localhost:8080/health
```

**200 OK**

```json
{
  "status": "healthy",
  "timestamp": "2025-10-12T16:04:57.215624Z"
}
```

---

## Users

Ścieżka bazowa: `/api/users`

### POST `/api/users`

**Tworzy nowego użytkownika.**

**Body**

```json
{
  "email": "smoke@gmail.com",
  "password": "P@ssw0rd!",
  "firstName": "Smoke",
  "lastName": "Tester",
  "nick": "smoker",
  "bio": "Created by smoke test",
  "age": 30
}
```

**Przykład:**

```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke@gmail.com","password":"P@ssw0rd!","firstName":"Smoke","lastName":"Tester","nick":"smoker","bio":"Created by smoke test","age":30}'
```

**201 Created**

```json
{
  "id": 6,
  "email": "smoke@gmail.com",
  "firstName": "Smoke",
  "lastName": "Tester",
  "nick": "smoker",
  "bio": "Created by smoke test",
  "age": 30,
  "isActive": false,
  "createdAt": "2025-10-16T10:12:00Z",
  "interests": [],
  "skills": []
}
```

**400 Bad Request** — np. dane niepoprawne/duplikaty (email/nick):

```json
{ "message": "An error occurred while saving the entity changes. See the inner exception for details." }
```

---

### GET `/api/users/{id}`

**Pobiera profil użytkownika (aktywny).**

```bash
curl -s http://localhost:8080/api/users/1
```

**200 OK**

```json
{
  "id": 1,
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "nick": "johnd",
  "bio": "Hi!",
  "age": 28,
  "isActive": true,
  "createdAt": "2025-10-01T08:00:00Z",
  "interests": [ { "id": 2, "name": "golang" } ],
  "skills": [ { "id": 3, "name": "C#", "level": "medium" } ]
}
```

**404 Not Found**

---

### PATCH `/api/users/{id}`

**Aktualizuje pola użytkownika.**

**Body (przykład)**

```json
{ "bio": "New bio" }
```

```bash
curl -X PATCH "http://localhost:8080/api/users/1" \
  -H "Content-Type: application/json" \
  -d '{"bio":"New bio"}'
```

**200 OK**

```json
{ "id": 1, "bio": "New bio", "...": "..." }
```

**404 Not Found**

---

### POST `/api/users/{id}/skills`

**Dodaje skill użytkownikowi.**

**Body**

```json
{ "skillId": 3, "level": "advanced" }
```

```bash
curl -X POST "http://localhost:8080/api/users/1/skills" \
  -H "Content-Type: application/json" \
  -d '{"skillId":3,"level":"advanced"}'
```

**200 OK**

```json
{ "message": "Skill added successfully" }
```

**400 Bad Request** – user/skill nie istnieje lub już dodany.

---

### DELETE `/api/users/{id}/skills/{skillId}`

**Usuwa skill użytkownika.**

```bash
curl -X DELETE "http://localhost:8080/api/users/1/skills/3"
```

**200 OK**

```json
{ "message": "Skill removed successfully" }
```

**404 Not Found** — brak tego skilla u usera.

---

### GET `/api/users/search`

**Wyszukiwanie użytkowników po atrybutach.**

**Query params (opcjonalne):**

* `skill` – np. `skill=python`
* `interest` – np. `interest=golang`
* `category` – np. `category=backend`

```bash
curl -s "http://localhost:8080/api/users/search?skill=python&interest=golang&category=backend"
```

**200 OK**

```json
[
  {
    "id": 2,
    "nick": "alice",
    "skills": [{ "id": 5, "name": "Python" }],
    "interests": [{ "id": 3, "name": "golang" }]
  }
]
```

---

## Match-Making

Ścieżka bazowa: `/api/matches`

### GET `/api/matches/suggestions`

**Sugestie kompatybilnych użytkowników.**

**Query**

* `userId` (wymagane): id użytkownika, dla którego liczymy sugestie
* `limit` (opc.): domyślnie 20

```bash
curl -s "http://localhost:8080/api/matches/suggestions?userId=1&limit=10"
```

**200 OK**

```json
[
  { "userId": 2, "nick": "alice", "score": 0.82 },
  { "userId": 4, "nick": "bob", "score": 0.79 }
]
```

**400 Bad Request** — brak `userId` lub nieprawidłowy.

---

### GET `/api/matches/compatibility`

**Liczy wynik kompatybilności między dwoma userami.**

**Query**: `user1Id`, `user2Id`

```bash
curl -s "http://localhost:8080/api/matches/compatibility?user1Id=1&user2Id=2"
```

**200 OK**

```json
{ "user1Id": 1, "user2Id": 2, "score": 0.73 }
```

**400 Bad Request** — brak parametrów.

---

## Conversations & Messages

### Kontroler Conversations

Ścieżka bazowa: `/api/conversations`

#### POST `/api/conversations/direct/{otherUserId}`

**Tworzy rozmowę 1:1.**

**Query:** `userId` (inicjator)

```bash
curl -X POST "http://localhost:8080/api/conversations/direct/2?userId=1"
```

**200 OK**

```json
{ "id": 1, "user1Id": 1, "user2Id": 2, "createdAt": "2025-10-16T10:20:00Z" }
```

**400 Bad Request** — rozmowa już istnieje itp.
**403 Forbid** — brak uprawnień (puste body możliwe).

---

#### GET `/api/conversations`

**Lista rozmów użytkownika.**

**Query:** `userId`

```bash
curl -s "http://localhost:8080/api/conversations?userId=1"
```

**200 OK**

```json
[
  {
    "id": 1,
    "user1Id": 1,
    "user2Id": 2,
    "lastMessage": { "id": 10, "content": "Hi", "sentAt": "2025-10-16T10:30:00Z" },
    "unreadCount": 2
  }
]
```

---

#### POST `/api/conversations/{id}/messages`

**Wysyła wiadomość w rozmowie.**

**Query:** `senderId` (nadawca musi należeć do rozmowy)
**Body:**

```json
{
  "receiverId": 2,
  "messageType": "text",
  "content": "Hello from smoke test"
}
```

```bash
curl -X POST "http://localhost:8080/api/conversations/1/messages?senderId=1" \
  -H "Content-Type: application/json" \
  -d '{"receiverId":2,"messageType":"text","content":"Hello from smoke test"}'
```

**201 Created**

```json
{
  "id": 21,
  "conversationId": 1,
  "senderId": 1,
  "receiverId": 2,
  "messageType": "text",
  "content": "Hello from smoke test",
  "sentAt": "2025-10-16T10:35:00Z",
  "isRead": false
}
```

**400 Bad Request** — np. brak treści/nieprawidłowy typ.
**403 Forbid** — jeśli `senderId` nie jest uczestnikiem konwersacji.
**404 Not Found** — konwersacja nie istnieje.

---

#### GET `/api/conversations/{id}/messages`

**Pobiera wiadomości z konwersacji (paginacja kursorem).**

**Query:**

* `userId` (wymagane)
* `cursor` (opc., `long`, domyślnie `0` oznacza najnowsze od końca)
* `limit` (opc., domyślnie `20`)

```bash
curl -s "http://localhost:8080/api/conversations/1/messages?userId=1&cursor=0&limit=20"
```

**200 OK**

```json
[
  { "id": 21, "senderId": 1, "receiverId": 2, "messageType": "text", "content": "Hello from smoke test", "sentAt": "2025-10-16T10:35:00Z", "isRead": false }
]
```

**400 / 403 / 404** — jak wyżej.

---

### Kontroler Messages

Ścieżka bazowa: `/api/messages`

#### PATCH `/api/messages/{id}`

**Edycja treści wiadomości.**

**Query:** `userId` (musi być nadawcą)
**Body:**

```json
{ "content": "Edited text" }
```

```bash
curl -X PATCH "http://localhost:8080/api/messages/21?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"content":"Edited text"}'
```

**200 OK**

```json
{
  "id": 21,
  "content": "Edited text",
  "messageType": "text",
  "senderId": 1,
  "receiverId": 2,
  "isRead": false
}
```

**403 Forbid** — jeśli `userId` ≠ nadawca.
**404 Not Found** — brak wiadomości.

---

#### DELETE `/api/messages/{id}`

**Usuwa wiadomość (tylko nadawca).**

**Query:** `userId`

```bash
curl -X DELETE "http://localhost:8080/api/messages/21?userId=1"
```

**200 OK**

```json
{ "message": "Message deleted successfully" }
```

**403 Forbid** / **404 Not Found**

---

#### POST `/api/messages/{id}/read`

**Oznacza wiadomość jako przeczytaną (odbiorca).**

**Query:** `userId`

```bash
curl -X POST "http://localhost:8080/api/messages/22/read?userId=2"
```

**200 OK**

```json
{ "message": "Message marked as read" }
```

**404 Not Found** — brak wiadomości lub nie dotyczy `userId`.

---

## Code Repository Management (Codes)

Ścieżka bazowa: `/api/codes`

> ⚠️ **Uwaga na `userId` w query** – wiele operacji wymaga właściciela repo!

### POST `/api/codes/repos`

**Tworzy repozytorium użytkownika.**

**Query:** `userId`
**Body:**

```json
{ "name": "smoke-repo", "description": "created by smoke test" }
```

```bash
curl -X POST "http://localhost:8080/api/codes/repos?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"name":"smoke-repo","description":"created by smoke test"}'
```

**201 Created**

```json
{ "id": 7, "userId": 1, "name": "smoke-repo", "description": "created by smoke test" }
```

**404/403/500** — jeśli `userId` nie istnieje (FK) lub błąd unikalności nazwy w obrębie usera.

---

### GET `/api/codes/repos/{id}`

**Pobiera repo.**

```bash
curl -s "http://localhost:8080/api/codes/repos/7"
```

**200 OK**

```json
{ "id": 7, "userId": 1, "name": "smoke-repo", "description": "created by smoke test", "metadata": { "totalFiles": 0, "visibility": "private" } }
```

**404 Not Found**

---

### DELETE `/api/codes/repos/{id}`

**Usuwa repo (tylko właściciel).**

**Query:** `userId`

```bash
curl -X DELETE "http://localhost:8080/api/codes/repos/7?userId=1"
```

**200 OK**

```json
{ "message": "Repository deleted successfully" }
```

**404 Not Found** — repo brak.
**403 Forbid** — nie ten właściciel.

---

### POST `/api/codes/folders`

**Tworzy folder w repo.**

**Query:** `userId`
**Body (przykład):**

```json
{ "repositoryId": 7, "name": "src", "parentId": null }
```

```bash
curl -X POST "http://localhost:8080/api/codes/folders?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"repositoryId":7,"name":"src","parentId":null}'
```

**201 Created**

```json
{ "id": 101, "repositoryId": 7, "name": "src", "isDirectory": true, "parentId": null }
```

**403 Forbid** — jeśli `userId` ≠ właściciel repo.

---

### GET `/api/codes/folders/{id}`

**Pobiera folder z dziećmi.**

```bash
curl -s "http://localhost:8080/api/codes/folders/101"
```

**200 OK**

```json
{
  "id": 101,
  "name": "src",
  "isDirectory": true,
  "children": []
}
```

**404 Not Found**

---

### DELETE `/api/codes/folders/{id}`

**Usuwa folder (tylko właściciel repo).**

**Query:** `userId`

```bash
curl -X DELETE "http://localhost:8080/api/codes/folders/101?userId=1"
```

**200 OK**

```json
{ "message": "Folder deleted successfully" }
```

**404 / 403** — jak wyżej.

---

### POST `/api/codes/files`

**Tworzy plik w repo.**

**Query:** `userId`
**Body (przykład):**

```json
{
  "repositoryId": 7,
  "name": "main.cs",
  "parentId": 101,
  "content": "Console.WriteLine(\"Hello\");",
  "extension": ".cs"
}
```

```bash
curl -X POST "http://localhost:8080/api/codes/files?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"repositoryId":7,"name":"main.cs","parentId":101,"content":"Console.WriteLine(\"Hello\");","extension":".cs"}'
```

**201 Created**

```json
{ "id": 102, "repositoryId": 7, "name": "main.cs", "isDirectory": false, "parentId": 101 }
```

**403 / 404** — brak uprawnień lub nieprawidłowe `repositoryId`/`parentId`.

---

### GET `/api/codes/files/{id}`

**Pobiera plik.**

```bash
curl -s "http://localhost:8080/api/codes/files/102"
```

**200 OK**

```json
{
  "id": 102,
  "name": "main.cs",
  "isDirectory": false,
  "content": "Console.WriteLine(\"Hello\");",
  "extension": ".cs",
  "parentId": 101
}
```

**404 Not Found**

---

### PUT `/api/codes/files/{id}/content`

**Aktualizuje zawartość pliku.**

**Query:** `userId`
**Body:**

```json
{ "content": "Console.WriteLine(\"Hello, world!\");" }
```

```bash
curl -X PUT "http://localhost:8080/api/codes/files/102/content?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"content":"Console.WriteLine(\"Hello, world!\");"}'
```

**200 OK**

```json
{ "message": "File content updated successfully" }
```

**404 / 403**

---

### DELETE `/api/codes/files/{id}`

**Usuwa plik.**

**Query:** `userId`

```bash
curl -X DELETE "http://localhost:8080/api/codes/files/102?userId=1"
```

**200 OK**

```json
{ "message": "File deleted successfully" }
```

**404 / 403**

---

### POST `/api/codes/snippets`

**Tworzy snippet użytkownika (w wewnętrznym repo „Code Snippets”).**

**Query:** `userId`
**Body (jak dla pliku):**

```json
{ "name": "example", "content": "print('hi')", "extension": ".py" }
```

```bash
curl -X POST "http://localhost:8080/api/codes/snippets?userId=1" \
  -H "Content-Type: application/json" \
  -d '{"name":"example","content":"print(\"hi\")","extension":".py"}'
```

**201 Created**

```json
{ "id": 205, "repositoryId": 9, "name": "example", "isDirectory": false }
```

**500** – jeśli user nie istnieje (FK na `repositories.user_id`) lub konflikt repo „Code Snippets”.

---

### GET `/api/codes/snippets/{id}`

**Pobiera snippet.**

```bash
curl -s "http://localhost:8080/api/codes/snippets/205"
```

**200 OK**

```json
{ "id": 205, "name": "example", "content": "print('hi')", "extension": ".py" }
```

**404 Not Found**

---

### GET `/api/codes/{repoId}/tree`

**Zwraca drzewo repo (całe lub od wybranego folderu).**

**Query (opcjonalny):** `folderId`

```bash
curl -s "http://localhost:8080/api/codes/7/tree"
# lub
curl -s "http://localhost:8080/api/codes/7/tree?folderId=101"
```

**200 OK**

```json
[
  {
    "id": 101,
    "name": "src",
    "isDirectory": true,
    "children": [
      { "id": 102, "name": "main.cs", "isDirectory": false }
    ]
  }
]
```

---

## Typowe błędy i wskazówki

* **403 Forbid z pustym body**: w kodzie wywoływane jest `Forbid()` – to normalne, komunikaty (np. „User is not part of this conversation”) mogą występować **tylko w logach**.
* **500 przy Codes**: sprawdź, czy **`userId` istnieje** (FK), a repo „Code Snippets” nie powoduje konfliktu unikatowości.
* **400 przy Users**: konflikty unikatowości (email/nick) w bazie; przydatna opcja w connection stringu: `Include Error Detail=true` (dla bogatszych błędów z Npgsql).

---

## Skrócona tabela endpointów

### Users

* `POST /api/users`
* `GET /api/users/{id}`
* `PATCH /api/users/{id}`
* `POST /api/users/{id}/skills`
* `DELETE /api/users/{id}/skills/{skillId}`
* `GET /api/users/search?skill=&interest=&category=`

### Matches

* `GET /api/matches/suggestions?userId=&limit=`
* `GET /api/matches/compatibility?user1Id=&user2Id=`

### Conversations

* `POST /api/conversations/direct/{otherUserId}?userId=`
* `GET /api/conversations?userId=`
* `POST /api/conversations/{id}/messages?senderId=`
* `GET /api/conversations/{id}/messages?userId=&cursor=&limit=`

### Messages

* `PATCH /api/messages/{id}?userId=`
* `DELETE /api/messages/{id}?userId=`
* `POST /api/messages/{id}/read?userId=`

### Codes

* `POST /api/codes/repos?userId=`
* `GET /api/codes/repos/{id}`
* `DELETE /api/codes/repos/{id}?userId=`
* `POST /api/codes/folders?userId=`
* `GET /api/codes/folders/{id}`
* `DELETE /api/codes/folders/{id}?userId=`
* `POST /api/codes/files?userId=`
* `GET /api/codes/files/{id}`
* `PUT /api/codes/files/{id}/content?userId=`
* `DELETE /api/codes/files/{id}?userId=`
* `POST /api/codes/snippets?userId=`
* `GET /api/codes/snippets/{id}`
* `GET /api/codes/{repoId}/tree?folderId=`

---

Jeśli chcesz, mogę dorzucić plik **OpenAPI/Swagger** na podstawie tej specyfikacji — łatwo wtedy wygenerujesz klienta dla dowolnego języka.
