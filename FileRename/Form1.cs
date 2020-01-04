using MediaInfoLib;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileRename
{
    public partial class Form1 : Form
    {
        AutoSizeFormClass asc = new AutoSizeFormClass();
        MediaInfo MediaInfo = new MediaInfo();
        string dir1 = @"";       //目标文件夹路径
        string dir2 = @"";       //关键词列表路径
        List<string> keywords_delete = new List<string>();
        List<string> keywords_skip = new List<string>();
        int fileCounts = 0;      //文件数量(除去跳过和删除的)
        int renamedCounts = 0;
        int deletedCounts = 0;
        Regex regex;
        bool flagForLoadCompleted = false;
        bool flagStop = false;

        private delegate void myDelegate(string str1, string str2, string str3, string str4, string str5);
        private delegate void myDelegate2(int nn, string tt);
        private void addRow(string s1, string s2, string s3, string s4, string s5)
        {
            if (dgv.InvokeRequired)
            {
                myDelegate md = new myDelegate(addRow);
                this.Invoke(md, s1, s2, s3, s4, s5);
            }
            else
                dgv.Rows.Add(++fileCounts, s1, s2, s3, s4, s5);
        }
        private void upStatus(int a, string txt)
        {
            if (InvokeRequired)
            {
                myDelegate2 md = new myDelegate2(upStatus);
                this.Invoke(md, a, txt);
            }
            else
            {
                if (a == 1)
                    toolStripStatusLabel2.Text = txt;
                else
                    toolStripStatusLabel4.Text = txt;
            }
        }


        //字符串反转
        static string ReverseStr(string original)
        {
            char[] arr = original.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
        //计算文件大小
        static string GetLength(long lengthOfDocument)
        {

            if (lengthOfDocument < 1024)
                return string.Format(lengthOfDocument.ToString() + 'B');
            else if (lengthOfDocument > 1024 && lengthOfDocument <= Math.Pow(1024, 2))
                return string.Format((lengthOfDocument / 1024.0).ToString("f2") + "KB");
            else if (lengthOfDocument > Math.Pow(1024, 2) && lengthOfDocument <= Math.Pow(1024, 3))
                return string.Format((lengthOfDocument / 1024.0 / 1024.0).ToString("f2") + "M");
            else
                return string.Format((lengthOfDocument / 1024.0 / 1024.0 / 1024.0).ToString("f2") + "GB");
        }
        //重命名文件夹内的文件（不包括子目录）
        private void GetFileInfo(string path)
        {
            try
            {
                Computer MyComputer = new Computer();
                MediaInfo info = new MediaInfo();
                DirectoryInfo root = new DirectoryInfo(path);
                string fileOldName = "";
                string fileNewName = "";
                string keyword = "";
                List<string> keywords = new List<string>();
                bool skip = false;
                int del_position = Convert.ToInt32(numericUpDown2.Value);
                int del_counts = Convert.ToInt32(numericUpDown3.Value);
                int add_position = Convert.ToInt32(numericUpDown1.Value);
                string format = "";
                string extensionName = "";
                string nameWithoutExtension = "";
                string size = "";
                if (File.Exists(dir2))
                {
                    StreamReader sr = new StreamReader(dir2, Encoding.UTF8);
                    while ((keyword = sr.ReadLine()) != null)
                        keywords.Add(keyword);
                    sr.Close();
                }
                if (checkBox1.Checked)
                    foreach (DirectoryInfo f in root.GetDirectories())
                    {
                        skip = false;
                        if (flagStop)
                            return;
                        fileNewName = fileOldName = f.Name;
                        //跳过
                        if (keywords_skip.Count > 0)
                            foreach (string s in keywords_skip)
                                if (fileNewName.Contains(s))
                                {
                                    skip = true;
                                    break;
                                }
                        if (skip)
                            continue;
                        //删除
                        if (keywords_delete.Count > 0)
                            foreach (string s in keywords_delete)
                                if (fileOldName.Contains(s))
                                {
                                    f.Delete(true);
                                    deletedCounts++;
                                    upStatus(2, deletedCounts.ToString());
                                    continue;
                                }
                        //关键词
                        if (keywords.Count > 0)
                            foreach (string st in keywords)
                            {
                                if (fileOldName.Contains(st))
                                    fileNewName = fileNewName.Replace(st, "");
                            }
                        //正则表达式     
                        if (regex != null)
                            if (regex.IsMatch(fileNewName))
                                fileNewName = regex.Replace(fileNewName, "");
                        fileNewName.Trim();
                        //重命名
                        if (fileNewName != fileOldName)
                        {
                            try
                            {
                                MyComputer.FileSystem.RenameFile(f.FullName, fileNewName);
                                renamedCounts++;
                                upStatus(1, renamedCounts.ToString());
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        }
                    }
                foreach (FileInfo f in root.GetFiles())
                {
                    if (flagStop)
                        return;
                    skip = false;
                    fileNewName = fileOldName = f.Name;
                    extensionName = f.Extension;
                    info.Open(f.FullName);
                    //跳过
                    if (keywords_skip.Count > 0)
                        foreach (string s in keywords_skip)
                            if (fileNewName.Contains(s))
                            {
                                info.Close();
                                skip = true;
                                break;
                            }
                    if (skip)
                        continue;
                    //删除
                    if (keywords_delete.Count > 0)
                        foreach (string s in keywords_delete)
                            if (fileOldName.Contains(s))
                            {
                                f.Delete();
                                deletedCounts++;
                                upStatus(2, deletedCounts.ToString());
                                info.Close();
                                continue;
                            }
                    //关键词
                    if (keywords.Count > 0)
                        foreach (string st in keywords)
                        {
                            if (fileOldName.Contains(st))
                                fileNewName = fileNewName.Replace(st, "");
                        }
                    //正则表达式     \[\d{6}\]
                    if (regex != null)
                        if (regex.IsMatch(fileNewName))
                            fileNewName = regex.Replace(fileNewName, "");
                    //删除指定位置字符
                    nameWithoutExtension = fileNewName.Replace(extensionName, "");
                    if (checkBox2.Checked && del_position > 1)
                    {
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        if (nameWithoutExtension.Length >= del_position)
                            nameWithoutExtension = nameWithoutExtension.Substring(0, del_position - 1);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        fileNewName = nameWithoutExtension + extensionName;
                    }
                    else if (!checkBox2.Checked && deletedCounts>= 1)
                    {
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        if(nameWithoutExtension.Length-del_position+1>=del_counts)
                            nameWithoutExtension = nameWithoutExtension.Remove(del_position - 1, del_counts);
                        else
                            nameWithoutExtension = nameWithoutExtension.Substring(0,del_position - 1);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        fileNewName = nameWithoutExtension + extensionName;
                    }
                    //在指定位置添加字符
                    else
                    {
                        if(txt_addFirst.Text.ToString()!="")
                        {
                            if (fileNewName.Length + txt_addFirst.Text.Length <= 255)
                                fileNewName = txt_addFirst.Text + fileNewName;
                        }
                        if(txt_addLast.Text.ToString()!="")
                        {
                            if (fileNewName.Length + txt_addLast.Text.Length <= 255)
                                fileNewName =  fileNewName+txt_addLast.Text;
                        }
                        if(textBox4.Text.ToString()!="")
                        {
                            if (fileNewName.Length + textBox4.Text.Length <= 255)
                                fileNewName = fileNewName.Insert((int)numericUpDown1.Value, textBox4.Text);
                        }
                    }
                    //替换
                    if (textBox5.Text.ToString() != "")
                        fileNewName.Replace(textBox5.Text, textBox6.Text.ToString());


                    fileNewName = fileNewName.Trim();
                    if (fileNewName != fileOldName)
                    {
                        try
                        {
                            MyComputer.FileSystem.RenameFile(f.FullName, fileNewName);
                            renamedCounts++;
                            upStatus(1, renamedCounts.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    format = "";
                    size = "";
                    format = info.Get(StreamKind.Video, 0, "Format");
                    if (format == "")
                        format = extensionName.Replace(".", "");
                    size = GetLength(f.Length);
                    addRow(f.Name, format, info.Get(StreamKind.Video, 0, "Width") + "x" + info.Get(StreamKind.Video, 0, "Height"), size, f.FullName);
                    info.Close();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }
        //重命名文件夹内的文件（包括子目录）
        private void getDirectory(string path)
        {
            GetFileInfo(path);
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo d in root.GetDirectories())
                getDirectory(d.FullName);
        }
        public Form1()
        {
            InitializeComponent();
            flagForLoadCompleted = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //asc.controllInitializeSize(this);
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            //asc.controlAutoSize(this);
        }
        private void btn_Start_Click(object sender, EventArgs e)
        {
            fileCounts = 0;
            flagStop = false;
            btn_Start.Enabled = false;
            btn_AddDeleteKey.Enabled = false;
            btn_AddSkipKey.Enabled = false;
            btn_OpenDire.Enabled = false;
            btn_OpenFile.Enabled = false;
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            dgv.Rows.Clear();
            keywords_delete.Clear();
            toolStripStatusLabel2.Text = "0";
            toolStripStatusLabel4.Text = "0";
            //获取删除关键词
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                    keywords_delete.Add(checkedListBox1.GetItemText(checkedListBox1.Items[i]));
            //获取跳过关键词
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
                if (checkedListBox2.GetItemChecked(i))
                    keywords_skip.Add(checkedListBox2.GetItemText(checkedListBox2.Items[i]));
            //获取正则表达式
            if (textBox1.Text.ToString() != "")
                regex = new Regex(textBox1.Text);

            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
            try
            {
                btn_Stop.Enabled = true;
                backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }
        #region Menu
        private void txtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv == null || dgv.Rows.Count == 0)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "TXT files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.FileName = "FileList";
            saveFileDialog.Title = "保存";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream = saveFileDialog.OpenFile();
                StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
                string strSplit = "          ";
                try
                {
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            streamWriter.Write(dgv.Rows[i].Cells[j].Value.ToString() + strSplit);
                        }
                        streamWriter.Write(Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    streamWriter.Close();
                    stream.Close();
                }
            }
        }
        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv == null || dgv.Rows.Count == 0)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.FileName = "FileList";
            saveFileDialog.Title = "保存";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream = saveFileDialog.OpenFile();
                StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);

                string strLine = "";
                try
                {
                    //表头
                    for (int i = 0; i < dgv.ColumnCount; i++)
                    {
                        if (i > 0)
                            strLine += ",";
                        strLine += dgv.Columns[i].HeaderText;
                    }
                    strLine.Remove(strLine.Length - 1);
                    sw.WriteLine(strLine);
                    strLine = "";
                    //表的内容
                    for (int j = 0; j < dgv.Rows.Count; j++)
                    {
                        strLine = "";
                        int colCount = dgv.Columns.Count;
                        for (int k = 0; k < colCount; k++)
                        {
                            if (k > 0 && k < colCount)
                                strLine += ",";
                            if (dgv.Rows[j].Cells[k].Value == null)
                                strLine += "";
                            else
                            {
                                string cell = dgv.Rows[j].Cells[k].Value.ToString().Trim();
                                //防止里面含有特殊符号
                                cell = cell.Replace("\"", "\"\"");
                                cell = "\"" + cell + "\"";
                                strLine += cell;
                            }
                        }
                        sw.WriteLine(strLine);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    sw.Close();
                    stream.Close();
                }
            }
        }
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
        #endregion

        #region Click
        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
        }
        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = false;
        }
        private void radioButton3_Click(object sender, EventArgs e)
        {
            radioButton4.Checked = false;
        }
        private void radioButton4_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = false;
        }
        //dgv中重命名文件
        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (flagForLoadCompleted)
                if (e.ColumnIndex == 1 && e.RowIndex < dgv.RowCount)
                {
                    try
                    {
                        Computer MyComputer = new Computer();
                        FileInfo fileInfo = new FileInfo(dgv.Rows[e.RowIndex].Cells[5].Value.ToString());
                        string dir1 = fileInfo.DirectoryName;
                        string nn = dgv.Rows[e.RowIndex].Cells[1].Value.ToString();
                        MyComputer.FileSystem.RenameFile(fileInfo.FullName, nn);
                        dgv.Rows[e.RowIndex].Cells[5].Value = dir1 + "\\" + nn;
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show(ee.Message);
                    }
                }
        }
        private void dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (flagForLoadCompleted)
                if (e.ColumnIndex == 1 && e.RowIndex < dgv.RowCount)
                {
                    if (e.FormattedValue.ToString() == "")
                        MessageBox.Show("文件名不能为空");
                    if (e.FormattedValue.ToString().Contains("DataGridView"))
                    {
                        e.Cancel = true;
                        dgv.CancelEdit();
                    }
                    List<char> invalid = new List<char> { '/', '\\', '<', '>', '|', '?', '*', '"', ':' };
                    foreach (char a in invalid)
                        if (e.FormattedValue.ToString().Contains(a))
                        {
                            MessageBox.Show("文件名不能包含" + a + "字符");
                            e.Cancel = true;
                            dgv.CancelEdit();
                            break;
                        }
                }
        }
        //定位文件
        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5)
            {
                if (e.RowIndex >= dgv.RowCount)
                    return;
                string temp = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (!File.Exists(temp))
                    return;
                System.Diagnostics.Process.Start("Explorer", "/select," + Path.GetDirectoryName(temp) + "\\" + Path.GetFileName(temp));
            }
        }
        private void btn_OpenDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
                dir1 = textBox2.Text;
            }
        }
        private void btn_OpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;
                dir2 = textBox3.Text;
            }
        }
        private void btn_AddSkipKey_Click(object sender, EventArgs e)
        {
            List<string> newStr = new List<string>();
            InputDialog.Show(out newStr);
            if (newStr.Count > 0)
            {
                foreach (string s in newStr)
                    checkedListBox2.Items.Add(s, true);
            }
        }
        private void btn_AddDeleteKey_Click(object sender, EventArgs e)
        {
            List<string> newStr = new List<string>();
            InputDialog.Show(out newStr);
            if (newStr.Count > 0)
            {
                foreach (string s in newStr)
                    checkedListBox1.Items.Add(s, true);
            }
        }
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            flagStop = true;
        }
        #endregion
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btn_Start.Enabled = true;
            btn_Stop.Enabled = false;
            btn_AddDeleteKey.Enabled = true;
            btn_AddSkipKey.Enabled = true;
            btn_OpenDire.Enabled = true;
            btn_OpenFile.Enabled = true;
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = false;
            textBox3.ReadOnly = false;
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!Directory.Exists(dir1))
                {
                    MessageBox.Show("路径" + dir1 + "不存在");
                    return;
                }
                if (radioButton1.Checked)
                    GetFileInfo(dir1);
                else
                    getDirectory(dir1);
            }
            catch (IOException ee)
            {
                MessageBox.Show(ee.Message);
            }
            finally
            {
            }
        }
        //拖动文件夹到panel1获取目标路径
        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            //如果拖的是文件，则返回文件所在的文件夹的路径
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
            textBox2.Text = path;
        }
        //拖动文件到tabPage2获取关键词列表路径
        private void tabPage2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void tabPage2_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (File.Exists(path))
                textBox3.Text = path;
        }
        private void numericUpDown2_Validating(object sender, CancelEventArgs e)
        {
            if (Convert.ToInt32(numericUpDown2.Value + numericUpDown3.Value) > 255)
                numericUpDown2.Value = 255 - numericUpDown3.Value;
        }
        private void numericUpDown3_Validating(object sender, CancelEventArgs e)
        {
            if (Convert.ToInt32(numericUpDown2.Value + numericUpDown3.Value) > 255)
                numericUpDown3.Value = 255 - numericUpDown2.Value;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                if (Convert.ToInt32(numericUpDown2.Value) == 1)
                    numericUpDown2.Value = 2;
        }
        private void txt_addFirst_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            List<char> invalid = new List<char> { '/', '\\', '<', '>', '|', '?', '*', '"', ':' };
            foreach (char a in invalid)
            {
                if (tb.Text.Contains(a))
                {
                    tb.Text = "";
                    MessageBox.Show("文件名不能包含" + a + "字符");
                    break;
                }
            }
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            List<char> invalid = new List<char> { '/', '\\', '<', '>', '|', '?', '*', '"', ':' };
            foreach (char a in invalid)
            {
                if (textBox4.Text.Contains(a))
                {
                    textBox4.Text = "";
                    MessageBox.Show("文件名不能包含" + a + "字符");
                    break;
                }
            }
            if (textBox4.Text.Length + (int)numericUpDown1.Value > 255)
            {
                MessageBox.Show("文件名不能超过255字符");
                textBox4.Text = textBox4.Text.Substring(0, 255 - (int)numericUpDown1.Value);
            }
        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            List<char> invalid = new List<char> { '/', '\\', '<', '>', '|', '?', '*', '"', ':' };
            foreach (char a in invalid)
            {
                if (textBox6.Text.Contains(a))
                {
                    textBox6.Text = "";
                    MessageBox.Show("文件名不能包含" + a + "字符");
                    break;
                }
            }
        }
    }
}
