CREATE PROCEDURE [dbo].[spItems_GetByCode]
	@code nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [Code], [Brand], [UnitPrice], [UnitCost], [StockQuantity], [ReorderLevel], [IsActive]
	FROM dbo.Items
	WHERE [Code] = @code;
END
