CREATE PROCEDURE [dbo].[spSuppliers_Add]
	@Name nvarchar(200),
	@ContactName nvarchar(100),
	@Email nvarchar(254),
	@Phone nvarchar(30),
	@Address nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Suppliers ([Name], [ContactName], [Email], [Phone], [Address])
	VALUES (@Name, @ContactName, @Email, @Phone, @Address);
	SELECT CAST(SCOPE_IDENTITY() AS int);
END