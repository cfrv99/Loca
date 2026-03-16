#!/usr/bin/env python3
"""
PreToolUse hook: blocks writes containing hardcoded secrets, passwords, or API keys.
Exit 0 = allow, Exit 2 = block.
"""
import json, re, sys

data = json.load(sys.stdin)
content = data.get("tool_input", {}).get("content", "") or ""
fp = data.get("tool_input", {}).get("file_path", "") or ""

# Skip non-code files
if not fp or not any(fp.endswith(ext) for ext in (".cs", ".ts", ".tsx", ".js", ".jsx", ".py", ".json")):
    sys.exit(0)

# Skip config templates and example files
if any(x in fp for x in ("example", "template", "appsettings.Development", ".env.example")):
    sys.exit(0)

PATTERNS = [
    (r'(?i)(password|passwd|pwd)\s*[=:]\s*["\'][^"\']{8,}["\']', "Hardcoded password"),
    (r'(?i)api[_-]?key\s*[=:]\s*["\'][a-zA-Z0-9]{16,}["\']', "Hardcoded API key"),
    (r'(?i)secret\s*[=:]\s*["\'][^"\']{10,}["\']', "Hardcoded secret"),
    (r'(?i)connection[_-]?string\s*[=:]\s*["\'](?:Host|Server|Data Source)[^"\']+["\']', "Hardcoded connection string"),
    (r'ghp_[a-zA-Z0-9]{36}', "GitHub personal access token"),
    (r'sk-[a-zA-Z0-9]{32,}', "API secret key pattern"),
    (r'-----BEGIN (?:RSA )?PRIVATE KEY-----', "Private key"),
]

for pattern, message in PATTERNS:
    if re.search(pattern, content):
        print(json.dumps({
            "decision": "block",
            "reason": f"🔴 SECURITY: {message} detected in {fp}. Move to environment variables or appsettings.Development.json."
        }))
        sys.exit(2)

sys.exit(0)
