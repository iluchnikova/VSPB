CREATE TABLE [dbo].[Users] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [Пользователь] NVARCHAR (50) NOT NULL,
    [Пароль]       NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([Пользователь])
);

