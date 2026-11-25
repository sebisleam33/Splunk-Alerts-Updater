# Splunk Alert Updater — local test environment

This repository includes a Docker-based Splunk lab for developing and testing the Splunk Alert Updater. It provides sample indexes, Alice and Bob accounts, and 240 saved-search alerts to exercise search, filtering, pagination, ownership, and email/webhook action updates.

## What the lab contains

- Two test users: `alice` and `bob`, each with 120 private alerts.
- All generated alerts are in Splunk's built-in **Search & Reporting** app (`app=search`).
- The alerts are scheduled but disabled, so Splunk will not run them or send email/webhook notifications.
- Email and webhook fields contain non-routable `example.invalid` values for configuration testing.
- Sample events are loaded into `poc`, `payments_test`, `security_test`, and `platform_test` indexes.
- The hidden `poc_seed` app contains only the bootstrap scripts and index/input configuration; it does not own the alert fixtures.

`app=search` identifies the app namespace, not the owner. Alice and Bob each own their private alerts, so they should sign in as their own accounts to test access. Admin can see both users' alerts.

## Prerequisites

- Windows with PowerShell.
- Docker Desktop or Rancher Desktop running Linux containers.
- Python 3 only if you want to run the optional offline checks before building.

## Build and start Splunk

Open PowerShell in the `splunk-test-env` folder, where this README and the Dockerfile are located. If this folder is at the repository root, first run `cd .\splunk-test-env`.

Run the offline checks (optional):

```powershell
python .\tests\validate_configs.py
```

Build the image:

```powershell
docker build -t poc-splunk .
```

If a container with this name is already running, remove it before recreating it:

```powershell
docker rm -f poc-splunk
```

Start a fresh container. Replace these local demo passwords with your own values; each must contain at least 8 printable ASCII characters.

```powershell
docker run -d --name poc-splunk `
  -e SPLUNK_PASSWORD="Admin123!" `
  -e ALICE_PASSWORD="Alice123!" `
  -e BOB_PASSWORD="Bob123!!" `
  -p 8000:8000 -p 8088:8088 -p 8089:8089 `
  poc-splunk
```

## If it fails rerun seeding manually

Startup runs the user and alert seed automatically. If seeding failed, it can be rerun using the same passwords supplied to `docker run`:

```powershell
docker exec --user splunk `
  -e SPLUNK_PASSWORD="Admin123!" `
  -e ALICE_PASSWORD="Alice123!" `
  -e BOB_PASSWORD="Bob123!!" `
  poc-splunk /opt/splunk/bin/python3 /opt/splunk/etc/apps/poc_seed/bin/seed_users_alerts.py
```

The container starts Splunk, generates sample events through scripted inputs, then creates users and alerts automatically. Follow startup with:

```powershell
docker logs -f poc-splunk
```

Wait for Splunk to start and for the seed script to report completion. Press `Ctrl+C` to stop following the logs; the container keeps running.

## Sign in and use the lab

Open [http://localhost:8000](http://localhost:8000).

| Splunk user | Password |
| --- | --- |
| `admin` | Value passed as `SPLUNK_PASSWORD` |
| `alice` | Value passed as `ALICE_PASSWORD` |
| `bob` | Value passed as `BOB_PASSWORD` |

To test the updater against the lab, connect it to the Splunk management API at `https://localhost:8089`. Use app `search` and select owner `alice` or `bob` to test private visibility and ownership. Use `admin` when you need to inspect both users' alerts.

To check that sample events were indexed, run searches such as:

```spl
index=poc
index=payments_test
index=security_test
index=platform_test
```

The app namespace is separate from the event index: saved searches are in `app=search`, while their SPL can query any of the configured indexes.

## Local HTTPS certificate (optional)

The Splunk management API uses a locally generated certificate. If the updater validates certificates, add the lab CA to the current Windows user's trusted roots:

```powershell
docker cp poc-splunk:/opt/splunk/etc/auth/poc-ca.crt .\poc-ca.crt
certutil -user -addstore Root .\poc-ca.crt
```

The certificate is for local development and covers `localhost` and `127.0.0.1`.

## Change the test data

| File | Purpose |
| --- | --- |
| `config/data/users.json` | Test usernames, roles, and password environment-variable names |
| `config/data/alerts.json` | Alert count, indexes, SPL conditions, schedules, action profiles, and execution state |
| `config/data/seed-data.json` | Event counts and scenario event rows |
| `config/splunk/*.conf` | Splunk server TLS and test roles |
| `config/app/default/*.conf` | Indexes, scripted/monitor inputs, parsing, and hidden seed-app settings |
| `scripts/` | JSON loading, event generation, REST seeding, and container startup |

After changing configuration or scripts, run the offline checks and rebuild the image. Recreate the container to use the new image. The scripts leave existing users' passwords unchanged and skip log files that were already generated.

Do not commit real passwords. Supply them at container startup using environment variables as shown above.

## Stop or reset the lab

Stop the running environment and keep its container:

```powershell
docker stop poc-splunk
```

Remove only the container:

```powershell
docker rm -f poc-splunk
```

For a fully clean lab, remove the container and its attached volumes. **This deletes the Splunk data stored in those volumes:**

```powershell
docker rm -fv poc-splunk
```

## Troubleshooting

- **Splunk is still starting:** use `docker logs -f poc-splunk` and wait for the startup and seed messages.
- **Alice or Bob cannot log in:** check the seed output in the container logs and confirm that both password environment variables were supplied at `docker run`.
- **A changed script/config does not appear:** rebuild the image and recreate the container; editing files alone does not change an existing image.
- **A port is already in use:** stop the other container or change the host-side port mapping, for example `-p 18000:8000` for Splunk Web.
- **Offline check fails:** run `python .\tests\validate_configs.py` from `splunk-test-env` and inspect the first reported assertion/error.
