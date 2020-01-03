using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileRename
{
    public static class InputDialog
    {
        public static DialogResult Show(out List<string> strText)
        {
            string strTemp = "";

            AddKeywords inputDialog = new AddKeywords();
            inputDialog.TextHandler = (str) => { strTemp = str; };

            DialogResult result = inputDialog.ShowDialog();


            strText = strTemp.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return result;
        }
    }
}
