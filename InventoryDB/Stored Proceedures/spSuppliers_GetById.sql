CREATE PROCEDURE [dbo].[spSuppliers_GetById]
	@Id int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [ContactName], [Email], [Phone], [Address], [IsActive]
	FROM dbo.Suppliers
	WHERE [Id] = @Id;
END