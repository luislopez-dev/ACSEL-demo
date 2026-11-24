-- ============================================================================
-- 02_datos_semilla.sql
-- Catálogos iniciales para pruebas rápidas
-- ============================================================================

INSERT INTO productos (id_producto, codigo, nombre, tasa_anual_pct, pct_impuesto, pct_deducible)
VALUES (seq_productos.NEXTVAL, 'AUTO-IND', 'Seguro Todo Riesgo Automóvil', 3.50, 13.00, 10.00);

INSERT INTO productos (id_producto, codigo, nombre, tasa_anual_pct, pct_impuesto, pct_deducible)
VALUES (seq_productos.NEXTVAL, 'HOGAR-IND', 'Seguro Incendio y Hogar', 1.20, 13.00, 5.00);

INSERT INTO terceros (id_tercero, documento_fiscal, nombre_completo, correo)
VALUES (seq_terceros.NEXTVAL, '0614-180588-102-4', 'Carlos Alberto Méndez', 'cmendez@ejemplo.com');

INSERT INTO terceros (id_tercero, documento_fiscal, nombre_completo, correo)
VALUES (seq_terceros.NEXTVAL, '0614-091192-105-1', 'Distribuidora Central S.A.', 'seguros@dicentral.com');

INSERT INTO terceros (id_tercero, documento_fiscal, nombre_completo, correo)
VALUES (seq_terceros.NEXTVAL, '0501-240390-008-9', 'Sofía Elena Valladares', 'svalladares@ejemplo.com');

COMMIT;
