CREATE TABLE [dbo].[Items]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] NVARCHAR(200) NOT NULL,
	[Code] NVARCHAR(50) NOT NULL,
	[Brand] NVARCHAR(100) NULL,
	[UnitPrice] DECIMAL(18,2) NOT NULL,
	[UnitCost] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Items_UnitCost] DEFAULT (0),
	[StockQuantity] INT NOT NULL CONSTRAINT [DF_Items_StockQuantity] DEFAULT (0),
	[ReorderLevel] INT NOT NULL CONSTRAINT [DF_Items_ReorderLevel] DEFAULT (0),
	[IsActive] BIT NOT NULL CONSTRAINT [DF_Items_IsActive] DEFAULT (1),
	CONSTRAINT [UQ_Items_Code] UNIQUE ([Code]),
	CONSTRAINT [CK_Items_UnitPrice_NonNegative] CHECK ([UnitPrice] >= 0),
	CONSTRAINT [CK_Items_UnitCost_NonNegative] CHECK ([UnitCost] >= 0),
	CONSTRAINT [CK_Items_StockQuantity_NonNegative] CHECK ([StockQuantity] >= 0),
	CONSTRAINT [CK_Items_ReorderLevel_NonNegative] CHECK ([ReorderLevel] >= 0)
)
