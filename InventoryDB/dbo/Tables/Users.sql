CREATE TABLE [dbo].[Users]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserName] NVARCHAR(16) NOT NULL,
    [FirstName] NVARCHAR(50) NOT NULL, 
    [LastName] NVARCHAR(50) NOT NULL, 
    [Password] NVARCHAR(200) NOT NULL,
    CONSTRAINT [UQ_Users_UserName] UNIQUE ([UserName])
)
