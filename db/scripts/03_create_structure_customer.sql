-- =================================================================================
-- Script: 03_create_structure_customer.sql
-- Autor: Gemini CLI - Senior Database Architect
-- Objetivo: Extender la base de datos [demo_utm_AGBM] para soportar el módulo de lealtad.
-- Motor de DB: Microsoft SQL Server 2022 Express
-- =================================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Asegurar el contexto de la base de datos correcta.
USE [demo_utm_AGBM];
GO

-- =================================================================================
-- Tabla: Clientes
-- Almacena la información de los clientes para el programa de lealtad.
-- Esta tabla mapea directamente a la entidad Customer.cs en el dominio de .NET.
-- =================================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Clientes] (
        [ClienteID]       INT IDENTITY(1,1) NOT NULL,
        [NombreCompleto]  NVARCHAR(150) NOT NULL,
        [Email]           VARCHAR(100) NOT NULL,
        [EsActivo]        BIT NOT NULL DEFAULT 1,
        [FechaRegistro]   DATETIME NOT NULL DEFAULT GETDATE(),

        -- Constraints de Integridad y Seguridad
        CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED ([ClienteID] ASC),
        CONSTRAINT [UQ_Clientes_Email] UNIQUE NONCLUSTERED ([Email] ASC),
        
        -- Validación de Email: Asegura que contenga '@' y '.' (Lógica de Negocio Reforzada)
        CONSTRAINT [CHK_Clientes_Email_Formato] CHECK ([Email] LIKE '%@%.%')
    );

    PRINT 'Tabla [dbo].[Clientes] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [dbo].[Clientes] ya existe.';
END
GO

-- =================================================================================
-- Evolución de Esquema: Modificación de Tabla [Venta]
-- Se agrega la columna ClienteID para establecer la relación 1:N (Un Cliente -> N Ventas).
-- =================================================================================

-- 1. Agregar columna ClienteID si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Venta]') AND name = 'ClienteID')
BEGIN
    ALTER TABLE [dbo].[Venta] 
    ADD [ClienteID] INT NULL; -- Se permite NULL para ventas de mostrador anónimas inicialmente.

    PRINT 'Columna [ClienteID] agregada a la tabla [Venta].';
END
GO

-- 2. Establecer la Relación (Foreign Key)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Venta_Clientes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Venta]'))
BEGIN
    ALTER TABLE [dbo].[Venta] WITH CHECK ADD CONSTRAINT [FK_Venta_Clientes]
    FOREIGN KEY ([ClienteID]) REFERENCES [dbo].[Clientes] ([ClienteID]);

    ALTER TABLE [dbo].[Venta] CHECK CONSTRAINT [FK_Venta_Clientes];

    PRINT 'Relación [FK_Venta_Clientes] establecida.';
END
GO

-- =================================================================================
-- Notas de Arquitectura:
-- - ClienteID: INT IDENTITY para eficiencia en índices.
-- - NVARCHAR(150): Soporte Unicode para nombres internacionales.
-- - EsActivo: BIT para borrado lógico (Soft Delete).
-- - CHECK Constraint: Refuerza la integridad de datos desde el motor de DB.
-- =================================================================================
