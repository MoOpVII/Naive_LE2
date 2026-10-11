using InventoryDataLibrary.Models;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryWinForms
{
    internal sealed class ItemEditorForm : Form
    {
        private readonly ItemModel? _existing;
        private readonly TextBox _name = new();
        private readonly TextBox _code = new();
        private readonly TextBox _brand = new();
        private readonly NumericUpDown _unitPrice = CreateMoneyInput();
        private readonly NumericUpDown _unitCost = CreateMoneyInput();
        private readonly NumericUpDown _reorderLevel = new();
        private readonly Label _validation = new() { AutoSize = true, Tag = "validation" };

        public ItemEditorForm(ItemModel? item)
        {
            _existing = item;
            Text = item == null ? "Add Product" : "Edit Product";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(480, 390);

            _name.MaxLength = 200;
            _code.MaxLength = 50;
            _brand.MaxLength = 100;
            _unitCost.Maximum = 9999999999999999m;
            _reorderLevel.Maximum = int.MaxValue;
            _reorderLevel.DecimalPlaces = 0;
            _reorderLevel.ThousandsSeparator = true;

            if (item != null)
            {
                _name.Text = item.Name;
                _code.Text = item.Code;
                _brand.Text = item.Brand ?? string.Empty;
                _unitPrice.Value = item.UnitPrice;
                _unitCost.Value = item.UnitCost;
                _unitCost.ReadOnly = true;
                _reorderLevel.Value = item.ReorderLevel;
            }

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 8 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            AddField(layout, 0, "Product name", _name);
            AddField(layout, 1, "Product code", _code);
            AddField(layout, 2, "Brand", _brand);
            AddField(layout, 3, "Retail price (PHP)", _unitPrice);
            AddField(layout, 4, "Average unit cost", _unitCost);
            AddField(layout, 5, "Reorder level", _reorderLevel);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var save = new Button { Text = "Save", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            save.Click += (_, e) =>
            {
                if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_code.Text))
                {
                    DialogResult = DialogResult.None;
                    _validation.Text = "Product name and code are required.";
                }
                else _validation.Text = string.Empty;
            };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(_validation, 0, 6);
            layout.SetColumnSpan(_validation, 2);
            layout.Controls.Add(buttons, 0, 7);
            layout.SetColumnSpan(buttons, 2);
            Controls.Add(layout);
            InventoryTheme.ApplyDialog(this);
            InventoryTheme.ApplyButton(save, InventoryButtonStyle.Primary);
            InventoryTheme.ApplyButton(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        public ItemModel GetItem()
        {
            return new ItemModel
            {
                Id = _existing?.Id ?? 0,
                Name = _name.Text.Trim(),
                Code = _code.Text.Trim(),
                Brand = string.IsNullOrWhiteSpace(_brand.Text) ? null : _brand.Text.Trim(),
                UnitPrice = _unitPrice.Value,
                UnitCost = _existing?.UnitCost ?? _unitCost.Value,
                StockQuantity = _existing?.StockQuantity ?? 0,
                ReorderLevel = decimal.ToInt32(_reorderLevel.Value),
                IsActive = _existing?.IsActive ?? true
            };
        }

        private static NumericUpDown CreateMoneyInput()
        {
            return new NumericUpDown
            {
                DecimalPlaces = 2,
                Increment = 0.25m,
                Maximum = 9999999999999999m,
                ThousandsSeparator = true,
                Dock = DockStyle.Fill
            };
        }

        private static void AddField(TableLayoutPanel layout, int row, string label, Control input)
        {
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(5);
            layout.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
            layout.Controls.Add(input, 1, row);
        }
    }
}
