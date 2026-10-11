CREATE PROCEDURE [dbo].[spSuppliers_List]
	@search nvarchar(100) = NULL,
	@includeInactive bit = 0
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Id], [Name], [ContactName], [Email], [Phone], [Address], [IsActive]
	FROM dbo.Suppliers
	WHERE (@includeInactive = 1 OR [IsActive] = 1)
		AND (@search IS NULL OR [Name] LIKE N'%' + @search + N'%' OR [ContactName] LIKE N'%' + @search + N'%');
END