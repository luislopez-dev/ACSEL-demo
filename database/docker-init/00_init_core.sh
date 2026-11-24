#!/bin/bash
set -euo pipefail

sqlplus -s "${APP_USER}/${APP_USER_PASSWORD}@//localhost:1521/FREEPDB1" <<'SQL'
WHENEVER SQLERROR EXIT SQL.SQLCODE
@/opt/acsel-scripts/01_tablas.sql
@/opt/acsel-scripts/02_datos_semilla.sql
@/opt/acsel-scripts/03_pkg_polizas.pks
@/opt/acsel-scripts/04_pkg_polizas.pkb
@/opt/acsel-scripts/05_pkg_siniestros.pks
@/opt/acsel-scripts/06_pkg_siniestros.pkb
EXIT;
SQL