CREATE PROCEDURE [dbo].[spItems_Add]
	@Name nvarchar(200),
	@Code nvarchar(50),
	@Brand nvarchar(100),
	@UnitPrice decimal(18,2)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Items ([Name], [Code], [Brand], [UnitPrice])
	VALUES (@Name, @Code, @Brand, @UnitPrice);
END
