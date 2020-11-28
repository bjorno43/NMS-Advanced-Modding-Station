using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedModdingStation
{
    class NMSPackagesManager
    {
        MainForm form;
        ErrorMessages em = new ErrorMessages();

        public NMSPackagesManager(MainForm form)
        {
            this.form = form;
        }

        public void importMod()
        {
            string projectsDir = ConfigurationManager.AppSettings.Get("projectsDir");

            if (String.IsNullOrWhiteSpace(projectsDir))
            {
                em.ErrorMessageInBox();
                string caption = "Error!";

                DialogResult errorResult = MessageBox.Show(em.ErrorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (errorResult == DialogResult.OK)
                {
                    return;
                }
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = projectsDir;
                openFileDialog.Filter = "Game Packages (*.pak)|*.pak|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (Directory.Exists(projectsDir + "\\" + fileName))
                        {
                            string errorMessage = "A project with the name " + fileName + " already exists. Do you wish to delete its contents and proceed with importing the mod?";
                            string caption = "Project already exists!";

                            DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if(errorResult == DialogResult.Yes)
                            {
                                // Directory not empty. Lets remove everything before proceding
                                DirectoryInfo di = new DirectoryInfo(projectsDir + "\\" + fileName);
                                string[] oldFiles = Directory.GetFiles(projectsDir + "\\" + fileName, "*.*", SearchOption.AllDirectories);
                                double percentagePerFile = 100.0 / oldFiles.Length;
                                double currentProgressPercentage = 0.0;

                                form.labelInfo.Text = "Deleting old project files..";
                                foreach (FileInfo file in di.EnumerateFiles())
                                {
                                    file.Delete();
                                    currentProgressPercentage += percentagePerFile;
                                    form.progressBarInfo.Value = (int)Math.Round(currentProgressPercentage, MidpointRounding.AwayFromZero);
                                }

                                form.labelInfo.Text = "Deleting old project directories..";
                                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                                {
                                    dir.Delete(true);
                                }
                                form.progressBarInfo.Value = 0;
                                form.labelInfo.Text = "Ready!";
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(projectsDir + "\\" + fileName);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                string errorMessage =  packageErrorMessage("unauthorized",this.form.applicationName);
                                errorMessage += packageErrorMessage("windows","", "(Access denied!)");
                                errorMessage += packageErrorMessage("sol1", "", projectsDir);
                                string caption = packageErrorMessage("");

                                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                                if (errorResult == DialogResult.Retry)
                                {
                                    importMod();
                                }
                                else
                                {
                                    return;
                                }
                            }
                            catch (PathTooLongException)
                            {
                                string errorMessage = packageErrorMessage("unauthorized", this.form.applicationName);
                                errorMessage += packageErrorMessage("windows", "", "(Path too long!)");
                                errorMessage += packageErrorMessage("sol2") ;
                                string caption = packageErrorMessage("");

                                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                                if (errorResult == DialogResult.Retry)
                                {
                                    importMod();
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                        PSArcXmlFile.XmlFileType xmlType = PSArcXmlFile.XmlFileType.Extract;
                        PSArcXmlFile modfilesXML = new PSArcXmlFile(xmlType);
                        modfilesXML.OutputFileName = projectsDir + "\\" + fileName;
                        modfilesXML.AddPakToExtract(filePath);

                        PSArc psarc = new PSArc();

                        BackgroundWorker bw = new BackgroundWorker();

                        // this allows our worker to report progress during work
                        bw.WorkerReportsProgress = true;

                        // what to do in the background thread
                        bw.DoWork += new DoWorkEventHandler(
                        delegate (object o, DoWorkEventArgs args)
                        {
                            BackgroundWorker b = o as BackgroundWorker;

                            b.ReportProgress(0);
                            psarc.Extract(modfilesXML);
                            b.ReportProgress(100);
                        });

                        // what to do when progress changed (update the progress bar for example)
                        bw.ProgressChanged += new ProgressChangedEventHandler(
                        delegate (object o, ProgressChangedEventArgs args)
                        {
                            form.Invoke((MethodInvoker)(() => form.labelInfo.Text = String.Format("Unpacking Mod {0}%...", args.ProgressPercentage)));
                            form.Invoke((MethodInvoker)(() => form.progressBarInfo.Value = args.ProgressPercentage));
                        });

                        // what to do when worker completes its task (notify the user)
                        bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                        delegate (object o, RunWorkerCompletedEventArgs args)
                        {
                            form.Invoke((MethodInvoker)(() => form.labelInfo.Text = "Ready!"));
                            form.Invoke((MethodInvoker)(() => form.progressBarInfo.Value = 0));
                            form.Invoke((MethodInvoker)(() => form.loadProjectNames()));
                        });

                        bw.RunWorkerAsync();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public void unpackGamePackages()
        {
            string unpackedDir = ConfigurationManager.AppSettings.Get("unpackedDir");
            string gameDir = ConfigurationManager.AppSettings.Get("gameDir");
            string pcBanksDir = gameDir + "\\GAMEDATA\\PCBANKS";
            string[] gameFiles = { };

            // If the unpacked game files directory doesn't exist, it's safe to assume that the game files aren't unpacked yet
            if (String.IsNullOrWhiteSpace(unpackedDir))
            {
                string errorMessage = packageErrorMessage("readUnpacked", this.form.applicationName);
                errorMessage += packageErrorMessage("misconfig");
                errorMessage += "Solution: Please go to Config => Settings and setup your Paths before attempting to unpack." + Environment.NewLine;
                string caption = packageErrorMessage("");

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if(errorResult == DialogResult.OK)
                {
                    return;
                }
            }

            if (!Directory.Exists(unpackedDir))
            {
                try
                {
                    DirectoryInfo di = Directory.CreateDirectory(unpackedDir);
                }
                catch (UnauthorizedAccessException)
                {
                    string errorMessage = packageErrorMessage("unpacked", this.form.applicationName);
                    errorMessage += packageErrorMessage("windows", "", "(Access denied!)");
                    errorMessage += packageErrorMessage("sol1", ConfigurationManager.AppSettings.Get("unpackedDir"));
                    string caption = packageErrorMessage("");

                    DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                    if (errorResult == DialogResult.Retry)
                    {
                        unpackGamePackages();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (PathTooLongException)
                {
                    string errorMessage = packageErrorMessage("unpacked", this.form.applicationName); 
                    errorMessage += packageErrorMessage("windows", "", "(Path too long!)");
                    errorMessage += packageErrorMessage("sol3");
                    string caption = packageErrorMessage("");

                    DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                    if (errorResult == DialogResult.Retry)
                    {
                        unpackGamePackages();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                // Directory already exists. Lets check if it's empty or not
                if (Directory.EnumerateFileSystemEntries(unpackedDir).Any())
                {
                    string infoMessage = this.form.applicationName + " detected that " + ConfigurationManager.AppSettings.Get("unpackedDir") + " is not empty." + Environment.NewLine + Environment.NewLine;
                    infoMessage += "Do you wish to use its contents as your Unpacked Game Files instead?" + Environment.NewLine + Environment.NewLine;
                    infoMessage += "Press Yes to use the existing contents. Press No to delete the existing contents and unpack new game files.";
                    string caption = "Unpack Game folder not empty!";

                    DialogResult infoResult = MessageBox.Show(infoMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if(infoResult == DialogResult.No)
                    {
                        // Directory not empty. Lets remove everything before proceding
                        DirectoryInfo di = new DirectoryInfo(unpackedDir);
                        string[] oldFiles = Directory.GetFiles(unpackedDir, "*.*", SearchOption.AllDirectories);
                        double percentagePerFile = 100.0 / oldFiles.Length;
                        double currentProgressPercentage = 0.0;

                        form.labelInfo.Text = "Deleting old unpacked game files..";
                        foreach (FileInfo file in di.EnumerateFiles())
                        {
                            file.Delete();
                            currentProgressPercentage += percentagePerFile;
                            form.progressBarInfo.Value = (int)Math.Round(currentProgressPercentage, MidpointRounding.AwayFromZero);
                        }

                        form.labelInfo.Text = "Deleting old unpacked game directories..";
                        foreach (DirectoryInfo dir in di.EnumerateDirectories())
                        {
                            dir.Delete(true);
                        }
                        form.progressBarInfo.Value = 0;
                        form.labelInfo.Text = "Ready!";
                    }
                    else
                    {
                        return;
                    }
                }
            }

            try
            {
                gameFiles = Directory.GetFiles(pcBanksDir, "NMSARC.*.pak", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException)
            {
                string errorMessage =  packageErrorMessage("error", this.form.applicationName, pcBanksDir);
                errorMessage += packageErrorMessage("windows", "", "(Access Denied!)");
                errorMessage += packageErrorMessage("sol1", "", pcBanksDir);
                string caption = packageErrorMessage("");

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (errorResult == DialogResult.Retry)
                {
                    unpackGamePackages();
                }
                else
                {
                    return;
                }
            }
            catch (DirectoryNotFoundException)
            {
                string errorMessage = packageErrorMessage("error", this.form.applicationName, pcBanksDir);
                errorMessage += packageErrorMessage("windows", "", "(Directory not found!)");
                errorMessage += packageErrorMessage("sol5","",pcBanksDir);
                string caption = packageErrorMessage("");

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (errorResult == DialogResult.Retry)
                {
                    unpackGamePackages();
                }
                else
                {
                    return;
                }
            }
            catch (PathTooLongException)
            {
                string errorMessage = packageErrorMessage("error", this.form.applicationName, pcBanksDir);
                errorMessage += packageErrorMessage("windows", "", "(Path too long!)");
                errorMessage += packageErrorMessage("sol4");
                string caption = packageErrorMessage("");

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (errorResult == DialogResult.OK)
                {
                    return;
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                string errorMessage = packageErrorMessage("path", this.form.applicationName ,pcBanksDir);
                        errorMessage += packageErrorMessage("bug");
                        errorMessage += packageErrorMessage("solBug");
                        errorMessage += e.Message + Environment.NewLine + Environment.NewLine;
                        errorMessage += packageErrorMessage("cancel",form.applicationName);
                string caption = packageErrorMessage("");

                DialogResult errorResult = MessageBox.Show(errorMessage, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                if (errorResult == DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    return;
                }
            }

            if (gameFiles != null && gameFiles.Length != 0)
            {
                string unpackPath = unpackedDir;
                const PSArcXmlFile.XmlFileType xmlType = PSArcXmlFile.XmlFileType.Extract;

                //PSArcXmlFile gamefilesXML1 = new PSArcXmlFile(xmlType);
                //gamefilesXML1.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML2 = new PSArcXmlFile(xmlType);
                //gamefilesXML2.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML3 = new PSArcXmlFile(xmlType);
                //gamefilesXML3.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML4 = new PSArcXmlFile(xmlType);
                //gamefilesXML4.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML5 = new PSArcXmlFile(xmlType);
                //gamefilesXML5.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML6 = new PSArcXmlFile(xmlType);
                //gamefilesXML6.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML7 = new PSArcXmlFile(xmlType);
                //gamefilesXML7.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML8 = new PSArcXmlFile(xmlType);
                //gamefilesXML8.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML9 = new PSArcXmlFile(xmlType);
                //gamefilesXML9.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML10 = new PSArcXmlFile(xmlType);
                //gamefilesXML10.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML11 = new PSArcXmlFile(xmlType);
                //gamefilesXML11.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML12 = new PSArcXmlFile(xmlType);
                //gamefilesXML12.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML13 = new PSArcXmlFile(xmlType);
                //gamefilesXML13.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML14 = new PSArcXmlFile(xmlType);
                //gamefilesXML14.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML15 = new PSArcXmlFile(xmlType);
                //gamefilesXML15.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML16 = new PSArcXmlFile(xmlType);
                //gamefilesXML16.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML17 = new PSArcXmlFile(xmlType);
                //gamefilesXML17.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML18 = new PSArcXmlFile(xmlType);
                //gamefilesXML18.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML19 = new PSArcXmlFile(xmlType);
                //gamefilesXML19.OutputFileName = unpackPath;
                //PSArcXmlFile gamefilesXML20 = new PSArcXmlFile(xmlType);
                //gamefilesXML20.OutputFileName = unpackPath;

                //foreach (string file in gameFiles)
                //{
                //    if (counter <= twenty)
                //    {
                //        gamefilesXML1.AddPakToExtract(file);
                //    }
                //    if (counter > twenty && counter <= twenty * 2)
                //    {
                //        gamefilesXML2.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 2 && counter <= twenty * 3)
                //    {
                //        gamefilesXML3.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 3 && counter <= twenty * 4)
                //    {
                //        gamefilesXML4.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 4 && counter <= twenty * 5)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 5 && counter <= twenty * 6)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 6 && counter <= twenty * 7)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 7 && counter <= twenty * 8)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 8 && counter <= twenty * 9)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 9 && counter <= twenty * 10)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 10 && counter <= twenty * 11)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 11 && counter <= twenty * 12)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 12 && counter <= twenty * 13)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 13 && counter <= twenty * 14)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 14 && counter <= twenty * 15)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 15 && counter <= twenty * 16)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 16 && counter <= twenty * 17)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 17 && counter <= twenty * 18)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 18 && counter <= twenty * 19)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //    if (counter > twenty * 19)
                //    {
                //        gamefilesXML5.AddPakToExtract(file);
                //    }
                //counter++;
                //}
                PSArc psarc = new PSArc();

                BackgroundWorker bw = new BackgroundWorker
                {

                    // this allows our worker to report progress during work
                    WorkerReportsProgress = true
                };

                // what to do in the background thread
                bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {

                    BackgroundWorker b = o as BackgroundWorker;

                    form.Invoke((MethodInvoker)(() => form.configToolStripMenuItem.Enabled = false));
                    form.Invoke((MethodInvoker)(() => form.buildToolStripMenuItem.Enabled = false));
                    form.Invoke((MethodInvoker)(() => form.labelUnpackGameFiles.Visible = false));
                    form.Invoke((MethodInvoker)(() => form.labelUnpackingInProgress.Visible = true));
                    form.Invoke((MethodInvoker)(() => form.backgroundWorkerInProgress = true));

                    int twenty = gameFiles.Length / 20;
                    int counter = 1;

                    foreach (string file in gameFiles)
                    {
                        PSArcXmlFile gamefilesXML00 = new PSArcXmlFile(xmlType)
                        {
                            OutputFileName = unpackPath
                        };
                        if (counter <= twenty)
                        {
                            gamefilesXML00.AddFileToExtract(file);
                            b.ReportProgress(0);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty && counter <= twenty * 2)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(5);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 2 && counter <= twenty * 3)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(10);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 3 && counter <= twenty * 4)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(15);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 4 && counter <= twenty * 5)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(20);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 5 && counter <= twenty * 6)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(25);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 6 && counter <= twenty * 7)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(30);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 7 && counter <= twenty * 8)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(35);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 8 && counter <= twenty * 9)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(40);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 9 && counter <= twenty * 10)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(45);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 10 && counter <= twenty * 11)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(50);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 11 && counter <= twenty * 12)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(55);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 12 && counter <= twenty * 13)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(60);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 13 && counter <= twenty * 14)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(65);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 14 && counter <= twenty * 15)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(70);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 15 && counter <= twenty * 16)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(75);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 16 && counter <= twenty * 17)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(80);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 17 && counter <= twenty * 18)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(85);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 18 && counter <= twenty * 19)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(90);
                            psarc.Extract(gamefilesXML00);
                        }
                        if (counter > twenty * 19)
                        {
                            gamefilesXML00.AddPakToExtract(file);
                            b.ReportProgress(95);
                            psarc.Extract(gamefilesXML00);
                        }
                        counter++;
                    }
                    b.ReportProgress(100);

                    //b.ReportProgress(0);
                    //psarc.Extract(gamefilesXML1);
                    //b.ReportProgress(5);
                    //psarc.Extract(gamefilesXML2);
                    //b.ReportProgress(10);
                    //psarc.Extract(gamefilesXML3);
                    //b.ReportProgress(15);
                    //psarc.Extract(gamefilesXML4);
                    //b.ReportProgress(20);
                    //psarc.Extract(gamefilesXML5);
                    //b.ReportProgress(25);
                    //psarc.Extract(gamefilesXML6);
                    //b.ReportProgress(30);
                    //psarc.Extract(gamefilesXML7);
                    //b.ReportProgress(35);
                    //psarc.Extract(gamefilesXML8);
                    //b.ReportProgress(40);
                    //psarc.Extract(gamefilesXML9);
                    //b.ReportProgress(45);
                    //psarc.Extract(gamefilesXML10);
                    //b.ReportProgress(50);
                    //psarc.Extract(gamefilesXML11);
                    //b.ReportProgress(55);
                    //psarc.Extract(gamefilesXML12);
                    //b.ReportProgress(60);
                    //psarc.Extract(gamefilesXML13);
                    //b.ReportProgress(65);
                    //psarc.Extract(gamefilesXML14);
                    //b.ReportProgress(70);
                    //psarc.Extract(gamefilesXML15);
                    //b.ReportProgress(75);
                    //psarc.Extract(gamefilesXML16);
                    //b.ReportProgress(80);
                    //psarc.Extract(gamefilesXML17);
                    //b.ReportProgress(85);
                    //psarc.Extract(gamefilesXML18);
                    //b.ReportProgress(90);
                    //psarc.Extract(gamefilesXML19);
                    //b.ReportProgress(95);
                    //psarc.Extract(gamefilesXML20);
                    //b.ReportProgress(100);

                });

                // what to do when progress changed (update the progress bar for example)
                bw.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    form.Invoke((MethodInvoker)(() => form.labelInfo.Text = String.Format("Unpacking Game Files {0}%...", args.ProgressPercentage)));
                    form.Invoke((MethodInvoker)(() => form.progressBarInfo.Value = args.ProgressPercentage));
                });

                // what to do when worker completes its task (notify the user)
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    form.Invoke((MethodInvoker)(() => form.labelInfo.Text = "Ready!"));
                    form.Invoke((MethodInvoker)(() => form.progressBarInfo.Value = 0));
                    form.Invoke((MethodInvoker)(() => form.buildToolStripMenuItem.Enabled = true));
                    form.Invoke((MethodInvoker)(() => form.configToolStripMenuItem.Enabled = true));
                    form.Invoke((MethodInvoker)(() => form.labelUnpackingInProgress.Visible = false));
                    form.Invoke((MethodInvoker)(() => form.labelFirstProject.Visible = true));
                    form.Invoke((MethodInvoker)(() => form.newToolStripMenuItem.Enabled = true));
                    form.Invoke((MethodInvoker)(() => form.backgroundWorkerInProgress = false));
                });

                bw.RunWorkerAsync();
            }
        }

        private string packageErrorMessage(string err, string applicationName = null, string path = null) => err switch
        {
            "unauthorized" => $"{applicationName} encountered an error while trying to create your Project directory (it doesn't exist yet). {Environment.NewLine}{Environment.NewLine}",
            "unknown" => $"{applicationName} encountered an unknown error while trying to access {path} {Environment.NewLine}{Environment.NewLine}",
            "error" => $"{applicationName} encountered an error while trying to access {path} {Environment.NewLine}{Environment.NewLine}",
            "unpacked" => $"{applicationName} encountered an error while trying to create your Unpacked Game Files directory (it doesn't exist yet). {Environment.NewLine}{Environment.NewLine}",
            "cancel" => $"Press Cancel to save any unsaved work or Ok to close {applicationName}",
            "readUnpacked" => $"{applicationName} encountered an error while trying to read your Unpacked Game Files setting. {Environment.NewLine}{Environment.NewLine}",
            "bug" => $"Error type: {MainForm.errorType.Bug} {Environment.NewLine}",
            "windows" => $"Error type: {MainForm.errorType.Windows} {path} {Environment.NewLine}",
            "misconfig" => $"Error type: {MainForm.errorType.Misconfiguration} {Environment.NewLine}",
            "sol1" => $"Solution: Please make sure you have full read / write access to {path}",
            "sol2" => @"Solution: Please use a short path for your Project directory. For example: C:\NMSAdvancedModdingStation\Project",
            "sol3" => @"Solution: Please use a short path for your Unpacked Game Files directory. For example: C:\NMSAdvancedModdingStation\Unpacked",
            "solBug" => $"Solution: Please file a bug report including the message below: {Environment.NewLine}{Environment.NewLine}",
            "sol4" => "Solution: This error should never pop up. It would mean that you managed to install No Man's Sky in a location not accessable by Windows.",
            "sol5" => $"Solution: Please make sure {path} exists before trying again.",
            _ => "Error!"
        };
    }
}
