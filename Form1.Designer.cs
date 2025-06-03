using System.Drawing;
using System.Windows.Forms;

namespace PixabayWinForms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelImages;
        private System.Windows.Forms.CheckBox chkSelectAll; // Add "Select All" checkbox

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes form controls
        /// </summary>
        private void InitializeComponent()
        {
            txtSearch = new TextBox();
            btnSearch = new Button();
            flowLayoutPanelImages = new FlowLayoutPanel();
            chkSelectAll = new System.Windows.Forms.CheckBox(); // Add "Select All" checkbox
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(12, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(500, 23);
            txtSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(620, 10);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 26);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // chkSelectAll
            // 
            chkSelectAll.Location = new Point(12, 50); // Positioned below the search box
            chkSelectAll.Name = "chkSelectAll";
            chkSelectAll.Size = new Size(100, 20);
            chkSelectAll.TabIndex = 2;
            chkSelectAll.Text = "Select All";
            chkSelectAll.UseVisualStyleBackColor = true;
            chkSelectAll.Checked = false; // Disabled by default
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
            // 
            // flowLayoutPanelImages
            // 
            flowLayoutPanelImages.AutoScroll = true;
            flowLayoutPanelImages.Location = new Point(12, 80); // Placed below the "Select All" checkbox
            flowLayoutPanelImages.Name = "flowLayoutPanelImages";
            flowLayoutPanelImages.Size = new Size(760, 520);
            flowLayoutPanelImages.TabIndex = 3;
            // 
            // Form1
            // 
            ClientSize = new Size(804, 890);
            Controls.Add(flowLayoutPanelImages);
            Controls.Add(chkSelectAll);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Name = "Michael_V";
            Text = "Image Search (Pixabay)";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
