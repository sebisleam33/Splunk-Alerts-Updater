# Splunk test environment

## Prerequisites

- Docker Desktop or Rancher Desktop running with Linux containers.
- The Dockerfile `splunk_seed_data` in the current folder.

## Build and start

Run these commands in PowerShell from the folder containing the Dockerfile:

```powershell
docker build -t poc-splunk -f ".\splunk_seed_data" .
```

If a container named `poc-splunk` already exists, remove it before recreating it. This also deletes its volumes and test data:

```powershell
docker rm -fv poc-splunk
```

Start a fresh instance. Use the same password values if you later run the seed script manually. Each password must have at least 8 printable ASCII characters.

```powershell
docker run -d --name poc-splunk `
  -e SPLUNK_PASSWORD="Admin123!" `
  -e ALICE_PASSWORD="Alice123!" `
  -e BOB_PASSWORD="Bob123!!" `
  -p 8000:8000 -p 8088:8088 -p 8089:8089 poc-splunk
```

#Passwords must be 8 characters long and include special characters!!!


The image seeds the users, apps, logs, indexes, and saved-search fixtures at startup. Follow startup with:

```powershell
docker logs -f poc-splunk
```

Wait for Splunk to finish starting and for the seed script to report completion. Press `Ctrl+C` to exit log viewing; this does not stop the container.

## Trust the local SSL certificate on Windows

Copy the CA certificate from the running container:

```powershell
docker cp poc-splunk:/opt/splunk-etc/auth/poc-ca.crt .\poc-ca.crt
```

Add it to your current Windows user’s trusted root certificates:

```powershell
certutil -user -addstore Root .\poc-ca.crt
```

## Sign in and inspect

Open <http://localhost:8000>.

- Admin: `admin` and the value passed as `SPLUNK_PASSWORD`.
- Alice: `alice` and the value passed as `ALICE_PASSWORD`.
- Bob: `bob` and the value passed as `BOB_PASSWORD`.

The saved searches include private (`user`), app-shared (`app`), and global (`global`) sharing. Email and webhook action settings are present for testing in the updater. Alerts are disabled (`disabled=1`), so Splunk will not execute them or send notifications.

## Run the seed script again (optional) - if some data seed failed

The startup process runs the seed automatically. If you need to rerun it manually after startup, use the same credentials passed to `docker run`:

```powershell
docker exec --user splunk `
  -e SPLUNK_PASSWORD="Admin123!" `
  -e ALICE_PASSWORD="Alice123!" `
  -e BOB_PASSWORD="Bob123!!" `
  poc-splunk /opt/splunk/etc/apps/poc_seed/bin/seed_users_alerts.py
```

## Stop the environment

```powershell
docker stop poc-splunk
```

To remove the container and its volumes, use `docker rm -fv poc-splunk`.


