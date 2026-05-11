"""
Kiểm tra DB có user nào không, và gọi SetBaseData nếu cần
"""
import urllib.request, urllib.parse, json, ssl

API = "https://localhost:7146"
ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

def api(method, path, data=None):
    req = urllib.request.Request(f"{API}{path}",
        data=json.dumps(data).encode() if data else None, method=method)
    if data: req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=10, context=ctx) as r:
            return r.status, r.read().decode()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

# Kiểm tra health
print("Kiểm tra API...")
status, body = api("GET", "/api/setup/set-base-data")
print(f"  GET /setup: {status}")

# Gọi SetBaseData
print("\nGọi POST /api/setup/set-base-data...")
status, body = api("POST", "/api/setup/set-base-data")
print(f"  Status: {status}")
print(f"  Response: {body[:200]}")

# Gọi lại /me sau khi có data
import base64
KC     = "http://localhost:8080"
REALM  = "vbdh-realm"
CLIENT = "vbdh-client"
SECRET = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

tok = post_form(f"{KC}/realms/{REALM}/protocol/openid-connect/token", {
    "grant_type": "password", "client_id": CLIENT, "client_secret": SECRET,
    "username": "testuser", "password": "Test@123", "scope": "openid profile email"
})
access = tok["access_token"]

print("\nGọi /api/auth/me sau SetBaseData:")
req = urllib.request.Request(f"{API}/api/auth/me")
req.add_header("Authorization", f"Bearer {access}")
try:
    with urllib.request.urlopen(req, timeout=5, context=ctx) as r:
        print(f"  Status: {r.status}")
        print(f"  Response: {json.dumps(json.loads(r.read()), ensure_ascii=False, indent=2)}")
except urllib.error.HTTPError as e:
    print(f"  Status: {e.code}")
    print(f"  Response: {e.read().decode()}")
