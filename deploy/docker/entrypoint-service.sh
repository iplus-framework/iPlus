#!/usr/bin/env bash
#
# Container entrypoint for the iPlus service.
#
# Renders /app/ConnectionStrings.config from environment variables so that the
# database password never has to be baked into an image or a committed file,
# then hands over to the service binary.
#
set -euo pipefail

DB_SERVER="${IPLUS_DB_SERVER:-sqlserver}"
DB_PORT="${IPLUS_DB_PORT:-1433}"
DB_NAME="${IPLUS_DB_NAME:-iPlusV5}"
DB_USER="${IPLUS_DB_USER:-sa}"
DB_PASSWORD="${IPLUS_DB_PASSWORD:?IPLUS_DB_PASSWORD must be set}"

cat > /app/ConnectionStrings.config <<EOF
<?xml version="1.0" encoding="utf-8"?>
<connectionStrings>
  <add name="iPlusV5_Entities"
       connectionString="Integrated Security=False;
                         Encrypt=False;
                         data source=${DB_SERVER},${DB_PORT};
                         initial catalog=${DB_NAME};
                         Trusted_Connection=False;
                         persist security info=True;
                         user id=${DB_USER};
                         password=${DB_PASSWORD};
                         multipleactiveresultsets=True;
                         application name=iPlus"/>
</connectionStrings>
EOF

echo "iPlus service starting (db=${DB_SERVER},${DB_PORT}/${DB_NAME}, user=${DB_USER})"

exec /app/gip.iplus.service "$@"
