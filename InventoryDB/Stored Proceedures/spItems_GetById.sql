CREATE PROCEDURE [dbo].[spItems_GetById]
	@Id int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [Code], [Brand], [UnitPrice], [UnitCost], [StockQuantity], [ReorderLevel], [IsActive]
	FROM dbo.Items
	WHERE [Id] = @Id;
END