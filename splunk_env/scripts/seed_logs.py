#!/usr/bin/env python3
import datetime
import os
from pathlib import Path

from config_loader import load_config

ROWS = [
    ("ERROR", "auth", "alice", "login failed", 120, 401),
    ("ERROR", "payments", "bob", "timeout calling bank", 2800, 500),
    ("ERROR", "db", "carol", "db connection refused", 1500, 500),
    ("ERROR", "api", "dave", "internal server error", 900, 500),
    ("WARN", "db", "erin", "slow query", 1800, 200),
    ("WARN", "web", "frank", "high memory usage", 600, 200),
    ("WARN", "api", "alice", "retrying request", 450, 200),
    ("INFO", "auth", "bob", "login ok", 50, 200),
    ("INFO", "payments", "carol", "payment processed", 150, 200),
    ("INFO", "web", "dave", "cache hit", 10, 200),
]

home = Path(os.environ.get("SPLUNK_HOME", "/opt/splunk"))
out_file = home / "var" / "poc_seed" / "sample.log"
if out_file.exists():
    print(f"poc_seed: {out_file} already exists, skipping")
    raise SystemExit(0)

settings = load_config("seed-data.json")["sampleLogs"]
count, spread = int(settings["count"]), int(settings["spreadSeconds"])
out_file.parent.mkdir(parents=True, exist_ok=True)
now = datetime.datetime.now(datetime.timezone.utc)
events = []
for i in range(count):
    level, service, user, message, duration, status = ROWS[i % len(ROWS)]
    ts = now - datetime.timedelta(seconds=i * spread // count)
    line = (f'{ts:%Y-%m-%dT%H:%M:%SZ} level={level} service={service} user={user} '
            f'msg="{message}" duration_ms={duration} status={status}')
    events.append((ts, line))
events.sort(key=lambda item: item[0])
tmp = out_file.with_suffix(".tmp")
tmp.write_text("\n".join(line for _, line in events) + "\n", encoding="utf-8")
tmp.replace(out_file)
print(f"poc_seed: wrote {count} events to {out_file}")
