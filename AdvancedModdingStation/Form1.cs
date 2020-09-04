using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        // private variables

        public MainForm()
        {
            InitializeComponent();

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

            // Set initial variables
            setInitialVariables();

            checkConfigurationSet();

            checkExistingProjects();

            hideOpenProjectRequiredControls();

            hideOpenDocumentRequiredControls();

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

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showConfigurationForm();
        }

        private void unpackGameFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NMSPackagesManager manager = new NMSPackagesManager(this);
            manager.unpackGamePackages();
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
