using System.Drawing;
using System.Windows.Forms;

namespace InventoryWinForms
{
    internal enum InventoryButtonStyle
    {
        Primary,
        Secondary,
        Destructive
    }

    internal static class InventoryTheme
    {
        public static readonly Color Charcoal = Color.FromArgb(37, 42, 46);
        public static readonly Color CharcoalLight = Color.FromArgb(55, 61, 65);
        public static readonly Color Canvas = Color.FromArgb(246, 243, 238);
        public static readonly Color Surface = Color.FromArgb(255, 254, 251);
        public static readonly Color Border = Color.FromArgb(220, 214, 205);
        public static readonly Color Text = Color.FromArgb(45, 49, 51);
        public static readonly Color MutedText = Color.FromArgb(105, 108, 107);
        public static readonly Color Copper = Color.FromArgb(184, 84, 43);
        public static readonly Color CopperHover = Color.FromArgb(158, 67, 33);
        public static readonly Color CopperTint = Color.FromArgb(246, 229, 217);
        public static readonly Color Amber = Color.FromArgb(221, 154, 49);
        public static readonly Color DisabledSurface = Color.FromArgb(232, 229, 224);
        public static readonly Color DisabledText = Color.FromArgb(137, 138, 135);
        public static readonly Color Success = Color.FromArgb(50, 119, 87);
        public static readonly Color Danger = Color.FromArgb(161, 61, 54);

        public static readonly Font BodyFont = new("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font ButtonFont = new("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        public static readonly Font SectionFont = new("Segoe UI Semibold", 12F, FontStyle.Bold);
        public static readonly Font MetricFont = new("Segoe UI Semibold", 18F, FontStyle.Bold);

        public static void ApplyWorkspace(Control root)
        {
            root.BackColor = Canvas;
            root.ForeColor = Text;
            root.Font = BodyFont;
            ApplyControlTree(root);
        }

        public static void ApplyHeader(Control header)
        {
            header.BackColor = Charcoal;
            header.ForeColor = Color.White;
            foreach (Control child in header.Controls)
                ApplyHeaderChild(child);
        }

        public static void ApplyHeaderChild(Control control)
        {
            control.ForeColor = Color.White;
            if (Equals(control.Tag, "brand-mark"))
            {
                control.BackColor = Copper;
                return;
            }
            if (control is Button button)
            {
                var style = button.Tag is InventoryButtonStyle buttonStyle ? buttonStyle : InventoryButtonStyle.Secondary;
                ApplyButton(button, style, onDarkSurface: true);
                return;
            }

            if (control is TextBox or ComboBox or NumericUpDown)
            {
                ApplyInput(control);
                control.BackColor = CharcoalLight;
                control.ForeColor = Color.White;
                return;
            }

            if (control is Label label)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = Equals(label.Tag, "header-muted") ? Color.FromArgb(211, 207, 200) : Color.White;
                return;
            }

            control.BackColor = Charcoal;
            if (control.HasChildren)
                foreach (Control child in control.Controls)
                    ApplyHeaderChild(child);
        }

        public static void ApplyButton(Button button, InventoryButtonStyle style = InventoryButtonStyle.Secondary, bool onDarkSurface = false)
        {
            var baseColor = style switch
            {
                InventoryButtonStyle.Primary => Copper,
                InventoryButtonStyle.Destructive => Danger,
                _ => onDarkSurface ? CharcoalLight : Surface
            };
            var hoverColor = style switch
            {
                InventoryButtonStyle.Primary => CopperHover,
                InventoryButtonStyle.Destructive => Color.FromArgb(133, 47, 42),
                _ => onDarkSurface ? Color.FromArgb(72, 79, 83) : CopperTint
            };
            var textColor = style == InventoryButtonStyle.Secondary && !onDarkSurface ? Text : Color.White;
            var borderColor = style == InventoryButtonStyle.Secondary && !onDarkSurface ? Border : baseColor;

            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = style == InventoryButtonStyle.Secondary && !onDarkSurface ? 1 : 0;
            button.FlatAppearance.BorderColor = borderColor;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
            button.FlatAppearance.MouseDownBackColor = style == InventoryButtonStyle.Primary ? CopperHover : hoverColor;
            button.BackColor = baseColor;
            button.ForeColor = textColor;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(10, 5, 10, 5);
            button.EnabledChanged += (_, _) =>
            {
                button.BackColor = button.Enabled ? baseColor : DisabledSurface;
                button.ForeColor = button.Enabled ? textColor : DisabledText;
            };
        }

        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Charcoal;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = ButtonFont;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Charcoal;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = CopperTint;
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.Padding = new Padding(6, 3, 6, 3);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Canvas;
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 40;
            grid.RowHeadersDefaultCellStyle.BackColor = Charcoal;
            grid.RowHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        public static void ApplyInput(Control control)
        {
            control.BackColor = Surface;
            control.ForeColor = Text;
            control.Font = BodyFont;
            if (control is TextBox textBox)
                textBox.BorderStyle = BorderStyle.FixedSingle;
            if (control is ComboBox comboBox)
                comboBox.FlatStyle = FlatStyle.Flat;
            if (control is NumericUpDown numericUpDown)
                numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            if (control is DateTimePicker dateTimePicker)
            {
                dateTimePicker.CalendarTitleBackColor = Charcoal;
                dateTimePicker.CalendarTitleForeColor = Color.White;
                dateTimePicker.CalendarMonthBackground = Surface;
            }
        }

        public static void ApplyDialog(Form form)
        {
            form.BackColor = Canvas;
            form.ForeColor = Text;
            form.Font = BodyFont;
            ApplyControlTree(form);
        }

        public static void ApplyMetric(Panel panel, Label value)
        {
            panel.BackColor = Surface;
            panel.BorderStyle = BorderStyle.FixedSingle;
            value.ForeColor = Copper;
            value.Font = MetricFont;
        }

        public static void ApplyTabControl(TabControl tabs)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.ItemSize = new Size(146, 38);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.Padding = new Point(12, 5);
            tabs.DrawItem += (_, e) =>
            {
                var selected = e.Index == tabs.SelectedIndex;
                var bounds = e.Bounds;
                using var background = new SolidBrush(selected ? Copper : Charcoal);
                using var text = new SolidBrush(Color.White);
                e.Graphics.FillRectangle(background, bounds);
                TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, ButtonFont, bounds, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if (selected)
                {
                    using var marker = new Pen(Amber, 3);
                    e.Graphics.DrawLine(marker, bounds.Left + 8, bounds.Bottom - 2, bounds.Right - 8, bounds.Bottom - 2);
                }
            };
        }

        private static void ApplyControlTree(Control control)
        {
            if (control.Tag is string role && role == "metric")
            {
                if (control is Panel metricPanel)
                {
                    metricPanel.BackColor = Surface;
                    metricPanel.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            else if (Equals(control.Tag, "metric-value") && control is Label metricValue)
            {
                metricValue.ForeColor = Copper;
                metricValue.Font = MetricFont;
                metricValue.BackColor = Color.Transparent;
            }
            else if (Equals(control.Tag, "metric-title") && control is Label metricTitle)
            {
                metricTitle.ForeColor = MutedText;
                metricTitle.BackColor = Color.Transparent;
            }
            else if (Equals(control.Tag, "validation") && control is Label validation)
            {
                validation.ForeColor = Danger;
                validation.Font = ButtonFont;
                validation.BackColor = Color.Transparent;
            }
            else if (control is DataGridView grid)
                ApplyGrid(grid);
            else if (control is Button button)
                ApplyButton(button, button.Tag is InventoryButtonStyle buttonStyle ? buttonStyle : InventoryButtonStyle.Secondary);
            else if (control is TextBox or ComboBox or NumericUpDown or DateTimePicker)
                ApplyInput(control);
            else if (control is TabControl tabs)
            {
                tabs.BackColor = Canvas;
                tabs.ForeColor = Text;
                ApplyTabControl(tabs);
            }
            else if (control is GroupBox)
            {
                control.BackColor = Canvas;
                control.ForeColor = Charcoal;
                control.Font = SectionFont;
            }
            else if (control is Panel or TableLayoutPanel or FlowLayoutPanel or TabPage)
            {
                control.BackColor = Canvas;
                control.ForeColor = Text;
            }
            else if (control is Label label)
            {
                label.ForeColor = Text;
                label.BackColor = Color.Transparent;
            }

            foreach (Control child in control.Controls)
                ApplyControlTree(child);
        }
    }
}
