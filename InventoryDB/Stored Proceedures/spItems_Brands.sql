CREATE PROCEDURE [dbo].[spItems_Brands]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT DISTINCT [Brand]
	FROM dbo.Items
	WHERE [Brand] IS NOT NULL AND [Brand] <> '' AND [IsActive] = 1;
END
