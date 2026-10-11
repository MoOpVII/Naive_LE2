using InventoryDataLibrary.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryWinForms
{
    internal sealed class InventoryApiClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly HttpClient _httpClient = new();
        private readonly string _apiBase;

        public InventoryApiClient()
        {
            _apiBase = (Environment.GetEnvironmentVariable("INVENTORY_API_BASE_URL") ?? "https://localhost:7060/api").TrimEnd('/') + "/";
        }

        public string? Token { get; set; }

        public async Task<string> LoginAsync(string username, string password)
        {
            var result = await SendAsync<LoginResponse>(HttpMethod.Post, "auth/login", new { Username = username, Password = password });
            return result.Token;
        }

        public Task RegisterAsync(string username, string firstName, string lastName, string password)
        {
            return SendAsync(HttpMethod.Post, "auth/register", new { Username = username, FirstName = firstName, LastName = lastName, Password = password });
        }

        public Task<List<ItemModel>> GetItemsAsync(string? search = null, bool lowStockOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (lowStockOnly) query.Add("lowStockOnly=true");
            return GetAsync<List<ItemModel>>("items" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query)));
        }

        public Task<ItemModel> CreateItemAsync(ItemModel item)
        {
            return SendAsync<ItemModel>(HttpMethod.Post, "items", new
            {
                item.Name,
                item.Code,
                item.Brand,
                item.UnitPrice,
                item.UnitCost,
                item.ReorderLevel
            });
        }

        public Task UpdateItemAsync(ItemModel item)
        {
            return SendAsync(HttpMethod.Put, $"items/{item.Id}", new
            {
                item.Name,
                item.Code,
                item.Brand,
                item.UnitPrice,
                item.ReorderLevel
            });
        }

        public Task RetireItemAsync(int id) => SendAsync(HttpMethod.Delete, $"items/{id}");

        public Task<List<SupplierModel>> GetSuppliersAsync(string? search = null)
        {
            var query = string.IsNullOrWhiteSpace(search) ? string.Empty : "?search=" + Uri.EscapeDataString(search.Trim());
            return GetAsync<List<SupplierModel>>("suppliers" + query);
        }

        public Task<SupplierModel> CreateSupplierAsync(SupplierModel supplier)
        {
            return SendAsync<SupplierModel>(HttpMethod.Post, "suppliers", new
            {
                supplier.Name,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address
            });
        }

        public Task UpdateSupplierAsync(SupplierModel supplier)
        {
            return SendAsync(HttpMethod.Put, $"suppliers/{supplier.Id}", new
            {
                supplier.Name,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address
            });
        }

        public Task RetireSupplierAsync(int id) => SendAsync(HttpMethod.Delete, $"suppliers/{id}");

        public Task ReceiveStockAsync(StockReceiptModel receipt) => SendAsync(HttpMethod.Post, "inventory/receipts", receipt);
        public Task IssueStockAsync(StockIssueModel issue) => SendAsync(HttpMethod.Post, "inventory/issues", issue);
        public Task AdjustStockAsync(StockAdjustmentModel adjustment) => SendAsync(HttpMethod.Post, "inventory/adjustments", adjustment);
        public Task<InventorySummaryModel> GetSummaryAsync() => GetAsync<InventorySummaryModel>("reports/inventory-summary");

        public Task<List<InventoryMovementModel>> GetMovementsAsync(InventoryMovementFilterModel filter)
        {
            var query = new List<string>();
            if (filter.ItemId.HasValue) query.Add("ItemId=" + filter.ItemId.Value.ToString(CultureInfo.InvariantCulture));
            if (filter.SupplierId.HasValue) query.Add("SupplierId=" + filter.SupplierId.Value.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(filter.MovementType)) query.Add("MovementType=" + Uri.EscapeDataString(filter.MovementType));
            if (filter.FromUtc.HasValue) query.Add("FromUtc=" + Uri.EscapeDataString(filter.FromUtc.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
            if (filter.ToUtc.HasValue) query.Add("ToUtc=" + Uri.EscapeDataString(filter.ToUtc.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
            return GetAsync<List<InventoryMovementModel>>("inventory/movements" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query)));
        }

        public void Dispose() => _httpClient.Dispose();

        private async Task<T> GetAsync<T>(string path)
        {
            var response = await SendCoreAsync(HttpMethod.Get, path, null);
            return JsonSerializer.Deserialize<T>(response, JsonOptions) ?? throw new InvalidOperationException("The API returned an empty response.");
        }

        private async Task<T> SendAsync<T>(HttpMethod method, string path, object? payload)
        {
            var response = await SendCoreAsync(method, path, payload);
            return JsonSerializer.Deserialize<T>(response, JsonOptions) ?? throw new InvalidOperationException("The API returned an empty response.");
        }

        private async Task SendAsync(HttpMethod method, string path, object? payload = null)
        {
            await SendCoreAsync(method, path, payload);
        }

        private async Task<string> SendCoreAsync(HttpMethod method, string path, object? payload)
        {
            using var request = new HttpRequestMessage(method, new Uri(new Uri(_apiBase), path));
            if (payload != null) request.Content = JsonContent.Create(payload);
            if (!string.IsNullOrWhiteSpace(Token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var detail = string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase : content;
                throw new HttpRequestException($"API request failed ({(int)response.StatusCode}): {detail}", null, response.StatusCode);
            }
            return content;
        }

        private sealed class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
