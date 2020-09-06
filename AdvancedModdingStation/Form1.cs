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

namespace AdvancedModdingStation
{

    public partial class MainForm : Form
    {
        public enum errorType
        {
            Bug,
            Misconfiguration,
            Windows
        }
        // public variables
        public string applicationName;
        public string errorBug;
        public string activeDocument;
        public static string projectFolder;
        public Dictionary<string, Scintilla> textAreas;
        public Dictionary<string, string> fileLocations;
        public Dictionary<string, bool> filesChanged;

        // private variables
        private bool backgroundWorkerInProgress;
        private bool SearchIsOpen;
        private TabPage previousTab;
        private Dictionary<TabPage, Color> TabColors;

        public MainForm()
        {
            InitializeComponent();

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            this.Resize += new EventHandler(Form1_Resize);

            // Set initial variables
            setInitialVariables();

            checkConfigurationSet();

            checkExistingProjects();

            hideOpenProjectRequiredControls();

            hideOpenDocumentRequiredControls();

            positionElements();

            string welcomeText = "Thank you for using " + this.applicationName + "! Please follow these steps to get started:"+ Environment.NewLine + Environment.NewLine;
            welcomeText += "1. Click Config => Settings and setup your Paths." + Environment.NewLine;
            welcomeText += "2. Click Build => Unpack Game Files and allow it to finish." + Environment.NewLine;
            welcomeText += "- " + this.applicationName + " automatically checks if your choosen Unpacked Game Files directory is empty or not." + Environment.NewLine;
            welcomeText += "  If it's not empty, it'll ask you if you wish to use its contents instead of unpacking new game files." + Environment.NewLine;
            welcomeText += "  If you choose to unpack anyway, all contents will be removed first!" + Environment.NewLine;
            welcomeText += "3. Click File => New => Project and choose a project name." + Environment.NewLine;
            welcomeText += "- Do NOT import / use existing projects from other programs!" + Environment.NewLine;
            welcomeText += "  " + this.applicationName + " does not use EXML files so it cannot handle those!" + Environment.NewLine;
            welcomeText += "4. You'll be presented with filetrees for your Unpacked Game Files and your Projects." + Environment.NewLine;
            welcomeText += "- Browse the Unpacked Game Files and rightclick the files you wish to copy to your project." + Environment.NewLine;
            welcomeText += "  " + this.applicationName + " automatically takes care of the correct pathways for your Mod." + Environment.NewLine;
            welcomeText += "5. Browse the Project Files and doubleclick on the files you wish the edit." + Environment.NewLine;
            welcomeText += "6. A build in XML editor will open where you can do all your work." + Environment.NewLine;
            welcomeText += "- " + this.applicationName + " has a few build in security measures to make sure you don't make any changes that will not work with the game." + Environment.NewLine;
            welcomeText += "- Make sure you save your work regularly! Especially during the Alpha phase of " + this.applicationName + "!" + Environment.NewLine;
            welcomeText += "7. Once you are done, click Build => Build Project to make your mod." + Environment.NewLine;
            welcomeText += "- Once finsihed, you can find your mod inside your Projects directory. Copy it to your game and have fun!";

            labelMainFormWelcome.Text = welcomeText;
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

        private void listBoxProjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBoxProjects.SelectedItem != null)
            {
                openProject(listBoxProjects.SelectedItem.ToString());
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

        private void hideConfigRequiredControls()
        {
            newToolStripMenuItem.Enabled = false;
        }

        private void showConfigRequiredControls()
        {
            newToolStripMenuItem.Enabled = true;
        }

        private void hideProjectRequiredControls()
        {
            openToolStripMenuItem.Enabled = false;
            buildProjectToolStripMenuItem.Enabled = false;
        }

        private void showProjectRequiredControls()
        {
            openToolStripMenuItem.Enabled = true;
            buildProjectToolStripMenuItem.Enabled = true;
            labelMainFormWelcome.Visible = false;
            labelSelectProject.Visible = true;
            listBoxProjects.Visible = true;
        }

        private void hideOpenProjectRequiredControls()
        {
            closeToolStripMenuItem.Enabled = false;
        }

        private void showOpenProjectRequiredControls()
        {
            closeToolStripMenuItem.Enabled = true;
        }

        public void hideOpenDocumentRequiredControls()
        {
            saveToolStripMenuItem.Enabled = false;
            saveAllToolStripMenuItem.Enabled = false;
            editToolStripMenuItem.Enabled = false;
            searchToolStripMenuItem.Enabled = false;
            viewToolStripMenuItem.Enabled = false;
        }

        public void showOpenDocumentRequiredControls()
        {
            saveToolStripMenuItem.Enabled = true;
            saveAllToolStripMenuItem.Enabled = true;
            editToolStripMenuItem.Enabled = true;
            searchToolStripMenuItem.Enabled = true;
            viewToolStripMenuItem.Enabled = true;
        }

        private void positionElements()
        {
            labelSelectProject.Left = (this.Width - labelSelectProject.Width) / 2;
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

            labelSelectProject.Visible = false;
            listBoxProjects.Visible = false;
            tabControl.Visible = true;
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
                                //TODO: ArgumentNullException
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

        private void OnApplicationExit(object sender, EventArgs e)
        {
        }
    }
}
