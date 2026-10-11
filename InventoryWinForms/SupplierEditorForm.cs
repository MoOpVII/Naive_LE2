using InventoryDataLibrary.Models;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryWinForms
{
    internal sealed class SupplierEditorForm : Form
    {
        private readonly SupplierModel? _existing;
        private readonly TextBox _name = new();
        private readonly TextBox _contactName = new();
        private readonly TextBox _email = new();
        private readonly TextBox _phone = new();
        private readonly TextBox _address = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly Label _validation = new() { AutoSize = true, Tag = "validation" };

        public SupplierEditorForm(SupplierModel? supplier)
        {
            _existing = supplier;
            Text = supplier == null ? "Add Supplier" : "Edit Supplier";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(500, 420);

            _name.MaxLength = 200;
            _contactName.MaxLength = 100;
            _email.MaxLength = 254;
            _phone.MaxLength = 30;
            if (supplier != null)
            {
                _name.Text = supplier.Name;
                _contactName.Text = supplier.ContactName;
                _email.Text = supplier.Email;
                _phone.Text = supplier.Phone;
                _address.Text = supplier.Address;
            }

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            AddField(layout, 0, "Supplier name", _name);
            AddField(layout, 1, "Contact name", _contactName);
            AddField(layout, 2, "Email", _email);
            AddField(layout, 3, "Phone", _phone);
            AddField(layout, 4, "Address", _address);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var save = new Button { Text = "Save", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            save.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(_name.Text))
                {
                    DialogResult = DialogResult.None;
                    _validation.Text = "Supplier name is required.";
                }
                else _validation.Text = string.Empty;
            };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(_validation, 0, 5);
            layout.SetColumnSpan(_validation, 2);
            layout.Controls.Add(buttons, 0, 6);
            layout.SetColumnSpan(buttons, 2);
            Controls.Add(layout);
            InventoryTheme.ApplyDialog(this);
            InventoryTheme.ApplyButton(save, InventoryButtonStyle.Primary);
            InventoryTheme.ApplyButton(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        public SupplierModel GetSupplier()
        {
            return new SupplierModel
            {
                Id = _existing?.Id ?? 0,
                Name = _name.Text.Trim(),
                ContactName = Clean(_contactName.Text),
                Email = Clean(_email.Text),
                Phone = Clean(_phone.Text),
                Address = Clean(_address.Text),
                IsActive = _existing?.IsActive ?? true
            };
        }

        private static string? Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void AddField(TableLayoutPanel layout, int row, string label, Control input)
        {
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(5);
            layout.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
            layout.Controls.Add(input, 1, row);
        }
    }
}
