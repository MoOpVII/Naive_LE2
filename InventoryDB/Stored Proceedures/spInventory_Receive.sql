CREATE PROCEDURE [dbo].[spInventory_Receive]
	@ItemId int,
	@SupplierId int,
	@Quantity int,
	@UnitCost decimal(18,2),
	@ExternalReference nvarchar(100),
	@Notes nvarchar(500),
	@CreatedByUserId int
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	IF @Quantity <= 0 OR @UnitCost < 0
		THROW 51001, 'Receipt quantity must be positive and unit cost cannot be negative.', 1;

	BEGIN TRY
		BEGIN TRANSACTION;
		IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = @SupplierId AND [IsActive] = 1)
			THROW 51002, 'Active supplier not found.', 1;

		UPDATE dbo.Items
		SET [UnitCost] = CASE WHEN [StockQuantity] = 0 THEN @UnitCost
			ELSE (([StockQuantity] * [UnitCost]) + (@Quantity * @UnitCost)) / ([StockQuantity] + @Quantity) END,
			[StockQuantity] = [StockQuantity] + @Quantity
		WHERE [Id] = @ItemId AND [IsActive] = 1;
		IF @@ROWCOUNT = 0
			THROW 51003, 'Active item not found.', 1;

		INSERT INTO dbo.InventoryMovements
			([ItemId], [SupplierId], [MovementType], [QuantityChange], [UnitCost], [ExternalReference], [Notes], [CreatedByUserId])
		VALUES
			(@ItemId, @SupplierId, N'Receipt', @Quantity, @UnitCost, @ExternalReference, @Notes, @CreatedByUserId);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END