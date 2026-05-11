"""
Test luồng login Keycloak qua API endpoint mới
Chạy sau khi khởi động reviewApi
"""
import urllib.request
import urllib.parse
import json

API_URL = "http://localhost:5104"  # port của reviewApi
KEYCLOAK_URL = "http://localhost:8080"
REALM = "vbdh-realm"
CLIENT_ID = "vbdh-client"
CLIENT_SECRET = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

def post_json(url, data, token=None):
    body = json.dumps(data).encode()
    req = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode())
    except Exception as e:
        return 0, {"error": str(e)}

def post_form(url, data):
    body = urllib.parse.urlencode(data).encode()
    req = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode())
    except Exception as e:
        return 0, {"error": str(e)}

print("=" * 60)
print("TEST 1: Tạo user test trên Keycloak (qua Admin API)")
# Lấy admin token
status, admin_token = post_form(
    f"{KEYCLOAK_URL}/realms/master/protocol/openid-connect/token",
    {"grant_type": "password", "client_id": "admin-cli",
     "username": "admin", "password": "admin@123"}
)
if status == 200:
    print("  ✅ Lấy admin token thành công")
    at = admin_token["access_token"]

    # Tạo user test
    import urllib.request
    create_req = urllib.request.Request(
        f"{KEYCLOAK_URL}/admin/realms/{REALM}/users",
        data=json.dumps({
            "username": "testuser",
            "enabled": True,
            "credentials": [{"type": "password", "value": "Test@123", "temporary": False}]
        }).encode()
    )
    create_req.add_header("Content-Type", "application/json")
    create_req.add_header("Authorization", f"Bearer {at}")
    try:
        with urllib.request.urlopen(create_req, timeout=5) as r:
            print(f"  ✅ Tạo user 'testuser' thành công (status {r.status})")
    except urllib.error.HTTPError as e:
        if e.code == 409:
            print("  ℹ️  User 'testuser' đã tồn tại")
        else:
            print(f"  ❌ Tạo user thất bại: {e.code} - {e.read().decode()}")
else:
    print(f"  ❌ Không lấy được admin token: {status}")
    print(json.dumps(admin_token, indent=2))

print("\n" + "=" * 60)
print("TEST 2: Login qua API /api/auth/keycloak-login")
status, resp = post_json(f"{API_URL}/api/auth/keycloak-login", {
    "username": "testuser",
    "password": "Test@123"
})
print(f"  Status: {status}")
if status == 200 and resp.get("success"):
    print("  ✅ Login thành công!")
    token = resp["access_token"]
    print(f"  expires_in: {resp.get('expires_in')}s")

    print("\n" + "=" * 60)
    print("TEST 3: Gọi API protected với Keycloak token")
    # Thử gọi một endpoint cần auth
    status2, resp2 = post_json(f"{API_URL}/api/auth/logout", {}, token=token)
    print(f"  Status: {status2}")
    print(f"  Response: {json.dumps(resp2, ensure_ascii=False)}")
    if status2 in [200, 400]:
        print("  ✅ Token Keycloak được API chấp nhận!")
    else:
        print("  ❌ Token bị từ chối")
else:
    print(f"  ❌ Login thất bại: {json.dumps(resp, indent=2, ensure_ascii=False)}")
    print("\n  Lưu ý: Đảm bảo reviewApi đang chạy trên port 5000")
    print("  Chạy: dotnet run --project reviewApi")
