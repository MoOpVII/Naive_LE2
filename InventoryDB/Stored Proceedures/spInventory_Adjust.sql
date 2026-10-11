CREATE PROCEDURE [dbo].[spInventory_Adjust]
	@ItemId int,
	@QuantityChange int,
	@ExternalReference nvarchar(100),
	@Notes nvarchar(500),
	@CreatedByUserId int
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	IF @QuantityChange = 0
		THROW 51012, 'Adjustment quantity cannot be zero.', 1;

	DECLARE @unitCost decimal(18,2);
	DECLARE @updatedCost table ([UnitCost] decimal(18,2));
	BEGIN TRY
		BEGIN TRANSACTION;
		UPDATE dbo.Items
		SET [StockQuantity] = [StockQuantity] + @QuantityChange
		OUTPUT inserted.[UnitCost] INTO @updatedCost
		WHERE [Id] = @ItemId AND [IsActive] = 1 AND [StockQuantity] + @QuantityChange >= 0;
		IF @@ROWCOUNT = 0
			THROW 51013, 'Item is inactive, missing, or adjustment would make stock negative.', 1;
		SELECT @unitCost = [UnitCost] FROM @updatedCost;

		INSERT INTO dbo.InventoryMovements
			([ItemId], [MovementType], [QuantityChange], [UnitCost], [ExternalReference], [Notes], [CreatedByUserId])
		VALUES
			(@ItemId, N'Adjustment', @QuantityChange, @unitCost, @ExternalReference, @Notes, @CreatedByUserId);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END