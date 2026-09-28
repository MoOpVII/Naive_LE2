CREATE PROCEDURE [dbo].[spPosts_Insert]
	@userid int,
	@title nvarchar(150),
	@body text,
	@dateCreated datetime2
AS
begin
	INSERT INTO dbo.Posts ([UserId], [Title], [Body], [DateCreated])
	VALUES (@userId, @title, @body, @dateCreated);
end
