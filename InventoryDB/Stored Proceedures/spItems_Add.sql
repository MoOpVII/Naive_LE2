CREATE PROCEDURE [dbo].[spItems_Add]
	@Name nvarchar(200),
	@Code nvarchar(50),
	@Brand nvarchar(100),
	@UnitPrice decimal(18,2),
	@UnitCost decimal(18,2),
	@ReorderLevel int
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Items ([Name], [Code], [Brand], [UnitPrice], [UnitCost], [ReorderLevel])
	VALUES (@Name, @Code, @Brand, @UnitPrice, @UnitCost, @ReorderLevel);
	SELECT CAST(SCOPE_IDENTITY() AS int);
END
