CREATE PROCEDURE [dbo].[spSuppliers_Update]
	@Id int,
	@Name nvarchar(200),
	@ContactName nvarchar(100),
	@Email nvarchar(254),
	@Phone nvarchar(30),
	@Address nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE dbo.Suppliers
	SET [Name] = @Name,
		[ContactName] = @ContactName,
		[Email] = @Email,
		[Phone] = @Phone,
		[Address] = @Address
	WHERE [Id] = @Id;
END