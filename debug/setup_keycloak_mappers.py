"""
1. Kiểm tra mappers hiện tại của vbdh-client
2. Thêm mappers để đưa OrganizeCode, OrganizeName, UserRoleCode vào token
3. Set thử attributes cho testuser rồi lấy token kiểm tra
"""
import urllib.request, urllib.parse, json, base64

KC     = "http://192.168.1.6:8080"
REALM  = "vbdh-realm"
CLIENT = "vbdh-client"
SECRET = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

def api(method, path, data=None, token=None):
    req = urllib.request.Request(f"{KC}{path}",
        data=json.dumps(data).encode() if data else None, method=method)
    if token: req.add_header("Authorization", f"Bearer {token}")
    if data:  req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=5) as r:
            return r.status, r.read().decode() or "{}"
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()

def decode_jwt(token):
    pad  = lambda s: s + "=" * (-len(s) % 4)
    part = token.split(".")[1].replace("-", "+").replace("_", "/")
    return json.loads(base64.urlsafe_b64decode(pad(part)))

# Admin token
at = post_form(f"{KC}/realms/master/protocol/openid-connect/token",
    {"grant_type":"password","client_id":"admin-cli","username":"admin","password":"admin@123"})["access_token"]

# Lấy client UUID
_, body = api("GET", f"/admin/realms/{REALM}/clients?clientId={CLIENT}", token=at)
client  = json.loads(body)[0]
cid     = client["id"]

# ── 1. Xem mappers hiện tại ───────────────────────────────────────
print("=" * 60)
print("MAPPERS hiện tại của vbdh-client:")
_, body = api("GET", f"/admin/realms/{REALM}/clients/{cid}/protocol-mappers/models", token=at)
mappers = json.loads(body)
for m in mappers:
    print(f"  [{m['protocolMapper']}] {m['name']} → {m.get('config',{}).get('claim.name','')}")

# ── 2. Thêm mappers cho custom attributes ─────────────────────────
print("\n" + "=" * 60)
print("Thêm Protocol Mappers...")

new_mappers = [
    {
        "name": "OrganizeCode",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-usermodel-attribute-mapper",
        "config": {
            "user.attribute":       "OrganizeCode",
            "claim.name":           "organize_code",
            "jsonType.label":       "String",
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
    {
        "name": "OrganizeName",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-usermodel-attribute-mapper",
        "config": {
            "user.attribute":       "OrganizeName",
            "claim.name":           "organize_name",
            "jsonType.label":       "String",
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
    {
        "name": "OrganizeParent",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-usermodel-attribute-mapper",
        "config": {
            "user.attribute":       "OrganizeParent",
            "claim.name":           "organize_parent",
            "jsonType.label":       "String",
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
    {
        "name": "OrganizeParentName",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-usermodel-attribute-mapper",
        "config": {
            "user.attribute":       "OrganizeParentName",
            "claim.name":           "organize_parent_name",
            "jsonType.label":       "String",
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
    {
        "name": "UserRoleCode",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-usermodel-attribute-mapper",
        "config": {
            "user.attribute":       "UserRoleCode",
            "claim.name":           "user_role_code",
            "jsonType.label":       "String",
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
    {
        "name": "FullName",
        "protocol": "openid-connect",
        "protocolMapper": "oidc-full-name-mapper",
        "config": {
            "id.token.claim":       "true",
            "access.token.claim":   "true",
            "userinfo.token.claim": "true",
        }
    },
]

existing_names = {m["name"] for m in mappers}
for m in new_mappers:
    if m["name"] in existing_names:
        print(f"  ⏭  {m['name']} đã tồn tại")
        continue
    status, body = api("POST", f"/admin/realms/{REALM}/clients/{cid}/protocol-mappers/models", data=m, token=at)
    if status == 201:
        print(f"  ✅ Thêm mapper: {m['name']}")
    else:
        print(f"  ❌ Lỗi {m['name']}: {status} - {body}")

# ── 3. Set attributes cho testuser ───────────────────────────────
print("\n" + "=" * 60)
print("Set attributes cho testuser...")
_, body = api("GET", f"/admin/realms/{REALM}/users?username=testuser", token=at)
users = json.loads(body)
if users:
    user = users[0]; uid = user["id"]
    user["attributes"] = {
        "OrganizeCode":       ["G01.501.001.000"],
        "OrganizeName":       ["Phòng 1"],
        "OrganizeParent":     ["G01.501.000"],
        "OrganizeParentName": ["Văn Phòng Bộ Công an"],
        "UserRoleCode":       ["CAN_BO"],
    }
    user["firstName"] = "Cán Bộ"
    user["lastName"]  = "Phòng 1"
    status, _ = api("PUT", f"/admin/realms/{REALM}/users/{uid}", data=user, token=at)
    print(f"  {'✅ Cập nhật testuser OK' if status == 204 else f'❌ Lỗi {status}'}")

# ── 4. Lấy token mới và kiểm tra ─────────────────────────────────
print("\n" + "=" * 60)
print("Token sau khi thêm mappers:")
tok = post_form(f"{KC}/realms/{REALM}/protocol/openid-connect/token", {
    "grant_type": "password", "client_id": CLIENT, "client_secret": SECRET,
    "username": "testuser", "password": "Test@123", "scope": "openid profile email"
})
ap = decode_jwt(tok["access_token"])
print(f"  name:                {ap.get('name')}")
print(f"  preferred_username:  {ap.get('preferred_username')}")
print(f"  organize_code:       {ap.get('organize_code')}  ← DepartmentCode")
print(f"  organize_name:       {ap.get('organize_name')}  ← DepartmentName")
print(f"  organize_parent:     {ap.get('organize_parent')}")
print(f"  organize_parent_name:{ap.get('organize_parent_name')}")
print(f"  user_role_code:      {ap.get('user_role_code')}  ← RoleCode")
print(f"  realm_access.roles:  {ap.get('realm_access',{}).get('roles')}")
