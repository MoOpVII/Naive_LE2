CREATE PROCEDURE [dbo].[spItems_Update]
	@Id int,
	@Name nvarchar(200),
	@Code nvarchar(50),
	@Brand nvarchar(100),
	@UnitPrice decimal(18,2),
	@ReorderLevel int
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE dbo.Items
	SET [Name] = @Name,
		[Code] = @Code,
		[Brand] = @Brand,
		[UnitPrice] = @UnitPrice,
		[ReorderLevel] = @ReorderLevel
	WHERE [Id] = @Id;
END