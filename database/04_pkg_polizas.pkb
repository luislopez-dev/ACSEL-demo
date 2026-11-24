-- ============================================================================
-- 04_pkg_polizas.pkb
-- Cuerpo del paquete de Pólizas, Tarificación y Recibos
-- ============================================================================
CREATE OR REPLACE PACKAGE BODY pkg_polizas AS

    PROCEDURE pr_listar_productos (
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_producto,
                   codigo,
                   nombre,
                   tasa_anual_pct,
                   pct_impuesto,
                   pct_deducible
              FROM productos
             ORDER BY id_producto;
    END pr_listar_productos;

    PROCEDURE pr_listar_terceros (
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_tercero,
                   documento_fiscal,
                   nombre_completo,
                   correo
              FROM terceros
             ORDER BY nombre_completo;
    END pr_listar_terceros;

    PROCEDURE pr_listar_polizas (
        p_pagina          IN  NUMBER DEFAULT 1,
        p_tamano_pagina   IN  NUMBER DEFAULT 10,
        p_total_registros OUT NUMBER,
        p_cursor          OUT SYS_REFCURSOR
    ) IS
        v_pagina NUMBER := GREATEST(NVL(TRUNC(p_pagina), 1), 1);
        v_tamano NUMBER := LEAST(GREATEST(NVL(TRUNC(p_tamano_pagina), 10), 1), 100);
        v_offset NUMBER := (v_pagina - 1) * v_tamano;
    BEGIN
        SELECT COUNT(*)
          INTO p_total_registros
          FROM polizas;

        OPEN p_cursor FOR
            SELECT p.id_poliza,
                   p.numero_poliza,
                   pr.codigo       AS codigo_producto,
                   pr.nombre       AS nombre_producto,
                   t.nombre_completo AS nombre_asegurado,
                   p.descripcion_bien,
                   p.fecha_desde,
                   p.fecha_hasta,
                   p.estado,
                   p.suma_asegurada,
                   p.prima_total
              FROM polizas p
              JOIN productos pr ON pr.id_producto = p.id_producto
              JOIN terceros t   ON t.id_tercero = p.id_tercero
             ORDER BY p.id_poliza DESC
            OFFSET v_offset ROWS FETCH NEXT v_tamano ROWS ONLY;
    END pr_listar_polizas;

    PROCEDURE pr_obtener_poliza (
        p_id_poliza      IN  NUMBER,
        p_cur_poliza     OUT SYS_REFCURSOR,
        p_cur_recibos    OUT SYS_REFCURSOR,
        p_cur_siniestros OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cur_poliza FOR
            SELECT p.id_poliza,
                   p.numero_poliza,
                   pr.codigo       AS codigo_producto,
                   pr.nombre       AS nombre_producto,
                   pr.tasa_anual_pct,
                   pr.pct_impuesto,
                   pr.pct_deducible,
                   t.documento_fiscal,
                   t.nombre_completo AS nombre_asegurado,
                   p.descripcion_bien,
                   p.fecha_desde,
                   p.fecha_hasta,
                   p.estado,
                   p.cantidad_cuotas,
                   p.suma_asegurada,
                   p.prima_neta,
                   p.monto_impuesto,
                   p.prima_total
              FROM polizas p
              JOIN productos pr ON pr.id_producto = p.id_producto
              JOIN terceros t   ON t.id_tercero = p.id_tercero
             WHERE p.id_poliza = p_id_poliza;

        OPEN p_cur_recibos FOR
            SELECT id_recibo,
                   id_poliza,
                   numero_recibo,
                   numero_cuota,
                   fecha_vencimiento,
                   monto_total,
                   estado,
                   fecha_cobro,
                   referencia_pago
              FROM recibos
             WHERE id_poliza = p_id_poliza
             ORDER BY numero_cuota;

        OPEN p_cur_siniestros FOR
            SELECT id_siniestro,
                   numero_siniestro,
                   id_poliza,
                   fecha_ocurrencia,
                   descripcion,
                   monto_reclamado,
                   monto_deducible,
                   monto_indemnizar,
                   estado,
                   fecha_pago
              FROM siniestros
             WHERE id_poliza = p_id_poliza
             ORDER BY id_siniestro DESC;
    END pr_obtener_poliza;

    PROCEDURE pr_emitir_poliza (
        p_id_producto      IN  NUMBER,
        p_id_tercero       IN  NUMBER,
        p_descripcion_bien IN  VARCHAR2,
        p_fecha_desde      IN  DATE,
        p_fecha_hasta      IN  DATE,
        p_cantidad_cuotas  IN  NUMBER,
        p_suma_asegurada   IN  NUMBER,
        p_id_poliza        OUT NUMBER,
        p_numero_poliza    OUT VARCHAR2
    ) IS
        v_tasa_pct     productos.tasa_anual_pct%TYPE;
        v_pct_impuesto productos.pct_impuesto%TYPE;
        v_prima_neta   NUMBER(14,2);
        v_impuesto     NUMBER(14,2);
        v_prima_total  NUMBER(14,2);
        v_meses_salto  NUMBER;
        v_cuota_monto  NUMBER(14,2);
        v_acumulado    NUMBER(14,2) := 0;
    BEGIN
        IF p_suma_asegurada IS NULL OR p_suma_asegurada <= 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'La suma asegurada debe ser mayor a cero.');
        END IF;

        IF p_fecha_desde IS NULL OR p_fecha_hasta IS NULL OR TRUNC(p_fecha_hasta) <= TRUNC(p_fecha_desde) THEN
            RAISE_APPLICATION_ERROR(-20002, 'La fecha fin de vigencia debe ser posterior a la fecha inicio.');
        END IF;

        IF p_cantidad_cuotas NOT IN (1, 2, 4, 12) THEN
            RAISE_APPLICATION_ERROR(-20003, 'Solo se permiten 1, 2, 4 o 12 cuotas.');
        END IF;

        IF TRIM(p_descripcion_bien) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20004, 'Debe indicar la descripción del bien asegurado.');
        END IF;

        -- 1. Leer tarifa parametrizada desde la tabla PRODUCTOS
        BEGIN
            SELECT tasa_anual_pct, pct_impuesto
              INTO v_tasa_pct, v_pct_impuesto
              FROM productos
             WHERE id_producto = p_id_producto;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20005, 'El producto seleccionado no existe.');
        END;

        v_prima_neta  := ROUND(p_suma_asegurada * (v_tasa_pct / 100), 2);
        v_impuesto    := ROUND(v_prima_neta * (v_pct_impuesto / 100), 2);
        v_prima_total := v_prima_neta + v_impuesto;

        -- 2. Insertar póliza
        p_id_poliza     := seq_polizas.NEXTVAL;
        p_numero_poliza := 'POL-' || TO_CHAR(p_fecha_desde, 'YYYY') || '-' || LPAD(seq_folio_poliza.NEXTVAL, 5, '0');

        INSERT INTO polizas (
            id_poliza, numero_poliza, id_producto, id_tercero,
            descripcion_bien, fecha_desde, fecha_hasta, estado,
            cantidad_cuotas, suma_asegurada, prima_neta, monto_impuesto, prima_total
        ) VALUES (
            p_id_poliza, p_numero_poliza, p_id_producto, p_id_tercero,
            TRIM(p_descripcion_bien), TRUNC(p_fecha_desde), TRUNC(p_fecha_hasta), 'VIGENTE',
            p_cantidad_cuotas, ROUND(p_suma_asegurada, 2), v_prima_neta, v_impuesto, v_prima_total
        );

        -- 3. Generar cuotas en RECIBOS cuadrando centavos en la última cuota
        v_meses_salto := 12 / p_cantidad_cuotas;

        FOR i IN 1 .. p_cantidad_cuotas LOOP
            IF i < p_cantidad_cuotas THEN
                v_cuota_monto := TRUNC(v_prima_total / p_cantidad_cuotas, 2);
                v_acumulado   := v_acumulado + v_cuota_monto;
            ELSE
                v_cuota_monto := v_prima_total - v_acumulado;
            END IF;

            INSERT INTO recibos (
                id_recibo, id_poliza, numero_recibo, numero_cuota,
                fecha_vencimiento, monto_total, estado
            ) VALUES (
                seq_recibos.NEXTVAL, p_id_poliza,
                'REC-' || LPAD(seq_folio_recibo.NEXTVAL, 6, '0'),
                i, ADD_MONTHS(TRUNC(p_fecha_desde), (i - 1) * v_meses_salto),
                v_cuota_monto, 'PENDIENTE'
            );
        END LOOP;
    END pr_emitir_poliza;

    PROCEDURE pr_cobrar_recibo (
        p_id_recibo       IN NUMBER,
        p_referencia_pago IN VARCHAR2
    ) IS
        v_estado recibos.estado%TYPE;
    BEGIN
        IF TRIM(p_referencia_pago) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20006, 'Debe indicar una referencia de pago.');
        END IF;

        BEGIN
            SELECT estado
              INTO v_estado
              FROM recibos
             WHERE id_recibo = p_id_recibo
               FOR UPDATE NOWAIT;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20007, 'El recibo indicado no existe.');
        END;

        IF v_estado <> 'PENDIENTE' THEN
            RAISE_APPLICATION_ERROR(-20007, 'El recibo ya fue cobrado previamente.');
        END IF;

        UPDATE recibos
           SET estado          = 'COBRADO',
               fecha_cobro     = SYSDATE,
               referencia_pago = TRIM(p_referencia_pago)
         WHERE id_recibo = p_id_recibo;
    END pr_cobrar_recibo;

END pkg_polizas;
/
