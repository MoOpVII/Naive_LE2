CREATE PROCEDURE [dbo].[spInventory_Issue]
	@ItemId int,
	@Quantity int,
	@ExternalReference nvarchar(100),
	@Notes nvarchar(500),
	@CreatedByUserId int
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	IF @Quantity <= 0
		THROW 51010, 'Issue quantity must be positive.', 1;

	DECLARE @unitCost decimal(18,2);
	DECLARE @updatedCost table ([UnitCost] decimal(18,2));
	BEGIN TRY
		BEGIN TRANSACTION;
		UPDATE dbo.Items
		SET [StockQuantity] = [StockQuantity] - @Quantity
		OUTPUT inserted.[UnitCost] INTO @updatedCost
		WHERE [Id] = @ItemId AND [IsActive] = 1 AND [StockQuantity] >= @Quantity;
		IF @@ROWCOUNT = 0
			THROW 51011, 'Item is inactive, missing, or does not have enough stock.', 1;
		SELECT @unitCost = [UnitCost] FROM @updatedCost;

		INSERT INTO dbo.InventoryMovements
			([ItemId], [MovementType], [QuantityChange], [UnitCost], [ExternalReference], [Notes], [CreatedByUserId])
		VALUES
			(@ItemId, N'Issue', -@Quantity, @unitCost, @ExternalReference, @Notes, @CreatedByUserId);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END