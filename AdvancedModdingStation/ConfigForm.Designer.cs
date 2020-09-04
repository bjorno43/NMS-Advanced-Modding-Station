namespace AdvancedModdingStation
{
    partial class ConfigForm
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Paths");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Layout");
            this.splitContainerConfig = new System.Windows.Forms.SplitContainer();
            this.listViewConfig = new System.Windows.Forms.ListView();
            this.buttonConfigSelectProjects = new System.Windows.Forms.Button();
            this.buttonConfigSelectUnpacked = new System.Windows.Forms.Button();
            this.buttonConfigSelectNMSInstall = new System.Windows.Forms.Button();
            this.textBoxConfigProjects = new System.Windows.Forms.TextBox();
            this.textBoxConfigUnpacked = new System.Windows.Forms.TextBox();
            this.textBoxConfigNMSInstall = new System.Windows.Forms.TextBox();
            this.labelConfigUnpacked = new System.Windows.Forms.Label();
            this.labelConfigProjects = new System.Windows.Forms.Label();
            this.labelConfigNMSInstall = new System.Windows.Forms.Label();
            this.panelConfigPaths = new System.Windows.Forms.Panel();
            this.labelConfigWelcome = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerConfig)).BeginInit();
            this.splitContainerConfig.Panel1.SuspendLayout();
            this.splitContainerConfig.Panel2.SuspendLayout();
            this.splitContainerConfig.SuspendLayout();
            this.panelConfigPaths.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerConfig
            // 
            this.splitContainerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerConfig.IsSplitterFixed = true;
            this.splitContainerConfig.Location = new System.Drawing.Point(0, 0);
            this.splitContainerConfig.Name = "splitContainerConfig";
            // 
            // splitContainerConfig.Panel1
            // 
            this.splitContainerConfig.Panel1.Controls.Add(this.listViewConfig);
            // 
            // splitContainerConfig.Panel2
            // 
            this.splitContainerConfig.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainerConfig.Panel2.Controls.Add(this.labelConfigWelcome);
            this.splitContainerConfig.Panel2.Controls.Add(this.panelConfigPaths);
            this.splitContainerConfig.Size = new System.Drawing.Size(748, 450);
            this.splitContainerConfig.SplitterDistance = 249;
            this.splitContainerConfig.TabIndex = 0;
            // 
            // listViewConfig
            // 
            this.listViewConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewConfig.FullRowSelect = true;
            this.listViewConfig.HideSelection = false;
            listViewItem1.ToolTipText = "Set the paths that NMS Advanced Mod Manager will use.";
            listViewItem2.ToolTipText = "Set the layout for NMS Advanced Mod Manager";
            this.listViewConfig.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this.listViewConfig.Location = new System.Drawing.Point(0, 0);
            this.listViewConfig.MultiSelect = false;
            this.listViewConfig.Name = "listViewConfig";
            this.listViewConfig.Size = new System.Drawing.Size(249, 450);
            this.listViewConfig.TabIndex = 0;
            this.listViewConfig.TileSize = new System.Drawing.Size(245, 18);
            this.listViewConfig.UseCompatibleStateImageBehavior = false;
            this.listViewConfig.View = System.Windows.Forms.View.Tile;
            this.listViewConfig.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewConfig_MouseClick);
            // 
            // buttonConfigSelectProjects
            // 
            this.buttonConfigSelectProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfigSelectProjects.Location = new System.Drawing.Point(427, 111);
            this.buttonConfigSelectProjects.Name = "buttonConfigSelectProjects";
            this.buttonConfigSelectProjects.Size = new System.Drawing.Size(28, 22);
            this.buttonConfigSelectProjects.TabIndex = 8;
            this.buttonConfigSelectProjects.Text = "...";
            this.buttonConfigSelectProjects.UseVisualStyleBackColor = true;
            this.buttonConfigSelectProjects.Click += new System.EventHandler(this.buttonConfigSelectProjects_Click);
            // 
            // buttonConfigSelectUnpacked
            // 
            this.buttonConfigSelectUnpacked.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfigSelectUnpacked.Location = new System.Drawing.Point(427, 66);
            this.buttonConfigSelectUnpacked.Name = "buttonConfigSelectUnpacked";
            this.buttonConfigSelectUnpacked.Size = new System.Drawing.Size(28, 22);
            this.buttonConfigSelectUnpacked.TabIndex = 7;
            this.buttonConfigSelectUnpacked.Text = "...";
            this.buttonConfigSelectUnpacked.UseVisualStyleBackColor = true;
            this.buttonConfigSelectUnpacked.Click += new System.EventHandler(this.buttonConfigSelectUnpacked_Click);
            // 
            // buttonConfigSelectNMSInstall
            // 
            this.buttonConfigSelectNMSInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfigSelectNMSInstall.Location = new System.Drawing.Point(427, 23);
            this.buttonConfigSelectNMSInstall.Name = "buttonConfigSelectNMSInstall";
            this.buttonConfigSelectNMSInstall.Size = new System.Drawing.Size(28, 22);
            this.buttonConfigSelectNMSInstall.TabIndex = 6;
            this.buttonConfigSelectNMSInstall.Text = "...";
            this.buttonConfigSelectNMSInstall.UseVisualStyleBackColor = true;
            this.buttonConfigSelectNMSInstall.Click += new System.EventHandler(this.buttonConfigSelectNMSInstall_Click);
            // 
            // textBoxConfigProjects
            // 
            this.textBoxConfigProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConfigProjects.Location = new System.Drawing.Point(15, 111);
            this.textBoxConfigProjects.Name = "textBoxConfigProjects";
            this.textBoxConfigProjects.ReadOnly = true;
            this.textBoxConfigProjects.Size = new System.Drawing.Size(406, 22);
            this.textBoxConfigProjects.TabIndex = 5;
            // 
            // textBoxConfigUnpacked
            // 
            this.textBoxConfigUnpacked.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConfigUnpacked.Location = new System.Drawing.Point(15, 67);
            this.textBoxConfigUnpacked.Name = "textBoxConfigUnpacked";
            this.textBoxConfigUnpacked.ReadOnly = true;
            this.textBoxConfigUnpacked.Size = new System.Drawing.Size(406, 22);
            this.textBoxConfigUnpacked.TabIndex = 4;
            // 
            // textBoxConfigNMSInstall
            // 
            this.textBoxConfigNMSInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConfigNMSInstall.Location = new System.Drawing.Point(15, 23);
            this.textBoxConfigNMSInstall.Name = "textBoxConfigNMSInstall";
            this.textBoxConfigNMSInstall.ReadOnly = true;
            this.textBoxConfigNMSInstall.Size = new System.Drawing.Size(406, 22);
            this.textBoxConfigNMSInstall.TabIndex = 3;
            // 
            // labelConfigUnpacked
            // 
            this.labelConfigUnpacked.AutoSize = true;
            this.labelConfigUnpacked.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConfigUnpacked.Location = new System.Drawing.Point(12, 48);
            this.labelConfigUnpacked.Name = "labelConfigUnpacked";
            this.labelConfigUnpacked.Size = new System.Drawing.Size(201, 16);
            this.labelConfigUnpacked.TabIndex = 2;
            this.labelConfigUnpacked.Text = "Unpacked Game Files directory:";
            // 
            // labelConfigProjects
            // 
            this.labelConfigProjects.AutoSize = true;
            this.labelConfigProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConfigProjects.Location = new System.Drawing.Point(12, 92);
            this.labelConfigProjects.Name = "labelConfigProjects";
            this.labelConfigProjects.Size = new System.Drawing.Size(146, 16);
            this.labelConfigProjects.TabIndex = 1;
            this.labelConfigProjects.Text = "Your Projects directory:";
            // 
            // labelConfigNMSInstall
            // 
            this.labelConfigNMSInstall.AutoSize = true;
            this.labelConfigNMSInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConfigNMSInstall.Location = new System.Drawing.Point(12, 4);
            this.labelConfigNMSInstall.Name = "labelConfigNMSInstall";
            this.labelConfigNMSInstall.Size = new System.Drawing.Size(215, 16);
            this.labelConfigNMSInstall.TabIndex = 0;
            this.labelConfigNMSInstall.Text = "No Man\'s Sky installation directory:";
            // 
            // panelConfigPaths
            // 
            this.panelConfigPaths.Controls.Add(this.textBoxConfigNMSInstall);
            this.panelConfigPaths.Controls.Add(this.buttonConfigSelectProjects);
            this.panelConfigPaths.Controls.Add(this.labelConfigNMSInstall);
            this.panelConfigPaths.Controls.Add(this.buttonConfigSelectUnpacked);
            this.panelConfigPaths.Controls.Add(this.labelConfigProjects);
            this.panelConfigPaths.Controls.Add(this.buttonConfigSelectNMSInstall);
            this.panelConfigPaths.Controls.Add(this.labelConfigUnpacked);
            this.panelConfigPaths.Controls.Add(this.textBoxConfigProjects);
            this.panelConfigPaths.Controls.Add(this.textBoxConfigUnpacked);
            this.panelConfigPaths.Location = new System.Drawing.Point(13, 12);
            this.panelConfigPaths.Name = "panelConfigPaths";
            this.panelConfigPaths.Size = new System.Drawing.Size(470, 150);
            this.panelConfigPaths.TabIndex = 9;
            this.panelConfigPaths.Visible = false;
            // 
            // labelConfigWelcome
            // 
            this.labelConfigWelcome.AutoSize = true;
            this.labelConfigWelcome.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConfigWelcome.Location = new System.Drawing.Point(10, 12);
            this.labelConfigWelcome.Name = "labelConfigWelcome";
            this.labelConfigWelcome.Size = new System.Drawing.Size(142, 16);
            this.labelConfigWelcome.TabIndex = 10;
            this.labelConfigWelcome.Text = "Welcome Placeholder";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 450);
            this.Controls.Add(this.splitContainerConfig);
            this.MaximizeBox = false;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configuration";
            this.splitContainerConfig.Panel1.ResumeLayout(false);
            this.splitContainerConfig.Panel2.ResumeLayout(false);
            this.splitContainerConfig.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerConfig)).EndInit();
            this.splitContainerConfig.ResumeLayout(false);
            this.panelConfigPaths.ResumeLayout(false);
            this.panelConfigPaths.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerConfig;
        private System.Windows.Forms.ListView listViewConfig;
        private System.Windows.Forms.Button buttonConfigSelectProjects;
        private System.Windows.Forms.Button buttonConfigSelectUnpacked;
        private System.Windows.Forms.Button buttonConfigSelectNMSInstall;
        private System.Windows.Forms.Label labelConfigUnpacked;
        private System.Windows.Forms.Label labelConfigProjects;
        private System.Windows.Forms.Label labelConfigNMSInstall;
        public System.Windows.Forms.TextBox textBoxConfigProjects;
        public System.Windows.Forms.TextBox textBoxConfigUnpacked;
        public System.Windows.Forms.TextBox textBoxConfigNMSInstall;
        private System.Windows.Forms.Panel panelConfigPaths;
        private System.Windows.Forms.Label labelConfigWelcome;
    }
}