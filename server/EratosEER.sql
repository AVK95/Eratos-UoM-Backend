-- -------------------------------------------------------
-- SQL Script to create the Eratos Backend Server Database
-- This script was run on Azure SQL Server
-- @author Aditya Vikram Khandelwal
-- -------------------------------------------------------

CREATE TABLE [User] (
  UserID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
  EratosUserID NVARCHAR(150) NOT NULL,
  Email NVARCHAR(320) NOT NULL,
  Name NVARCHAR(100) NOT NULL,
  Auth0ID NVARCHAR(150) NOT NULL,
  CreatedAt DATETIME NOT NULL,
  Info NVARCHAR(300))

CREATE TABLE [Order] (
  OrderID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
  Price FLOAT,
  Status NVARCHAR(100),
  OrderTime DATETIME NULL,
  UserID INT NOT NULL REFERENCES [User](UserID))

CREATE TABLE [Task] (
  TaskID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
  CreatedAt DATETIME NOT NULL,
  LastUpdatedAt DATETIME NULL,
  StartedAt DATETIME,
  EndedAt DATETIME,
  Priority NVARCHAR(45),
  State NVARCHAR(45),
  Type NVARCHAR(45),
  Meta NVARCHAR(300),
  Error NVARCHAR(300),
  UserID INT NOT NULL REFERENCES [User](UserID),
  OrderID INT NOT NULL REFERENCES [Order](OrderID))

CREATE TABLE [Resource] (
  ResourceID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
  EratosResourceID NVARCHAR(150) NOT NULL,
  Date DATETIME,
  Policy NVARCHAR(150),
  Geo NVARCHAR(300),
  Meta NVARCHAR(300))

CREATE TABLE [ResourceTask] (
  ResourceID INT REFERENCES [Resource](ResourceID),
  TaskID INT NOT NULL REFERENCES [Task](TaskID))

CREATE TABLE [Module] (
  ModuleID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
  EratosModuleID NVARCHAR(150) NOT NULL,
  ModuleName NVARCHAR(100) NOT NULL,
  ModuleSchema NVARCHAR(100),
  isActive TINYINT)

CREATE TABLE [ResourceModule] (
  ResourceID INT NOT NULL REFERENCES [Resource](ResourceID),
  ModuleID INT NOT NULL REFERENCES [Module](ModuleID))
