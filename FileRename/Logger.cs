
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileRename
{
    public enum InformationType {Info,Success,Failure,Error}
    
    public class Logger
    {
        string basePath = System.AppDomain.CurrentDomain.BaseDirectory+"\\LogFiles";
        bool inited = false;
        StreamWriter sw;
        
        public Logger()
        {
            Init();
        }

        public Logger(string filename)
        {
            Init(filename);
        }

        private void Init(string fileName = null)
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            string dataString = DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(basePath + @"\" + dataString))
            {
                Directory.CreateDirectory(basePath + @"\" + dataString);
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName + ".log";
            }
            else
                fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            fileName = basePath + @"\" + dataString + @"\" + fileName;        
            sw = new StreamWriter(fileName, true, Encoding.UTF8);
            inited = true;
        }

        public void Write(string content,InformationType type)
        {
            try
            {
                if (!inited)
                    Init();
                string logText = DateTime.Now.ToString("hh:mm:ss")+"  "+type.ToString()+"  "+content;
                sw.WriteLine(logText);
                sw.Flush();
            }
            catch(Exception ee)
            {
                
            }
        }
        public void Close()
        {
            if(sw!=null)
               { 
                    sw.Close();
                    sw.Dispose();
               }
            inited = false;
        }
    }
}
