-- ============================================================================
-- 03_pkg_polizas.pks
-- Especificación del paquete de Pólizas, Tarificación y Recibos
-- ============================================================================
CREATE OR REPLACE PACKAGE pkg_polizas AS

    PROCEDURE pr_listar_productos (
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE pr_listar_terceros (
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE pr_listar_polizas (
        p_pagina          IN  NUMBER DEFAULT 1,
        p_tamano_pagina   IN  NUMBER DEFAULT 10,
        p_total_registros OUT NUMBER,
        p_cursor          OUT SYS_REFCURSOR
    );

    PROCEDURE pr_obtener_poliza (
        p_id_poliza      IN  NUMBER,
        p_cur_poliza     OUT SYS_REFCURSOR,
        p_cur_recibos    OUT SYS_REFCURSOR,
        p_cur_siniestros OUT SYS_REFCURSOR
    );

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
    );

    PROCEDURE pr_cobrar_recibo (
        p_id_recibo       IN NUMBER,
        p_referencia_pago IN VARCHAR2
    );

END pkg_polizas;
/
