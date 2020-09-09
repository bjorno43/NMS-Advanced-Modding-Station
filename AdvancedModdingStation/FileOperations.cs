using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedModdingStation
{
    class FileOperations
    {
        MainForm form;

        public FileOperations(MainForm form)
        {
            this.form = form;
        }

        public void CutText()
        {
            form.textAreas[form.activeDocument].Cut();
        }

        public void CopyText()
        {
            form.textAreas[form.activeDocument].Copy();
        }

        public void PasteText()
        {
            form.textAreas[form.activeDocument].Paste();
        }

        public void SelectAllText()
        {
            form.textAreas[form.activeDocument].SelectAll();
        }

        public void SelectLineText()
        {
            Line line = form.textAreas[form.activeDocument].Lines[form.textAreas[form.activeDocument].CurrentLine];
            form.textAreas[form.activeDocument].SetSelection(line.Position + line.Length, line.Position);
        }

        public void SelectClearText()
        {
            form.textAreas[form.activeDocument].SetEmptySelection(0);
        }

        public void IndentText()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            // we use this hack to send "Tab" to scintilla, since there is no known API to indent,
            // although the indentation function exists. Pressing TAB with the editor focused confirms this.
            form.GenerateKeystrokes("{TAB}");
        }

        public void OutdentText()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            // we use this hack to send "Shift+Tab" to scintilla, since there is no known API to outdent,
            // although the indentation function exists. Pressing Shift+Tab with the editor focused confirms this.
            form.GenerateKeystrokes("+{TAB}");
        }

        public void Lowercase()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            // save the selection
            int start = form.textAreas[form.activeDocument].SelectionStart;
            int end = form.textAreas[form.activeDocument].SelectionEnd;

            // modify the selected text
            form.textAreas[form.activeDocument].ReplaceSelection(form.textAreas[form.activeDocument].GetTextRange(start, end - start).ToLower());

            // preserve the original selection
            form.textAreas[form.activeDocument].SetSelection(start, end);
        }

        public void Uppercase()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            // save the selection
            int start = form.textAreas[form.activeDocument].SelectionStart;
            int end = form.textAreas[form.activeDocument].SelectionEnd;

            // modify the selected text
            form.textAreas[form.activeDocument].ReplaceSelection(form.textAreas[form.activeDocument].GetTextRange(start, end - start).ToUpper());

            // preserve the original selection
            form.textAreas[form.activeDocument].SetSelection(start, end);
        }

        public void WordWrap()
        {
            // toggle word wrap
            form.wordWrapToolStripMenuItem.Checked = !form.wordWrapToolStripMenuItem.Checked;
            form.textAreas[form.activeDocument].WrapMode = form.wordWrapToolStripMenuItem.Checked ? WrapMode.Word : WrapMode.None;
        }

        public void IndentGuides()
        {
            // toggle indent guides
            form.showIndentGuidesToolStripMenuItem.Checked = !form.showIndentGuidesToolStripMenuItem.Checked;
            form.textAreas[form.activeDocument].IndentationGuides = form.showIndentGuidesToolStripMenuItem.Checked ? IndentView.LookBoth : IndentView.None;
        }

        public void WhiteSpace()
        {
            // toggle view whitespace
            form.showWhitespaceToolStripMenuItem.Checked = !form.showWhitespaceToolStripMenuItem.Checked;
            form.textAreas[form.activeDocument].ViewWhitespace = form.showWhitespaceToolStripMenuItem.Checked ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
        }

        public void ZoomIn()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            form.textAreas[form.activeDocument].ZoomIn();
        }

        public void ZoomOut()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            form.textAreas[form.activeDocument].ZoomOut();
        }

        public void ZoomDefault()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            form.textAreas[form.activeDocument].Zoom = 0;
        }

        public void CollapseAll()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            form.textAreas[form.activeDocument].FoldAll(FoldAction.Contract);
        }

        public void ExpandAll()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            form.textAreas[form.activeDocument].FoldAll(FoldAction.Expand);
        }

        public void OpenSearch()
        {
            if (String.IsNullOrEmpty(form.activeDocument))
            {
                return;
            }
            SearchManager.SearchBox = form.textBoxSearch;
            SearchManager.TextArea = form.textAreas[form.activeDocument];

            if (!form.SearchIsOpen)
            {
                form.SearchIsOpen = true;
                form.PanelSearch.Visible = true;
                form.PanelSearch.BringToFront();
                form.textBoxSearch.Text = SearchManager.LastSearch;
                form.textBoxSearch.Focus();
                form.textBoxSearch.SelectAll();
            }
            else
            {
                form.textBoxSearch.Focus();
                form.textBoxSearch.SelectAll();
            }
        }
    }
}
