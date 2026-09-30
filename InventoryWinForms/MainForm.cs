using InventoryDataLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryWinForms
{
    public class MainForm : Form
    {
        private readonly string _apiBase = Environment.GetEnvironmentVariable("INVENTORY_API_BASE_URL") ?? "https://localhost:7060/api";
        private string _token;
        private HttpClient _http = new HttpClient();

        private TextBox txtUsername = new TextBox();
        private TextBox txtPassword = new TextBox() { PasswordChar = '*' };
        private TextBox txtFirstName = new TextBox();
        private TextBox txtLastName = new TextBox();
        private Button btnLogin = new Button() { Text = "Login" };
        private Button btnRegister = new Button() { Text = "Register" };

        private ComboBox cboBrands = new ComboBox();
        private TextBox txtSearchCode = new TextBox();
        private Button btnSearch = new Button() { Text = "Search" };
        private DataGridView itemsGrid = new DataGridView();
        private Button btnAdd = new Button() { Text = "Add Item" };
        private Button btnUpdatePrice = new Button() { Text = "Update Price" };

        public MainForm()
        {
            Text = "Naive's Electronics and Technology Inventory Manager";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1300, 850);
            MinimumSize = new System.Drawing.Size(1000, 650);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var accountPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            accountPanel.Controls.AddRange(new Control[] { txtUsername, txtPassword, txtFirstName, txtLastName, btnLogin, btnRegister });

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            filterPanel.Controls.AddRange(new Control[] { cboBrands, txtSearchCode, btnSearch });

            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 8, 0, 0)
            };
            actionPanel.Controls.AddRange(new Control[] { btnAdd, btnUpdatePrice });

            txtUsername.PlaceholderText = "Username";
            txtPassword.PlaceholderText = "Password";
            txtFirstName.PlaceholderText = "First Name";
            txtLastName.PlaceholderText = "Last Name";
            txtSearchCode.PlaceholderText = "Product Code";

            var inputFont = new System.Drawing.Font("Segoe UI", 12F);
            foreach (var input in new[] { txtUsername, txtPassword, txtFirstName, txtLastName, txtSearchCode })
            {
                input.AutoSize = false;
                input.Font = inputFont;
                input.Height = 42;
                input.Margin = new Padding(6, 8, 6, 8);
            }

            txtUsername.Width = 190;
            txtPassword.Width = 190;
            txtFirstName.Width = 150;
            txtLastName.Width = 150;
            txtSearchCode.Width = 300;
            cboBrands.Width = 250;
            cboBrands.Font = inputFont;
            cboBrands.Margin = new Padding(6, 8, 6, 8);
            btnLogin.Size = new System.Drawing.Size(110, 44);
            btnRegister.Size = new System.Drawing.Size(125, 44);
            btnSearch.Size = new System.Drawing.Size(135, 44);
            btnAdd.Size = new System.Drawing.Size(160, 48);
            btnUpdatePrice.Size = new System.Drawing.Size(210, 48);
            foreach (var button in new[] { btnLogin, btnRegister, btnSearch, btnAdd, btnUpdatePrice })
            {
                button.Font = inputFont;
                button.Margin = new Padding(6);
            }

            itemsGrid.Dock = DockStyle.Fill;
            itemsGrid.Margin = new Padding(0);
            itemsGrid.ReadOnly = true;
            itemsGrid.AllowUserToAddRows = false;
            itemsGrid.AllowUserToDeleteRows = false;
            itemsGrid.AllowUserToOrderColumns = true;
            itemsGrid.AutoGenerateColumns = false;
            itemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            itemsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            itemsGrid.MultiSelect = false;
            itemsGrid.RowHeadersVisible = false;
            itemsGrid.ColumnHeadersHeight = 48;
            itemsGrid.RowTemplate.Height = 40;
            itemsGrid.BackgroundColor = System.Drawing.Color.White;
            itemsGrid.Font = new System.Drawing.Font("Segoe UI", 12F);
            itemsGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            itemsGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn
                {
                    Name = "Name",
                    HeaderText = "Product",
                    SortMode = DataGridViewColumnSortMode.Automatic,
                    FillWeight = 35
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Code",
                    HeaderText = "Code",
                    SortMode = DataGridViewColumnSortMode.Automatic,
                    FillWeight = 20
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Brand",
                    HeaderText = "Brand",
                    SortMode = DataGridViewColumnSortMode.Automatic,
                    FillWeight = 25
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    HeaderText = "Unit Price (PHP)",
                    SortMode = DataGridViewColumnSortMode.Automatic,
                    FillWeight = 20,
                    ValueType = typeof(decimal),
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "'₱' #,##0.00",
                        FormatProvider = System.Globalization.CultureInfo.InvariantCulture,
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                });
            layout.Controls.Add(accountPanel, 0, 0);
            layout.Controls.Add(filterPanel, 0, 1);
            layout.Controls.Add(itemsGrid, 0, 2);
            layout.Controls.Add(actionPanel, 0, 3);
            Controls.Add(layout);

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;
            btnSearch.Click += BtnSearch_Click;
            btnAdd.Click += BtnAdd_Click;
            btnUpdatePrice.Click += BtnUpdatePrice_Click;

            Load += MainForm_Load;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadBrands();
            await LoadItems();
        }

        private async Task LoadBrands()
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, _apiBase + "/items/brands");
                if (!string.IsNullOrEmpty(_token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var resp = await _http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var brands = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    cboBrands.Items.Clear();
                    cboBrands.Items.Add("(All)");
                    if (brands != null)
                        cboBrands.Items.AddRange(brands.ToArray());
                    cboBrands.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async Task LoadItems(string brand = null, string code = null)
        {
            try
            {
                string url = _apiBase + "/items";
                if (!string.IsNullOrEmpty(brand)) url += "?brand=" + Uri.EscapeDataString(brand);
                if (!string.IsNullOrEmpty(code)) url += (url.Contains("?") ? "&" : "?") + "code=" + Uri.EscapeDataString(code);

                var req = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var resp = await _http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ItemModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    DisplayItems(items ?? new List<ItemModel>());
                }
                else if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    DisplayMessage("Login required to view inventory.");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage("Error loading items: " + ex.Message);
            }
        }

        private void DisplayItems(List<ItemModel> items)
        {
            itemsGrid.Rows.Clear();
            foreach (var item in items)
                itemsGrid.Rows.Add(item.Name, item.Code, item.Brand, item.UnitPrice);
        }

        private void DisplayMessage(string message)
        {
            itemsGrid.Rows.Clear();
            itemsGrid.Rows.Add(message);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            var payload = new { Username = txtUsername.Text, Password = txtPassword.Text };
            var json = JsonSerializer.Serialize(payload);
            var resp = await _http.PostAsync(_apiBase + "/auth/login", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                _token = doc.RootElement.GetProperty("token").GetString();
                await LoadBrands();
                await LoadItems();
                MessageBox.Show("Login successful.");
            }
            else
            {
                MessageBox.Show("Login failed, try again.");
            }
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Enter first and last name to register.");
                return;
            }

            var payload = new { Username = txtUsername.Text, FirstName = txtFirstName.Text, LastName = txtLastName.Text, Password = txtPassword.Text };
            var json = JsonSerializer.Serialize(payload);
            var resp = await _http.PostAsync(_apiBase + "/auth/register", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show("Registration successful. Please log in.");
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                MessageBox.Show("Username already exists.");
            }
            else
            {
                MessageBox.Show("Registration failed.");
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            string brand = (cboBrands.SelectedIndex > 0) ? cboBrands.SelectedItem.ToString() : null;
            await LoadItems(brand, txtSearchCode.Text);
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddItemForm(_apiBase, _token);
            form.ShowDialog();
            await LoadBrands();
            await LoadItems();
        }

        private async void BtnUpdatePrice_Click(object sender, EventArgs e)
        {
            if (itemsGrid.SelectedRows.Count == 0) { MessageBox.Show("Select an item first."); return; }
            var code = Convert.ToString(itemsGrid.SelectedRows[0].Cells["Code"].Value);
            if (string.IsNullOrWhiteSpace(code)) { MessageBox.Show("Select an item first."); return; }

            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new price in Philippine pesos (PHP):", "Update Price (PHP)", "0.00");
            if (decimal.TryParse(input, out decimal price))
            {
                var req = new HttpRequestMessage(HttpMethod.Put, _apiBase + $"/items/{Uri.EscapeDataString(code)}/price")
                {
                    Content = new StringContent(JsonSerializer.Serialize(price), System.Text.Encoding.UTF8, "application/json")
                };
                if (!string.IsNullOrEmpty(_token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var resp = await _http.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Price updated.");
                    await LoadItems();
                }
                else
                {
                    MessageBox.Show("Failed to update price.");
                }
            }
            else
            {
                MessageBox.Show("Invalid price.");
            }
        }
    }

    class AddItemForm : Form
    {
        private string _apiBase; private string _token; private HttpClient _http = new HttpClient();
        TextBox txtName = new TextBox();
        TextBox txtCode = new TextBox();
        TextBox txtBrand = new TextBox();
        TextBox txtPrice = new TextBox();
        Button btnSave = new Button() { Text = "Save" };

        public AddItemForm(string apiBase, string token)
        {
            _apiBase = apiBase; _token = token;
            Text = "Add Item";
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(620, 440);
            MinimumSize = new System.Drawing.Size(540, 400);

            txtName.PlaceholderText = "Name"; txtCode.PlaceholderText = "Code"; txtBrand.PlaceholderText = "Brand"; txtPrice.PlaceholderText = "Unit Price (PHP ₱)";
            var formFont = new System.Drawing.Font("Segoe UI", 12F);
            foreach (var input in new[] { txtName, txtCode, txtBrand, txtPrice })
            {
                input.AutoSize = false;
                input.Dock = DockStyle.Fill;
                input.Font = formFont;
                input.Margin = new Padding(6, 8, 6, 8);
            }

            btnSave.Size = new System.Drawing.Size(150, 48);
            btnSave.Font = formFont;
            btnSave.Margin = new Padding(6, 8, 6, 8);

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(18)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var row = 0; row < 4; row++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            formLayout.Controls.Add(txtName, 0, 0);
            formLayout.Controls.Add(txtCode, 0, 1);
            formLayout.Controls.Add(txtBrand, 0, 2);
            formLayout.Controls.Add(txtPrice, 0, 3);
            formLayout.Controls.Add(btnSave, 0, 4);
            Controls.Add(formLayout);
            btnSave.Click += BtnSave_Click;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Name and code required."); return; }
            if (!decimal.TryParse(txtPrice.Text, out decimal price)) { MessageBox.Show("Invalid price."); return; }

            var item = new InventoryDataLibrary.Models.ItemModel { Name = txtName.Text, Code = txtCode.Text, Brand = txtBrand.Text, UnitPrice = price };
            var json = JsonSerializer.Serialize(item);
            var req = new HttpRequestMessage(HttpMethod.Post, _apiBase + "/items") { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
            if (!string.IsNullOrEmpty(_token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) { MessageBox.Show("Added."); Close(); }
            else if (resp.StatusCode == System.Net.HttpStatusCode.Conflict) MessageBox.Show("Duplicate item code.");
            else MessageBox.Show("Failed to add item.");
        }
    }
}

