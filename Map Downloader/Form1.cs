using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Map_Downloader
{
 
    public partial class Form1 : Form
    {
        WebClient webClient;
        Stopwatch sw = new Stopwatch();
        List<string> filesToDownload = new List<string>();
        List<string> selectedFiles = new List<string>();
        int completed = 0;
        int n = 0;
        public Form1()
        {
            InitializeComponent();
        }
        string GetFileURL(string str)
        {
            string baseUrl = "http://oceandata.sci.gsfc.nasa.gov/cgi/getfile/";
            string url = baseUrl + str + comboBox1.Text;
            return url;
        }
        string GetFileSize(string URL)
        {
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(URL);
            req.Method = "HEAD";
            System.Net.WebResponse resp = req.GetResponse();
            long ContentLength = 0;
            long result;
            if (long.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
            {
                string File_Size;
                result = ContentLength / 1048576;
                File_Size = string.Format("{0} MB's",
                (result / 1024d / 1024d).ToString("0.00"));
                return File_Size;
            }
            return "unknown";
        }
        string GetFileName(string url)
        {
            string[] str = url.Split('/');
            return str[str.Length - 1];

        }
        public void DownloadFile(string urlAddress, string location)
        {
            
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
 
                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
 
                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();
 
                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                    
                    label4.Text = urlAddress;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            labelSpeed.Text = string.Format("Speed : {0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
 
            // Update the progressbar percentage only when the value is not the same.
            progressBar1.Value = e.ProgressPercentage;
 
            // Show the percentage on our label.
            labelPerc.Text = e.ProgressPercentage.ToString() + "%";
 
            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            labelDownloaded.Text = string.Format("Downloaded : {0} MB's / {1} MB's",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            
        }
 
        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            //int i = int.Parse(selectedFiles[n]);
            // Reset the stopwatch.
            sw.Reset();
            foreach (ListViewItem lv in listView1.Items)
            {
                //MessageBox.Show(lv.Text + " - " + i.ToString());
                if (lv.SubItems[1].Text == filesToDownload[n])
                {
                    lv.SubItems[2].Text = "Complete";
                    label1.Text = "Status : Completed";
                    completed += 1;
                    label5.Text = string.Format("Completed : {0}", completed.ToString());
                    continue;
                }
            }
            if (e.Cancelled == true)
            {
                MessageBox.Show("Download has been canceled.");
            }
            else
            {
                n++;
                if (n <= selectedFiles.Count - 1)
                {
                    //i = int.Parse(selectedFiles[n]);
                    string day = filesToDownload[n].Substring(5, 3);
                    string path = textBox2.Text + @"\" + day;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    DownloadFile(GetFileURL(filesToDownload[n]), path + @"\" + GetFileName(filesToDownload[n]) + ".bz2");
                    foreach(ListViewItem lv in listView1.Items)
                    {
                        //MessageBox.Show(lv.Text);
                        if (lv.SubItems[1].Text == filesToDownload[n])
                        {
                            lv.SubItems[2].Text = "Downloading";
                            label1.Text = "Status : Downloading";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Download Complete!");
                }
                
                //MessageBox.Show("Download completed!");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox2.Text == null)
            {
                MessageBox.Show("Please select file destination!");
                this.Close();
            }
            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();
            
            string str = File.ReadAllText(open.FileName);
            str = str.Replace(" ", "");
            string[] lines = str.Split('\n');
            int i = 1;
            foreach(string line in lines)
            {
                string[] files = line.Split('\t');
                foreach(string file in files)
                {
                     //filesToDownload.Add(file.Remove(file.Length - 3));
                     string[] list = new string[3];
                     list[0] = i.ToString();
                     list[1] = file.Remove(file.Length - 3);
                     list[2] = "Pending";
                     ListViewItem lv = new ListViewItem(list);
                     listView1.Items.Add(lv);
                     i++; 
                }
            }
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = true;
            }


        }

        private void labelSpeed_Click(object sender, EventArgs e)
        {
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog save = new FolderBrowserDialog();
            save.ShowDialog();
            textBox2.Text = save.SelectedPath;  
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                //MessageBox.Show()
            }
        }


        private void button3_Click(object sender, EventArgs e) //start download
        {
            n = 0;
            foreach(ListViewItem lv in listView1.CheckedItems)
            {
                filesToDownload.Add(lv.SubItems[1].Text);
            }
            string day = filesToDownload[n].Substring(5, 3);
            string path = textBox2.Text + @"\" + day;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            DownloadFile(GetFileURL(filesToDownload[n]), path + @"\" + GetFileName(filesToDownload[n]) + ".bz2");
            foreach (ListViewItem lv in listView1.Items)
            {
                //MessageBox.Show(lv.Text);
                if (lv.SubItems[1].Text == filesToDownload[n])
                {
                    label1.Text = "Status : Downloading";
                    lv.SubItems[2].Text = "Downloading";
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            foreach(ListViewItem lv in listView1.Items)
            {
                if (button4.Text == "Diselect All")
                {
                    lv.Checked = false;
                }
                else
                {
                    lv.Checked = true;
                }
                
            }
            if (button4.Text == "Diselect All")
            {
                button4.Text = "Select All";
            }
            else
            {
                button4.Text = "Diselect All";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lv in listView1.Items)
            {
                lv.Checked = false;
                if (lv.SubItems[1].Text.Contains("OC")) lv.Checked = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lv in listView1.Items)
            {
                lv.Checked = false;
                if (lv.SubItems[1].Text.Contains("SST")) lv.Checked = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string[] list = new string[3];
            list[0] = listView1.Items.Count.ToString();
            list[1] = textBox1.Text;
            list[2] = "Pending";
            ListViewItem lv = new ListViewItem(list);
            listView1.Items.Add(lv);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            foreach(ListViewItem lv in listView1.SelectedItems)
            {
                lv.Checked = true;
            }
            
        }

        
    }
}
