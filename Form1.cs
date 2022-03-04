using System;
using System.Windows.Forms;
using NAudio.Wave;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SpeechToText
{
    public partial class Form1 : Form
    {
        WaveIn waveIn;
        WaveFileWriter writer;
        readonly string outputFilename = "demo.wav";
        bool ON = false;
        string language = "ru-RU";
        [Obsolete]
        public Form1()
        {
            InitializeComponent();
        }
        [Obsolete]
        private void record_Click(object sender, EventArgs e)
        {
            read.Enabled = false;
            write.Enabled = false;
            if (ON == false)
            {
                textBox1.Text = "Запись...";
                waveIn = new WaveIn();
                waveIn.DeviceNumber = 0;
                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(waveIn_RecordingStopped);
                waveIn.WaveFormat = new WaveFormat(16000, 1);
                writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                record.Text = "Стоп";
                waveIn.StartRecording();
                ON = true;
                recognize.Enabled = false;
            }
            else
            {
                textBox1.Text = "Конец записи";
                waveIn.StopRecording();
                ON = false;
                record.Text = "Запись";
                recognize.Enabled = true;
            }
        }
        [Obsolete]
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);
        }
        void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            waveIn.Dispose();
            waveIn = null;
            writer.Close();
            writer = null;
        }
        private bool keyword_processing(string result)
        {
            switch (result.ToLower().Split()[0])
            {
                case "язык":
                case "language":
                    switch (result.ToLower().Split()[1])
                    {
                        case "английский":
                            language = "en-US";
                            textBox1.Text = "Language is set to English";
                            break;
                        case "russian":
                            language = "ru-RU";
                            textBox1.Text = "Язык установлен на Русский";
                            break;
                        default:
                            language = "ru-RU";
                            break;
                    }
                    break;
                case "выход":
                case "exit":
                    Close();
                    break;
                case "интернет":
                case "internet":
                    Process.Start("https://www.google.ru");
                    textBox1.Text = "Google";
                    break;
                case "автор":
                case "author":
                    Process.Start("https://vk.com/msneron");
                    textBox1.Text = "VK: msneron";
                    break;
                case "найти":
                case "search":
                    string t = "";
                    foreach(var word in result.ToLower().Split())
                        t += word + "+";
                    t = t.Substring(result.ToLower().Split()[0].Length + 1);
                    t = t.Remove(t.Length - 1, 1);
                    Process.Start($"https://www.google.com/search?q={t}");
                    textBox1.Text = "Google";
                    break;
                default:
                    return true;
            }
            return false;
        }
        private void recognize_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Распознавание...";
            WebRequest request = WebRequest.Create($"https://www.google.com/speech-api/v2/recognize?output=json&lang={language}&key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw");
            request.Method = "POST";
            byte[] byteArray = File.ReadAllBytes(outputFilename);
            request.ContentType = "audio/l16; rate=16000";
            request.ContentLength = byteArray.Length;
            request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string strtrs = reader.ReadToEnd();
            var rg = new Regex(@"transcript" + '"' + ":" + '"' + "([A-Z, А-Я, a-z,а-я, ,0-9]*)");
            var result = rg.Match(strtrs).Groups[1].Value;
            if (keyword_processing(result))
            {
                textBox1.Text = result;
                read.Enabled = true;
                write.Enabled = true;
            }
            reader.Close();
            response.Close();
        }
        private async void read_Click(object sender, EventArgs e)
        {
            var synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            await Task.Run(() => synth.Speak(textBox1.Text));
        }

        private void write_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("text.txt", false, System.Text.Encoding.Default))
                {
                    sw.Write(textBox1.Text);
                }
                textBox1.Text = "Запись выполнена";
                write.Enabled = false;
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.Message;
            }
        }
    }
}
