CREATE PROCEDURE [dbo].[spItems_List]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [Code], [Brand], [UnitPrice]
	FROM dbo.Items;
END
