#!/bin/bash
set -u

/sbin/entrypoint.sh start-service &
SPLUNK_ENTRYPOINT_PID=$!
stop_splunk() {
    kill -TERM "$SPLUNK_ENTRYPOINT_PID" 2>/dev/null || true
    wait "$SPLUNK_ENTRYPOINT_PID" 2>/dev/null || true
}
trap stop_splunk TERM INT

SEED_SCRIPT="${SPLUNK_HOME:-/opt/splunk}/etc/apps/poc_seed/bin/seed_users_alerts.py"
if [[ -n "${ALICE_PASSWORD:-}" && -n "${BOB_PASSWORD:-}" ]]; then
    /opt/splunk/bin/python3 "$SEED_SCRIPT" || echo "poc-entrypoint: seed failed; see error above"
else
    echo "poc-entrypoint: ALICE_PASSWORD and BOB_PASSWORD are required to seed test users"
fi
wait "$SPLUNK_ENTRYPOINT_PID"
