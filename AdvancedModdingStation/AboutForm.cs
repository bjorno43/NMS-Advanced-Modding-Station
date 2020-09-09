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
    public partial class AboutForm : Form
    {
        MainForm form;

        public AboutForm(MainForm form)
        {
            InitializeComponent();

            this.form = form;

            string aboutText = form.applicationName + Environment.NewLine + Environment.NewLine;
            aboutText += "Author: Bjorn van de Peut (icecub)" + Environment.NewLine;
            aboutText += "Version: 0.0.1 (Alpha)" + Environment.NewLine;
            aboutText += "License: MIT License" + Environment.NewLine + Environment.NewLine;
            aboutText += "Copyright (c) 2020 Bjorn van de Peut" + Environment.NewLine + Environment.NewLine;
            aboutText += "Permission is hereby granted, free of charge, to any person obtaining a copy" + Environment.NewLine;
            aboutText += "of this software and associated documentation files (the \"Software\"), to deal" + Environment.NewLine;
            aboutText += "in the Software without restriction, including without limitation the rights" + Environment.NewLine;
            aboutText += "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell" + Environment.NewLine;
            aboutText += "copies of the Software, and to permit persons to whom the Software is" + Environment.NewLine;
            aboutText += "furnished to do so, subject to the following conditions:" + Environment.NewLine + Environment.NewLine;
            aboutText += "The above copyright notice and this permission notice shall be included in all" + Environment.NewLine;
            aboutText += "copies or substantial portions of the Software." + Environment.NewLine + Environment.NewLine;
            aboutText += "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR" + Environment.NewLine;
            aboutText += "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY," + Environment.NewLine;
            aboutText += "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE" + Environment.NewLine;
            aboutText += "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER" + Environment.NewLine;
            aboutText += "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM," + Environment.NewLine;
            aboutText += "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE" + Environment.NewLine;
            aboutText += "SOFTWARE.";

            labelAbout.Text = aboutText;
        }
    }
}
