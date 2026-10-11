CREATE PROCEDURE [dbo].[spInventory_Summary]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT COUNT_BIG(*) AS [ProductCount],
		COALESCE(SUM(CONVERT(bigint, [StockQuantity])), 0) AS [TotalUnits],
		COALESCE(SUM(CASE WHEN [StockQuantity] <= [ReorderLevel] THEN CONVERT(bigint, 1) ELSE CONVERT(bigint, 0) END), 0) AS [LowStockCount],
		COALESCE(SUM(CONVERT(decimal(28,2), [StockQuantity]) * [UnitCost]), 0) AS [CostValue],
		COALESCE(SUM(CONVERT(decimal(28,2), [StockQuantity]) * [UnitPrice]), 0) AS [RetailValue]
	FROM dbo.Items
	WHERE [IsActive] = 1;
END