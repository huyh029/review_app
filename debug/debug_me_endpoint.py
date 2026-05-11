"""
Debug /api/auth/me — xem token claims vs DB users
"""
import urllib.request, urllib.parse, json, base64, ssl

KC     = "http://localhost:8080"
REALM  = "vbdh-realm"
CLIENT = "vbdh-client"
SECRET = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"
API    = "https://localhost:7146"

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

def decode_jwt(token):
    pad  = lambda s: s + "=" * (-len(s) % 4)
    part = token.split(".")[1].replace("-", "+").replace("_", "/")
    return json.loads(base64.urlsafe_b64decode(pad(part)))

def api_get(url, token=None):
    req = urllib.request.Request(url)
    if token: req.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(req, timeout=5, context=ctx) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

def kc_get(path, at):
    req = urllib.request.Request(f"{KC}{path}")
    req.add_header("Authorization", f"Bearer {at}")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

# Admin token Keycloak
at_kc = post_form(f"{KC}/realms/master/protocol/openid-connect/token",
    {"grant_type":"password","client_id":"admin-cli","username":"admin","password":"admin@123"})["access_token"]
print("✅ Admin KC token OK")

# Token testuser
tok = post_form(f"{KC}/realms/{REALM}/protocol/openid-connect/token", {
    "grant_type": "password", "client_id": CLIENT, "client_secret": SECRET,
    "username": "testuser", "password": "Test@123", "scope": "openid profile email"
})
access  = tok["access_token"]
payload = decode_jwt(access)

print("\n" + "=" * 60)
print("TOKEN CLAIMS của testuser:")
for k in ["sub", "name", "preferred_username", "given_name", "family_name",
          "organize_code", "organize_name", "user_role_code"]:
    print(f"  {k}: {payload.get(k)}")

print("\n" + "=" * 60)
print("Gọi /api/auth/me với token testuser:")
status, resp = api_get(f"{API}/api/auth/me", token=access)
print(f"  Status: {status}")
print(f"  Response: {json.dumps(resp, ensure_ascii=False, indent=2) if isinstance(resp, dict) else resp[:300]}")

print("\n" + "=" * 60)
print("Keycloak attributes của testuser:")
users = kc_get(f"/admin/realms/{REALM}/users?username=testuser", at_kc)
if users:
    u = users[0]
    print(f"  firstName: {u.get('firstName')}, lastName: {u.get('lastName')}")
    print(f"  attributes: {json.dumps(u.get('attributes', {}), ensure_ascii=False)}")

print("\n" + "=" * 60)
print("PHÂN TÍCH vấn đề 404:")
print(f"  Token 'name' claim: '{payload.get('name')}'")
print(f"  Token 'sub':        '{payload.get('sub')}'")
print()
print("  → /me tìm user theo FullName =", repr(payload.get('name')))
print("  → Nếu DB không có user với FullName này → 404")
print()
print("  Giải pháp: map theo organize_code + user_role_code thay vì FullName")
