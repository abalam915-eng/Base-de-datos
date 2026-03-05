-- =================================================================================
-- Script: 04_seeder_cliente.sql
-- Autor: Gemini CLI - Senior Database Architect
-- Objetivo: Poblar la tabla Clientes con datos realistas del mercado mexicano 2025.
-- =================================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [demo_utm_AGBM];
GO

BEGIN TRANSACTION;
BEGIN TRY
    -- 1. Limpieza Idempotente
    -- Desactivar restricciones temporalmente si fuera necesario (aquí no hay FKs entrantes aún)
    DELETE FROM [dbo].[Clientes];
    
    -- Reiniciar el contador de identidad
    DBCC CHECKIDENT ('[dbo].[Clientes]', RESEED, 0);

    -- 2. Activación de Inserción de Identidad
    SET IDENTITY_INSERT [dbo].[Clientes] ON;


    -- Bloque 1: Clientes Activos (1-15)
    INSERT INTO [dbo].[Clientes] ([ClienteID], [NombreCompleto], [Email], [EsActivo], [FechaRegistro])
    VALUES (1, N'Santiago Hern�ndez Garc�a', 's.hernandez@gmail.com', 1, '2024-10-15 10:30:00'),
           (2, N'Sof�a Garc�a Mart�nez', 'sofia.garcia@outlook.com', 1, '2024-10-22 14:45:00'),
           (3, N'Mateo Mart�nez L�pez', 'mateo.m@itmerida.edu.mx', 1, '2024-11-05 09:15:00'),
           (4, N'Valentina L�pez Rodr�guez', 'v.lopez@gmail.com', 1, '2024-11-12 11:20:00'),
           (5, N'Sebasti�n Rodr�guez Gonz�lez', 's.rodriguez@outlook.com', 1, '2024-11-20 16:35:00'),
           (6, N'Camila Gonz�lez P�rez', 'camila.g@itmerida.edu.mx', 1, '2024-12-02 12:50:00'),
           (7, N'Leonardo P�rez S�nchez', 'l.perez@gmail.com', 1, '2024-12-10 15:10:00'),
           (8, N'Isabella S�nchez Ram�rez', 'i.sanchez@outlook.com', 1, '2024-12-18 08:25:00'),
           (9, N'Diego Ram�rez Cruz', 'diego.r@itmerida.edu.mx', 1, '2024-12-28 13:40:00'),
           (10, N'Mariana Cruz Flores', 'm.cruz@gmail.com', 1, '2025-01-05 10:05:00'),
           (11, N'Emiliano Flores G�mez', 'e.flores@outlook.com', 1, '2025-01-12 14:15:00'),
           (12, N'Regina G�mez Morales', 'regina.g@itmerida.edu.mx', 1, '2025-01-18 09:30:00'),
           (13, N'Daniel Morales V�zquez', 'd.morales@gmail.com', 1, '2025-01-25 11:55:00'),
           (14, N'Ximena V�zquez Jim�nez', 'x.vazquez@outlook.com', 1, '2025-02-02 16:20:00'),
           (15, N'Alexander Jim�nez Reyes', 'a.jimenez@itmerida.edu.mx', 1, '2025-02-08 12:45:00');

    -- Bloque 2: Clientes Activos e Inactivos (16-30)
    INSERT INTO [dbo].[Clientes] ([ClienteID], [NombreCompleto], [Email], [EsActivo], [FechaRegistro])
    VALUES (16, N'Victoria Reyes Torres', 'v.reyes@gmail.com', 1, '2025-02-12 10:10:00'),
           (17, N'Gabriel Torres D�az', 'g.torres@outlook.com', 1, '2025-02-15 14:35:00'),
           (18, N'Renata D�az Guti�rrez', 'r.diaz@itmerida.edu.mx', 1, '2025-02-18 08:50:00'),
           (19, N'Juli�n Guti�rrez Ruiz', 'j.gutierrez@gmail.com', 1, '2025-02-20 11:25:00'),
           (20, N'Natalia Ruiz Mendoza', 'n.ruiz@outlook.com', 1, '2025-02-22 15:40:00'),
           (21, N'Samuel Mendoza Aguilar', 's.mendoza@itmerida.edu.mx', 1, '2025-02-24 09:15:00'),
           (22, N'Paula Aguilar Ortiz', 'p.aguilar@gmail.com', 1, '2025-02-25 12:30:00'),
           (23, N'Nicol�s Ortiz Castillo', 'n.ortiz@outlook.com', 1, '2025-02-26 16:55:00'),
           (24, N'Elena Castillo Moreno', 'e.castillo@itmerida.edu.mx', 1, '2025-02-27 10:20:00'),
           (25, N'�ngel Moreno Rivera', 'a.moreno@gmail.com', 1, '2025-02-28 14:45:00'),
           (26, N'Daniela Rivera Vargas', 'd.rivera@outlook.com', 1, '2025-02-28 08:10:00'),
           (27, N'Ricardo Vargas Castro', 'r.vargas@itmerida.edu.mx', 1, '2025-03-01 11:35:00'),
           (28, N'Fernanda Castro Jim�nez', 'f.castro@gmail.com', 0, '2024-11-30 15:00:00'), -- Inactivo
           (29, N'Luis Jim�nez Silva', 'l.jimenez@outlook.com', 0, '2024-12-15 09:25:00'),  -- Inactivo
           (30, N'Andrea Silva Hern�ndez', 'a.silva@itmerida.edu.mx', 0, '2025-01-20 13:50:00'); -- Inactivo

    -- 3. Desactivaci�n de Inserci�n de Identidad
    SET IDENTITY_INSERT [dbo].[Clientes] OFF;

    -- Sincronizar el contador para las pr�ximas inserciones manuales/app
    DBCC CHECKIDENT ('[dbo].[Clientes]', RESEED, 30);

    COMMIT TRANSACTION;
    PRINT 'Sembrado de Clientes completado exitosamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;

-- 4. Validaci�n Final
SELECT COUNT(*) AS [TotalClientesCargados] FROM [dbo].[Clientes];
GO
