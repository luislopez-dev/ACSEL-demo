-- ============================================================================
-- 05_pkg_siniestros.pks
-- Especificación del paquete de Siniestros y Deducibles
-- ============================================================================
CREATE OR REPLACE PACKAGE pkg_siniestros AS

    PROCEDURE pr_registrar_siniestro (
        p_id_poliza        IN  NUMBER,
        p_fecha_ocurrencia IN  DATE,
        p_descripcion      IN  VARCHAR2,
        p_monto_reclamado  IN  NUMBER,
        p_id_siniestro     OUT NUMBER,
        p_numero_siniestro OUT VARCHAR2
    );

    PROCEDURE pr_pagar_siniestro (
        p_id_siniestro IN NUMBER
    );

END pkg_siniestros;
/
