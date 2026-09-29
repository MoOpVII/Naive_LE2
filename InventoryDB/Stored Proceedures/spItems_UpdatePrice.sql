CREATE PROCEDURE [dbo].[spItems_UpdatePrice]
	@code nvarchar(50),
	@price decimal(18,2)
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE dbo.Items
	SET UnitPrice = @price
	WHERE [Code] = @code;
END
