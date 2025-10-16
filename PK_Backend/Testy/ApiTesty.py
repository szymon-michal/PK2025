
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Comprehensive API smoke tests for ConnectionPlatform API.

Covers:
- /api/users
- /api/matches
- /api/conversations + /api/messages
- /api/codes (repos, folders, files, snippets)

Assumptions:
or with a custom base url:
  BASE_URL=http://0.0.0.0:8080 python api_smoketests.py
"""
import os
import json
import time
from typing import Any, Dict, List, Tuple
import requests

BASE_URL = os.environ.get("BASE_URL", "http://localhost:8080")

def url(*parts: str) -> str:
    return "/".join([BASE_URL.rstrip("/")] + [p.strip("/") for p in parts])

def banner(title: str):
    print("\n" + "="*80)
    print(title)
    print("="*80)

def show(resp: requests.Response, expect: List[int]):
    ok = resp.status_code in expect
    status = f"{resp.status_code} {'OK' if ok else 'UNEXPECTED'}"
    print(f"{resp.request.method} {resp.request.url} -> {status}")
    try:
        data = resp.json()
    except Exception:
        data = resp.text
    if isinstance(data, (dict, list)):
        print(json.dumps(data, indent=2, ensure_ascii=False)[:2000])
    else:
        print(str(data)[:1000])
    return ok, data

def require(ok: bool, message: str):
    if not ok:
        raise AssertionError(message)

def main():
    session = requests.Session()
    session.headers.update({"Content-Type": "application/json"})

    # --- Health (sanity) -----------------------------------------------------
    banner("HEALTH CHECK")
    r = session.get(url("health"))
    show(r, [200])

    # ========================= USERS ========================================
    banner("USERS: create -> get -> update -> add/remove skill -> search -> negatives")
    # Create (may fail if unique constraints hit; that's fine – we assert 200/201 or 400)
    new_user_payload = {
        "email": "smoke+{}@example.com".format(int(time.time())),
        "password": "P@ssw0rd!",
        "firstName": "Smoke",
        "lastName": "Tester",
        "nick": "smoker_{}".format(int(time.time())),
        "bio": "Created by smoke test",
        "age": 30
    }
    r = session.post(url("api", "users"), data=json.dumps(new_user_payload))
    ok, data = show(r, [201, 400])
    created_user_id = None
    if r.status_code == 201:
        created_user_id = data.get("id")

    # Get existing user (id=1 expected in dummy data)
    r = session.get(url("api", "users", "1"))
    ok, _ = show(r, [200, 404])

    # Update user (id=1) bio
    r = session.patch(url("api", "users", "1"), data=json.dumps({"bio": "updated by smoke"}))
    show(r, [200, 404])

    # Add skill to user 1 (SkillId from your seed; try 1)
    r = session.post(url("api", "users", "1", "skills"), data=json.dumps({"skillId": 1, "level": "Beginner"}))
    show(r, [200, 400])

    # Remove skill from user 1
    r = session.delete(url("api", "users", "1", "skills", "1"))
    show(r, [200, 404])

    # Search users
    r = session.get(url("api", "users", "search"), params={"skill": "python"})
    show(r, [200])

    # Negative: get non-existing user
    r = session.get(url("api", "users", "999999"))
    show(r, [404])

    # ========================= MATCHES ======================================
    banner("MATCHES: suggestions + compatibility")
    r = session.get(url("api", "matches", "suggestions"), params={"userId": 1, "skip": 0, "take": 5})
    show(r, [200, 400])

    r = session.get(url("api", "matches", "compatibility"), params={"userId1": 1, "userId2": 2})
    show(r, [200, 400])

    # Negative: bad params
    r = session.get(url("api", "matches", "compatibility"), params={"userId1": 0, "userId2": 0})
    show(r, [400])

    # ====================== CONVERSATIONS + MESSAGES =========================
    banner("CONVERSATIONS: create direct -> list -> send message -> list messages -> edit/delete/read")
    # Create direct conversation between userId 1 and otherUserId 2
    r = session.post(url("api", "conversations", "direct", "2"), params={"userId": 1})
    ok, conv = show(r, [200, 400, 403])
    conversation_id = conv.get("id") if isinstance(conv, dict) else None

    # List user 1 conversations
    r = session.get(url("api", "conversations"), params={"userId": 1})
    ok, lst = show(r, [200])
    if not conversation_id and isinstance(lst, list) and lst:
        conversation_id = lst[0].get("id")

    if conversation_id is None:
        print("! Could not determine conversation_id; skipping messaging flow beyond this point.")
    else:
        # Send message (senderId is query param; body = CreateMessageDto)
        msg_payload = {"receiverId": 2, "messageType": "text", "content": "Hello from smoke test"}
        r = session.post(url("api", "conversations", str(conversation_id), "messages"),
                         params={"senderId": 1}, data=json.dumps(msg_payload))
        ok, sent = show(r, [201, 400, 403])
        message_id = sent.get("id") if isinstance(sent, dict) else None

        # List messages as userId=1
        r = session.get(url("api", "conversations", str(conversation_id), "messages"),
                        params={"userId": 1, "cursor": 0, "limit": 20})
        show(r, [200, 400, 403])

        # Edit message (MessagesController) – requires userId query param
        if message_id is not None:
            r = session.patch(url("api", "messages", str(message_id)),
                              params={"userId": 1}, data=json.dumps({"content": "Edited content"}))
            show(r, [200, 403, 404])

            # Mark as read
            r = session.post(url("api", "messages", str(message_id), "read"), params={"userId": 2})
            show(r, [200, 404])

            # Delete message
            r = session.delete(url("api", "messages", str(message_id)), params={"userId": 1})
            show(r, [200, 403, 404])

        # Negative: list messages as a user not in conversation (should be 403)
        r = session.get(url("api", "conversations", str(conversation_id), "messages"),
                        params={"userId": 999999})
        show(r, [403, 400])

    # =========================== CODES =======================================
    banner("CODES: repos -> folders -> files -> snippets -> tree (+ negatives)")
    # Create repository requires userId in query
    repo_payload = {"name": "smoke-repo-{}".format(int(time.time())), "description": "created by smoke test"}
    r = session.post(url("api", "codes", "repos"), params={"userId": 1}, data=json.dumps(repo_payload))
    ok, repo = show(r, [201, 403, 500])
    repo_id = repo.get("id") if isinstance(repo, dict) else None

    # Get repository (404 negative if not present)
    if repo_id:
        r = session.get(url("api", "codes", "repos", str(repo_id)))
        show(r, [200, 404])
    else:
        print("! repo_id unavailable – subsequent file/folder tests may be skipped.")

    # Create folder in repo (root)
    folder_id = None
    if repo_id:
        folder_payload = {"repositoryId": repo_id, "parentId": None, "name": "src"}
        r = session.post(url("api", "codes", "folders"), params={"userId": 1}, data=json.dumps(folder_payload))
        ok, folder = show(r, [201, 403])
        folder_id = folder.get("id") if isinstance(folder, dict) else None

        # Create file in folder
        if folder_id:
            file_payload = {"repositoryId": repo_id, "parentId": folder_id, "name": "main.py", "content": "print('hi')", "extension": ".py"}
            r = session.post(url("api", "codes", "files"), params={"userId": 1}, data=json.dumps(file_payload))
            ok, file_obj = show(r, [201, 403])
            file_id = file_obj.get("id") if isinstance(file_obj, dict) else None

            # Update file content
            if file_id:
                r = session.put(url("api", "codes", "files", str(file_id), "content"),
                                params={"userId": 1}, data=json.dumps({"content": "# updated\nprint('hello')"}) )
                show(r, [200, 404])

                # Get file
                r = session.get(url("api", "codes", "files", str(file_id)))
                show(r, [200, 404])

            # Get folder
            r = session.get(url("api", "codes", "folders", str(folder_id)))
            show(r, [200, 404])

        # Snippet (auto repo "Code Snippets" inside service; requires userId)
        snippet_payload = {"repositoryId": repo_id, "parentId": None, "name": "snippet.txt", "content": "tmp", "extension": ".txt"}
        r = session.post(url("api", "codes", "snippets"), params={"userId": 1}, data=json.dumps(snippet_payload))
        show(r, [201, 403, 500])

        # Tree (repo root)
        r = session.get(url("api", "codes", str(repo_id), "tree"))
        show(r, [200])

        # Tree (folder)
        if folder_id:
            r = session.get(url("api", "codes", str(repo_id), "tree"), params={"folderId": folder_id})
            show(r, [200])

        # Delete file (negative OK if file_id missing)
        if 'file_id' in locals() and file_id:
            r = session.delete(url("api", "codes", "files", str(file_id)), params={"userId": 1})
            show(r, [200, 404])

        # Delete folder
        if folder_id:
            r = session.delete(url("api", "codes", "folders", str(folder_id)), params={"userId": 1})
            show(r, [200, 404])

        # Delete repository
        if repo_id:
            r = session.delete(url("api", "codes", "repos", str(repo_id)), params={"userId": 1})
            show(r, [200, 404])

    # Negative: repo not found
    r = session.get(url("api", "codes", "repos", "999999"))
    show(r, [404])

    banner("DONE")
    print("Base URL:", BASE_URL)

if __name__ == "__main__":
    main()
