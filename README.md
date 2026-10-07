# Splunk Environment Setup

Run these commands from the folder containing `splunk_seed_v1`.

## 1. Build and start Splunk

```powershell
docker build -f splunk_seed_v1 -t poc-splunk .
```

```powershell
docker run -d --name poc-splunk `
  -p 8000:8000 `
  -p 8088:8088 `
  -p 8089:8089 `
  -e SPLUNK_PASSWORD='ChooseYourPasswordHere!' `
  poc-splunk
```

Replace `ChooseYourPasswordHere!` with the password you want to use for Splunk’s `admin` account.

## 2. Trust the local SSL certificate on Windows

Copy the CA certificate from the running container:

```powershell
docker cp poc-splunk:/opt/splunk-etc/auth/poc-ca.crt .\poc-ca.crt
```

Add it to your current Windows user’s trusted root certificates:

```powershell
certutil -user -addstore Root .\poc-ca.crt
```

Your console app should connect to Splunk using `localhost` on port `8089` over HTTPS.

## 3. Set environment variables for the console app

Before running the app, set:

| Variable | Value |
|---|---|
| `SPLUNK_USERNAME` | `admin` |
| `SPLUNK_PASSWORD` | The same password used in the `docker run` command |
