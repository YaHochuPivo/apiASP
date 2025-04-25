-- Создание базы данных
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'zachet')
BEGIN
    ALTER DATABASE zachet SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE zachet;
END
GO

CREATE DATABASE zachet;
GO

USE zachet;
GO

-- Создание таблицы для задач
CREATE TABLE TodoItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Priority INT NOT NULL,
    Deadline DATETIME NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0
);
GO

-- Добавление тестовых данных
INSERT INTO TodoItems (Title, Description, Priority, Deadline, IsCompleted)
VALUES 
    ('Завершить проект', 'Завершить разработку приложения для управления задачами', 1, '20240501', 0),
    ('Подготовить презентацию', 'Создать презентацию для защиты проекта', 2, '20240425', 0),
    ('Написать документацию', 'Составить подробную документацию по проекту', 3, '20240430', 0);
GO 