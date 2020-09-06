using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedModdingStation
{
    public partial class HelpForm : Form
    {
        MainForm form;
        public HelpForm(MainForm form)
        {
            InitializeComponent();

            this.form = form;

            helpText();
            importText();
        }

        private void listViewHelp_MouseClick(object sender, MouseEventArgs e)
        {
            var items = listViewHelp.SelectedItems;

            for (int i = 0; i < items.Count; i++)
            {
                string itemName = items[i].Text;

                if (itemName.Equals("Beginners Guide", StringComparison.OrdinalIgnoreCase))
                {
                    richTextBoxImportGuide.Visible = false;
                    richTextBoxBeginnersGuide.Visible = true;
                }
                else
                {
                    richTextBoxBeginnersGuide.Visible = false;
                    richTextBoxImportGuide.Visible = true;
                }
            }
        }

        private void helpText()
        {
            richTextBoxBeginnersGuide.Dock = DockStyle.Fill;
            richTextBoxBeginnersGuide.Clear();
            richTextBoxBeginnersGuide.SelectedText = "Please follow these steps to get started:" + Environment.NewLine + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = true;
            richTextBoxBeginnersGuide.SelectedText = "Click Config => Settings and setup your Paths." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "Click Build => Unpack Game Files and allow it to finish." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = false;
            richTextBoxBeginnersGuide.SelectedText = "- "+ form.applicationName + " automatically checks if your choosen Unpacked Game Files directory is empty or not." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "  If it's not empty, it'll ask you if you wish to use its contents instead of unpacking new game files." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "  If you choose to unpack anyway, all contents will be removed first!" + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = true;
            richTextBoxBeginnersGuide.SelectedText = "Click File => New => Project and choose a project name." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = false;
            richTextBoxBeginnersGuide.SelectedText = "- Do NOT import / use existing projects from other programs without following the guide for that!" + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = true;
            richTextBoxBeginnersGuide.SelectedText = "You'll be presented with filetrees for your Unpacked Game Files and your Projects." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = false;
            richTextBoxBeginnersGuide.SelectedText = "- Browse the Unpacked Game Files and rightclick the files you wish to copy to your project." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "  " + form.applicationName + " automatically takes care of the correct pathways for your Mod." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = true;
            richTextBoxBeginnersGuide.SelectedText = "Browse the Project Files and doubleclick on the files you wish the edit." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "A build in XML editor will open where you can do all your work." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = false;
            richTextBoxBeginnersGuide.SelectedText = "- " + form.applicationName + " has a few build in security measures to make sure you don't make any changes that will not work with the game." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectedText = "- Make sure you save your work regularly! Especially during the Alpha phase of " + form.applicationName + "!" + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = true;
            richTextBoxBeginnersGuide.SelectedText = "Once you are done, click Build => Build Project to make your mod." + Environment.NewLine;
            richTextBoxBeginnersGuide.SelectionBullet = false;
            richTextBoxBeginnersGuide.SelectedText = "- Once finsihed, you can find your mod inside your Projects directory. Copy it to your game and have fun!";
        }

        private void importText()
        {
            richTextBoxImportGuide.Dock = DockStyle.Fill;
            richTextBoxImportGuide.Clear();
            richTextBoxImportGuide.SelectedText = "Please follow these steps to import an existing mod:" + Environment.NewLine + Environment.NewLine;
            richTextBoxImportGuide.SelectionBullet = true;
            richTextBoxImportGuide.SelectedText = "Create a new folder inside your Projects folder." + Environment.NewLine;
            richTextBoxImportGuide.SelectedText = "Name the folder whatever you wish to name your Project." + Environment.NewLine;
            richTextBoxImportGuide.SelectedText = "Unpack your mod using an external program like PSArc inside the folder you just created." + Environment.NewLine;
            richTextBoxImportGuide.SelectionBullet = false;
            richTextBoxImportGuide.SelectedText = "- Do NOT put decompiled EXML files inside your project folder! " + form.applicationName + " does not work with EXML files!" + Environment.NewLine;
            richTextBoxImportGuide.SelectedText = "  " + form.applicationName + " works directly with MBIN files instead." + Environment.NewLine;
            richTextBoxImportGuide.SelectionBullet = true;
            richTextBoxImportGuide.SelectedText = "After all done, restart " + form.applicationName + ". Your imported mod should now be available as a project.";
            richTextBoxImportGuide.SelectionBullet = false;
        }
    }
}
