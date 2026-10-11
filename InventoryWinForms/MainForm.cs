using System.Drawing;
using System.Windows.Forms;

namespace InventoryWinForms
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "Naive Electronics & Technology Inventory";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InventoryTheme.Canvas;
            ForeColor = InventoryTheme.Text;
            Font = InventoryTheme.BodyFont;
            ClientSize = new Size(1380, 920);
            MinimumSize = new Size(1080, 700);
            Controls.Add(new InventoryWorkspaceControl { Dock = DockStyle.Fill });
        }
    }
}
