"""
Tìm user 17fd1ca4-8b2d-4073-ab2a-eedafb14d900 trong DB (qua API) và Keycloak
"""
import urllib.request, urllib.parse, json, ssl, base64

TARGET_ID = "17fd1ca4-8b2d-4073-ab2a-eedafb14d900"
API       = "https://192.168.1.6:7146"
KC        = "http://192.168.1.6:8080"
REALM     = "vbdh-realm"
CLIENT    = "vbdh-client"
SECRET    = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

def api_get(url):
    req = urllib.request.Request(url)
    try:
        with urllib.request.urlopen(req, timeout=10, context=ctx) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

def kc_get(path, at):
    req = urllib.request.Request(f"{KC}{path}")
    req.add_header("Authorization", f"Bearer {at}")
    try:
        with urllib.request.urlopen(req, timeout=10) as r: return json.loads(r.read())
    except urllib.error.HTTPError as e:
        return {"error": e.code, "body": e.read().decode()}

# ── 1. Tìm qua API /uow/users/{id} ───────────────────────────
print("=" * 60)
print(f"Tìm user {TARGET_ID} qua API...")
status, resp = api_get(f"{API}/api/keycloak-test/uow/users/{TARGET_ID}")
print(f"  Status: {status}")
print(f"  Response: {json.dumps(resp, ensure_ascii=False, indent=2) if isinstance(resp, dict) else resp[:300]}")

# ── 2. Tìm trong Keycloak bằng sub ───────────────────────────
print("\n" + "=" * 60)
print("Tìm trong Keycloak theo sub (id)...")
at = post_form(f"{KC}/realms/master/protocol/openid-connect/token",
    {"grant_type":"password","client_id":"admin-cli","username":"admin","password":"admin@123"})["access_token"]

kc_user = kc_get(f"/admin/realms/{REALM}/users/{TARGET_ID}", at)
if "error" not in str(kc_user):
    print(f"  ✅ Tìm thấy trong Keycloak!")
    attrs = kc_user.get("attributes", {})
    print(f"  username:      {kc_user.get('username')}")
    print(f"  firstName:     {kc_user.get('firstName')}")
    print(f"  lastName:      {kc_user.get('lastName')}")
    print(f"  OrganizeCode:  {attrs.get('OrganizeCode', [''])[0]}")
    print(f"  UserRoleCode:  {attrs.get('UserRoleCode', [''])[0]}")
else:
    print(f"  ❌ Không tìm thấy trong Keycloak: {kc_user}")

# ── 3. Kiểm tra DB có data không ─────────────────────────────
print("\n" + "=" * 60)
print("Kiểm tra DB qua API...")
status2, resp2 = api_get(f"{API}/api/keycloak-test/uow/users")
if isinstance(resp2, dict):
    print(f"  DB/Keycloak users count: {resp2.get('getAll', {}).get('total', 'N/A')}")
    sample = resp2.get('getAll', {}).get('sample', [])
    if sample:
        print(f"  Sample IDs:")
        for u in sample:
            print(f"    {u.get('id')} — {u.get('fullName')}")
