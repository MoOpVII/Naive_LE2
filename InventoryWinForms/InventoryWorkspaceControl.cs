using InventoryDataLibrary.Models;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace InventoryWinForms
{
    internal sealed class InventoryWorkspaceControl : UserControl
    {
        private readonly InventoryApiClient _api = new();
        private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, Enabled = false };
        private readonly Panel _accountHeader = new();
        private readonly TextBox _username = new() { Width = 170, PlaceholderText = "Username" };
        private readonly TextBox _password = new() { Width = 170, PlaceholderText = "Password", UseSystemPasswordChar = true };
        private readonly Label _status = new() { AutoSize = true, Text = "Sign in to manage inventory." };
        private readonly DataGridView _productsGrid = CreateGrid();
        private readonly DataGridView _suppliersGrid = CreateGrid();
        private readonly DataGridView _lowStockGrid = CreateGrid();
        private readonly DataGridView _movementsGrid = CreateGrid();
        private readonly TextBox _productSearch = new() { Width = 240, PlaceholderText = "Search name or code" };
        private readonly CheckBox _lowStockOnly = new() { Text = "Low stock only", AutoSize = true };
        private readonly TextBox _supplierSearch = new() { Width = 240, PlaceholderText = "Search supplier" };
        private readonly ComboBox _receiptItem = CreateLookup();
        private readonly ComboBox _receiptSupplier = CreateLookup();
        private readonly NumericUpDown _receiptQuantity = CreateQuantityInput(1);
        private readonly NumericUpDown _receiptUnitCost = CreateMoneyInput();
        private readonly TextBox _receiptReference = new() { MaxLength = 100 };
        private readonly TextBox _receiptNotes = new() { MaxLength = 500 };
        private readonly ComboBox _issueItem = CreateLookup();
        private readonly NumericUpDown _issueQuantity = CreateQuantityInput(1);
        private readonly TextBox _issueReference = new() { MaxLength = 100 };
        private readonly TextBox _issueNotes = new() { MaxLength = 500 };
        private readonly ComboBox _adjustItem = CreateLookup();
        private readonly NumericUpDown _adjustQuantity = new() { Minimum = -int.MaxValue, Maximum = int.MaxValue, Value = 1, ThousandsSeparator = true };
        private readonly TextBox _adjustReference = new() { MaxLength = 100 };
        private readonly TextBox _adjustNotes = new() { MaxLength = 500 };
        private readonly ComboBox _movementItem = CreateLookup(true);
        private readonly ComboBox _movementSupplier = CreateLookup(true);
        private readonly ComboBox _movementType = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly DateTimePicker _fromDate = CreateDatePicker();
        private readonly DateTimePicker _toDate = CreateDatePicker();
        private readonly Label _productCount = new() { AutoSize = true };
        private readonly Label _totalUnits = new() { AutoSize = true };
        private readonly Label _lowStockCount = new() { AutoSize = true };
        private readonly Label _costValue = new() { AutoSize = true };
        private readonly Label _retailValue = new() { AutoSize = true };
        private List<InventoryMovementModel> _loadedMovements = new();

        public InventoryWorkspaceControl()
        {
            Dock = DockStyle.Fill;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(18) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(BuildAccountPanel(), 0, 0);
            root.Controls.Add(_tabs, 0, 1);
            _tabs.TabPages.Add(BuildDashboardTab());
            _tabs.TabPages.Add(BuildProductsTab());
            _tabs.TabPages.Add(BuildSuppliersTab());
            _tabs.TabPages.Add(BuildStockOperationsTab());
            _tabs.TabPages.Add(BuildMovementsTab());
            Controls.Add(root);
            InventoryTheme.ApplyWorkspace(this);
            InventoryTheme.ApplyHeader(_accountHeader);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _api.Dispose();
            base.Dispose(disposing);
        }

        private Control BuildAccountPanel()
        {
            _accountHeader.Dock = DockStyle.Fill;
            _accountHeader.Height = 124;
            _accountHeader.MinimumSize = new System.Drawing.Size(0, 124);
            _accountHeader.Margin = new Padding(0, 0, 0, 14);
            _accountHeader.Padding = new Padding(18, 12, 18, 12);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63));

            var identity = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 5, 0, 0) };
            var brandMark = new Panel { Width = 48, Height = 48, Margin = new Padding(0, 5, 12, 0), Tag = "brand-mark" };
            brandMark.Controls.Add(new Label { Text = "N", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Font = InventoryTheme.SectionFont, ForeColor = System.Drawing.Color.White, BackColor = InventoryTheme.Copper });
            var brandText = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0, 3, 0, 0) };
            brandText.Controls.Add(new Label { Text = "NAIVE ELECTRONICS", AutoSize = true, Font = InventoryTheme.SectionFont });
            brandText.Controls.Add(new Label { Text = "INVENTORY MANAGEMENT", AutoSize = true, Font = InventoryTheme.BodyFont, Tag = "header-muted" });
            identity.Controls.Add(brandMark);
            identity.Controls.Add(brandText);

            var account = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            account.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            account.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            account.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            var loginRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 2, 0, 0)
            };
            var login = CreateButton("Sign in", async () => await LoginAsync(), InventoryButtonStyle.Primary);
            var register = CreateButton("Create account", async () => await RegisterAsync(), InventoryButtonStyle.Secondary);
            var logout = CreateButton("Sign out", async () => Logout(), InventoryButtonStyle.Secondary);
            _username.Width = 130;
            _password.Width = 130;
            loginRow.Controls.AddRange(new Control[] { _username, _password, login, register, logout });

            _status.AutoSize = false;
            _status.Dock = DockStyle.Fill;
            _status.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            account.Controls.Add(loginRow, 0, 0);
            account.Controls.Add(_status, 0, 1);

            layout.Controls.Add(identity, 0, 0);
            layout.Controls.Add(account, 1, 0);
            _accountHeader.Controls.Add(layout);
            return _accountHeader;
        }

        private TabPage BuildDashboardTab()
        {
            var page = new TabPage("Dashboard");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var metrics = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            metrics.Controls.AddRange(new Control[]
            {
                Metric("Active products", _productCount), Metric("Units on hand", _totalUnits),
                Metric("Low-stock products", _lowStockCount), Metric("Inventory cost value", _costValue), Metric("Retail value", _retailValue),
                CreateButton("Refresh", async () => await RefreshDashboardAsync())
            });
            layout.Controls.Add(metrics, 0, 0);
            layout.Controls.Add(_lowStockGrid, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildProductsTab()
        {
            var page = new TabPage("Products");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            toolbar.Controls.AddRange(new Control[]
            {
                _productSearch, _lowStockOnly,
                CreateButton("Search", async () => await RefreshProductsAsync()),
                CreateButton("Add product", async () => await AddProductAsync(), InventoryButtonStyle.Primary),
                CreateButton("Edit selected", async () => await EditProductAsync()),
                CreateButton("Retire selected", async () => await RetireProductAsync(), InventoryButtonStyle.Destructive),
                CreateButton("Refresh", async () => await RefreshProductsAsync())
            });
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_productsGrid, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildSuppliersTab()
        {
            var page = new TabPage("Suppliers");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            toolbar.Controls.AddRange(new Control[]
            {
                _supplierSearch,
                CreateButton("Search", async () => await RefreshSuppliersAsync()),
                CreateButton("Add supplier", async () => await AddSupplierAsync(), InventoryButtonStyle.Primary),
                CreateButton("Edit selected", async () => await EditSupplierAsync()),
                CreateButton("Retire selected", async () => await RetireSupplierAsync(), InventoryButtonStyle.Destructive),
                CreateButton("Refresh", async () => await RefreshSuppliersAsync())
            });
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_suppliersGrid, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildStockOperationsTab()
        {
            var page = new TabPage("Stock Operations");
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, Padding = new Padding(8) };
            flow.Controls.Add(BuildReceiptGroup());
            flow.Controls.Add(BuildIssueGroup());
            flow.Controls.Add(BuildAdjustmentGroup());
            page.Controls.Add(flow);
            return page;
        }

        private TabPage BuildMovementsTab()
        {
            var page = new TabPage("Movement History & Reports");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _movementType.Items.AddRange(new object[] { "All types", "Receipt", "Issue", "Adjustment" });
            _movementType.SelectedIndex = 0;
            _fromDate.ShowCheckBox = true;
            _toDate.ShowCheckBox = true;
            _fromDate.Checked = false;
            _toDate.Checked = false;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            toolbar.Controls.AddRange(new Control[]
            {
                LabeledControl("Item", _movementItem), LabeledControl("Supplier", _movementSupplier),
                LabeledControl("Type", _movementType), LabeledControl("From", _fromDate), LabeledControl("To", _toDate),
                CreateButton("Filter", async () => await RefreshMovementsAsync()),
                CreateButton("Export CSV", async () => await ExportMovementsAsync(), InventoryButtonStyle.Primary)
            });
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_movementsGrid, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private Control BuildReceiptGroup()
        {
            var group = CreateOperationPanel("Receive Stock", out var panel);
            AddOperationField(panel, 0, "Product", _receiptItem);
            AddOperationField(panel, 1, "Supplier", _receiptSupplier);
            AddOperationField(panel, 2, "Quantity", _receiptQuantity);
            AddOperationField(panel, 3, "Unit Cost (PHP)", _receiptUnitCost);
            AddOperationField(panel, 4, "Reference", _receiptReference);
            AddOperationField(panel, 5, "Notes", _receiptNotes);
            AddOperationButton(panel, "Record Receipt", async () => await ReceiveStockAsync());
            return group;
        }

        private Control BuildIssueGroup()
        {
            var group = CreateOperationPanel("Issue Stock", out var panel);
            AddOperationField(panel, 0, "Product", _issueItem);
            AddOperationField(panel, 2, "Quantity", _issueQuantity);
            AddOperationField(panel, 4, "Reference", _issueReference);
            AddOperationField(panel, 5, "Notes", _issueNotes);
            AddOperationButton(panel, "Record Issue", async () => await IssueStockAsync());
            return group;
        }

        private Control BuildAdjustmentGroup()
        {
            var group = CreateOperationPanel("Adjust Stock", out var panel);
            AddOperationField(panel, 0, "Product", _adjustItem);
            AddOperationField(panel, 2, "Quantity Change", _adjustQuantity);
            AddOperationField(panel, 4, "Reference", _adjustReference);
            AddOperationField(panel, 5, "Notes", _adjustNotes);
            AddOperationButton(panel, "Record Adjustment", async () => await AdjustStockAsync());
            return group;
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_password.Text))
                throw new InvalidOperationException("Enter a username and password.");
            _api.Token = await _api.LoginAsync(_username.Text.Trim(), _password.Text);
            _tabs.Enabled = true;
            _status.Text = "Signed in.";
            await RefreshAllAsync();
        }

        private async Task RegisterAsync()
        {
            using var form = new RegistrationForm();
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _api.RegisterAsync(form.Username, form.FirstName, form.LastName, form.Password);
            _status.Text = "Registration successful. Sign in to continue.";
            MessageBox.Show(FindForm(), "Registration successful. Sign in to continue.", "Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Task Logout()
        {
            _api.Token = null;
            _tabs.Enabled = false;
            _password.Clear();
            _status.Text = "Signed out.";
            _productsGrid.DataSource = null;
            _suppliersGrid.DataSource = null;
            _movementsGrid.DataSource = null;
            return Task.CompletedTask;
        }

        private async Task RefreshAllAsync()
        {
            await RefreshProductsAsync();
            await RefreshSuppliersAsync();
            await RefreshDashboardAsync();
            await RefreshMovementsAsync();
        }

        private async Task RefreshProductsAsync()
        {
            var items = await _api.GetItemsAsync(_productSearch.Text, _lowStockOnly.Checked);
            _productsGrid.DataSource = items;
            FormatGrid(_productsGrid);
            RefreshLookup(_receiptItem, items, false);
            RefreshLookup(_issueItem, items, false);
            RefreshLookup(_adjustItem, items, false);
            RefreshLookup(_movementItem, items, true);
        }

        private async Task RefreshSuppliersAsync()
        {
            var suppliers = await _api.GetSuppliersAsync(_supplierSearch.Text);
            _suppliersGrid.DataSource = suppliers;
            FormatGrid(_suppliersGrid);
            RefreshSupplierLookup(_receiptSupplier, suppliers, false);
            RefreshSupplierLookup(_movementSupplier, suppliers, true);
        }

        private async Task RefreshDashboardAsync()
        {
            var summary = await _api.GetSummaryAsync();
            _productCount.Text = summary.ProductCount.ToString("N0", CultureInfo.CurrentCulture);
            _totalUnits.Text = summary.TotalUnits.ToString("N0", CultureInfo.CurrentCulture);
            _lowStockCount.Text = summary.LowStockCount.ToString("N0", CultureInfo.CurrentCulture);
            _costValue.Text = "₱ " + summary.CostValue.ToString("N2", CultureInfo.InvariantCulture);
            _retailValue.Text = "₱ " + summary.RetailValue.ToString("N2", CultureInfo.InvariantCulture);
            _lowStockGrid.DataSource = await _api.GetItemsAsync(lowStockOnly: true);
            FormatGrid(_lowStockGrid);
        }

        private async Task RefreshMovementsAsync()
        {
            var filter = new InventoryMovementFilterModel
            {
                ItemId = GetLookupId(_movementItem),
                SupplierId = GetLookupId(_movementSupplier),
                MovementType = _movementType.SelectedIndex > 0 ? Convert.ToString(_movementType.SelectedItem) : null,
                FromUtc = _fromDate.Checked ? _fromDate.Value.Date : null,
                ToUtc = _toDate.Checked ? _toDate.Value.Date.AddDays(1) : null
            };
            _loadedMovements = await _api.GetMovementsAsync(filter);
            _movementsGrid.DataSource = _loadedMovements;
            FormatGrid(_movementsGrid);
        }

        private async Task AddProductAsync()
        {
            using var form = new ItemEditorForm(null);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _api.CreateItemAsync(form.GetItem());
            await RefreshAllAsync();
        }

        private async Task EditProductAsync()
        {
            var item = Selected<ItemModel>(_productsGrid);
            if (item == null) throw new InvalidOperationException("Select a product first.");
            using var form = new ItemEditorForm(item);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _api.UpdateItemAsync(form.GetItem());
            await RefreshAllAsync();
        }

        private async Task RetireProductAsync()
        {
            var item = Selected<ItemModel>(_productsGrid);
            if (item == null) throw new InvalidOperationException("Select a product first.");
            if (MessageBox.Show(FindForm(), $"Retire {item.Name}? Products with stock on hand cannot be retired.", "Retire product", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            await _api.RetireItemAsync(item.Id);
            await RefreshAllAsync();
        }

        private async Task AddSupplierAsync()
        {
            using var form = new SupplierEditorForm(null);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _api.CreateSupplierAsync(form.GetSupplier());
            await RefreshSuppliersAsync();
        }

        private async Task EditSupplierAsync()
        {
            var supplier = Selected<SupplierModel>(_suppliersGrid);
            if (supplier == null) throw new InvalidOperationException("Select a supplier first.");
            using var form = new SupplierEditorForm(supplier);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
            await _api.UpdateSupplierAsync(form.GetSupplier());
            await RefreshSuppliersAsync();
        }

        private async Task RetireSupplierAsync()
        {
            var supplier = Selected<SupplierModel>(_suppliersGrid);
            if (supplier == null) throw new InvalidOperationException("Select a supplier first.");
            if (MessageBox.Show(FindForm(), $"Retire {supplier.Name}?", "Retire supplier", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            await _api.RetireSupplierAsync(supplier.Id);
            await RefreshSuppliersAsync();
        }

        private async Task ReceiveStockAsync()
        {
            var itemId = GetLookupId(_receiptItem);
            var supplierId = GetLookupId(_receiptSupplier);
            if (!itemId.HasValue || !supplierId.HasValue) throw new InvalidOperationException("Select both a product and supplier.");
            await _api.ReceiveStockAsync(new StockReceiptModel
            {
                ItemId = itemId.Value,
                SupplierId = supplierId.Value,
                Quantity = decimal.ToInt32(_receiptQuantity.Value),
                UnitCost = _receiptUnitCost.Value,
                ExternalReference = Clean(_receiptReference.Text),
                Notes = Clean(_receiptNotes.Text)
            });
            _receiptReference.Clear();
            _receiptNotes.Clear();
            await RefreshAllAsync();
        }

        private async Task IssueStockAsync()
        {
            var itemId = GetLookupId(_issueItem);
            if (!itemId.HasValue) throw new InvalidOperationException("Select a product.");
            await _api.IssueStockAsync(new StockIssueModel
            {
                ItemId = itemId.Value,
                Quantity = decimal.ToInt32(_issueQuantity.Value),
                ExternalReference = Clean(_issueReference.Text),
                Notes = Clean(_issueNotes.Text)
            });
            _issueReference.Clear();
            _issueNotes.Clear();
            await RefreshAllAsync();
        }

        private async Task AdjustStockAsync()
        {
            var itemId = GetLookupId(_adjustItem);
            if (!itemId.HasValue) throw new InvalidOperationException("Select a product.");
            var quantity = decimal.ToInt32(_adjustQuantity.Value);
            if (quantity == 0) throw new InvalidOperationException("Adjustment quantity must not be zero.");
            await _api.AdjustStockAsync(new StockAdjustmentModel
            {
                ItemId = itemId.Value,
                QuantityChange = quantity,
                ExternalReference = Clean(_adjustReference.Text),
                Notes = Clean(_adjustNotes.Text)
            });
            _adjustReference.Clear();
            _adjustNotes.Clear();
            await RefreshAllAsync();
        }

        private async Task ExportMovementsAsync()
        {
            using var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "inventory-movements.csv", DefaultExt = "csv" };
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
            var builder = new StringBuilder();
            builder.AppendLine("Id,Item,Code,Supplier,Type,QuantityChange,UnitCost,Reference,Notes,CreatedBy,CreatedAtUtc");
            foreach (var row in _loadedMovements)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    row.Id.ToString(CultureInfo.InvariantCulture), Csv(row.ItemName), Csv(row.ItemCode), Csv(row.SupplierName),
                    Csv(row.MovementType), row.QuantityChange.ToString(CultureInfo.InvariantCulture),
                    row.UnitCost.ToString(CultureInfo.InvariantCulture), Csv(row.ExternalReference), Csv(row.Notes),
                    Csv(row.CreatedByUserName), row.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture)
                }));
            }
            await File.WriteAllTextAsync(dialog.FileName, builder.ToString(), new UTF8Encoding(true));
            _status.Text = $"Exported {_loadedMovements.Count:N0} movement records.";
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.Color.White
            };
        }

        private static void FormatGrid(DataGridView grid)
        {
            if (grid.Columns[nameof(ItemModel.Id)] != null) grid.Columns[nameof(ItemModel.Id)].Visible = false;
            if (grid.Columns[nameof(ItemModel.UnitPrice)] != null)
            {
                grid.Columns[nameof(ItemModel.UnitPrice)].DefaultCellStyle.Format = "'₱ ' #,##0.00";
                grid.Columns[nameof(ItemModel.UnitPrice)].DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
            }
            if (grid.Columns[nameof(ItemModel.UnitCost)] != null)
            {
                grid.Columns[nameof(ItemModel.UnitCost)].DefaultCellStyle.Format = "'₱ ' #,##0.00";
                grid.Columns[nameof(ItemModel.UnitCost)].DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
            }
            if (grid.Columns[nameof(InventoryMovementModel.Id)] != null) grid.Columns[nameof(InventoryMovementModel.Id)].Visible = false;
            if (grid.Columns[nameof(InventoryMovementModel.CreatedAtUtc)] != null) grid.Columns[nameof(InventoryMovementModel.CreatedAtUtc)].DefaultCellStyle.Format = "g";
        }

        private static Button CreateButton(string text, Func<Task> action, InventoryButtonStyle style = InventoryButtonStyle.Secondary)
        {
            var button = new Button { Text = text, AutoSize = true, Margin = new Padding(5), Tag = style };
            button.Click += async (_, _) => await RunActionAsync(button, action);
            return button;
        }

        private static async Task RunActionAsync(Control parent, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(parent.FindForm(), ex.Message, "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Control Metric(string title, Label value)
        {
            var panel = new Panel { Width = 215, Height = 92, Margin = new Padding(6), Padding = new Padding(12), Tag = "metric" };
            panel.Controls.Add(new Label
            {
                Text = title,
                Location = new System.Drawing.Point(12, 8),
                Size = new System.Drawing.Size(panel.Width - 24, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Tag = "metric-title"
            });
            value.AutoSize = true;
            value.Location = new System.Drawing.Point(12, 34);
            value.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            value.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            value.Tag = "metric-value";
            panel.Controls.Add(value);
            value.BringToFront();
            return panel;
        }

        private static ComboBox CreateLookup(bool includeAll = false)
        {
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            if (includeAll) combo.Items.Add(new LookupOption(null, "All"));
            return combo;
        }

        private static NumericUpDown CreateQuantityInput(int minimum)
        {
            return new NumericUpDown { Minimum = minimum, Maximum = int.MaxValue, Value = minimum, DecimalPlaces = 0, ThousandsSeparator = true };
        }

        private static NumericUpDown CreateMoneyInput()
        {
            return new NumericUpDown { Minimum = 0, Maximum = 9999999999999999m, DecimalPlaces = 2, Increment = 0.25m, ThousandsSeparator = true };
        }

        private static DateTimePicker CreateDatePicker()
        {
            return new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd", Width = 130 };
        }

        private static Control MetricControl(string label, Control control)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, Margin = new Padding(4) };
            panel.Controls.Add(new Label { Text = label, AutoSize = true });
            panel.Controls.Add(control);
            return panel;
        }

        private static Control LabeledControl(string label, Control control) => MetricControl(label, control);

        private static void AddOperationField(Control panel, int row, string label, Control input)
        {
            if (panel is not TableLayoutPanel layout) return;
            layout.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(5),
                AutoEllipsis = true
            }, 0, row);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(5);
            layout.Controls.Add(input, 1, row);
        }

        private static void AddOperationButton(Control panel, string text, Func<Task> action)
        {
            if (panel is not TableLayoutPanel layout) return;
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var button = CreateButton(text, action, InventoryButtonStyle.Primary);
            button.Anchor = AnchorStyles.Bottom;
            button.Margin = new Padding(0, 0, 0, 14);
            footer.Controls.Add(button, 1, 0);
            layout.Controls.Add(footer, 0, 6);
            layout.SetColumnSpan(footer, 2);
        }

        private static Control CreateOperationPanel(string title, out TableLayoutPanel layout)
        {
            var group = new GroupBox { Text = title, Width = 390, Height = 410, Padding = new Padding(10), Margin = new Padding(8) };
            layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var row = 0; row < 6; row++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            group.Controls.Add(layout);
            return group;
        }

        private static void RefreshLookup(ComboBox combo, IReadOnlyList<ItemModel> items, bool includeAll)
        {
            var selected = GetLookupId(combo);
            combo.Items.Clear();
            if (includeAll) combo.Items.Add(new LookupOption(null, "All products"));
            foreach (var item in items) combo.Items.Add(new LookupOption(item.Id, $"{item.Name} ({item.Code})"));
            SelectLookup(combo, selected, includeAll);
        }

        private static void RefreshSupplierLookup(ComboBox combo, IReadOnlyList<SupplierModel> suppliers, bool includeAll)
        {
            var selected = GetLookupId(combo);
            combo.Items.Clear();
            if (includeAll) combo.Items.Add(new LookupOption(null, "All suppliers"));
            foreach (var supplier in suppliers) combo.Items.Add(new LookupOption(supplier.Id, supplier.Name));
            SelectLookup(combo, selected, includeAll);
        }

        private static void SelectLookup(ComboBox combo, int? selectedId, bool includeAll)
        {
            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is LookupOption option && option.Id == selectedId)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = includeAll ? 0 : (combo.Items.Count > 0 ? 0 : -1);
        }

        private static int? GetLookupId(ComboBox combo)
        {
            return combo.SelectedItem is LookupOption option ? option.Id : null;
        }

        private static T? Selected<T>(DataGridView grid) where T : class => grid.CurrentRow?.DataBoundItem as T;
        private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string Csv(string? value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

        private sealed record LookupOption(int? Id, string Name)
        {
            public override string ToString() => Name;
        }
    }
}
