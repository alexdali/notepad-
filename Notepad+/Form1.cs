using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notepad_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Draw mode on tab for icons
            this.tabControl1.Padding = new Point(12, 4);
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            this.tabControl1.Padding = new Point(12, 4);
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            this.tabControl1.DrawItem += TabControl1_DrawItem;
            this.tabControl1.MouseDown += TabControl1_MouseDown; 
            this.tabControl1.Selecting += TabControl1_Selecting;
            this.tabControl1.HandleCreated += TabControl1_HandleCreated;

            // filter for file dialog: open and save
            openFileDialog1.Filter = "Text files(*.txt)|*.txt|RTF files(*.rtf)|*.rtf";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|RTF files(*.rtf)|*.rtf";

            //add new empty tab on run programm
            IntPtr h = this.tabControl1.Handle;     //The TabControl's handle must be created for the Insert method to work
            AddNewTab();
            // add last tab as Add Button
            //AddPlusTab();
        }

        

        // init list for tabs and metadata collection
        //Dictionary<int, TabPageItem> dictTabPages = new Dictionary<int, TabPageItem>();
        List<TabPageItem> listTabPages = new List<TabPageItem>();

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl1_Selected(Object sender, TabControlEventArgs e)
        {
            //set info about tab and content into status bar
            if (tabControl1.TabCount > 1)
            {
                SetTabStatusInfo();
            }
            
            //for test purpose
            System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "TabPage", e.TabPage);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "TabPageIndex", e.TabPageIndex);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "Action", e.Action);
            messageBoxCS.AppendLine();
            //MessageBox.Show(messageBoxCS.ToString(), "Selected Event");
            System.Diagnostics.Debug.WriteLine(messageBoxCS.ToString(), "Selected Event");
        }

        
        //TO-DO: add open file to the end

        /// <summary>
        /// Handle for 'Open' strip menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string pathToFile = openFileDialog1.FileName;
            string fileName = Path.GetFileName(pathToFile); 
            //string fileText = File.ReadAllText(pathToFile);
            string title = fileName;

            TabPage newTabPage = new TabPage(title);
            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };

            string extensionOfFile = Path.GetExtension(pathToFile);

            try
            {
                if (extensionOfFile == ".txt")
                {
                    richTextBox.LoadFile(pathToFile, RichTextBoxStreamType.PlainText);
                }
                else if(extensionOfFile == ".rtf")
                {
                    richTextBox.LoadFile(pathToFile, RichTextBoxStreamType.RichText);
                }
            }
            catch (Exception)
            {

                throw;
            }
            
            richTextBox.TextChanged += new EventHandler(CurrencyTextBox_TextChanged);

            newTabPage.Controls.Add(richTextBox);
            
            int indexNewTab = GetIndexForInsertTab();
            //tabControl1.TabPages.Add(newTabPage);
            tabControl1.TabPages.Insert(indexNewTab, newTabPage);

            ListTabPagesAdd(tabControl1.TabPages[indexNewTab]);
            listTabPages[indexNewTab].Title = newTabPage.Text;

            tabControl1.SelectTab(indexNewTab);

            SetContentInfo(richTextBox);
        }


        /// <summary>
        /// /// Handle for 'Save' strip menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int indexCurrentTab = tabControl1.SelectedIndex;

            string fileName = listTabPages[indexCurrentTab].Title;

            string textCurrentTab = listTabPages[indexCurrentTab].RichTextBox.Text;
            File.WriteAllText(fileName, textCurrentTab);
        }

        /// <summary>
        /// /// Handle for 'SaveAs' strip menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentTab = tabControl1.SelectedTab;
            int indexCurrentTab = tabControl1.SelectedIndex;
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = saveFileDialog1.FileName;

            //Check exist the current tab in Dictionary. If not then add the tab to dictionary
            foreach (TabPageItem tabPageItem in listTabPages)
            {
                if (!Object.ReferenceEquals(tabPageItem.TabPage,currentTab))            // if (!listTabPages.Contains(currentTab))
                {
                    ListTabPagesAdd(currentTab); 
                }
            }


            // update property Title of TabPageItem from ListTabPages
            listTabPages[indexCurrentTab].Title = fileName;

            var textCurrentTab = listTabPages[indexCurrentTab].RichTextBox.Text;    

            File.WriteAllText(fileName, textCurrentTab);

            tabControl1.SelectedTab.Text = fileName;
        }


        /// <summary>
        /// /// Handle for 'New' strip menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewTab();
        }

        /// <summary>
        /// Method for adding new tab to tabControl and Tab List
        /// </summary>
        private void AddNewTab()
        {
            int lastIndex = GetIndexForInsertTab();

            InsertNewTab(lastIndex);
        }

        
        /// <summary>
        /// Method for inserting new tab in Tab Collection and Tab List
        /// </summary>
        /// <param name="index">index for inserting the tab into collections</param>
        private void InsertNewTab(int index)
        {
            var newTabPage = InitNewTab(index);

            tabControl1.TabPages.Insert(index, newTabPage);
            
            ListTabPagesAdd(tabControl1.TabPages[index]);
            listTabPages[index].Title = newTabPage.Text;
            tabControl1.SelectTab(index);
            SetTabStatusInfo();
        }

        /// <summary>
        /// Method for init new tab
        /// </summary>
        /// <returns>TabPage</returns>
        private TabPage InitNewTab(int index)
        {
            //string title = "New " + (tabControl1.TabCount + 1).ToString();
            string title = "New " + (index + 1).ToString();
            TabPage newTabPage = new TabPage(title);

            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };
            richTextBox.TextChanged += new EventHandler(CurrencyTextBox_TextChanged);

            newTabPage.Controls.Add(richTextBox);

            return newTabPage;
        }

        #region Helpers

        private int GetIndexForInsertTab()
        {
            int indexForInsertTab;
            if (tabControl1.TabCount > 1)
            {
                indexForInsertTab = tabControl1.TabCount - 1;
                return indexForInsertTab;
            }

            return indexForInsertTab = 0;

        }

        #endregion

        #region List methods

        /// <summary>
        /// Add tabPage to Tabs List
        /// </summary>
        /// <param name="tabPage">tabPage add to List</param>
        private void ListTabPagesAdd(TabPage tabPage)
        {

            TabPageItem tabPageItem = new TabPageItem(tabPage);

            foreach (Control control in tabPage.Controls)
            {
                if (control.GetType() == typeof(RichTextBox))
                    tabPageItem.RichTextBox = (RichTextBox)control;
            }

            try
            {
                listTabPages.Add(tabPageItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error by Add TabPage to List", MessageBoxButtons.OK);
            }


        }
        /// <param name="indexNewTab">current index of the tab in the tabpages collection</param>
        /// <param name="richTextBox">the instance of RichTextBox of the tab </param>
        //private void DictTabPagesAdd(int indexTab, TabPage tabPage)
        //{
        //    //TabPageItem tabPageItem = new TabPageItem(tabControl1.TabPages[indexNewTab]);
        //    TabPageItem tabPageItem = new TabPageItem(tabPage);
        //    //tabPageItem.FileName = filename;
        //    foreach(Control control in tabPage.Controls)
        //    {
        //            if(control.GetType() == typeof(RichTextBox))
        //                tabPageItem.RichTextBox = (RichTextBox)control;
        //    }

        //    try
        //    {
        //        if (!dictTabPages.ContainsKey(indexTab))
        //        {
        //            dictTabPages.Add(indexTab, tabPageItem);
        //        }
        //        else
        //        {
        //            throw new Exception("Dictionary dictTabPages contains the Index already!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Index already exist", MessageBoxButtons.OK);
        //    }
        //}


        /// <summary>
        /// Remove tabPage from List
        /// </summary>
        /// <param name="indexTab">index of the tab to be removed</param>
        private void ListTabPagesRemove(int indexTab)
        {
            try
            {
                listTabPages.RemoveAt(indexTab);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        #endregion


        #region Methods for Tab Icons: Close and Add Buttons

        /// <summary>
        /// Draw Close Button and Add Button for Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = this.tabControl1.TabPages[e.Index];
            var tabRect = this.tabControl1.GetTabRect(e.Index);
            tabRect.Inflate(-2, -2);
            if (e.Index == this.tabControl1.TabCount - 1)
            {
                var addImage = Properties.Resources.AddButton_Image;
                e.Graphics.DrawImage(addImage,
                    tabRect.Left + (tabRect.Width - addImage.Width) / 2,
                    tabRect.Top + (tabRect.Height - addImage.Height) / 2);
            }
            else
            {
                var closeImage = Properties.Resources.CloseButton_Image;
            e.Graphics.DrawImage(closeImage,
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2);
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                    tabRect, tabPage.ForeColor, TextFormatFlags.Left);
            }
        }

        /// <summary>
        /// To prevent the select the last tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //if (e.TabPageIndex == this.tabControl1.TabCount - 1)
            if ((e.TabPageIndex == this.tabControl1.TabCount - 1) && (tabControl1.TabPages[e.TabPageIndex].Text == "+"))
                    e.Cancel = true;
        }

        /// <summary>
        /// Handle click on close button and add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            var lastIndex = this.tabControl1.TabCount - 1;
            if (this.tabControl1.GetTabRect(lastIndex).Contains(e.Location))
            {
                InsertNewTab(lastIndex);
            }
            else
            {
                for (var i = 0; i < this.tabControl1.TabPages.Count; i++)
                {
                    var tabRect = this.tabControl1.GetTabRect(i);
                    tabRect.Inflate(-2, -2);
                    var closeImage = Properties.Resources.CloseButton_Image;
                    var imageRect = new Rectangle(
                        (tabRect.Right - closeImage.Width),
                        tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                        closeImage.Width,
                        closeImage.Height);
                    if (imageRect.Contains(e.Location))
                    {
                        this.tabControl1.TabPages.RemoveAt(i);
                        // and remove from list
                        ListTabPagesRemove(i);
                        break;
                    }
                }
            }
        }

        

        //private void AddPlusTab()
        //{
        //    string title = "+";
        //    TabPage plusTabPage = new TabPage(title);
        //    tabControl1.TabPages.Add(plusTabPage);
        //    if(tabControl1.TabCount > 1)
        //        tabControl1.SelectTab(tabControl1.TabCount - 2);
        //}


        /// <summary>
        /// Adjust Tab width
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wp"></param>
        /// <param name="lp"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;
        
        private void TabControl1_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(this.tabControl1.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)16);
        }

        #endregion

        #region RichTextBox Methods

        /// <summary>
        /// Handle for Text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void CurrencyTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("Event: currencyTextBox_TextChanged");
                int indexCurrentTab = tabControl1.SelectedIndex;
                //var richTextBoxCurrentTab = dictTabPages[indexCurrentTab].RichTextBox;
                // testlist
                var richTextBoxCurrentTab = listTabPages[indexCurrentTab].RichTextBox;
                SetContentInfo(richTextBoxCurrentTab);
            }
            catch (Exception)
            {
                // If there is an error, display the error' message.
                throw;
            }
        }
        #endregion

        #region Methods for Status Bar

        /// <summary>
        /// set info about tab and content into status bar
        /// </summary>
        private void SetTabStatusInfo()
        {
            int countOfTab = (tabControl1.TabCount <= 1) ? 0 : (tabControl1.TabCount - 1);
            label_CountOfTab.Text = $"Count of tab: {countOfTab}";
            label_currTabName.Text = $"Tab Name: {tabControl1.SelectedTab.Text}";
            label_IndexOfTab.Text = $"Tab Index: {tabControl1.Controls.IndexOf(tabControl1.SelectedTab)}";
        }

        /// <summary>
        /// set info about tab content into status bar
        /// </summary>
        /// <param name="richTextBoxCurrentTab">the richTextBox with content</param>
        private void SetContentInfo(RichTextBox richTextBoxCurrentTab)
        {
            string lines = richTextBoxCurrentTab.Lines.Length.ToString();
            string textLength = richTextBoxCurrentTab.TextLength.ToString();
            //string textLength2 = richTextBoxCurrentTab.Text.Length.ToString();
            string[] arrLines = richTextBoxCurrentTab.Text.Split('\n');
            label_CountOfLines.Text = $"Count of lines: {lines}";
            label_IndexOfTab.Text = $"Count of lines: {arrLines.Length}";
            label_CountOfSymbols.Text = $"Count of symbols: {textLength}";
        }

        #endregion

    }
}
