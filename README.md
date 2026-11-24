# Sistema de  Aseguradoras Inspirado en ACSEL

> **Nota de autoría:** Implementación con fines academicos. No contiene código ni objetos propietarios de Consis International ni de ninguna compañía aseguradora.

---

## Demo

[screen-capture.webm](https://github.com/user-attachments/assets/7e09f61d-fd21-47b5-a66a-a35fd60cf017)


## Qué demuestra este proyecto

1. **Lógica de negocio centralizada en Packages Oracle PL/SQL (`.pks` / `.pkb`)**:
   - .NET **no** ejecuta `INSERT`, `UPDATE` ni cálculos de primas o deducibles en C#. Todas las reglas viven en `PKG_POLIZAS` y `PKG_SINIESTROS`.
2. **Tarificación Parametrizada (`PRODUCTOS`)**:
   - Las tasas anuales, porcentajes de impuesto y deducibles se leen dinámicamente desde tabla (`PKG_POLIZAS.PR_EMITIR_POLIZA`).
3. **Reglas reales del negocio asegurador en PL/SQL**:
   - **Emisión y Fraccionamiento (`PKG_POLIZAS`)**: Emite pólizas y genera cuotas en `RECIBOS` (`1, 2, 4 o 12`), ajustando centavos en la última cuota para cuadrar exacto con la prima total.
   - **Cobranza con bloqueo pesimista**: `SELECT ... FOR UPDATE NOWAIT` al cobrar un recibo.
   - **Siniestros con validación de mora y deducible (`PKG_SINIESTROS`)**: Rechaza reclamos con `RAISE_APPLICATION_ERROR(-20012)` si la póliza no tiene al menos un recibo `COBRADO`, valida vigencia (`-20011`) y calcula automáticamente el deducible del producto.
4. **Arquitectura MVC + Services ADO.NET (`ODP.NET`)**:
   - Flujo directo: `Views -> Controllers -> Services (ADO.NET) -> Packages PL/SQL`.
   - Paginación real en servidor Oracle (`OFFSET ... ROWS FETCH NEXT ... ROWS ONLY` + `p_total_registros OUT`).
   - Lectura de múltiples `SYS_REFCURSOR` en una sola llamada (`PR_OBTENER_POLIZA`).
   - Traducción de errores `ORA-20xxx` a `OracleBusinessRuleException`.
5. **Pruebas de Integración con `Testcontainers.Oracle`**:
   - Levanta un contenedor limpio de **Oracle 23ai Free**, compila todos los scripts `.sql`/`.pks`/`.pkb`, verifica `USER_ERRORS = 0` y audita cuotas, deducibles, paginación y rechazos `ORA-20xxx`.

---

## Cómo ejecutarlo localmente

1. **Configurar variables de entorno para Docker:**
   ```bash
   cp .env.example .env
   ```
   *(Edita `.env` con las contraseñas que prefieras para Oracle).*

2. **Levantar Oracle en Docker:**
   ```bash
   docker compose up -d oracle-core
   ```

3. **Configurar la cadena de conexión en User Secrets (fuera del repositorio):**
   ```bash
   dotnet user-secrets set "ConnectionStrings:OracleCore" "Data Source=localhost:1522/FREEPDB1;User Id=acsel_demo;Password=TU_CLAVE_APP;Pooling=true;" --project AcselDemo/AcselDemo.csproj
   ```

4. **Ejecutar la aplicación MVC:**
   ```bash
   dotnet run --project AcselDemo/AcselDemo.csproj
   ```

5. **Ejecutar las pruebas de integración (`Testcontainers.Oracle`):**
   ```bash
   dotnet test
   ```

