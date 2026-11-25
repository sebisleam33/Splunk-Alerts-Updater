#!/usr/bin/env python3
"""Fast offline checks for JSON-driven fixtures and seed scripts."""
import json
import os
import runpy
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
os.environ["POC_CONFIG_DIR"] = str(ROOT / "config" / "data")
sys.path.insert(0, str(ROOT / "scripts"))

for path in (ROOT / "config" / "data").glob("*.json"):
    json.loads(path.read_text(encoding="utf-8"))

users = json.loads((ROOT / "config/data/users.json").read_text())["users"]
alerts = json.loads((ROOT / "config/data/alerts.json").read_text())
data = json.loads((ROOT / "config/data/seed-data.json").read_text())
module = runpy.run_path(str(ROOT / "scripts/seed_users_alerts.py"))
assert alerts["app"] == "search"
assert alerts["disableExecution"] is True
assert alerts["fixturesPerUser"] >= 100
assert len(data["scenarios"]) >= 3
for user in users:
    fixtures = [module["make_fixture"](user, n) for n in range(1, alerts["fixturesPerUser"] + 1)]
    assert len({f["name"] for f in fixtures}) == alerts["fixturesPerUser"]
    assert {f["app"] for f in fixtures} == {"search"}
    assert {f["sharing"] for f in fixtures} == {"user"}
    assert len({f["cron_schedule"] for f in fixtures}) >= 5
    assert all(f["actions"]["action.webhook.param.url"].startswith("https://example.invalid/") for f in fixtures)
    combinations = {
        (f["actions"].get("action.email", "0"), f["actions"].get("action.webhook", "0"))
        for f in fixtures
    }
    assert len(combinations) >= 4

# Exercise both file generators outside Splunk and verify restart idempotency.
with tempfile.TemporaryDirectory() as temp_home:
    env = dict(os.environ, SPLUNK_HOME=temp_home)
    for script in ("seed_logs.py", "seed_scenario_logs.py"):
        command = [sys.executable, str(ROOT / "scripts" / script)]
        subprocess.run(command, env=env, check=True, capture_output=True, text=True)
        before = {p: p.read_bytes() for p in (Path(temp_home) / "var/poc_seed").rglob("*.log")}
        subprocess.run(command, env=env, check=True, capture_output=True, text=True)
        after = {p: p.read_bytes() for p in (Path(temp_home) / "var/poc_seed").rglob("*.log")}
        assert before == after, f"{script} should not rewrite generated logs on restart"
    assert len((Path(temp_home) / "var/poc_seed/sample.log").read_text().splitlines()) == 200
    scenario_files = list((Path(temp_home) / "var/poc_seed/scenarios").glob("*/events.log"))
    assert len(scenario_files) == 3
    assert all(len(path.read_text().splitlines()) == 90 for path in scenario_files)
print(f"Validated JSON configs, {len(users) * alerts['fixturesPerUser']} private alert fixtures, and scenario data")
