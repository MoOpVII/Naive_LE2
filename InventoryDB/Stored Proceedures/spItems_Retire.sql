CREATE PROCEDURE [dbo].[spItems_Retire]
	@Id int
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRANSACTION;
		IF NOT EXISTS (SELECT 1 FROM dbo.Items WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = @Id)
			THROW 51020, 'Item not found.', 1;
		IF EXISTS (SELECT 1 FROM dbo.Items WHERE [Id] = @Id AND [StockQuantity] > 0)
			THROW 51021, 'Items with stock on hand cannot be retired.', 1;
		UPDATE dbo.Items SET [IsActive] = 0 WHERE [Id] = @Id;
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END