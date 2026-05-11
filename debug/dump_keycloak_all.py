"""
Dump toàn bộ Users, Departments (OrganizeCode), Roles (UserRoleCode) từ Keycloak
"""
import urllib.request, urllib.parse, json, base64

KC    = "http://localhost:8080"
REALM = "vbdh-realm"

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req  = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    with urllib.request.urlopen(req, timeout=5) as r: return json.loads(r.read())

def kc(path, at, params=None):
    url = f"{KC}{path}"
    if params: url += "?" + urllib.parse.urlencode(params)
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {at}")
    try:
        with urllib.request.urlopen(req, timeout=10) as r: return json.loads(r.read())
    except urllib.error.HTTPError as e:
        return {"error": e.code, "body": e.read().decode()}

# Admin token
at = post_form(f"{KC}/realms/master/protocol/openid-connect/token",
    {"grant_type":"password","client_id":"admin-cli","username":"admin","password":"admin@123"})["access_token"]
print("✅ Admin token OK\n")

# ── 1. Lấy tất cả users (phân trang 100/lần) ─────────────────────
print("=" * 70)
print("USERS (tất cả):")
all_users = []
first = 0
while True:
    batch = kc(f"/admin/realms/{REALM}/users", at, {"first": first, "max": 100})
    if not batch or isinstance(batch, dict): break
    all_users.extend(batch)
    if len(batch) < 100: break
    first += 100

print(f"  Tổng: {len(all_users)} users\n")

# Tập hợp unique values
organize_codes   = set()
organize_names   = set()
role_codes       = set()
users_summary    = []

for u in all_users:
    attrs = u.get("attributes", {})
    oc  = attrs.get("OrganizeCode",       [""])[0]
    on  = attrs.get("OrganizeName",       [""])[0]
    opc = attrs.get("OrganizeParent",     [""])[0]
    opn = attrs.get("OrganizeParentName", [""])[0]
    rc  = attrs.get("UserRoleCode",       [""])[0]
    fn  = f"{u.get('firstName','')} {u.get('lastName','')}".strip() or u.get("username","")

    if oc: organize_codes.add(oc)
    if on: organize_names.add(on)
    if rc: role_codes.add(rc)

    users_summary.append({
        "id":           u["id"],
        "username":     u.get("username",""),
        "fullName":     fn,
        "enabled":      u.get("enabled", True),
        "OrganizeCode": oc,
        "OrganizeName": on,
        "OrganizeParent": opc,
        "OrganizeParentName": opn,
        "UserRoleCode": rc,
    })

# In bảng users
print(f"  {'USERNAME':<20} {'FULLNAME':<25} {'ORGANIZE_CODE':<20} {'ROLE_CODE':<20} {'ENABLED'}")
print(f"  {'-'*20} {'-'*25} {'-'*20} {'-'*20} {'-'*7}")
for u in users_summary:
    print(f"  {u['username']:<20} {u['fullName']:<25} {u['OrganizeCode']:<20} {u['UserRoleCode']:<20} {u['enabled']}")

# ── 2. Unique Departments ─────────────────────────────────────────
print("\n" + "=" * 70)
print("DEPARTMENTS (unique OrganizeCode từ users):")
dept_map = {}
for u in users_summary:
    if u["OrganizeCode"] and u["OrganizeCode"] not in dept_map:
        dept_map[u["OrganizeCode"]] = {
            "code":       u["OrganizeCode"],
            "name":       u["OrganizeName"],
            "parentCode": u["OrganizeParent"],
            "parentName": u["OrganizeParentName"],
        }

print(f"  {'CODE':<25} {'NAME':<30} {'PARENT_CODE':<25} {'PARENT_NAME'}")
print(f"  {'-'*25} {'-'*30} {'-'*25} {'-'*30}")
for d in sorted(dept_map.values(), key=lambda x: x["code"]):
    print(f"  {d['code']:<25} {d['name']:<30} {d['parentCode']:<25} {d['parentName']}")

# ── 3. Unique Roles ───────────────────────────────────────────────
print("\n" + "=" * 70)
print("ROLES (unique UserRoleCode từ users):")
print(f"  {'ROLE_CODE'}")
print(f"  {'-'*30}")
for rc in sorted(role_codes):
    print(f"  {rc}")

# ── 4. Realm roles (roles được định nghĩa trong Keycloak) ─────────
print("\n" + "=" * 70)
print("REALM ROLES (định nghĩa trong Keycloak):")
realm_roles = kc(f"/admin/realms/{REALM}/roles", at)
if isinstance(realm_roles, list):
    for r in realm_roles:
        if not r.get("composite") and not r["name"].startswith("default-roles"):
            print(f"  {r['name']}")

# ── 5. Export JSON để dùng cho SetupService ───────────────────────
print("\n" + "=" * 70)
print("EXPORT JSON (dùng cho SetupService / seed data):")
export = {
    "users": users_summary,
    "departments": list(dept_map.values()),
    "roleCodes": sorted(list(role_codes)),
}
with open("C:/Users/MSI Gaming/Desktop/codechay/debug/keycloak_export.json", "w", encoding="utf-8") as f:
    json.dump(export, f, ensure_ascii=False, indent=2)
print("  ✅ Đã lưu vào debug/keycloak_export.json")

# ── 6. Thống kê ───────────────────────────────────────────────────
print("\n" + "=" * 70)
print("THỐNG KÊ:")
print(f"  Tổng users:       {len(all_users)}")
print(f"  Users có attrs:   {sum(1 for u in users_summary if u['OrganizeCode'])}")
print(f"  Users không attrs:{sum(1 for u in users_summary if not u['OrganizeCode'])}")
print(f"  Unique depts:     {len(dept_map)}")
print(f"  Unique roles:     {len(role_codes)}")
