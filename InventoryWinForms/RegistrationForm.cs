using System.Drawing;
using System.Windows.Forms;

namespace InventoryWinForms
{
    internal sealed class RegistrationForm : Form
    {
        private readonly TextBox _username = new();
        private readonly TextBox _firstName = new();
        private readonly TextBox _lastName = new();
        private readonly TextBox _password = new() { UseSystemPasswordChar = true };
        private readonly Label _validation = new() { AutoSize = true, Tag = "validation" };

        public RegistrationForm()
        {
            Text = "Create Inventory Account";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(440, 370);

            _username.MaxLength = 16;
            _firstName.MaxLength = 50;
            _lastName.MaxLength = 50;
            _password.MaxLength = 128;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 6 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            AddField(layout, 0, "Username", _username);
            AddField(layout, 1, "First name", _firstName);
            AddField(layout, 2, "Last name", _lastName);
            AddField(layout, 3, "Password", _password);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var register = new Button { Text = "Register", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            register.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_firstName.Text) ||
                    string.IsNullOrWhiteSpace(_lastName.Text) || _password.Text.Length < 8)
                {
                    DialogResult = DialogResult.None;
                    _validation.Text = "Enter all fields; password must be at least 8 characters.";
                }
                else _validation.Text = string.Empty;
            };
            buttons.Controls.Add(register);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(_validation, 0, 4);
            layout.SetColumnSpan(_validation, 2);
            layout.Controls.Add(buttons, 0, 5);
            layout.SetColumnSpan(buttons, 2);
            Controls.Add(layout);
            InventoryTheme.ApplyDialog(this);
            InventoryTheme.ApplyButton(register, InventoryButtonStyle.Primary);
            InventoryTheme.ApplyButton(cancel);
            AcceptButton = register;
            CancelButton = cancel;
        }

        public string Username => _username.Text.Trim();
        public string FirstName => _firstName.Text.Trim();
        public string LastName => _lastName.Text.Trim();
        public string Password => _password.Text;

        private static void AddField(TableLayoutPanel layout, int row, string label, Control input)
        {
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(5);
            layout.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
            layout.Controls.Add(input, 1, row);
        }
    }
}
