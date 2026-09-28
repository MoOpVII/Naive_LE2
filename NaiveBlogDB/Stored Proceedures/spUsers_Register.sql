CREATE PROCEDURE [dbo].[spUsers_Register]
	@username nvarchar(16),
	@firstname nvarchar(50),
	@lastname nvarchar(50),
	@password nvarchar(50)
AS
begin
	set nocount on;

	INSERT INTO dbo.Users ([UserName], [FirstName], [LastName], [Password])
	VALUES (@userName, @firstName, @lastName, @password);
end
