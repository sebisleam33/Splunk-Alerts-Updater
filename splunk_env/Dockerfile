# syntax=docker/dockerfile:1
# Thin image definition. Editable test data and Splunk settings live under config/.
FROM alpine:3.24 AS tls-certs
RUN apk add --no-cache openssl
RUN set -eu; \
    mkdir -p /tls/poc-tls; \
    openssl genrsa -out /tmp/poc-ca.key 4096; \
    openssl req -x509 -new -sha256 -days 3650 -key /tmp/poc-ca.key -out /tls/poc-ca.crt \
      -subj "/CN=Splunk POC Local Development CA" \
      -addext "basicConstraints=critical,CA:TRUE" \
      -addext "keyUsage=critical,keyCertSign,cRLSign"; \
    openssl genrsa -out /tls/poc-tls/localhost.key 2048; \
    openssl req -new -sha256 -key /tls/poc-tls/localhost.key -out /tmp/localhost.csr -subj "/CN=localhost"; \
    printf '%s\n' 'basicConstraints=critical,CA:FALSE' 'keyUsage=critical,digitalSignature,keyEncipherment' \
      'extendedKeyUsage=serverAuth' 'subjectAltName=DNS:localhost,IP:127.0.0.1' > /tmp/localhost.ext; \
    openssl x509 -req -sha256 -days 825 -in /tmp/localhost.csr -CA /tls/poc-ca.crt \
      -CAkey /tmp/poc-ca.key -CAcreateserial -extfile /tmp/localhost.ext -out /tls/poc-tls/localhost.crt; \
    cat /tls/poc-tls/localhost.crt /tls/poc-tls/localhost.key > /tls/poc-tls/localhost.pem; \
    rm -f /tmp/poc-ca.key /tmp/localhost.csr /tmp/localhost.ext /tls/poc-tls/localhost.key

FROM splunk/splunk:latest
ENV SPLUNK_START_ARGS=--accept-license \
    SPLUNK_GENERAL_TERMS=--accept-sgt-current-at-splunk-com

COPY --from=tls-certs --chown=splunk:splunk /tls/poc-ca.crt /opt/splunk-etc/auth/poc-ca.crt
COPY --from=tls-certs --chown=splunk:splunk /tls/poc-tls/localhost.pem /opt/splunk-etc/auth/poc-tls/localhost.pem

# Splunk-native configuration remains in its native .conf formats.
COPY --chown=splunk:splunk config/splunk/server.conf /opt/splunk-etc/system/local/server.conf
COPY --chown=splunk:splunk config/splunk/authorize.conf /opt/splunk-etc/system/local/authorize.conf
COPY --chown=splunk:splunk config/app/default/ /opt/splunk-etc/apps/poc_seed/default/
COPY --chown=splunk:splunk config/app/metadata/ /opt/splunk-etc/apps/poc_seed/metadata/

# JSON is for data and fixture definitions that are convenient to edit independently.
COPY --chown=splunk:splunk config/data/ /opt/splunk-etc/apps/poc_seed/config/
COPY --chown=splunk:splunk scripts/ /opt/splunk-etc/apps/poc_seed/bin/
COPY --chown=root:root scripts/poc-entrypoint.sh /usr/local/bin/poc-entrypoint.sh

EXPOSE 8000 8088 8089
USER root
# Normalize CRLF copied from Windows and enforce readable permissions for Splunk.
RUN find /opt/splunk-etc/apps/poc_seed -type f -exec sed -i 's/\r$//' {} + \
 && sed -i 's/\r$//' /usr/local/bin/poc-entrypoint.sh \
 && chmod 755 /usr/local/bin/poc-entrypoint.sh \
 && chown -R splunk:splunk /opt/splunk-etc/apps/poc_seed \
 && test -x /usr/local/bin/poc-entrypoint.sh \
 && python3 -m py_compile /opt/splunk-etc/apps/poc_seed/bin/*.py

ENTRYPOINT ["/usr/local/bin/poc-entrypoint.sh"]
USER ansible
