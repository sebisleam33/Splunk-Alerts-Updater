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
    # Splunk's Ansible provisioning copies files from /opt/splunk-etc into
    # $SPLUNK_HOME/etc. Wait for that copy to finish before starting the seed.
    # The REST seed script has its own readiness retry once the file is present.
    for attempt in {1..120}; do
        [[ -f "$SEED_SCRIPT" ]] && break
        sleep 5
    done

    if [[ ! -f "$SEED_SCRIPT" ]]; then
        echo "poc-entrypoint: seed script was not provisioned at $SEED_SCRIPT"
        echo "poc-entrypoint: check that config/app/poc_seed was copied into /opt/splunk-etc/apps/poc_seed"
    else
        # Splunk bundles OpenSSL in $SPLUNK_HOME/lib. Its Python SSL extension
        # needs this path when Python is launched directly from the entrypoint.
        SPLUNK_LIB_DIR="${SPLUNK_HOME:-/opt/splunk}/lib"
        LD_LIBRARY_PATH="${SPLUNK_LIB_DIR}${LD_LIBRARY_PATH:+:${LD_LIBRARY_PATH}}" \
          /opt/splunk/bin/python3 "$SEED_SCRIPT" \
          || echo "poc-entrypoint: seed failed; see error above"
    fi
else
    echo "poc-entrypoint: ALICE_PASSWORD and BOB_PASSWORD are required to seed test users"
fi
wait "$SPLUNK_ENTRYPOINT_PID"