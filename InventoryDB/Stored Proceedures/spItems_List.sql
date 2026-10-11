CREATE PROCEDURE [dbo].[spItems_List]
	@search nvarchar(100) = NULL,
	@brand nvarchar(100) = NULL,
	@includeInactive bit = 0,
	@lowStockOnly bit = 0
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [Code], [Brand], [UnitPrice], [UnitCost], [StockQuantity], [ReorderLevel], [IsActive]
	FROM dbo.Items
	WHERE (@includeInactive = 1 OR [IsActive] = 1)
		AND (@lowStockOnly = 0 OR [StockQuantity] <= [ReorderLevel])
		AND (@brand IS NULL OR [Brand] = @brand)
		AND (@search IS NULL OR [Name] LIKE N'%' + @search + N'%' OR [Code] LIKE N'%' + @search + N'%');
END
