"""
Lấy token Keycloak bằng testuser và decode để xem
user, department, role được map thế nào
"""
import urllib.request, urllib.parse, json, base64

KC      = "http://localhost:8080"
REALM   = "vbdh-realm"
CLIENT  = "vbdh-client"
SECRET  = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r:
        return json.loads(r.read())

def decode_jwt(token):
    pad  = lambda s: s + "=" * (-len(s) % 4)
    part = token.split(".")[1].replace("-", "+").replace("_", "/")
    return json.loads(base64.urlsafe_b64decode(pad(part)))

def api_get(url, token):
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(req, timeout=5) as r:
            return json.loads(r.read())
    except urllib.error.HTTPError as e:
        return {"error": e.code, "body": e.read().decode()}

# ── 1. Lấy admin token ────────────────────────────────────────────
admin = post_form(f"{KC}/realms/master/protocol/openid-connect/token", {
    "grant_type": "password", "client_id": "admin-cli",
    "username": "admin", "password": "admin@123"
})
at = admin["access_token"]
print("✅ Admin token OK\n")

# ── 2. Lấy token của testuser (password grant để debug) ───────────
print("=" * 60)
print("Lấy token testuser...")
try:
    tok = post_form(f"{KC}/realms/{REALM}/protocol/openid-connect/token", {
        "grant_type": "password", "client_id": CLIENT, "client_secret": SECRET,
        "username": "testuser", "password": "Test@123", "scope": "openid profile email"
    })
    access  = tok["access_token"]
    id_tok  = tok.get("id_token", "")
    print("✅ Lấy token thành công\n")
except Exception as e:
    print(f"❌ Lỗi: {e}"); exit(1)

# ── 3. Decode access_token ────────────────────────────────────────
print("=" * 60)
print("ACCESS TOKEN payload:")
ap = decode_jwt(access)
print(json.dumps(ap, indent=2, ensure_ascii=False))

# ── 4. Decode id_token ────────────────────────────────────────────
if id_tok:
    print("\n" + "=" * 60)
    print("ID TOKEN payload:")
    ip = decode_jwt(id_tok)
    print(json.dumps(ip, indent=2, ensure_ascii=False))

# ── 5. Gọi userinfo endpoint ──────────────────────────────────────
print("\n" + "=" * 60)
print("USERINFO endpoint:")
ui = api_get(f"{KC}/realms/{REALM}/protocol/openid-connect/userinfo", access)
print(json.dumps(ui, indent=2, ensure_ascii=False))

# ── 6. Lấy thông tin user từ Admin API ───────────────────────────
print("\n" + "=" * 60)
print("Admin API - User detail:")
users = api_get(f"{KC}/admin/realms/{REALM}/users?username=testuser", at)
if users and not isinstance(users, dict):
    user = users[0]
    uid  = user["id"]
    print(json.dumps(user, indent=2, ensure_ascii=False))

    # Groups
    print("\nGroups của user:")
    groups = api_get(f"{KC}/admin/realms/{REALM}/users/{uid}/groups", at)
    print(json.dumps(groups, indent=2, ensure_ascii=False))

    # Roles
    print("\nRoles của user (realm):")
    roles = api_get(f"{KC}/admin/realms/{REALM}/users/{uid}/role-mappings/realm", at)
    print(json.dumps(roles, indent=2, ensure_ascii=False))

# ── 7. Kết luận ───────────────────────────────────────────────────
print("\n" + "=" * 60)
print("PHÂN TÍCH:")
print(f"  sub (userId):          {ap.get('sub')}")
print(f"  preferred_username:    {ap.get('preferred_username')}")
print(f"  name:                  {ap.get('name')}")
print(f"  email:                 {ap.get('email')}")
print(f"  realm_access.roles:    {ap.get('realm_access', {}).get('roles')}")
print(f"  resource_access:       {list(ap.get('resource_access', {}).keys())}")
print(f"  groups:                {ap.get('groups')}")
print(f"  department_code:       {ap.get('department_code')} ← custom attribute")
print(f"  department_name:       {ap.get('department_name')} ← custom attribute")
print()
print("→ Keycloak KHÔNG tự có department/role của hệ thống.")
print("→ Cần map qua: sub (UUID) ↔ User.Id trong DB, hoặc dùng custom attributes/mapper.")
