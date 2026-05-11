"""
Debug script: kiểm tra Keycloak đang chạy và lấy thông tin cần thiết để tích hợp
"""
import urllib.request
import urllib.parse
import json
import sys

KEYCLOAK_URL = "http://localhost:8080"
REALM = "vbdh-realm"
CLIENT_ID = "vbdh-client"
CLIENT_SECRET = "cS7whhY0aVyaLn79tiA8iDnfIrMswUJn"

def get(url):
    try:
        req = urllib.request.Request(url)
        with urllib.request.urlopen(req, timeout=5) as r:
            return json.loads(r.read())
    except Exception as e:
        return {"error": str(e)}

def post(url, data):
    try:
        body = urllib.parse.urlencode(data).encode()
        req = urllib.request.Request(url, data=body)
        req.add_header("Content-Type", "application/x-www-form-urlencoded")
        with urllib.request.urlopen(req, timeout=5) as r:
            return json.loads(r.read())
    except urllib.error.HTTPError as e:
        return {"error": f"HTTP {e.code}", "body": e.read().decode()}
    except Exception as e:
        return {"error": str(e)}

print("=" * 60)
print("1. Kiểm tra Keycloak health")
health = get(f"{KEYCLOAK_URL}/health")
print(json.dumps(health, indent=2, ensure_ascii=False))

print("\n" + "=" * 60)
print("2. OpenID Connect Discovery (JWKS URI, issuer, endpoints)")
oidc = get(f"{KEYCLOAK_URL}/realms/{REALM}/.well-known/openid-configuration")
if "error" not in oidc:
    keys = ["issuer", "jwks_uri", "token_endpoint", "authorization_endpoint", "introspection_endpoint"]
    for k in keys:
        print(f"  {k}: {oidc.get(k)}")
else:
    print(json.dumps(oidc, indent=2))

print("\n" + "=" * 60)
print("3. Lấy token bằng client_credentials")
token_url = f"{KEYCLOAK_URL}/realms/{REALM}/protocol/openid-connect/token"
token_resp = post(token_url, {
    "grant_type": "client_credentials",
    "client_id": CLIENT_ID,
    "client_secret": CLIENT_SECRET,
})
if "access_token" in token_resp:
    print("  ✅ Lấy token thành công!")
    print(f"  token_type: {token_resp.get('token_type')}")
    print(f"  expires_in: {token_resp.get('expires_in')}s")
    # Decode header của JWT (không verify)
    token = token_resp["access_token"]
    parts = token.split(".")
    if len(parts) == 3:
        import base64
        pad = lambda s: s + "=" * (-len(s) % 4)
        header = json.loads(base64.urlsafe_b64decode(pad(parts[0])))
        payload = json.loads(base64.urlsafe_b64decode(pad(parts[1])))
        print(f"\n  JWT Header: {json.dumps(header, indent=4)}")
        print(f"\n  JWT Payload (tóm tắt):")
        for k in ["iss", "aud", "azp", "typ", "alg"]:
            if k in header: print(f"    header.{k}: {header[k]}")
        for k in ["iss", "aud", "sub", "preferred_username", "email", "realm_access"]:
            if k in payload: print(f"    payload.{k}: {payload[k]}")
else:
    print("  ❌ Lấy token thất bại:")
    print(json.dumps(token_resp, indent=2, ensure_ascii=False))

print("\n" + "=" * 60)
print("4. JWKS (public keys để verify token)")
jwks = get(f"{KEYCLOAK_URL}/realms/{REALM}/protocol/openid-connect/certs")
if "keys" in jwks:
    print(f"  ✅ Có {len(jwks['keys'])} key(s)")
    for k in jwks["keys"]:
        print(f"    kid={k.get('kid')}, alg={k.get('alg')}, use={k.get('use')}")
else:
    print(json.dumps(jwks, indent=2))

print("\n" + "=" * 60)
print("SUMMARY - Thông tin cần cho appsettings.json:")
if "error" not in oidc:
    print(f"""
  \"Keycloak\": {{
    \"Authority\": \"{oidc.get('issuer')}\",
    \"Audience\": \"{CLIENT_ID}\",
    \"ClientId\": \"{CLIENT_ID}\",
    \"ClientSecret\": \"{CLIENT_SECRET}\",
    \"MetadataAddress\": \"{KEYCLOAK_URL}/realms/{REALM}/.well-known/openid-configuration\"
  }}
""")
