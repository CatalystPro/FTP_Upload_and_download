using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FTPDownloadGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() => FileDownload());
        }

        private void FileDownload()
        {
            try
            {
                NetworkCredential credentials = new NetworkCredential(txtUserName.Text, txtPassword.Text);
                lblStatus.BeginInvoke(new Action(() =>
                {
                    lblStatus.Text = "Downloading...";
                }));
                WebRequest sizeRequest = WebRequest.Create(txtUrl.Text);
                sizeRequest.Credentials = credentials;
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                int size = (int)sizeRequest.GetResponse().ContentLength;
                progressBar1.Invoke((MethodInvoker)(() => progressBar1.Maximum = size));
                WebRequest request = WebRequest.Create(txtUrl.Text);
                request.Credentials = credentials;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                {
                    using (Stream fileStream = File.Create($"{Application.StartupPath}\\{txtFileName.Text}"))
                    {
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, read);
                            int position = (int)fileStream.Position;
                            progressBar1.BeginInvoke(new Action(() =>
                            {
                                progressBar1.Value = position;
                            }));
                            if (position == size)
                            {
                                lblStatus.BeginInvoke(new Action(() =>
                                {
                                    lblStatus.Text = "Finished";
                                }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        struct FtpSetting
        {
            public string Server { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string FileName { get; set; }
            public string FullName { get; set; }

        }

        FtpSetting _inputParameter;
        

        private void btnUpload_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Multiselect = false, ValidateNames = true, Filter = "All files|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    _inputParameter.UserName = txtUserName2.Text;
                    _inputParameter.Password = txtPassword2.Text;
                    _inputParameter.Server = txtServer.Text;
                    _inputParameter.FileName = fi.Name;
                    _inputParameter.FullName = fi.FullName;
                    backgroundWorker1.RunWorkerAsync(_inputParameter);
                }
            }
        }

        private void backgroundWorker1_DoWork_1(object sender, DoWorkEventArgs e)
        {
            string fileName = ((FtpSetting)e.Argument).FileName;
            string fullName = ((FtpSetting)e.Argument).FullName;
            string userName = ((FtpSetting)e.Argument).UserName;
            string password = ((FtpSetting)e.Argument).Password;
            string server = ((FtpSetting)e.Argument).Server;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", server, fileName)));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userName, password);
            Stream ftpStream = request.GetRequestStream();
            FileStream fs = File.OpenRead(fullName);
            byte[] buffer = new byte[1024];
            double total = (double)fs.Length;
            int byteRead = 0;
            double read = 0;
            do
            {
                if (!backgroundWorker1.CancellationPending)
                {
                    byteRead = fs.Read(buffer, 0, 1024);
                    ftpStream.Write(buffer, 0, byteRead);
                    read += (double)byteRead;
                    double percentage = read / total * 100;
                    backgroundWorker1.ReportProgress((int)percentage);
                }
            }
            while (byteRead != 0);
            fs.Close();
            ftpStream.Close();
        }

        private void backgroundWorker1_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            IbIStatus2.Text = $"Uploaded {e.ProgressPercentage} %";
            progressBar.Value = e.ProgressPercentage;
            progressBar.Update();
        }

        private void backgroundWorker1_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            IbIStatus2.Text = "Upload Completed ! ";
        }

    }
}