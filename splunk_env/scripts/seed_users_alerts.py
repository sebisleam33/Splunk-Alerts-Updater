#!/usr/bin/env python3
"""Seed runtime users and private, disabled saved-search fixtures via Splunk REST."""
import base64
import os
import ssl
import time
from urllib.error import HTTPError, URLError
from urllib.parse import quote, urlencode
from urllib.request import Request, urlopen

from config_loader import load_config

BASE = "https://localhost:8089"
ADMIN = "admin"
ADMIN_PASSWORD = os.environ.get("SPLUNK_PASSWORD", "")
TLS = ssl._create_unverified_context()  # only for the generated local development certificate
USER_CONFIG = load_config("users.json")["users"]
ALERT_CONFIG = load_config("alerts.json")


def call(method, path, data=None, username=None, password=None):
    username, password = username or ADMIN, password or ADMIN_PASSWORD
    if not password:
        raise RuntimeError("SPLUNK_PASSWORD is missing from the container environment")
    body = urlencode(data or {}, doseq=True).encode() if data is not None else None
    token = base64.b64encode(f"{username}:{password}".encode()).decode()
    req = Request(BASE + path, data=body, method=method, headers={
        "Authorization": "Basic " + token,
        "Content-Type": "application/x-www-form-urlencoded",
        "Accept": "application/json",
    })
    try:
        with urlopen(req, context=TLS, timeout=30) as response:
            return response.status
    except HTTPError as exc:
        detail = exc.read().decode("utf-8", "replace")[:800]
        raise RuntimeError(f"{method} {path}: HTTP {exc.code}: {detail}") from exc


def exists(path):
    try:
        call("GET", path + "?output_mode=json")
        return True
    except RuntimeError as exc:
        if "HTTP 404" in str(exc):
            return False
        raise


def create_user(user, password):
    path = "/services/authentication/users/" + quote(user["name"], safe="")
    if exists(path):
        print(f"seed: {user['name']} already exists; keeping existing password")
        return
    call("POST", "/services/authentication/users", {
        "name": user["name"], "password": password, "roles": user["role"],
        "realname": user["realName"],
    })
    print(f"seed: created user {user['name']}")


def make_fixture(user, sequence):
    """Build one fixture from config/data/alerts.json."""
    cfg = ALERT_CONFIG
    profile = cfg["actionProfiles"][(sequence - 1) % len(cfg["actionProfiles"])]
    name = f"{user['name'].upper()} - Page Test {sequence:03d} - {profile['label']}"
    webhook_url = f"https://example.invalid/hooks/{user['name']}/{sequence:03d}"
    actions = {
        "action.email": "0", "action.webhook": "0",
        # Keep the URL field present on every saved search: Splunk validates this
        # custom action field even when the action is disabled.
        "action.webhook.param.url": webhook_url,
    }
    email_mode = profile["email"]
    email_on = email_mode is True or email_mode == "configured-disabled"
    webhook_mode = profile["webhook"]
    webhook_on = webhook_mode is True or webhook_mode == "configured-disabled"
    if email_on:
        actions.update({
            "action.email": "0" if email_mode == "configured-disabled" else "1",
            "action.email.to": f"{user['name']}@example.invalid",
            "action.email.from": "splunk-test@example.invalid",
            "action.email.subject": f"{user['name'].title()} fixture {sequence:03d}",
            "action.email.maxresults": str(sequence % 50 + 1),
            "action.email.include.results_link": str(sequence % 2),
            "action.email.include.search": str((sequence // 2) % 2),
            "action.email.include.trigger": "1",
            "action.email.include.trigger_time": str((sequence // 3) % 2),
            "action.email.include.view_link": str((sequence // 4) % 2),
            "action.email.inline": str(sequence % 2),
            "action.email.sendcsv": str((sequence // 2) % 2),
            "action.email.sendresults": "1",
            "action.email.sendpdf": str((sequence // 3) % 2),
            "action.email.sendpng": str((sequence // 4) % 2),
            "action.email.allow_empty_attachment": str((sequence // 5) % 2),
        })
    if webhook_on:
        actions["action.webhook"] = "0" if webhook_mode == "configured-disabled" else "1"
        actions["action.webhook.param.post_key"] = f"{user['name']}-fixture-{sequence:03d}"

    return {
        "app": cfg["app"], "name": name,
        "search": f"index={cfg['indexes'][(sequence - 1) % len(cfg['indexes'])]} "
                  f"{cfg['conditions'][(sequence - 1) % len(cfg['conditions'])]}",
        "actions": actions,
        "cron_schedule": cfg["schedules"][(sequence - 1) % len(cfg["schedules"])],
        "earliest_time": cfg["earliestTimes"][(sequence - 1) % len(cfg["earliestTimes"])],
        "sharing": "user",
    }


def saved_search(user, password, fixture, sequence):
    owner = quote(user["name"], safe="")
    app = quote(fixture["app"], safe="")
    name = quote(fixture["name"], safe="")
    path = f"/servicesNS/{owner}/{app}/saved/searches/{name}"
    cfg = ALERT_CONFIG
    fields = {
        "name": fixture["name"], "search": fixture["search"],
        "is_scheduled": "1", "disabled": "1" if cfg["disableExecution"] else "0",
        "cron_schedule": fixture["cron_schedule"],
        "dispatch.earliest_time": fixture["earliest_time"],
        "dispatch.latest_time": "now", "alert.track": "1",
        "alert_type": "number of events", "alert_comparator": "greater than",
        "alert_threshold": str(sequence % 5), "alert.severity": str(sequence % 5 + 1),
        "alert.digest_mode": str(int(sequence % 2 == 0)),
        "alert.suppress": str(int(sequence % 6 == 0)),
        "alert.suppress.period": cfg["suppressionPeriods"][sequence % len(cfg["suppressionPeriods"])],
        "alert.suppress.fields": cfg["suppressionFields"][sequence % len(cfg["suppressionFields"])],
    }
    fields.update(fixture["actions"])
    collection = f"/servicesNS/{owner}/{app}/saved/searches"
    if exists(path):
        call("POST", path, fields, username=user["name"], password=password)
    else:
        call("POST", collection, fields, username=user["name"], password=password)
    role = user["role"]
    call("POST", path + "/acl", {
        "owner": user["name"], "sharing": "user",
        "perms.read": role, "perms.write": role,
    })


def main():
    users_passwords = []
    for user in USER_CONFIG:
        password = os.environ.get(user["passwordEnv"], "")
        if len(password) < 8 or any(ord(ch) < 32 or ord(ch) > 126 for ch in password):
            raise RuntimeError(f"{user['passwordEnv']} must have at least 8 printable ASCII characters")
        users_passwords.append((user, password))
    if not ADMIN_PASSWORD:
        raise RuntimeError("Set SPLUNK_PASSWORD at docker run")

    for attempt in range(90):
        try:
            call("GET", "/services/server/info?output_mode=json")
            break
        except (RuntimeError, URLError):
            if attempt == 89:
                raise
            time.sleep(5)

    for user, password in users_passwords:
        create_user(user, password)
    count = int(ALERT_CONFIG["fixturesPerUser"])
    for user, password in users_passwords:
        for sequence in range(1, count + 1):
            fixture = make_fixture(user, sequence)
            saved_search(user, password, fixture, sequence)
            if sequence % 25 == 0:
                print(f"seed: {user['name']} {sequence}/{count} private alerts in app={ALERT_CONFIG['app']}")
    total = len(users_passwords) * count
    print(f"seed: completed {len(users_passwords)} users and {total} private alerts in app={ALERT_CONFIG['app']}")
    print("seed: alerts are disabled; email recipients and webhook URLs use example.invalid")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"seed: FAILED: {exc}")
        raise
