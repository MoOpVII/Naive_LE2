CREATE PROCEDURE [dbo].[spItems_Brands]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT DISTINCT [Brand]
	FROM dbo.Items
	WHERE [Brand] IS NOT NULL AND [Brand] <> '';
END
