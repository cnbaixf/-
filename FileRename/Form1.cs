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
        Logger logger = new Logger();
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
        Computer MyComputer = new Computer();
        List<string> keywords = new List<string>();
        int del_position = 0;
        int del_counts = 0;
        int add_position = 0;
        bool t2has = false;
        bool t3has = false;



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
                MediaInfo info = new MediaInfo();
                DirectoryInfo root = new DirectoryInfo(path);
                string fileOldName = "";
                string fileNewName = "";
                bool skip = false;
                string format = "";
                string extensionName = "";
                string nameWithoutExtension = "";
                string size = "";                  
                //重命名文件夹
                if (checkBox1.Checked)
                {
                    foreach (DirectoryInfo f in root.GetDirectories())
                    {
                        skip = false;
                        if (flagStop)
                            return;
                        fileNewName = fileOldName = f.Name;
                        //跳过
                        if (keywords_skip.Count > 0)
                        {
                            foreach (string s in keywords_skip)
                                if (fileNewName.Contains(s))
                                {
                                    skip = true;
                                    logger.Write("文件夹" + fileOldName + "包含跳过关键词 " + s + " ，跳过", InformationType.Info);
                                    break;
                                }
                        }
                        if (skip)
                            continue;
                        //删除
                        if (keywords_delete.Count > 0)
                        {
                            foreach (string s in keywords_delete)
                                if (fileOldName.Contains(s))
                                {
                                    f.Delete(true);
                                    logger.Write("文件夹" + fileOldName + "包含删除关键词 " + s + " ，删除", InformationType.Success);
                                    deletedCounts++;
                                    upStatus(2, deletedCounts.ToString());
                                    continue;
                                }
                        }
                        //关键词
                        if (keywords.Count > 0)
                            foreach (string st in keywords)
                            {
                                if (fileNewName.ToLower().Contains(st))
                                {
                                    fileNewName = fileNewName.Replace(st, "");
                                    logger.Write("删除文件夹" + fileOldName + "名称中的" + st, InformationType.Info);
                                }
                            }
                        //正则表达式     
                        if (regex != null)
                            if (regex.IsMatch(fileNewName))
                            {
                                fileNewName = regex.Replace(fileNewName, "");
                                logger.Write("删除文件夹" + fileOldName + "名称中的正则表达式匹配项", InformationType.Info);
                            }

                        if (checkBox2.Checked && del_position > 1)
                        {
                            logger.Write("删除文件夹" + fileNewName + "名称中第" + del_position + "字符后所有字符", InformationType.Info);
                            if (radioButton4.Checked)
                                nameWithoutExtension = ReverseStr(nameWithoutExtension);
                            if (nameWithoutExtension.Length >= del_position)
                                nameWithoutExtension = nameWithoutExtension.Substring(0, del_position - 1);
                            if (radioButton4.Checked)
                                nameWithoutExtension = ReverseStr(nameWithoutExtension);
                            fileNewName = nameWithoutExtension + extensionName;
                        }
                        else if (!checkBox2.Checked && deletedCounts >= 1)
                        {
                            logger.Write("删除文件夹" + fileNewName + "名称中第" + del_position + "字符后" + del_counts + "个字符", InformationType.Info);
                            if (radioButton4.Checked)
                                nameWithoutExtension = ReverseStr(nameWithoutExtension);
                            if (nameWithoutExtension.Length - del_position + 1 >= del_counts)
                                nameWithoutExtension = nameWithoutExtension.Remove(del_position - 1, del_counts);
                            else
                                nameWithoutExtension = nameWithoutExtension.Substring(0, del_position - 1);
                            if (radioButton4.Checked)
                                nameWithoutExtension = ReverseStr(nameWithoutExtension);
                            fileNewName = nameWithoutExtension + extensionName;
                        }
                        //在指定位置添加字符
                        else
                        {
                            if (!String.IsNullOrEmpty(txt_addFirst.Text))
                            {
                                if (fileNewName.Length + txt_addFirst.Text.Length <= 255)
                                {
                                    logger.Write("在文件夹" + fileNewName + "前添加" + txt_addFirst, InformationType.Info);
                                    fileNewName = txt_addFirst.Text + fileNewName;
                                }
                            }
                            if (!String.IsNullOrEmpty(txt_addLast.Text))
                            {
                                if (fileNewName.Length + txt_addLast.Text.Length <= 255)
                                {
                                    logger.Write("在文件夹" + fileNewName + "后添加" + txt_addLast, InformationType.Info);
                                    fileNewName = fileNewName + txt_addLast.Text;
                                }
                            }
                            if (!String.IsNullOrEmpty(textBox4.Text))
                            {
                                if (fileNewName.Length + textBox4.Text.Length <= 255)
                                {
                                    logger.Write("在文件夹" + fileNewName + "第" + (int)numericUpDown1.Value + "字符后添加" + textBox4.Text, InformationType.Info);
                                    fileNewName = fileNewName.Insert((int)numericUpDown1.Value, textBox4.Text);
                                }
                            }
                        }
                        //替换
                        if (!String.IsNullOrEmpty(textBox5.Text))
                        {
                            if (fileNewName.Contains(textBox5.Text))
                            {
                                if (String.IsNullOrEmpty(textBox6.Text))
                                    fileNewName.Replace(textBox5.Text, "");
                                else
                                    fileNewName.Replace(textBox5.Text, textBox6.Text);
                                logger.Write("将文件夹" + fileNewName + "名称中" + textBox5.Text + "替换为" + (String.IsNullOrEmpty(textBox6.Text) ? "" : textBox6.Text), InformationType.Info);
                            }
                        }
                        fileNewName.Trim();
                        //重命名
                        if (fileNewName != fileOldName)
                        {
                            try
                            {
                                MyComputer.FileSystem.RenameFile(f.FullName, fileNewName);
                                logger.Write("文件夹" + fileOldName + "重命名为" + fileNewName, InformationType.Success);
                                renamedCounts++;
                                upStatus(1, renamedCounts.ToString());
                            }
                            catch (Exception e)
                            {
                                logger.Write("文件夹" + fileOldName + "重命名失败。" + e.Message, InformationType.Failure);
                                MessageBox.Show(e.Message);
                            }
                        }
                        else
                            logger.Write("文件夹" + fileOldName + "不需要重命名" , InformationType.Info);
                    }
                }
                //重命名文件
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
                                logger.Write("文件" + fileOldName + "包含跳过关键词 " + s + " ，跳过", InformationType.Info);
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
                                logger.Write("文件" + fileOldName + "包含删除关键词 " + s + " ，删除", InformationType.Success);
                                deletedCounts++;
                                upStatus(2, deletedCounts.ToString());
                                info.Close();
                                continue;
                            }
                    //关键词
                    if (keywords.Count > 0)
                        foreach (string st in keywords)
                        {
                            if (fileNewName.ToLower().Contains(st))
                            {
                                fileNewName = fileNewName.Replace(st,"");
                                logger.Write("删除文件" + fileOldName + "名称中的" + st, InformationType.Info);
                            }
                        }
                    //正则表达式     \[\d{6}\]
                    if (regex != null)
                        if (regex.IsMatch(fileNewName))
                        {
                            fileNewName = regex.Replace(fileNewName, "");
                            logger.Write("删除文件" + fileOldName + "名称中的正则表达式匹配项", InformationType.Info);
                        }
                    //删除指定位置字符
                    if (!String.IsNullOrEmpty(extensionName))
                        nameWithoutExtension = fileNewName.Replace(extensionName, "");
                    else
                        nameWithoutExtension = fileNewName;
                    if (checkBox2.Checked && del_position > 1)
                    {
                        logger.Write("删除文件" + fileNewName + "名称中第" + del_position + "字符后所有字符", InformationType.Info);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        if (nameWithoutExtension.Length >= del_position)
                            nameWithoutExtension = nameWithoutExtension.Substring(0, del_position - 1);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        fileNewName = nameWithoutExtension + extensionName;
                    }
                    else if (!checkBox2.Checked && deletedCounts >= 1)
                    {
                        logger.Write("删除文件" + fileNewName + "名称中第" + del_position + "字符后" + del_counts + "个字符", InformationType.Info);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        if (nameWithoutExtension.Length - del_position + 1 >= del_counts)
                            nameWithoutExtension = nameWithoutExtension.Remove(del_position - 1, del_counts);
                        else
                            nameWithoutExtension = nameWithoutExtension.Substring(0, del_position - 1);
                        if (radioButton4.Checked)
                            nameWithoutExtension = ReverseStr(nameWithoutExtension);
                        fileNewName = nameWithoutExtension + extensionName;
                    }
                    //在指定位置添加字符
                    else
                    {
                        if (!String.IsNullOrEmpty(txt_addFirst.Text))
                        {
                            if (fileNewName.Length + txt_addFirst.Text.Length <= 255)
                            {
                                logger.Write("在文件" + fileNewName + "前添加" + txt_addFirst, InformationType.Info);
                                fileNewName = txt_addFirst.Text + fileNewName;
                            }
                        }
                        if (!String.IsNullOrEmpty(txt_addLast.Text))
                        {
                            if (fileNewName.Length + txt_addLast.Text.Length <= 255)
                            {
                                logger.Write("在文件" + fileNewName + "后添加" + txt_addLast, InformationType.Info);
                                fileNewName = fileNewName + txt_addLast.Text;
                            }
                        }
                        if (!String.IsNullOrEmpty(textBox4.Text))
                        {
                            if (fileNewName.Length + textBox4.Text.Length <= 255)
                            {
                                logger.Write("在文件" + fileNewName + "第" + (int)numericUpDown1.Value + "字符后添加" + textBox4.Text, InformationType.Info);
                                fileNewName = fileNewName.Insert((int)numericUpDown1.Value, textBox4.Text);
                            }
                        }
                    }
                    //替换
                    if (!String.IsNullOrEmpty(textBox5.Text))
                    {
                        if (fileNewName.Contains(textBox5.Text))
                        {
                            if (String.IsNullOrEmpty(textBox6.Text))
                                fileNewName.Replace(textBox5.Text, "");
                            else
                                fileNewName.Replace(textBox5.Text, textBox6.Text);
                            logger.Write("将文件" + fileNewName + "名称中" + textBox5.Text + "替换为" + (String.IsNullOrEmpty(textBox6.Text) ? "" : textBox6.Text), InformationType.Info);
                        }
                    }
                    fileNewName = fileNewName.Trim();
                    if (fileNewName != fileOldName)
                    {
                        try
                        {
                            MyComputer.FileSystem.RenameFile(f.FullName, fileNewName);
                            logger.Write("文件" + fileOldName + "重命名为" + fileNewName, InformationType.Success);
                            renamedCounts++;
                            upStatus(1, renamedCounts.ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.Write("文件" + fileOldName + "重命名失败。" + ex.Message, InformationType.Failure);
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                        logger.Write("文件" + fileOldName + "不需要重命名", InformationType.Info);
                    format = "";
                    size = "";
                    format = info.Get(StreamKind.Video, 0, "Format");
                    if (String.IsNullOrEmpty(format))
                        if (!String.IsNullOrEmpty(extensionName))
                            format = extensionName.Replace(".", "");
                    size = GetLength(f.Length);
                    addRow(f.Name, format, info.Get(StreamKind.Video, 0, "Width") + "x" + info.Get(StreamKind.Video, 0, "Height"), size, f.FullName);
                    info.Close();
                }
            }
            catch (Exception ee)
            {
                logger.Write("异常：" + ee.Message, InformationType.Error);
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
            renamedCounts = 0;
            deletedCounts = 0;
            toolStripStatusLabel2.Text = "0";
            toolStripStatusLabel4.Text = "0";
            dir1 = textBox2.Text;
            dir2 = textBox3.Text;
            del_position = Convert.ToInt32(numericUpDown2.Value);
            del_counts = Convert.ToInt32(numericUpDown3.Value);
            add_position = Convert.ToInt32(numericUpDown1.Value);

            //读取关键词列表
            string keyword = "";
            if (File.Exists(dir2))
            {
                StreamReader sr = new StreamReader(dir2, Encoding.UTF8);
                while ((keyword = sr.ReadLine()) != null)
                    keywords.Add(keyword.ToLower());
                sr.Close();
            }
            logger.Write("删除文件名中的下列关键词：" + String.Join(",", keywords.ToArray()), InformationType.Info);
            //获取删除关键词
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                    keywords_delete.Add(checkedListBox1.GetItemText(checkedListBox1.Items[i]));
            //获取跳过关键词
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
                if (checkedListBox2.GetItemChecked(i))
                    keywords_skip.Add(checkedListBox2.GetItemText(checkedListBox2.Items[i]));
            //获取正则表达式
            if (!String.IsNullOrEmpty(textBox1.Text))
                regex = new Regex(textBox1.Text);
            backgroundWorker1.WorkerSupportsCancellation = true;
            try
            {
                btn_Stop.Enabled = true;
                backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception ee)
            {
                logger.Write(ee.Message, InformationType.Error);
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
            Stream stream;
            StreamWriter streamWriter;
            try
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    stream = saveFileDialog.OpenFile();
                    streamWriter = new StreamWriter(stream, Encoding.UTF8);
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
                        if (streamWriter != null)
                            streamWriter.Close();
                        if (stream != null)
                            stream.Close();
                    }
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message);
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
            Stream stream;
            StreamWriter sw;
            try
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    stream = saveFileDialog.OpenFile();
                    sw = new StreamWriter(stream, Encoding.UTF8);
                    try
                    {
                        string strLine = "";
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
                        if (sw != null)
                            sw.Close();
                        if (stream != null)
                            stream.Close();
                    }
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message);
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
                    if (e.FormattedValue.ToString().Length < 1)
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
                    System.Diagnostics.Process.Start("Explorer", Path.GetDirectoryName(temp));
                else if (Directory.Exists(temp.Substring(0, temp.LastIndexOf("\\"))))
                    System.Diagnostics.Process.Start("Explorer", "/select," + Path.GetDirectoryName(temp) + "\\" + Path.GetFileName(temp));
                else
                    MessageBox.Show(temp+"不存在");
            }
        }
        private void btn_OpenDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
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
            logger.Write("\r\n****************************************************\r\n", InformationType.Info);
            logger.Close();
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                logger.Write("开始重命名", InformationType.Info);
                if (!Directory.Exists(dir1))
                {
                    MessageBox.Show("路径" + dir1 + "不存在");
                    logger.Write("路径" + dir1 + "不存在", InformationType.Error);
                }
                else
                {
                    if (radioButton1.Checked)
                        GetFileInfo(dir1);
                    else
                        getDirectory(dir1);
                }
            }
            catch (IOException ee)
            {
                MessageBox.Show(ee.Message);
                logger.Write(ee.Message, InformationType.Error);
            }
            finally
            {
                logger.Write("完成重命名", InformationType.Info);
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
            textBox2.ForeColor = Color.Black;
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
            textBox3.ForeColor = Color.Black;
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

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text))
            {
                t2has = false;
                textBox2.Text = "可将文件夹拖动至此区域";
                textBox2.ForeColor = SystemColors.ButtonShadow;
            }
            else
                t2has = true;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if(!t2has)
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }
        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox3.Text))
            {
                t3has = false;
                textBox3.Text = "可将文件拖动至此区域";
                textBox3.ForeColor = SystemColors.ButtonShadow;
            }
            else
                t3has = true;
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (!t3has)
            {
                textBox3.Text = "";
                textBox3.ForeColor = Color.Black;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text))
                t2has = true;
            else
                t2has = false;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox3.Text))
                t3has = true;
            else
                t3has = false;
        }
    }
}
