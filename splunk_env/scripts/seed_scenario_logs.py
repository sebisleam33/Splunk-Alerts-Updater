#!/usr/bin/env python3
import datetime
import os
from pathlib import Path

from config_loader import load_config

home = Path(os.environ.get("SPLUNK_HOME", "/opt/splunk"))
base = home / "var" / "poc_seed" / "scenarios"
config = load_config("seed-data.json")
count = int(config["scenarioCount"])
now = datetime.datetime.now(datetime.timezone.utc)

for group, rows in config["scenarios"].items():
    out_dir = base / group
    out_file = out_dir / "events.log"
    if out_file.exists():
        print(f"scenario seed: {out_file} already exists; skipping")
        continue
    out_dir.mkdir(parents=True, exist_ok=True)
    lines = []
    for i in range(count):
        level, service, event, status, duration = rows[i % len(rows)]
        ts = now - datetime.timedelta(seconds=(count - i))
        lines.append(f'{ts:%Y-%m-%dT%H:%M:%SZ} level={level} service={service} '
                     f'event="{event}" user=user{i % 7} status={status} duration_ms={duration} '
                     f'transaction_id=tx-{group}-{i:04d}')
    tmp = out_file.with_suffix(".tmp")
    tmp.write_text("\n".join(lines) + "\n", encoding="utf-8")
    tmp.replace(out_file)
    print(f"scenario seed: wrote {len(lines)} events to {out_file}")
