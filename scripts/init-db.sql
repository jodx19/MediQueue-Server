-- ============================================================================
-- MediQueue Database Initialization Script
-- Runs on SQL Server container startup (one-time)
-- ============================================================================

-- Create database if not exists
IF DB_ID('MediQueueProd') IS NULL
BEGIN
    CREATE DATABASE MediQueueProd;
    PRINT 'Database MediQueueProd created successfully.';
END
ELSE
BEGIN
    PRINT 'Database MediQueueProd already exists. Skipping creation.';
END

USE MediQueueProd;

-- Create audit schema (for future Phase 2)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'audit')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA audit AUTHORIZATION dbo;'
    PRINT 'Audit schema created successfully.';
END
ELSE
BEGIN
    PRINT 'Audit schema already exists.';
END

-- Final verification
IF DB_ID('MediQueueProd') IS NOT NULL
BEGIN
    PRINT 'Database MediQueueProd is ready for EF Core migrations.';
END