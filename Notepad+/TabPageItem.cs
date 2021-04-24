using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notepad_
{
    class TabPageItem
    {
        public TabPageItem()
        {

        }

        public TabPageItem(TabPage tabPage)
        {
            this.TabPage = tabPage;
        }
        public TabPage TabPage { get; set; }
        public RichTextBox RichTextBox { get; set; }
        public string Title { get; set; }
    }

}
