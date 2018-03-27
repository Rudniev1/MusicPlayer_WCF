using System;
using System.Windows.Forms;
using WMPLib;
using MusicPlayer_WCF.MusicServiceReference;
using System.IO;
using System.Security.AccessControl;
using System.Collections.Generic;

namespace MusicPlayer_WCF
{
    public partial class Form1 : Form
    {
        private WindowsMediaPlayer Wmp = new WindowsMediaPlayer();

        string[] files, paths;
        Timer timer = new Timer();
        double temp;
        FileStream fileStream;
        int flag = 1;
        string outputFile = "C:\\users\\"+Environment.UserName +"\\desktop\\outputfile.mp3";
        Dictionary<string, string> FilesPaths = new Dictionary<string, string>();
        Stream stream;

        List<string> results = new List<string>();

        public Form1()
        {
            InitializeComponent();
            Wmp.PlayStateChange += Wmp_PlayStateChange;
            timer.Tick += Timer_Tick;
        }

        private void Wmp_PlayStateChange(int NewState)
        {
            if ((WMPPlayState)NewState == WMPPlayState.wmppsPlaying)
            { 
            double dur = Wmp.controls.currentItem.duration;
            trackBarMusic.Maximum = (int)dur;
            }


            if (WMPPlayState.wmppsMediaEnded == (WMPPlayState)NewState)
            {
                Wmp.controls.stop();
                Wmp.controls.currentPosition = 0;
                if (listBox1.SelectedIndex < listBox1.Items.Count-1)
                    listBox1.SelectedIndex++;
                else
                    listBox1.SelectedIndex = 0;
                Wmp.URL = FilesPaths[listBox1.SelectedItem.ToString()];
                trackBarMusic.Value = 0;
                
                timer.Enabled = true;
            }
            if (Wmp.playState == WMPPlayState.wmppsWaiting)
                Wmp.controls.play();

            if (Wmp.playState == WMPPlayState.wmppsReady)
                try
                {
                    Wmp.controls.play();
                }
                catch (InvalidCastException ex)
                {
                    listBoxError.Items.Add(ex.Message);

                }
                catch(System.Runtime.InteropServices.COMException ex)
                {
                    listBoxError.Items.Add(ex.Message);
                }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                label4.Text = Wmp.controls.currentPositionString + " | " + Wmp.controls.currentItem.durationString;
                temp = Wmp.controls.currentPosition;
                trackBarMusic.Value = (int)temp;
            }
           catch
            {
                label4.Text = "0.0 | 0.0";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "pliki Mp3 (*.mp3)|*.mp3|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                files = ofd.SafeFileNames;
                paths = ofd.FileNames;
                FilesPaths.Clear();
                for (int i = 0; i < paths.Length; i++)
                {
                    FilesPaths.Add(files[i], paths[i]);                 
                }

                foreach (KeyValuePair<string, string> key in FilesPaths)
                {
                    string temp = key.Key;
                    listBox1.Items.Add(temp);
                }
            }

            flag = 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Wmp.controls.play();
            timer.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Wmp.controls.pause();
            timer.Stop();
        }

        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            Wmp.controls.stop();       
            ItransferServiceClient client = new ItransferServiceClient();


            if (flag == 1)
            {
                
                try
                {
                    using (fileStream = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        foreach (var key in FilesPaths.Keys)
                        {
                            if (key == listBox1.GetItemText(listBox1.SelectedItem))
                            {
                                stream = await client.GetLargeObjectAsync(key);
                            }
                        }
                        stream.CopyTo(fileStream);
                        Wmp.URL = outputFile;
                        File.Create(outputFile, 1, FileOptions.RandomAccess);
                        stream.Close();
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    listBoxError.Items.Add(ex.Message);
                }
                catch (IOException ex)
                {
                    listBoxError.Items.Add(ex.Message);
                }

                finally
                {
                    client.Close();
                }
            }

            if (flag == 2)
            {
                Wmp.URL = FilesPaths[listBox1.SelectedItem.ToString()];
            }
            try
            {
                Wmp_PlayStateChange(Wmp.controls.currentItem.attributeCount);
            }
            catch(Exception ex)
            {
                listBoxError.Items.Add(ex.Message);
            }
            timer.Enabled = true;
            label1.Text = "Nazwa: " + listBox1.SelectedItem.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Wmp.controls.stop();
            timer.Enabled = false;
            trackBarMusic.Value = 0;
            label4.Text = "0.0 | 0.0";

        }

        private void trackBarVolume_ValueChanged(object sender, EventArgs e)
        {
            Wmp.settings.volume = trackBarVolume.Value;
            label5.Text = trackBarVolume.Value.ToString();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            ItransferServiceClient client = new ItransferServiceClient();
            try
            {
                paths = await client.GetPlaylistAsync();
            }
            catch (Exception ex)
            {
                listBoxError.Items.Add(ex.Message);
            }
            finally
            {
                
            }
            listBox1.Items.Clear();
            FilesPaths.Clear();
            if (paths != null)
            {
                files = new string[paths.Length];

                for (int i = 0; i < paths.Length; i++)
                {
                    files[i] = Path.GetFileName(paths[i]);
                    FilesPaths.Add(files[i], paths[i]);

                }

                foreach (var key in FilesPaths.Keys)
                {
                    listBox1.Items.Add(key);
                }
                client.Close();
                flag = 1;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            results.Clear();
            try
            {
                if (String.IsNullOrWhiteSpace(textBoxSearch.Text))
                {
                    FilesPaths.Clear();
                    for (int i = 0; i < paths.Length; i++)
                    {
                        FilesPaths.Add(files[i], paths[i]);
                    }
                    foreach (var key in FilesPaths)
                    {

                        listBox1.Items.Add(key.Key);
                    }
                }
                else
                {

                    string target = textBoxSearch.Text;
                    Dictionary<string, string> tempPlaylist = new Dictionary<string, string>();

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].ToLower().Contains(target))
                        {
                            tempPlaylist.Add(files[i], paths[i]);
                        }
                    }

                    foreach (var key in tempPlaylist)
                    {
                        listBox1.Items.Add(key.Key);
                    }

                    FilesPaths = tempPlaylist;

                }
            }
            catch (System.NullReferenceException ex)
            {
                listBox1.Items.Clear();
                listBoxError.Items.Add(ex.Message);
            }
                
            
        }

        private void trackBarMusic_ValueChanged(object sender, EventArgs e)
        {
            if(Wmp.controls.currentPosition <= trackBarMusic.Value)
            Wmp.controls.currentPosition = trackBarMusic.Value;  
            
            if(MouseButtons == MouseButtons.Left)
                Wmp.controls.currentPosition = trackBarMusic.Value;
                 
        }
    }
}
