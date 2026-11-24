-- ============================================================================
-- 06_pkg_siniestros.pkb
-- Cuerpo del paquete de Siniestros y Deducibles
-- ============================================================================
CREATE OR REPLACE PACKAGE BODY pkg_siniestros AS

    PROCEDURE pr_registrar_siniestro (
        p_id_poliza        IN  NUMBER,
        p_fecha_ocurrencia IN  DATE,
        p_descripcion      IN  VARCHAR2,
        p_monto_reclamado  IN  NUMBER,
        p_id_siniestro     OUT NUMBER,
        p_numero_siniestro OUT VARCHAR2
    ) IS
        v_estado          polizas.estado%TYPE;
        v_fecha_desde     polizas.fecha_desde%TYPE;
        v_fecha_hasta     polizas.fecha_hasta%TYPE;
        v_suma_asegurada  polizas.suma_asegurada%TYPE;
        v_pct_deducible   productos.pct_deducible%TYPE;
        v_recibos_pagados NUMBER;
        v_monto_reclamo   NUMBER(14,2);
        v_deducible       NUMBER(14,2);
        v_indemnizar      NUMBER(14,2);
    BEGIN
        IF TRIM(p_descripcion) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20010, 'Debe indicar la descripción del siniestro.');
        END IF;

        BEGIN
            SELECT p.estado,
                   p.fecha_desde,
                   p.fecha_hasta,
                   p.suma_asegurada,
                   pr.pct_deducible
              INTO v_estado,
                   v_fecha_desde,
                   v_fecha_hasta,
                   v_suma_asegurada,
                   v_pct_deducible
              FROM polizas p
              JOIN productos pr ON pr.id_producto = p.id_producto
             WHERE p.id_poliza = p_id_poliza;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20010, 'La póliza indicada no existe.');
        END;

        IF v_estado <> 'VIGENTE' THEN
            RAISE_APPLICATION_ERROR(-20010, 'Siniestro rechazado: la póliza no se encuentra VIGENTE.');
        END IF;

        IF p_fecha_ocurrencia IS NULL
           OR TRUNC(p_fecha_ocurrencia) < TRUNC(v_fecha_desde)
           OR TRUNC(p_fecha_ocurrencia) > TRUNC(v_fecha_hasta) THEN
            RAISE_APPLICATION_ERROR(-20011, 'Siniestro rechazado: la fecha de ocurrencia está fuera de vigencia.');
        END IF;

        -- Regla de mora: al menos un recibo debe estar COBRADO
        SELECT COUNT(*)
          INTO v_recibos_pagados
          FROM recibos
         WHERE id_poliza = p_id_poliza
           AND estado = 'COBRADO';

        IF v_recibos_pagados = 0 THEN
            RAISE_APPLICATION_ERROR(-20012, 'Siniestro improcedente por mora: la póliza no registra ningún recibo cobrado.');
        END IF;

        v_monto_reclamo := ROUND(p_monto_reclamado, 2);

        IF v_monto_reclamo IS NULL OR v_monto_reclamo <= 0 OR v_monto_reclamo > v_suma_asegurada THEN
            RAISE_APPLICATION_ERROR(-20013, 'El monto reclamado debe ser mayor a cero y no superar la suma asegurada.');
        END IF;

        -- Cálculo automático de deducible parametrizado en PRODUCTOS
        v_deducible  := ROUND(v_monto_reclamo * (v_pct_deducible / 100), 2);
        v_indemnizar := v_monto_reclamo - v_deducible;

        p_id_siniestro     := seq_siniestros.NEXTVAL;
        p_numero_siniestro := 'SIN-' || TO_CHAR(p_fecha_ocurrencia, 'YYYY') || '-' || LPAD(seq_folio_siniestro.NEXTVAL, 5, '0');

        INSERT INTO siniestros (
            id_siniestro, numero_siniestro, id_poliza, fecha_ocurrencia,
            descripcion, monto_reclamado, monto_deducible, monto_indemnizar, estado
        ) VALUES (
            p_id_siniestro, p_numero_siniestro, p_id_poliza, TRUNC(p_fecha_ocurrencia),
            TRIM(p_descripcion), v_monto_reclamo, v_deducible, v_indemnizar, 'REGISTRADO'
        );
    END pr_registrar_siniestro;

    PROCEDURE pr_pagar_siniestro (
        p_id_siniestro IN NUMBER
    ) IS
        v_estado siniestros.estado%TYPE;
    BEGIN
        BEGIN
            SELECT estado
              INTO v_estado
              FROM siniestros
             WHERE id_siniestro = p_id_siniestro
               FOR UPDATE NOWAIT;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20014, 'El siniestro indicado no existe.');
        END;

        IF v_estado <> 'REGISTRADO' THEN
            RAISE_APPLICATION_ERROR(-20014, 'El siniestro ya fue pagado previamente.');
        END IF;

        UPDATE siniestros
           SET estado     = 'PAGADO',
               fecha_pago = SYSDATE
         WHERE id_siniestro = p_id_siniestro;
    END pr_pagar_siniestro;

END pkg_siniestros;
/
