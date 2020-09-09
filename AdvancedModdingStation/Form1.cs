using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using libMBIN;
using ScintillaNET;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedModdingStation.Properties;
using System.Diagnostics;

namespace AdvancedModdingStation
{

    public partial class MainForm : Form
    {
        public enum errorType
        {
            Bug,
            Misconfiguration,
            Syntax,
            Windows
        }
        // public variables
        public string applicationName;
        public string errorBug;
        public string activeDocument;
        public static string projectFolder;
        public bool backgroundWorkerInProgress;
        public bool SearchIsOpen;
        public Dictionary<string, Scintilla> textAreas;
        public Dictionary<string, string> fileLocations;
        public Dictionary<string, bool> filesChanged;

        // private variables
        private TabPage previousTab;
        private Dictionary<TabPage, Color> TabColors;
        private FileOperations fileOperator;

        public MainForm()
        {
            InitializeComponent();

            contextMenuStripGameFiles.Opening += new CancelEventHandler(contextMenuStripGameFiles_Opened);
            tabControl.Deselecting += new TabControlCancelEventHandler(tabControl_Deselecting);
            tabControl.Selecting += new TabControlCancelEventHandler(tabControl_Selecting);
            tabControl.DrawItem += new DrawItemEventHandler(DrawOnTab);

            this.Resize += new EventHandler(Form1_Resize);
            this.backgroundWorkerInProgress = false;
            this.SearchIsOpen = false;

            textAreas = new Dictionary<string, Scintilla>();
            fileLocations = new Dictionary<string, string>();
            filesChanged = new Dictionary<string, bool>();
            TabColors = new Dictionary<TabPage, Color>();

            tabControl.SizeMode = TabSizeMode.Normal;

            TabPage page0 = tabControl.TabPages[0];
            TabPage page1 = tabControl.TabPages[1];
            Color col = new Color();
            col = Color.FromArgb(255, 255, 255, 255);
            SetTabHeader(page0, col);
            SetTabHeader(page1, col);

            // Set initial variables
            setInitialVariables();

            positionElements();

            checkConfigurationSet();

            checkExistingProjects();

            hideOpenProjectRequiredControls();

            hideOpenDocumentRequiredControls();

            string welcomeText = "Thank you for using " + this.applicationName + "!" + Environment.NewLine + Environment.NewLine;
                   welcomeText += "Please click Config => Settings and setup your Paths to begin!";

            labelMainFormWelcome.Text = welcomeText;

            fileOperator = new FileOperations(this);
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            // Position elements
            positionElements();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showConfigurationForm();
        }

        private void projectToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            newProject();
        }

        private void unpackGameFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NMSPackagesManager manager = new NMSPackagesManager(this);
            manager.unpackGamePackages();
        }

        private void helpMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showHelpMenu();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveMbinFile();
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAllMbinFiles();
        }

        private void listBoxProjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBoxProjects.SelectedItem != null)
            {
                openProject(listBoxProjects.SelectedItem.ToString());
            }
        }

        private void listViewProjectFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            openMBINFile();
        }

        private void copyToProjectFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedItems = listViewGameFiles.SelectedItems;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                string sourcePath = selectedItems[i].SubItems[3].Text;
                string sourceRoot = ConfigurationManager.AppSettings.Get("unpackedDir") + "\\";
                string destinationRoot = ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + projectFolder;

                FileAttributes attr = File.GetAttributes(sourcePath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string relativePath = MakeRelativePath(sourceRoot, sourcePath);
                    string destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, relativePath));
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                    var diSource = new DirectoryInfo(sourcePath);
                    var diTarget = new DirectoryInfo(destinationPath);

                    CopyAll(diSource, diTarget);
                }
                else
                {
                    string fileName = selectedItems[i].Text;
                    string relativePath = MakeRelativePath(sourceRoot, sourcePath);
                    string destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, relativePath));
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                    // Copy the file over.
                    if (File.Exists(destinationPath))
                    {
                        var confirmResult = MessageBox.Show(fileName + " already exists in your project. Do you wish to overwrite it?",
                            "Overwrite " + fileName + "?",
                                     MessageBoxButtons.YesNo);
                        if (confirmResult == DialogResult.Yes)
                        {
                            File.Copy(sourcePath, destinationPath, true);
                        }
                    }
                    else
                    {
                        File.Copy(sourcePath, destinationPath);
                    }
                }
            }
            PopulateProjectTreeView();
        }

        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            TabPage tab = (sender as TabControl).SelectedTab;
            for (var i = 2; i < this.tabControl.TabPages.Count; i++)
            {
                var tabRect = this.tabControl.GetTabRect(i);
                tabRect.Inflate(-2, -2);
                var closeImage = new Bitmap(Resources.close02);
                var imageRect = new Rectangle(
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                    closeImage.Width,
                    closeImage.Height);
                if (imageRect.Contains(e.Location))
                {
                    CloseTab(tab);
                    break;
                }
            }
        }

        private void treeViewGameFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listViewGameFiles.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            Cursor.Current = Cursors.WaitCursor;
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, dir.FullName)};
                item.SubItems.AddRange(subItems);
                listViewGameFiles.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {

                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
                new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, file.FullName)};

                item.SubItems.AddRange(subItems);
                listViewGameFiles.Items.Add(item);
            }
            Cursor.Current = Cursors.Default;

            listViewGameFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void treeViewProjectFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listViewProjectFiles.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            Cursor.Current = Cursors.WaitCursor;
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, dir.FullName)};
                item.SubItems.AddRange(subItems);
                listViewProjectFiles.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                string fileExt = file.Extension;

                if (fileExt.Equals(".mbin", StringComparison.OrdinalIgnoreCase))
                {
                    item = new ListViewItem(file.Name, 1);
                    subItems = new ListViewItem.ListViewSubItem[]
                        { new ListViewItem.ListViewSubItem(item, "File"),
                 new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString()),
                        new ListViewItem.ListViewSubItem(item, file.FullName)};

                    item.SubItems.AddRange(subItems);
                    listViewProjectFiles.Items.Add(item);
                }
            }
            Cursor.Current = Cursors.Default;

            listViewProjectFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.CutText();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.CopyText();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.PasteText();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.SelectAllText();
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.SelectClearText();
        }

        private void indentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.IndentText();
        }

        private void outdentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.OutdentText();
        }

        private void uppercaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.Uppercase();
        }

        private void lowercaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.Lowercase();
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.WordWrap();
        }

        private void showIndentGuidesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.IndentGuides();
        }

        private void showWhitespaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.WhiteSpace();
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.ZoomIn();
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.ZoomOut();
        }

        private void zoom100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.ZoomDefault();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.CollapseAll();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.ExpandAll();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileOperator.OpenSearch();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (HotKeyManager.IsHotkey(e, Keys.Enter))
            {
                SearchManager.Find(true, false);
            }
            if (HotKeyManager.IsHotkey(e, Keys.Enter, true) || HotKeyManager.IsHotkey(e, Keys.Enter, false, true))
            {
                SearchManager.Find(false, false);
            }
        }

        private void BtnPrevSearch_Click(object sender, EventArgs e)
        {
            SearchManager.Find(false, false);
        }

        private void BtnNextSearch_Click(object sender, EventArgs e)
        {
            SearchManager.Find(true, false);
        }

        private void BtnCloseSearch_Click(object sender, EventArgs e)
        {
            CloseSearch();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkUnpackingInProgress())
            {
                if (Application.MessageLoop)
                {
                    Application.Exit();
                }
                else
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                return;
            }
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TabPage tab = tabControl.SelectedTab;

            if(tab.Name == "tabPage1" || tab.Name == "tabPage2")
            {
                return;
            }
            else
            {
                CloseTab(tab);
            }
        }

        private void projectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CloseProject();
        }

        private void buildProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] projectFiles = { };

            projectFiles = Directory.GetFiles(ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + projectFolder, "*.*", SearchOption.AllDirectories);
            if (projectFiles != null && projectFiles.Length != 0)
            {
                File.WriteAllLines(@"pakFiles.txt", projectFiles);

                var path = Path.GetTempFileName() + ".exe";
                var txtFile = new FileInfo(@"pakFiles.txt");
                File.WriteAllBytes(path, Properties.Resources.psarc);
                var arguments = string.Format("create -a --zlib --inputfile=\"{0}\" --output=\"{1}\"", AppDomain.CurrentDomain.BaseDirectory + "\\pakFiles.txt", ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + projectFolder + "\\" + projectFolder + ".pak");
                var procInfo = new ProcessStartInfo(path, arguments);
                procInfo.WorkingDirectory = ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + projectFolder;
                procInfo.CreateNoWindow = true;
                procInfo.UseShellExecute = false;
                Process p = new Process();
                p.StartInfo = procInfo;
                p.Start();
                p.WaitForExit();

                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pakFiles.txt");

                MessageBox.Show(projectFolder + ".pak succesfully created!");
            }
        }

        private void contextMenuStripGameFiles_Opened(object sender, CancelEventArgs e)
        {
            if (listViewGameFiles.SelectedIndices.Count < 1)
            {
                e.Cancel = true;
            }
        }

        private void tabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            previousTab = current;
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (tabControl.SelectedIndex > 1)
            {
                showOpenDocumentRequiredControls();
                if (current.Text.EndsWith("*"))
                {
                    activeDocument = current.Text.Substring(0, current.Text.Length - 2);
                }
                else
                {
                    activeDocument = current.Text;
                }
                labelInfo.Text = activeDocument;
            }
            else
            {
                hideOpenDocumentRequiredControls();
                activeDocument = "";
                labelInfo.Text = "No document selected.";
            }
        }

        private void setInitialVariables()
        {
            this.applicationName = "NMS Advanced Modding Station";
            this.errorBug = "This is a bug. Please report it together with the technical error message below.";
        }

        private void showConfigurationForm()
        {
            ConfigForm configForm = new ConfigForm(this);

            try
            {
                configForm.ShowDialog(this);
            }
            catch (InvalidOperationException e)
            {
                string errorMessage = this.applicationName + " encountered an error while trying to open the Configuration Window" + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Bug + Environment.NewLine;
                       errorMessage += "Solution: Try saving your work and restart " + this.applicationName + ". If that doesn't work, please file a but report including the details below:" + Environment.NewLine + Environment.NewLine;
                       errorMessage += e.Message;
                string caption = "Error!";

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (errorResult == DialogResult.OK)
                {
                    configForm.Dispose();
                }
                else
                {
                    configForm.Dispose();
                }
            }
        }

        private void showHelpMenu()
        {
            HelpForm helpForm = new HelpForm(this);

            try
            {
                helpForm.ShowDialog(this);
            }
            catch (InvalidOperationException e)
            {
                string errorMessage = this.applicationName + " encountered an error while trying to open the Help Menu." + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + errorType.Bug + Environment.NewLine;
                errorMessage += "Solution: Try saving your work and restart " + this.applicationName + ". If that doesn't work, please file a but report including the details below:" + Environment.NewLine + Environment.NewLine;
                errorMessage += e.Message;
                string caption = "Error!";

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (errorResult == DialogResult.OK)
                {
                    helpForm.Dispose();
                }
                else
                {
                    helpForm.Dispose();
                }
            }
        }

        private void DrawOnTab(object sender, DrawItemEventArgs e)
        {

            using (Brush br = new SolidBrush(TabColors[tabControl.TabPages[e.Index]]))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
                SizeF sz = e.Graphics.MeasureString(tabControl.TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

                Rectangle rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(Pens.DarkGray, rect);
                e.DrawFocusRectangle();
            }

            if (e.Index > 1)
            {
                var tabPage = this.tabControl.TabPages[e.Index];
                var tabRect = this.tabControl.GetTabRect(e.Index);
                tabRect.Inflate(-2, -2);
                var closeImage = new Bitmap(Resources.close02);
                e.Graphics.DrawImage(closeImage,
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2);
            }
        }

        public void checkConfigurationSet()
        {
            if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("gameDir")))
            {
                hideConfigRequiredControls();
            }
            else if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("unpackedDir")))
            {
                hideConfigRequiredControls();
            }
            else if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("projectsDir")))
            {
                hideConfigRequiredControls();
            }
            else
            {
                showConfigRequiredControls();
            }
        }

        private void checkExistingProjects()
        {
            try
            {
                if (Directory.EnumerateFileSystemEntries(ConfigurationManager.AppSettings.Get("projectsDir")).Any())
                {
                    showProjectRequiredControls();
                    loadProjectNames();
                }
                else
                {
                    hideProjectRequiredControls();
                }
            }
            catch (ArgumentException)
            {
                hideProjectRequiredControls();
            }
        }

        private bool checkUnpackedGameFiles()
        {
            try
            {
                if (Directory.EnumerateFileSystemEntries(ConfigurationManager.AppSettings.Get("unpackedDir")).Any())
                {
                    return true;
                } else
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private void hideConfigRequiredControls()
        {
            newToolStripMenuItem.Enabled = false;
            buildToolStripMenuItem.Enabled = false;
        }

        private void showConfigRequiredControls()
        {
            labelMainFormWelcome.Visible = false;
            if (checkUnpackedGameFiles())
            {
                newToolStripMenuItem.Enabled = true;
                labelFirstProject.Visible = true;
            } else
            {
                labelUnpackGameFiles.Visible = true;
            }
            buildToolStripMenuItem.Enabled = true;
        }

        private void hideProjectRequiredControls()
        {
            buildProjectToolStripMenuItem.Enabled = false;
        }

        private void showProjectRequiredControls()
        {
            labelMainFormWelcome.Visible = false;
            labelSelectProject.Visible = true;
            listBoxProjects.Visible = true;
        }

        private void hideOpenProjectRequiredControls()
        {
            closeToolStripMenuItem.Enabled = false;
            buildProjectToolStripMenuItem.Enabled = false;
        }

        private void showOpenProjectRequiredControls()
        {
            closeToolStripMenuItem.Enabled = true;
            buildProjectToolStripMenuItem.Enabled = true;
            newToolStripMenuItem.Enabled = false;
        }

        public void hideOpenDocumentRequiredControls()
        {
            saveToolStripMenuItem.Enabled = false;
            saveAllToolStripMenuItem.Enabled = false;
            editToolStripMenuItem.Enabled = false;
            searchToolStripMenuItem.Enabled = false;
            viewToolStripMenuItem.Enabled = false;
            fileToolStripMenuItem1.Enabled = false;
        }

        public void showOpenDocumentRequiredControls()
        {
            saveToolStripMenuItem.Enabled = true;
            saveAllToolStripMenuItem.Enabled = true;
            editToolStripMenuItem.Enabled = true;
            searchToolStripMenuItem.Enabled = true;
            viewToolStripMenuItem.Enabled = true;
            fileToolStripMenuItem1.Enabled = true;
        }

        private void positionElements()
        {
            labelSelectProject.Left = (this.Width - labelSelectProject.Width) / 2;
            labelUnpackGameFiles.Left = (this.Width - labelUnpackGameFiles.Width) / 2;
            labelUnpackGameFiles.Top = (this.Height - labelUnpackGameFiles.Height) / 2;
            labelFirstProject.Left = (this.Width - labelFirstProject.Width) / 2;
            labelFirstProject.Top = (this.Height - labelFirstProject.Height) / 2;
            labelUnpackingInProgress.Left = (this.Width - labelUnpackingInProgress.Width) / 2;
            labelUnpackingInProgress.Top = (this.Height - labelUnpackingInProgress.Height) / 2;
            listBoxProjects.Left = labelSelectProject.Left;
            listBoxProjects.Width = labelSelectProject.Width;
            listBoxProjects.Height = this.Height - 200;
            tabControl.Size = new System.Drawing.Size(this.Width - 13, this.Height - 85);
            tabControl.Location = new Point(0, 24);
            PanelSearch.Left = tabControl.Right - 315;
            PanelSearch.Top = tabControl.Top + 25;
        }

        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException e)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to save your configuration." + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Bug + Environment.NewLine;
                       errorMessage += "Solution: This is a bug. Please report it together with the technical error message below." + Environment.NewLine + Environment.NewLine;
                       errorMessage += e.Message;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadProjectNames()
        {
            string dir = ConfigurationManager.AppSettings.Get("projectsDir");
            labelFirstProject.Visible = false;
            try
            {
                string[] subdirs = Directory.GetDirectories(dir).Select(Path.GetFileName).ToArray();

                for (int i = 0; i < subdirs.Length; i++)
                {
                    listBoxProjects.Items.Add(subdirs[i]);
                }
            }
            catch (UnauthorizedAccessException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to access " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + " (Access denied!)"+ Environment.NewLine;
                       errorMessage += "Solution: Please make sure you have full read / write access to " + dir + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to access " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + Environment.NewLine;
                       errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!"+ Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (PathTooLongException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to access " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + " (Path too long!)" + Environment.NewLine;
                       errorMessage += "Solution: Please use a Projects directory with a shorter path to access it!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to access " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + " (IO Exception)" + Environment.NewLine;
                       errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to access " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Bug + " (Unknown)" + Environment.NewLine;
                       errorMessage += "Solution: This is a bug. Please report it together with the technical error message below." + Environment.NewLine + Environment.NewLine;
                       errorMessage += e.Message;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void newProject()
        {
            string newFolder = newProjectName("");
            string dir = ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + newFolder;

            Regex rx = new Regex(@"^[0-9a-zA-Z_\-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (rx.IsMatch(newFolder))
            {
                if (newFolder.Length > 0 && newFolder.Length < 33)
                {
                    if (!Directory.Exists(ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + newFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(ConfigurationManager.AppSettings.Get("projectsDir") + "\\" + newFolder);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            string errorMessage = "NMS Advanced Modding Station encountered an error while trying to create " + dir + Environment.NewLine + Environment.NewLine;
                            errorMessage += "Error type: " + errorType.Windows + " (Access denied!)" + Environment.NewLine;
                            errorMessage += "Solution: Please make sure you have full read / write access to " + dir + Environment.NewLine + Environment.NewLine;
                            string caption = "Error!";

                            MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        catch (ArgumentException)
                        {
                            string errorMessage = "NMS Advanced Modding Station encountered an error while trying to create " + dir + Environment.NewLine + Environment.NewLine;
                            errorMessage += "Error type: " + errorType.Windows + Environment.NewLine;
                            errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!" + Environment.NewLine + Environment.NewLine;
                            string caption = "Error!";

                            MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        catch (PathTooLongException)
                        {
                            string errorMessage = "NMS Advanced Modding Station encountered an error while trying to create " + dir + Environment.NewLine + Environment.NewLine;
                            errorMessage += "Error type: " + errorType.Windows + " (Path too long!)" + Environment.NewLine;
                            errorMessage += "Solution: Please use a Projects directory with a shorter path to access it!" + Environment.NewLine + Environment.NewLine;
                            string caption = "Error!";

                            MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        catch (IOException)
                        {
                            string errorMessage = "NMS Advanced Modding Station encountered an error while trying to create " + dir + Environment.NewLine + Environment.NewLine;
                            errorMessage += "Error type: " + errorType.Windows + " (IO Exception)" + Environment.NewLine;
                            errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!" + Environment.NewLine + Environment.NewLine;
                            string caption = "Error!";

                            MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        catch (Exception e)
                        {
                            string errorMessage = "NMS Advanced Modding Station encountered an error while trying to create " + dir + Environment.NewLine + Environment.NewLine;
                            errorMessage += "Error type: " + errorType.Bug + " (Unknown)" + Environment.NewLine;
                            errorMessage += "Solution: This is a bug. Please report it together with the technical error message below." + Environment.NewLine + Environment.NewLine;
                            errorMessage += e.Message;
                            string caption = "Error!";

                            MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        listBoxProjects.Items.Add(newFolder);
                        CloseAllTabs();
                        openProject(newFolder);
                    }
                    else
                    {
                        MessageBox.Show("A project with the name " + newFolder + " already exists!", "Error!", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Your project name should not contain more than 32 characters!", "Error!", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("Your project name may only use alphanumeric characters, hyphens and underscores!", "Error!", MessageBoxButtons.OK);
            }
        }

        private void openProject(string projectName)
        {
            projectFolder = projectName;

            PopulateTreeView();
            PopulateProjectTreeView();
            showOpenProjectRequiredControls();

            labelSelectProject.Visible = false;
            listBoxProjects.Visible = false;
            tabControl.Visible = true;
            labelFirstProject.Visible = false;
            unpackGameFilesToolStripMenuItem.Enabled = false;
        }

        private void openMBINFile()
        {
            var selectedItems = listViewProjectFiles.SelectedItems;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                string fileName = selectedItems[i].Text;
                string sourcePath = selectedItems[i].SubItems[3].Text;
                string sourcePathNoExt = Path.ChangeExtension(sourcePath, "exml");

                FileAttributes attr = File.GetAttributes(sourcePath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    listViewProjectFiles.Items.Clear();
                    DirectoryInfo mainDir = new DirectoryInfo(sourcePath);
                    ListViewItem.ListViewSubItem[] subItems;
                    ListViewItem item = null;

                    Cursor.Current = Cursors.WaitCursor;
                    foreach (DirectoryInfo dir in mainDir.GetDirectories())
                    {
                        item = new ListViewItem(dir.Name, 0);
                        subItems = new ListViewItem.ListViewSubItem[]
                            {
                                new ListViewItem.ListViewSubItem(item, "Directory"),
                                new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString()),
                                new ListViewItem.ListViewSubItem(item, dir.FullName)
                            };
                        item.SubItems.AddRange(subItems);
                        listViewProjectFiles.Items.Add(item);
                    }
                    foreach (FileInfo file in mainDir.GetFiles())
                    {
                        item = new ListViewItem(file.Name, 1);
                        subItems = new ListViewItem.ListViewSubItem[]
                            {
                                new ListViewItem.ListViewSubItem(item, "File"),
                                new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString()),
                                new ListViewItem.ListViewSubItem(item, file.FullName)
                            };

                        item.SubItems.AddRange(subItems);
                        listViewProjectFiles.Items.Add(item);
                    }
                    Cursor.Current = Cursors.Default;

                    listViewProjectFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                else
                {
                    CreateXmlEditor editor = new CreateXmlEditor(this);
                    NMSTemplate obj = libMBIN.FileIO.LoadMbin(sourcePath);
                    string exmlCode = EXmlFile.WriteTemplate(obj);
                    //obj.WriteToExml(sourcePathNoExt);
                    Scintilla edit = editor.createControl(exmlCode);

                    this.textAreas.Add(fileName, edit);
                    this.fileLocations.Add(fileName, sourcePath);
                    this.filesChanged.Add(fileName, false);
                    createTab(fileName, edit);
                    break;
                }
            }
        }

        public void createTab(string tabName, Scintilla scintilla)
        {
            int? index = searchTabs(tabName);
            if (index == null)
            {
                TabPage newTab = new TabPage();
                newTab.Text = tabName;
                newTab.Controls.Add(scintilla);
                Color col = new Color();
                col = Color.FromArgb(255, 255, 255, 255);
                SetTabHeader(newTab, col);
                tabControl.TabPages.Add(newTab);
                tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
            }
            else
            {
                tabControl.SelectedIndex = Convert.ToInt32(index);
            }
        }

        private int? searchTabs(string tabName)
        {
            int? index = null;

            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Text == tabName)
                {
                    index = tabControl.TabPages.IndexOf(tabPage);
                }
            }

            return index;
        }

        private void SetTabHeader(TabPage page, Color color)
        {
            TabColors[page] = color;
            tabControl.Invalidate();
        }

        private string newProjectName(string newName)
        {
            string newFolder = Interaction.InputBox("Please enter the name of your project", "New Project", newName);

            return newFolder;
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            string dir = ConfigurationManager.AppSettings.Get("unpackedDir");

            try
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    GetDirectories(info.GetDirectories(), rootNode);
                    treeViewGameFiles.Nodes.Clear();
                    treeViewGameFiles.Nodes.Add(rootNode);
                }
            }
            catch (System.Security.SecurityException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your unpacked game files directory " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + " (Access denied!)" + Environment.NewLine;
                       errorMessage += "Solution: Please make sure you have full read / write access to " + dir + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your unpacked game files directory " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + Environment.NewLine;
                       errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (PathTooLongException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your unpacked game files directory " + dir + Environment.NewLine + Environment.NewLine;
                       errorMessage += "Error type: " + errorType.Windows + " (Path too long!)" + Environment.NewLine;
                       errorMessage += "Solution: Please use a Projects directory with a shorter path to access it!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PopulateProjectTreeView()
        {
            TreeNode rootNode;
            string dir = ConfigurationManager.AppSettings.Get("projectsDir");

            try
            {
                DirectoryInfo info = new DirectoryInfo(@"Projects\\" + projectFolder);
                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    GetDirectories(info.GetDirectories(), rootNode);
                    listViewProjectFiles.Items.Clear();
                    treeViewProjectFiles.Nodes.Clear();
                    treeViewProjectFiles.Nodes.Add(rootNode);
                }
            }
            catch (System.Security.SecurityException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your project files directory " + dir + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + errorType.Windows + " (Access denied!)" + Environment.NewLine;
                errorMessage += "Solution: Please make sure you have full read / write access to " + dir + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your project files directory " + dir + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + errorType.Windows + Environment.NewLine;
                errorMessage += "Solution: Please make sure " + dir + " exists and is accessable!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (PathTooLongException)
            {
                string errorMessage = "NMS Advanced Modding Station encountered an error while trying to load your project files directory " + dir + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + errorType.Windows + " (Path too long!)" + Environment.NewLine;
                errorMessage += "Solution: Please use a Projects directory with a shorter path to access it!" + Environment.NewLine + Environment.NewLine;
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void CloseAllTabs()
        {
            TabControl.TabPageCollection pages = tabControl.TabPages;
            foreach (TabPage page in pages)
            {
                if (page.Name.Equals("tabPage1", StringComparison.OrdinalIgnoreCase) || page.Name.Equals("tabPage2", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    tabControl.SelectTab(page);
                    CloseTab(page);
                }
            }
        }

        private void CloseTab(TabPage page)
        {
            string tabName = page.Text;
            string fileName = "";

            if (tabName.EndsWith("*"))
            {
                fileName = tabName.Substring(0, tabName.Length - 2);

                DialogResult result = MessageBox.Show("Would you like to save changes to " + fileName + "?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string exmlCode = textAreas[fileName].Text;
                    string fileSource = fileLocations[fileName];
                    try
                    {
                        NMSTemplate obj = EXmlFile.ReadTemplateFromString(exmlCode);
                        obj.WriteToMbin(fileSource);

                        textAreas.Remove(fileName);
                        fileLocations.Remove(fileName);
                        filesChanged.Remove(fileName);

                        if (tabControl.SelectedTab == previousTab)
                        {
                            TabPage projectTab = tabControl.TabPages[1];
                            tabControl.SelectTab(projectTab);
                        }
                        else
                        {
                            try
                            {
                                tabControl.SelectTab(previousTab);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                tabControl.SelectTab(tabControl.TabPages[1]);
                            }
                        }
                        tabControl.TabPages.Remove(page);
                    }
                    catch (FormatException)
                    {
                        DialogResult errorResult = MessageBox.Show("Unable to save changes to " + fileName + " due to an error in your XML code. This usually means that you've put an illegal value in one of the attributes, like a string that's too long or string placed where a number is expected.", "Error!", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);

                        if (errorResult == DialogResult.Abort)
                        {
                            return;
                        }
                        else if (errorResult == DialogResult.Retry)
                        {
                            CloseTab(page);
                        }
                        else
                        {
                            textAreas.Remove(fileName);
                            fileLocations.Remove(fileName);
                            filesChanged.Remove(fileName);

                            if (tabControl.SelectedTab == previousTab)
                            {
                                TabPage projectTab = tabControl.TabPages[1];
                                tabControl.SelectTab(projectTab);
                            }
                            else
                            {
                                try
                                {
                                    tabControl.SelectTab(previousTab);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    tabControl.SelectTab(tabControl.TabPages[1]);
                                }
                            }
                            tabControl.TabPages.Remove(page);
                        }
                    }
                    catch (Exception e)
                    {
                        DialogResult errorResult = MessageBox.Show("Unable to save changes to " + fileName + " due to an unknown error. If you wish to report this, please include the message below:" + Environment.NewLine + Environment.NewLine + e.Message + Environment.NewLine + Environment.NewLine + "If you wish to manually copy / paste your code before closing, please click Cancel. Click OK to close without saving changes.", "Error!", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (errorResult == DialogResult.OK)
                        {
                            textAreas.Remove(fileName);
                            fileLocations.Remove(fileName);
                            filesChanged.Remove(fileName);

                            if (tabControl.SelectedTab == previousTab)
                            {
                                TabPage projectTab = tabControl.TabPages[1];
                                tabControl.SelectTab(projectTab);
                            }
                            else
                            {
                                try
                                {
                                    tabControl.SelectTab(previousTab);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    tabControl.SelectTab(tabControl.TabPages[1]);
                                }
                                catch (ArgumentNullException)
                                {
                                    tabControl.SelectTab(tabControl.TabPages[0]);
                                }
                            }
                            tabControl.TabPages.Remove(page);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else if (result == DialogResult.No)
                {
                    textAreas.Remove(fileName);
                    fileLocations.Remove(fileName);
                    filesChanged.Remove(fileName);

                    if (tabControl.SelectedTab == previousTab)
                    {
                        TabPage projectTab = tabControl.TabPages[1];
                        tabControl.SelectTab(projectTab);
                    }
                    else
                    {
                        try
                        {
                            tabControl.SelectTab(previousTab);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            tabControl.SelectTab(tabControl.TabPages[1]);
                        }
                    }
                    tabControl.TabPages.Remove(page);
                }
                else
                {
                    return;
                }
            }
            else
            {
                fileName = tabName;
                try
                {
                    textAreas.Remove(fileName);
                    fileLocations.Remove(fileName);
                    filesChanged.Remove(fileName);

                    if (tabControl.SelectedTab == previousTab)
                    {
                        TabPage projectTab = tabControl.TabPages[1];
                        tabControl.SelectTab(projectTab);
                    }
                    else
                    {
                        try
                        {
                            tabControl.SelectTab(previousTab);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            tabControl.SelectTab(tabControl.TabPages[1]);
                        }
                    }
                    tabControl.TabPages.Remove(page);
                }
                catch (ArgumentNullException)
                {
                    // This error only means that one of the Dictionaries doesn't contain the file
                    // Since this will not affect continuous use of the program, we can just ignore it
                    return;
                }
            }
        }

        private void CloseProject()
        {
            TabControl.TabPageCollection pages = tabControl.TabPages;
            foreach (TabPage page in pages)
            {
                if (page.Text.EndsWith("*"))
                {
                    string message = "Would you like to save changes to your project before closing?";
                    string caption = "Save changes?";

                    DialogResult errorResult = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (errorResult == DialogResult.Yes)
                    {
                        saveAllMbinFiles();
                        break;
                    }
                }
            }

            foreach (TabPage page in pages)
            {
                if (page.Name == "tabPage1" || page.Name == "tabPage2")
                {
                    continue;
                }
                else
                {
                    tabControl.TabPages.Remove(page);
                }
            }

            hideOpenDocumentRequiredControls();
            hideOpenProjectRequiredControls();

            tabControl.Visible = false;
            listBoxProjects.Visible = true;
            labelSelectProject.Visible = true;

            newToolStripMenuItem.Enabled = true;
            unpackGameFilesToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (File.Exists(Path.Combine(target.FullName, fi.Name)))
                {
                    var confirmResult = MessageBox.Show(fi.Name + " already exists in your project. Do you wish to overwrite it?",
                        "Overwrite " + fi.Name + "?",
                                 MessageBoxButtons.YesNo);
                    if (confirmResult == DialogResult.Yes)
                    {
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void setFileEdited()
        {
            if (!String.IsNullOrWhiteSpace(activeDocument))
            {
                TabPage current = tabControl.SelectedTab;
                if (!current.Text.EndsWith("*") && tabControl.SelectedIndex > 1)
                {
                    this.filesChanged[activeDocument] = true;
                    current.Text = current.Text + " *";
                }
            }
        }

        private void saveMbinFile()
        {
            if (String.IsNullOrEmpty(activeDocument))
            {
                return;
            }
            string exmlCode = textAreas[activeDocument].Text;
            string fileSource = fileLocations[activeDocument];
            try
            {
                NMSTemplate obj = EXmlFile.ReadTemplateFromString(exmlCode);
                obj.WriteToMbin(fileSource);
            }
            catch (FormatException)
            {
                string errorMessage = this.applicationName + " encountered an error while trying to save " + activeDocument + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + MainForm.errorType.Syntax + " (Invalid Value!) " + Environment.NewLine;
                errorMessage += "Solution: Please make sure you don't put invalid values in your EXML code! (Like a string where an integer expected)";
                string caption = "Error!";

                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            string fileName;

            TabPage current = tabControl.SelectedTab;
            if (current.Text.EndsWith("*"))
            {
                fileName = current.Text.Substring(0, current.Text.Length - 2);
            }
            else
            {
                fileName = current.Text;
            }
            current.Text = fileName;
        }

        private void saveAllMbinFiles()
        {
            foreach (KeyValuePair <string, Scintilla> entry in textAreas)
            {
                string exmlCode = entry.Value.Text;
                string fileSource = fileLocations[entry.Key];

                try
                {
                    NMSTemplate obj = EXmlFile.ReadTemplateFromString(exmlCode);
                    obj.WriteToMbin(fileSource);
                }
                catch (FormatException)
                {
                    string errorMessage = this.applicationName + " encountered an error while trying to save " + activeDocument + Environment.NewLine + Environment.NewLine;
                    errorMessage += "Error type: " + MainForm.errorType.Syntax + " (Invalid Value!) " + Environment.NewLine;
                    errorMessage += "Solution: Please make sure you don't put invalid values in your EXML code! (Like a string where an integer expected)";
                    string caption = "Error!";

                    MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    continue;
                }

                TabControl.TabPageCollection pages = tabControl.TabPages;
                foreach (TabPage page in pages)
                {
                    if(page.Text.StartsWith(entry.Key) && page.Text.EndsWith("*"))
                    {
                        page.Text = entry.Key;
                    }
                }
            }
        }

        public void GenerateKeystrokes(string keys)
        {
            HotKeyManager.Enable = false;
            textAreas[activeDocument].Focus();
            SendKeys.Send(keys);
            HotKeyManager.Enable = true;
        }

        private void CloseSearch()
        {
            if (String.IsNullOrEmpty(activeDocument))
            {
                return;
            }
            if (SearchIsOpen)
            {
                SearchIsOpen = false;
                PanelSearch.Visible = false;
            }
        }

        private bool checkUnpackingInProgress()
        {
            if (backgroundWorkerInProgress)
            {
                string errorMessage = this.applicationName + " is still busy unpacking your game files. Closing now will give unexpected results next time you launch " + this.applicationName + "!" + Environment.NewLine + Environment.NewLine;
                errorMessage += "Are you sure you wish to exit now?";
                string caption = "Unpacking in progress!";

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (errorResult == DialogResult.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkUnpackingInProgress())
            {
                if (Application.MessageLoop)
                {
                    Application.Exit();
                }
                else
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void closeApp()
        {
            Properties.Settings.Default.Save();

            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm(this);

            try
            {
                aboutForm.ShowDialog(this);
            }
            catch (InvalidOperationException ex)
            {
                string errorMessage = this.applicationName + " encountered an error while trying to open the About form." + Environment.NewLine + Environment.NewLine;
                errorMessage += "Error type: " + errorType.Bug + Environment.NewLine;
                errorMessage += "Solution: Try saving your work and restart " + this.applicationName + ". If that doesn't work, please file a but report including the details below:" + Environment.NewLine + Environment.NewLine;
                errorMessage += ex.Message;
                string caption = "Error!";

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (errorResult == DialogResult.OK)
                {
                    aboutForm.Dispose();
                }
                else
                {
                    aboutForm.Dispose();
                }
            }
        }
    }
}
