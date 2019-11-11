using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Convert_ANSI_to_UTF8
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool ConvertAnsiToUTF8(string inputFilePath, string outputFilePath)
        {
            try
            {
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                string fileContent = File.ReadAllText(inputFilePath, Encoding.GetEncoding("Windows-874"));
                File.WriteAllText(outputFilePath, fileContent, utf8WithoutBom);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
           
        }
        private bool BackUpfile(string sPath_Source, string filepath,string sPath_BAK)
        {
            try
            {
                #region Move File
                string result_Move;
                result_Move = Path.GetFileName(filepath);
                string sPath_bak = sPath_BAK + "/" + DateTime.Now.ToString("yyyyMMdd");
                bool exists = System.IO.Directory.Exists(sPath_bak);
                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(sPath_bak);
                }
                System.IO.File.Move(sPath_Source + "/" + result_Move, sPath_bak + "/" + result_Move);
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }

        private bool CopyFile(string sPath_Source, string filepath, string PathInput)
        {
            try
            {
                #region Move File
                string result_Move;
                result_Move = Path.GetFileName(filepath);
                //string sPath_bak = sPath_BAK + "/" + DateTime.Now.ToString("yyyyMMdd");
                //bool exists = System.IO.Directory.Exists(sPath_bak);
                //if (!exists)
                //{
                //    System.IO.Directory.CreateDirectory(sPath_bak);
                //}
            
                System.IO.File.Copy(sPath_Source + "/" + result_Move, PathInput + "/" + result_Move);
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string PathInput = System.Configuration.ConfigurationSettings.AppSettings["PathInput"].ToString();
            string PathOutput = System.Configuration.ConfigurationSettings.AppSettings["PathOutput"].ToString();
            string PathBack = System.Configuration.ConfigurationSettings.AppSettings["PathBack"].ToString();
            string PathError = System.Configuration.ConfigurationSettings.AppSettings["PathError"].ToString();

            string MailFrom = System.Configuration.ConfigurationSettings.AppSettings["MailFrom"].ToString();
            string MailTo = System.Configuration.ConfigurationSettings.AppSettings["MailTo"].ToString();
            string smtp = System.Configuration.ConfigurationSettings.AppSettings["SMTP"].ToString();
            string strSubject = System.Configuration.ConfigurationSettings.AppSettings["sSubjectmail"].ToString();
            string Pathhsefle = System.Configuration.ConfigurationSettings.AppSettings["Pathhsefle"].ToString();

            string FtpSync_Inbound = System.Configuration.ConfigurationSettings.AppSettings["FtpSync_Inbound"].ToString();
            string WorkingDirectory = System.Configuration.ConfigurationSettings.AppSettings["WorkingDirectory"].ToString();

            string FtpSync_Bat2 = System.Configuration.ConfigurationSettings.AppSettings["FtpSync_Bat2"].ToString();
            string WorkingDirectory_Bat2 = System.Configuration.ConfigurationSettings.AppSettings["WorkingDirectory_Bat2"].ToString();


            ClassLibrarySendMail.ClassLibrarySendMail classmail = new ClassLibrarySendMail.ClassLibrarySendMail();
            string fileName = string.Empty;
            try
            {
                foreach (string filepaths in Directory.GetFiles(Pathhsefle, "*.hse"))
                {
                    bool bResult = false;
                    fileName = string.Empty;
                    fileName = filepaths;
                    bResult = CopyFile(Pathhsefle, filepaths, PathInput);

                }

                foreach (string filepath in Directory.GetFiles(PathInput, "*.hse"))
                {
                    fileName = string.Empty;
                    fileName = filepath;
                    bool bResult = false;
                    bResult = ConvertAnsiToUTF8(filepath, PathOutput + "/" + Path.GetFileName(filepath));
                    if (BackUpfile(PathInput, filepath, PathBack))
                    {

                    }
                }
                #region SFTP
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.WorkingDirectory = WorkingDirectory;
                pProcess.StartInfo.FileName = FtpSync_Inbound;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                pProcess.WaitForExit();
                #endregion

                #region Bat TTN
                pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.WorkingDirectory = WorkingDirectory_Bat2;
                pProcess.StartInfo.FileName = FtpSync_Bat2;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                pProcess.WaitForExit();
                #endregion

                classmail.Sendmail(MailTo, smtp, "Success Convert TTN ANSI to UTF-8", MailFrom, "Success TTN Convert ANSI to UTF-8");
            }
            catch (Exception ex)
            {
                classmail.Sendmail(MailTo, smtp, "Error Convert TTN ANSI to UTF-8 File : " + fileName + " " + ex.Message.ToString(), MailFrom, strSubject);
                if (BackUpfile(PathInput, fileName, PathError))
                {

                }
                return;
            }
            finally
            {
                this.Dispose();
            }
        }
    }
}
