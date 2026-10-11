CREATE PROCEDURE [dbo].[spInventory_ListMovements]
	@ItemId int = NULL,
	@SupplierId int = NULL,
	@MovementType nvarchar(20) = NULL,
	@FromUtc datetime2(0) = NULL,
	@ToUtc datetime2(0) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SELECT m.[Id], m.[ItemId], i.[Name] AS [ItemName], i.[Code] AS [ItemCode],
		m.[SupplierId], s.[Name] AS [SupplierName], m.[MovementType], m.[QuantityChange],
		m.[UnitCost], m.[ExternalReference], m.[Notes], m.[CreatedByUserId], u.[UserName] AS [CreatedByUserName], m.[CreatedAtUtc]
	FROM dbo.InventoryMovements AS m
	INNER JOIN dbo.Items AS i ON i.[Id] = m.[ItemId]
	LEFT JOIN dbo.Suppliers AS s ON s.[Id] = m.[SupplierId]
	INNER JOIN dbo.Users AS u ON u.[Id] = m.[CreatedByUserId]
	WHERE (@ItemId IS NULL OR m.[ItemId] = @ItemId)
		AND (@SupplierId IS NULL OR m.[SupplierId] = @SupplierId)
		AND (@MovementType IS NULL OR m.[MovementType] = @MovementType)
		AND (@FromUtc IS NULL OR m.[CreatedAtUtc] >= @FromUtc)
		AND (@ToUtc IS NULL OR m.[CreatedAtUtc] < @ToUtc)
	ORDER BY m.[CreatedAtUtc] DESC, m.[Id] DESC;
END