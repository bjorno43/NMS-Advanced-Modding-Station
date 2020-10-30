using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace AdvancedModdingStation
{
    public partial class ConfigForm : Form
    {
        MainForm form;
        public ConfigForm(MainForm form)
        {
            InitializeComponent();
            this.form = form;

            labelConfigWelcome.Text = "This is " + form.applicationName + "'s configuration panel." + Environment.NewLine + " Here you can set up the folder paths that " + form.applicationName + Environment.NewLine + " needs in order to function properly and change its layout. " + Environment.NewLine + Environment.NewLine + " Other features might be added in the future as well.";

            loadTextBoxes();
        }

        private void listViewConfig_MouseClick(object sender, MouseEventArgs e)
        {
            var items = listViewConfig.SelectedItems;

            for (int i = 0; i < items.Count; i++)
            {
                string itemName = items[i].Text;
                panelConfigPaths.Visible = itemName.Equals("Paths", StringComparison.OrdinalIgnoreCase);
                labelConfigWelcome.Visible = !itemName.Equals("Paths", StringComparison.OrdinalIgnoreCase); ;
            }
        }

        private void buttonConfigSelectNMSInstall_Click(object sender, EventArgs e)
        {
            SelectGameDirectory();
        }

        private void buttonConfigSelectUnpacked_Click(object sender, EventArgs e)
        {
            SelectUnpackDirectory();
        }

        private void buttonConfigSelectProjects_Click(object sender, EventArgs e)
        {
            SelectProjectsDirectory();
        }

        private void buttonConfigResetPaths_Click(object sender, EventArgs e)
        {
            MainForm.AddOrUpdateAppSettings("gameDir", "");
            MainForm.AddOrUpdateAppSettings("unpackedDir", "");
            MainForm.AddOrUpdateAppSettings("projectsDir", "");

            textBoxConfigNMSInstall.Text = "";
            textBoxConfigProjects.Text = "";
            textBoxConfigUnpacked.Text = "";
        }

        private void loadTextBoxes()
        {
            textBoxConfigNMSInstall.Text = ConfigurationManager.AppSettings.Get("gameDir");
            textBoxConfigUnpacked.Text = ConfigurationManager.AppSettings.Get("unpackedDir");
            textBoxConfigProjects.Text = ConfigurationManager.AppSettings.Get("projectsDir");
        }

        private void SelectGameDirectory()
        {
            FolderBrowserDialog gameDirDlg = new FolderBrowserDialog
            {
                Description = "Please select your No Man's Sky installation directory.",
                ShowNewFolderButton = false
            };
            DialogResult result = gameDirDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (Directory.Exists(gameDirDlg.SelectedPath + "\\GAMEDATA\\PCBANKS"))
                {
                    try
                    {
                        MainForm.AddOrUpdateAppSettings("gameDir", gameDirDlg.SelectedPath);
                        textBoxConfigNMSInstall.Text = gameDirDlg.SelectedPath;
                    }
                    catch (Exception e)
                    {
                        string errorMessage = applicationErrorMessage("save", form.applicationName);
                        errorMessage += applicationErrorMessage("bug");
                        errorMessage += "Solution: " + form.errorBug + Environment.NewLine + Environment.NewLine;
                        errorMessage += e.Message;
                        string caption = applicationErrorMessage("");

                        DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        if (errorResult == DialogResult.OK)
                        {
                            form.closeApp();
                        }
                        else
                        {
                            form.closeApp();
                        }
                    }
                }
                else
                {
                    string errorMessage = applicationErrorMessage("save", form.applicationName);
                    errorMessage += applicationErrorMessage("misconfig");
                    errorMessage += $"Solution: {gameDirDlg.Description}";
                    string caption = applicationErrorMessage("");

                    DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (errorResult == DialogResult.OK)
                    {
                        SelectGameDirectory();
                    }
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get("gameDir")))
                {
                    string errorMessage = applicationErrorMessage("installDir", form.applicationName);
                    errorMessage += applicationErrorMessage("misconfig");
                    errorMessage += $"Solution: {gameDirDlg.Description}";
                    string caption = applicationErrorMessage("");

                    DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (errorResult == DialogResult.OK)
                    {
                        SelectGameDirectory();
                    }
                    else
                    {
                        form.closeApp();
                    }
                }
            }

            form.checkConfigurationSet();
        }

        private void SelectUnpackDirectory()
        {
            FolderBrowserDialog unpackDirDlg = new FolderBrowserDialog
            {
                Description = "Please select the location where you wish to unpack your game files.",
                ShowNewFolderButton = true
            };
            DialogResult result = unpackDirDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                string rootPath = Path.GetPathRoot(unpackDirDlg.SelectedPath);

                if (!String.IsNullOrEmpty(rootPath))
                {
                    try
                    {
                        DriveInfo drive = new DriveInfo(rootPath);

                        long freeSpace = drive.AvailableFreeSpace;
                        long requiredSpace = 32212254720;

                        if (freeSpace < requiredSpace)
                        {
                            string errorMessage = applicationErrorMessage("gameFile", this.form.applicationName, unpackDirDlg.SelectedPath);
                            errorMessage += applicationErrorMessage("windows");
                            errorMessage += applicationErrorMessage("sol1");
                            string caption = applicationErrorMessage("");

                            DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                            if (result == DialogResult.Retry)
                            {
                                SelectUnpackDirectory();
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            MainForm.AddOrUpdateAppSettings("unpackedDir", unpackDirDlg.SelectedPath);
                            textBoxConfigUnpacked.Text = unpackDirDlg.SelectedPath;
                        }
                    }
                    catch (ArgumentException)
                    {
                        string errorMessage = applicationErrorMessage("space", form.applicationName);
                        errorMessage += applicationErrorMessage("bug");
                        errorMessage += applicationErrorMessage("sol2");
                        string caption = applicationErrorMessage("");

                        DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                        if (result == DialogResult.Retry)
                        {
                            SelectUnpackDirectory();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            form.checkConfigurationSet();
        }

        private void SelectProjectsDirectory()
        {
            FolderBrowserDialog projectsDirDlg = new FolderBrowserDialog
            {
                Description = "Please select the location where you wish to unpack your game files.",
                ShowNewFolderButton = true
            };
            DialogResult result = projectsDirDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                string rootPath = Path.GetPathRoot(projectsDirDlg.SelectedPath);

                if (!String.IsNullOrEmpty(rootPath))
                {
                    try
                    {
                        DriveInfo drive = new DriveInfo(rootPath);

                        long freeSpace = drive.AvailableFreeSpace;
                        long requiredSpace = 1073741824;

                        if (freeSpace < requiredSpace)
                        {
                            string errorMessage = applicationErrorMessage("path",this.form.applicationName,projectsDirDlg.SelectedPath);
                            errorMessage += applicationErrorMessage("windows");
                            errorMessage += applicationErrorMessage("sol3");
                            string caption = applicationErrorMessage("");

                            DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                            if (result == DialogResult.Retry)
                            {
                                SelectProjectsDirectory();
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            MainForm.AddOrUpdateAppSettings("projectsDir", projectsDirDlg.SelectedPath);
                            textBoxConfigProjects.Text = projectsDirDlg.SelectedPath;
                        }
                    }
                    catch (ArgumentException)
                    {
                        string errorMessage = applicationErrorMessage("space", form.applicationName);
                        errorMessage += applicationErrorMessage("bug");
                        errorMessage += applicationErrorMessage("sol4");
                        string caption = applicationErrorMessage("");

                        DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                        if (result == DialogResult.Retry)
                        {
                            SelectProjectsDirectory();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            form.checkConfigurationSet();
        }

        private string applicationErrorMessage(string err, string applicationName = null, string path = null) => err switch
        {
            "save" => $"{applicationName} encountered an error while trying to save your No Man's Sky installation directory. {Environment.NewLine}{Environment.NewLine}",
            "installDir" => $"{applicationName} can't function without knowing where your No Man's Sky installation directory is located. Press Ok to try again or Cancel to exit. {Environment.NewLine}{Environment.NewLine}",
            "space" => $"{applicationName} encountered an error while trying to select the choosen hard drive in order to calculate free space. {Environment.NewLine}{Environment.NewLine}",
            "path" => $"{applicationName} encountered an error while trying to set {path} as your Projects directory. {Environment.NewLine}{Environment.NewLine}",
            "gameFile" => $"{applicationName} encountered an error while trying to set {path} as your Unpacked Games Files directory. {Environment.NewLine}{Environment.NewLine}",
            "bug" => $"Error type: {MainForm.errorType.Bug} {Environment.NewLine}",
            "windows" => $"Error type: {MainForm.errorType.Windows} {Environment.NewLine}",
            "misconfig" => $"Error type: {MainForm.errorType.Misconfiguration} {Environment.NewLine}",
            "sol1" => "Solution: Please make sure you have at least 30 GigaBytes of free space in the target location!",
            "sol2" => "Solution: Please make sure you do NOT select a network or removable drive as your Unpacked Game Files location.",
            "sol3" => "Solution: Please make sure you have at least 1 GigaByte of free space in the target location!",
            "sol4" => "Solution: Please make sure you do NOT select a network or removable drive as your Projects location.",
            _ => "Error!"
        };
    }
}
