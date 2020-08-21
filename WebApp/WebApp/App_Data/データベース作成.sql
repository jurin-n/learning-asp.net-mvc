/* devデータベース作成 */
USE master;
GO
IF DB_ID (N'dev') IS NOT NULL
DROP DATABASE dev;
GO
CREATE DATABASE dev
COLLATE Japanese_CS_AS
WITH TRUSTWORTHY ON, DB_CHAINING ON;
GO
--Verifying collation and option settings.
SELECT name, collation_name, is_trustworthy_on, is_db_chaining_on
FROM sys.databases
WHERE name = N'dev';
GO