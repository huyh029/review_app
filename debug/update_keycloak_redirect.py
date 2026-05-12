"""
Cập nhật redirectUris của vbdh-client để cho phép 192.168.1.6:4200/callback
"""
import urllib.request, urllib.parse, json

KC = "http://192.168.1.6:8080"
REALM = "vbdh-realm"
CLIENT_ID = "vbdh-client"

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r:
        return json.loads(r.read())

def api(method, path, data=None, token=None):
    url = f"{KC}{path}"
    body = json.dumps(data).encode() if data else None
    req = urllib.request.Request(url, data=body, method=method)
    if token: req.add_header("Authorization", f"Bearer {token}")
    if data:  req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=5) as r:
            return r.status, r.read().decode() or "{}"
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

# Lấy admin token
admin = post_form(f"{KC}/realms/master/protocol/openid-connect/token",
    {"grant_type":"password","client_id":"admin-cli","username":"admin","password":"admin@123"})
at = admin["access_token"]
print("✅ Admin token OK")

# Lấy danh sách clients
status, body = api("GET", f"/admin/realms/{REALM}/clients?clientId={CLIENT_ID}", token=at)
clients = json.loads(body)
if not clients:
    print("❌ Không tìm thấy client"); exit(1)

client = clients[0]
cid = client["id"]
print(f"✅ Client UUID: {cid}")

# Cập nhật redirectUris
new_uris = list(set(client.get("redirectUris", []) + [
    "http://192.168.1.6:4200/*",
    "http://192.168.1.6:4200/callback",
]))
new_origins = list(set(client.get("webOrigins", []) + ["http://192.168.1.6:4200"]))

client["redirectUris"] = new_uris
client["webOrigins"]   = new_origins

status, body = api("PUT", f"/admin/realms/{REALM}/clients/{cid}", data=client, token=at)
if status == 204:
    print(f"✅ Đã cập nhật redirectUris: {new_uris}")
else:
    print(f"❌ Lỗi {status}: {body}")
