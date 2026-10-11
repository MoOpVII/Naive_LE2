CREATE TABLE [dbo].[InventoryMovements]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[ItemId] INT NOT NULL,
	[SupplierId] INT NULL,
	[MovementType] NVARCHAR(20) NOT NULL,
	[QuantityChange] INT NOT NULL,
	[UnitCost] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_InventoryMovements_UnitCost] DEFAULT (0),
	[ExternalReference] NVARCHAR(100) NULL,
	[Notes] NVARCHAR(500) NULL,
	[CreatedByUserId] INT NOT NULL,
	[CreatedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryMovements_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
	CONSTRAINT [FK_InventoryMovements_Items] FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items] ([Id]),
	CONSTRAINT [FK_InventoryMovements_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id]),
	CONSTRAINT [FK_InventoryMovements_Users] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [CK_InventoryMovements_Type] CHECK ([MovementType] IN (N'Receipt', N'Issue', N'Adjustment')),
	CONSTRAINT [CK_InventoryMovements_QuantityChange] CHECK ([QuantityChange] <> 0),
	CONSTRAINT [CK_InventoryMovements_UnitCost] CHECK ([UnitCost] >= 0),
	CONSTRAINT [CK_InventoryMovements_SupplierByType] CHECK
	(
		([MovementType] = N'Receipt' AND [SupplierId] IS NOT NULL)
		OR ([MovementType] <> N'Receipt' AND [SupplierId] IS NULL)
	),
	CONSTRAINT [CK_InventoryMovements_QuantityByType] CHECK
	(
		([MovementType] = N'Receipt' AND [QuantityChange] > 0)
		OR ([MovementType] = N'Issue' AND [QuantityChange] < 0)
		OR [MovementType] = N'Adjustment'
	)
)
GO
CREATE INDEX [IX_InventoryMovements_ItemId_CreatedAtUtc]
	ON [dbo].[InventoryMovements] ([ItemId], [CreatedAtUtc] DESC)
GO
CREATE INDEX [IX_InventoryMovements_SupplierId_CreatedAtUtc]
	ON [dbo].[InventoryMovements] ([SupplierId], [CreatedAtUtc] DESC)
