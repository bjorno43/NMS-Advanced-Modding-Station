namespace AdvancedModdingStation
{
    partial class HelpForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Beginners Guide");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Import Existing Mod");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("FAQ");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpForm));
            this.splitContainerHelp = new System.Windows.Forms.SplitContainer();
            this.listViewHelp = new System.Windows.Forms.ListView();
            this.richTextBoxImportGuide = new System.Windows.Forms.RichTextBox();
            this.richTextBoxBeginnersGuide = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerHelp)).BeginInit();
            this.splitContainerHelp.Panel1.SuspendLayout();
            this.splitContainerHelp.Panel2.SuspendLayout();
            this.splitContainerHelp.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerHelp
            // 
            this.splitContainerHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerHelp.IsSplitterFixed = true;
            this.splitContainerHelp.Location = new System.Drawing.Point(0, 0);
            this.splitContainerHelp.Name = "splitContainerHelp";
            // 
            // splitContainerHelp.Panel1
            // 
            this.splitContainerHelp.Panel1.Controls.Add(this.listViewHelp);
            // 
            // splitContainerHelp.Panel2
            // 
            this.splitContainerHelp.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainerHelp.Panel2.Controls.Add(this.richTextBoxImportGuide);
            this.splitContainerHelp.Panel2.Controls.Add(this.richTextBoxBeginnersGuide);
            this.splitContainerHelp.Size = new System.Drawing.Size(748, 450);
            this.splitContainerHelp.SplitterDistance = 249;
            this.splitContainerHelp.TabIndex = 0;
            // 
            // listViewHelp
            // 
            this.listViewHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewHelp.FullRowSelect = true;
            this.listViewHelp.HideSelection = false;
            this.listViewHelp.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.listViewHelp.Location = new System.Drawing.Point(0, 0);
            this.listViewHelp.MultiSelect = false;
            this.listViewHelp.Name = "listViewHelp";
            this.listViewHelp.Size = new System.Drawing.Size(249, 450);
            this.listViewHelp.TabIndex = 0;
            this.listViewHelp.TileSize = new System.Drawing.Size(245, 18);
            this.listViewHelp.UseCompatibleStateImageBehavior = false;
            this.listViewHelp.View = System.Windows.Forms.View.Tile;
            this.listViewHelp.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewHelp_MouseClick);
            // 
            // richTextBoxImportGuide
            // 
            this.richTextBoxImportGuide.BackColor = System.Drawing.Color.White;
            this.richTextBoxImportGuide.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxImportGuide.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxImportGuide.Location = new System.Drawing.Point(13, 64);
            this.richTextBoxImportGuide.Name = "richTextBoxImportGuide";
            this.richTextBoxImportGuide.Size = new System.Drawing.Size(172, 49);
            this.richTextBoxImportGuide.TabIndex = 1;
            this.richTextBoxImportGuide.Text = "ImportGuide placeholder";
            this.richTextBoxImportGuide.Visible = false;
            // 
            // richTextBoxBeginnersGuide
            // 
            this.richTextBoxBeginnersGuide.BackColor = System.Drawing.Color.White;
            this.richTextBoxBeginnersGuide.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxBeginnersGuide.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxBeginnersGuide.Location = new System.Drawing.Point(13, 12);
            this.richTextBoxBeginnersGuide.Name = "richTextBoxBeginnersGuide";
            this.richTextBoxBeginnersGuide.ReadOnly = true;
            this.richTextBoxBeginnersGuide.Size = new System.Drawing.Size(218, 46);
            this.richTextBoxBeginnersGuide.TabIndex = 0;
            this.richTextBoxBeginnersGuide.Text = "BeginnersGuide placeholder";
            this.richTextBoxBeginnersGuide.Visible = false;
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 450);
            this.Controls.Add(this.splitContainerHelp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HelpForm";
            this.splitContainerHelp.Panel1.ResumeLayout(false);
            this.splitContainerHelp.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerHelp)).EndInit();
            this.splitContainerHelp.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerHelp;
        private System.Windows.Forms.ListView listViewHelp;
        private System.Windows.Forms.RichTextBox richTextBoxBeginnersGuide;
        private System.Windows.Forms.RichTextBox richTextBoxImportGuide;
    }
}