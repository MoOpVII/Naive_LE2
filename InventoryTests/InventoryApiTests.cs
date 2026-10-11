using InventoryAPI.Contracts;
using InventoryAPI.Controllers;
using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InventoryTests
{
    [TestClass]
    public class InventoryApiTests
    {
        [TestMethod]
        public void ItemCreateRequestRejectsNegativePrices()
        {
            var request = new ItemCreateRequest { Name = "Device", Code = "DEV-01", UnitPrice = -1, UnitCost = 0 };
            var results = Validate(request);
            Assert.IsTrue(results.Any(result => result.MemberNames.Contains(nameof(ItemCreateRequest.UnitPrice))));
        }

        [TestMethod]
        public void AdjustmentRequestRejectsZeroAndMinimumInteger()
        {
            var zeroRequest = new AdjustStockRequest { ItemId = 1, QuantityChange = 0 };
            Assert.IsFalse(Validator.TryValidateObject(zeroRequest, new ValidationContext(zeroRequest), new List<ValidationResult>(), true));
            var request = new AdjustStockRequest { ItemId = 1, QuantityChange = int.MinValue };
            Assert.IsTrue(Validate(request).Any(result => result.MemberNames.Contains(nameof(AdjustStockRequest.QuantityChange))));
        }

        [TestMethod]
        public void GetByIdHidesRetiredItems()
        {
            var data = new FakeSqlData { Item = new ItemModel { Id = 7, Name = "Retired", Code = "RET-7", IsActive = false } };
            var controller = new ItemsController(data);

            var result = controller.GetById(7);

            Assert.IsInstanceOfType<NotFoundResult>(result.Result);
        }

        [TestMethod]
        public void IssueRecordsAuthenticatedUserAndReturnsSuccess()
        {
            var data = new FakeSqlData();
            var controller = new InventoryController(data)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "27") }, "test"))
                    }
                }
            };

            var result = controller.Issue(new IssueStockRequest { ItemId = 4, Quantity = 2 });

            Assert.IsInstanceOfType<OkResult>(result);
            Assert.AreEqual(27, data.RecordedUserId);
            Assert.AreEqual(4, data.IssuedItemId);
        }

        private static List<ValidationResult> Validate(object value)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), results, true);
            return results;
        }

        private sealed class FakeSqlData : ISqlData
        {
            public ItemModel? Item { get; set; }
            public int RecordedUserId { get; private set; }
            public int IssuedItemId { get; private set; }

            public UserModel Authenticate(string username) => throw new NotImplementedException();
            public void Register(string username, string firstName, string lastName, string password) => throw new NotImplementedException();
            public List<ItemModel> ListItems(string? search = null, string? brand = null, bool includeInactive = false, bool lowStockOnly = false) => new();
            public ItemModel? GetItemById(int id) => Item;
            public ItemModel? GetItemByCode(string code) => null;
            public int AddItem(ItemModel item) => throw new NotImplementedException();
            public void UpdateItem(ItemModel item) => throw new NotImplementedException();
            public void RetireItem(int id) => throw new NotImplementedException();
            public void UpdateItemPrice(string code, decimal price) => throw new NotImplementedException();
            public List<string> ListBrands() => new();
            public List<SupplierModel> ListSuppliers(string? search = null, bool includeInactive = false) => new();
            public SupplierModel? GetSupplierById(int id) => null;
            public int AddSupplier(SupplierModel supplier) => throw new NotImplementedException();
            public void UpdateSupplier(SupplierModel supplier) => throw new NotImplementedException();
            public void RetireSupplier(int id) => throw new NotImplementedException();
            public void ReceiveStock(StockReceiptModel receipt, int createdByUserId) => throw new NotImplementedException();
            public void IssueStock(StockIssueModel issue, int createdByUserId)
            {
                RecordedUserId = createdByUserId;
                IssuedItemId = issue.ItemId;
            }
            public void AdjustStock(StockAdjustmentModel adjustment, int createdByUserId) => throw new NotImplementedException();
            public List<InventoryMovementModel> ListMovements(InventoryMovementFilterModel filter) => new();
            public InventorySummaryModel GetInventorySummary() => new();
        }
    }

    [TestClass]
    public class InventorySqlInvariantTests
    {
        [TestMethod]
        public async Task FailedIssueDoesNotChangeStockOrCreateMovement()
        {
            var connectionString = Environment.GetEnvironmentVariable("INVENTORY_TEST_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Inconclusive("Set INVENTORY_TEST_CONNECTION_STRING to a deployed disposable test database to run SQL integration tests.");

            await using var connection = new SqlConnection(connectionString!);
            await connection.OpenAsync();
            var fixture = await CreateFixtureAsync(connection);
            try
            {
                var exception = await Assert.ThrowsExceptionAsync<SqlException>(() => ExecuteIssueAsync(connection, fixture.ItemId, fixture.UserId, 2));
                Assert.AreEqual(51011, exception.Number);
                var state = await ReadInventoryStateAsync(connection, fixture.ItemId);
                Assert.AreEqual(1, state.StockQuantity);
                Assert.AreEqual(0, state.MovementCount);
            }
            finally
            {
                await DeleteFixtureAsync(connection, fixture);
            }
        }

        [TestMethod]
        public async Task ReceiptUpdatesAverageCostAndAddsMovementAtomically()
        {
            var connectionString = Environment.GetEnvironmentVariable("INVENTORY_TEST_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Inconclusive("Set INVENTORY_TEST_CONNECTION_STRING to a deployed disposable test database to run SQL integration tests.");

            await using var connection = new SqlConnection(connectionString!);
            await connection.OpenAsync();
            var fixture = await CreateFixtureAsync(connection);
            try
            {
                await ExecuteReceiptAsync(connection, fixture);
                var state = await ReadInventoryStateAsync(connection, fixture.ItemId);
                Assert.AreEqual(2, state.StockQuantity);
                Assert.AreEqual(15m, state.UnitCost);
                Assert.AreEqual(1, state.MovementCount);
            }
            finally
            {
                await DeleteFixtureAsync(connection, fixture);
            }
        }

        private static async Task<Fixture> CreateFixtureAsync(SqlConnection connection)
        {
            var suffix = Guid.NewGuid().ToString("N")[..12];
            var userId = await ScalarIntAsync(connection,
                "INSERT INTO dbo.Users (UserName, FirstName, LastName, [Password]) OUTPUT INSERTED.Id VALUES (@username, N'Test', N'User', N'test-hash');",
                ("@username", "t" + suffix));
            var supplierId = await ScalarIntAsync(connection,
                "INSERT INTO dbo.Suppliers ([Name]) OUTPUT INSERTED.Id VALUES (@name);",
                ("@name", "Test Supplier " + suffix));
            var itemId = await ScalarIntAsync(connection,
                "INSERT INTO dbo.Items ([Name], [Code], [UnitPrice], [UnitCost], [StockQuantity], [ReorderLevel]) OUTPUT INSERTED.Id VALUES (@name, @code, 25, 10, 1, 0);",
                ("@name", "Test Item " + suffix), ("@code", "TEST-" + suffix));
            return new Fixture(userId, supplierId, itemId);
        }

        private static async Task ExecuteIssueAsync(SqlConnection connection, int itemId, int userId, int quantity)
        {
            using var command = new SqlCommand("dbo.spInventory_Issue", connection) { CommandType = System.Data.CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@ItemId", itemId);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@ExternalReference", DBNull.Value);
            command.Parameters.AddWithValue("@Notes", DBNull.Value);
            command.Parameters.AddWithValue("@CreatedByUserId", userId);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task ExecuteReceiptAsync(SqlConnection connection, Fixture fixture)
        {
            using var command = new SqlCommand("dbo.spInventory_Receive", connection) { CommandType = System.Data.CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@ItemId", fixture.ItemId);
            command.Parameters.AddWithValue("@SupplierId", fixture.SupplierId);
            command.Parameters.AddWithValue("@Quantity", 1);
            command.Parameters.AddWithValue("@UnitCost", 20m);
            command.Parameters.AddWithValue("@ExternalReference", DBNull.Value);
            command.Parameters.AddWithValue("@Notes", DBNull.Value);
            command.Parameters.AddWithValue("@CreatedByUserId", fixture.UserId);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<InventoryState> ReadInventoryStateAsync(SqlConnection connection, int itemId)
        {
            using var command = new SqlCommand("SELECT i.StockQuantity, i.UnitCost, (SELECT COUNT(*) FROM dbo.InventoryMovements m WHERE m.ItemId = i.Id) FROM dbo.Items i WHERE i.Id = @id", connection);
            command.Parameters.AddWithValue("@id", itemId);
            using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            return new InventoryState(reader.GetInt32(0), reader.GetDecimal(1), reader.GetInt32(2));
        }

        private static async Task DeleteFixtureAsync(SqlConnection connection, Fixture fixture)
        {
            using var command = new SqlCommand("DELETE FROM dbo.InventoryMovements WHERE ItemId = @itemId; DELETE FROM dbo.Items WHERE Id = @itemId; DELETE FROM dbo.Suppliers WHERE Id = @supplierId; DELETE FROM dbo.Users WHERE Id = @userId;", connection);
            command.Parameters.AddWithValue("@itemId", fixture.ItemId);
            command.Parameters.AddWithValue("@supplierId", fixture.SupplierId);
            command.Parameters.AddWithValue("@userId", fixture.UserId);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<int> ScalarIntAsync(SqlConnection connection, string sql, params (string Name, object Value)[] parameters)
        {
            using var command = new SqlCommand(sql, connection);
            foreach (var parameter in parameters) command.Parameters.AddWithValue(parameter.Name, parameter.Value);
            return Convert.ToInt32(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
        }

        private sealed record Fixture(int UserId, int SupplierId, int ItemId);
        private sealed record InventoryState(int StockQuantity, decimal UnitCost, int MovementCount);
    }
}
