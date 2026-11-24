-- ============================================================================
-- 01_tablas.sql
-- Esquema simplificado de 5 tablas para Core Asegurador Demo
-- ============================================================================

CREATE SEQUENCE seq_productos START WITH 1 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_terceros START WITH 1 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_polizas START WITH 1 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_folio_poliza START WITH 1001 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_recibos START WITH 1 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_folio_recibo START WITH 10001 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_siniestros START WITH 1 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE seq_folio_siniestro START WITH 5001 INCREMENT BY 1 NOCACHE;

-- 1. Productos configurables (tasa, impuesto y deducible parametrizados)
CREATE TABLE productos (
    id_producto     NUMBER        NOT NULL,
    codigo          VARCHAR2(20)  NOT NULL,
    nombre          VARCHAR2(100) NOT NULL,
    tasa_anual_pct  NUMBER(6,2)   NOT NULL,
    pct_impuesto    NUMBER(5,2)   NOT NULL,
    pct_deducible   NUMBER(5,2)   NOT NULL,
    CONSTRAINT pk_productos PRIMARY KEY (id_producto),
    CONSTRAINT uq_productos_codigo UNIQUE (codigo)
);

-- 2. Terceros (clientes / contratantes)
CREATE TABLE terceros (
    id_tercero        NUMBER        NOT NULL,
    documento_fiscal  VARCHAR2(25)  NOT NULL,
    nombre_completo   VARCHAR2(150) NOT NULL,
    correo            VARCHAR2(120),
    CONSTRAINT pk_terceros PRIMARY KEY (id_tercero),
    CONSTRAINT uq_terceros_doc UNIQUE (documento_fiscal)
);

-- 3. Pólizas emitidas
CREATE TABLE polizas (
    id_poliza         NUMBER        NOT NULL,
    numero_poliza     VARCHAR2(30)  NOT NULL,
    id_producto       NUMBER        NOT NULL,
    id_tercero        NUMBER        NOT NULL,
    descripcion_bien  VARCHAR2(200) NOT NULL,
    fecha_desde       DATE          NOT NULL,
    fecha_hasta       DATE          NOT NULL,
    estado            VARCHAR2(20)  NOT NULL,
    cantidad_cuotas   NUMBER(2)     NOT NULL,
    suma_asegurada    NUMBER(14,2)  NOT NULL,
    prima_neta        NUMBER(14,2)  NOT NULL,
    monto_impuesto    NUMBER(14,2)  NOT NULL,
    prima_total       NUMBER(14,2)  NOT NULL,
    CONSTRAINT pk_polizas PRIMARY KEY (id_poliza),
    CONSTRAINT uq_polizas_numero UNIQUE (numero_poliza),
    CONSTRAINT fk_polizas_producto FOREIGN KEY (id_producto) REFERENCES productos (id_producto),
    CONSTRAINT fk_polizas_tercero FOREIGN KEY (id_tercero) REFERENCES terceros (id_tercero),
    CONSTRAINT ck_polizas_estado CHECK (estado IN ('VIGENTE', 'ANULADA')),
    CONSTRAINT ck_polizas_cuotas CHECK (cantidad_cuotas IN (1, 2, 4, 12))
);

-- 4. Recibos / Cuotas de prima
CREATE TABLE recibos (
    id_recibo         NUMBER        NOT NULL,
    id_poliza         NUMBER        NOT NULL,
    numero_recibo     VARCHAR2(30)  NOT NULL,
    numero_cuota      NUMBER(3)     NOT NULL,
    fecha_vencimiento DATE          NOT NULL,
    monto_total       NUMBER(14,2)  NOT NULL,
    estado            VARCHAR2(20)  NOT NULL,
    fecha_cobro       DATE,
    referencia_pago   VARCHAR2(60),
    CONSTRAINT pk_recibos PRIMARY KEY (id_recibo),
    CONSTRAINT uq_recibos_numero UNIQUE (numero_recibo),
    CONSTRAINT fk_recibos_poliza FOREIGN KEY (id_poliza) REFERENCES polizas (id_poliza),
    CONSTRAINT ck_recibos_estado CHECK (estado IN ('PENDIENTE', 'COBRADO'))
);

-- 5. Siniestros / Reclamos
CREATE TABLE siniestros (
    id_siniestro      NUMBER        NOT NULL,
    numero_siniestro  VARCHAR2(30)  NOT NULL,
    id_poliza         NUMBER        NOT NULL,
    fecha_ocurrencia  DATE          NOT NULL,
    descripcion       VARCHAR2(300) NOT NULL,
    monto_reclamado   NUMBER(14,2)  NOT NULL,
    monto_deducible   NUMBER(14,2)  NOT NULL,
    monto_indemnizar  NUMBER(14,2)  NOT NULL,
    estado            VARCHAR2(20)  NOT NULL,
    fecha_pago        DATE,
    CONSTRAINT pk_siniestros PRIMARY KEY (id_siniestro),
    CONSTRAINT uq_siniestros_numero UNIQUE (numero_siniestro),
    CONSTRAINT fk_siniestros_poliza FOREIGN KEY (id_poliza) REFERENCES polizas (id_poliza),
    CONSTRAINT ck_siniestros_estado CHECK (estado IN ('REGISTRADO', 'PAGADO'))
);
