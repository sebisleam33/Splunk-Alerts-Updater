#!/usr/bin/env python3
"""Load JSON settings shipped in the hidden poc_seed app."""
import json
import os
from pathlib import Path


def config_path(name):
    override = os.environ.get("POC_CONFIG_DIR")
    if override:
        return Path(override) / name
    home = Path(os.environ.get("SPLUNK_HOME", "/opt/splunk"))
    return home / "etc" / "apps" / "poc_seed" / "config" / name


def load_config(name):
    with config_path(name).open(encoding="utf-8") as stream:
        return json.load(stream)
