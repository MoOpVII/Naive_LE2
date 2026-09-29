CREATE PROCEDURE [dbo].[spUsers_Authenticate]
	@username nvarchar(16)
AS
begin
	set nocount on;


	SELECT [Id], [UserName], [FirstName], [LastName], [Password]
	FROM dbo.Users
	WHERE [UserName] = @username;

end
