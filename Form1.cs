using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Speech;
using System.Speech.Synthesis;

namespace Eva_Compiler_And_IDE
{
    public partial class Form1 : Form
    {
        bool saved = false;
        string lastpath;
        public Form1()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            saved = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "EVA Files|*.eva";
            if(s.ShowDialog() == DialogResult.OK)
            {
                using (Stream str = File.Open(s.FileName, FileMode.CreateNew))
                using(StreamWriter sw = new StreamWriter(str))
                {
                    sw.Write(richTextBox1.Text + "\n");
                }
                saved = true;
                lastpath = s.FileName;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            saved = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream s;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "EVA Files|*.eva";
            if(o.ShowDialog() == DialogResult.OK)
            {
                if((s = o.OpenFile()) != null)
                {
                    string file = o.FileName;
                    string contents = File.ReadAllText(file);
                    richTextBox1.Text = contents;
                    saved = true;
                    lastpath = o.FileName;
                }
            }
        }

        private void highlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool doresave = false;
            if(saved == true)
            {
                doresave = true;
            }
            int index = 0;
            String text = richTextBox1.Text;
            richTextBox1.Text = "";
            richTextBox1.Text = text;
            string[] keywords = { "printe", "printdebug", "printerror", "inputprint", "mb", "new", "islegiteva", "dir", "dl", "info", "lastconsoleitem", "speak", "inputspeak", "sleep", "requires" };
            foreach (string keyword in keywords)
            {
                while (index < richTextBox1.Text.LastIndexOf(keyword))
                {
                    richTextBox1.Find(keyword, index, richTextBox1.TextLength, RichTextBoxFinds.WholeWord);
                    richTextBox1.SelectionColor = Color.Purple;
                    index = richTextBox1.Text.IndexOf(keyword, index) + 1;

                }
                index = 0;
            }
            if(doresave)
            {
                saved = true;
            }
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if(saved)
            {
                SoundPlayer s = new SoundPlayer(Properties.Resources.Windows_XP_Logon_Sound);
                s.Play();
                compile(lastpath);
            } else
            {
                
                SoundPlayer s = new SoundPlayer(Properties.Resources.Windows_XP_Critical_Stop);
                s.Play();
                MessageBox.Show("You need to save the file first");
            }
        }
        bool error = false;
        public void compile(string path)
        {
            Output.Items.Clear();            
             if (!File.ReadAllText(path).ToLower().Contains("islegiteva"))
            {
                error = true;
                SoundPlayer s = new SoundPlayer(Properties.Resources.Windows_XP_Critical_Stop);
                s.Play();
                Output.Items.Add("File Not Legit. Add islegiteva to the top of your file");
                Output.Items.Add("Error Code 1");
            } else if (File.ReadAllText(path).IndexOf("islegiteva") > 1)
            {
                error = true;
                SoundPlayer s = new SoundPlayer(Properties.Resources.Windows_XP_Critical_Stop);
                s.Play();
                Output.Items.Add("You cannot have more than one islegiteva keyword in a file");
                Output.Items.Add("Error Code 2");
            } else if(File.ReadAllText(path).Contains("new") && !File.ReadAllText(path).Contains("requires IO") || File.ReadAllText(path).Contains("dir") && !File.ReadAllText(path).Contains("requires IO") || File.ReadAllText(path).Contains("dl") && !File.ReadAllText(path).Contains("requires IO") || File.ReadAllText(path).Contains("info") && !File.ReadAllText(path).Contains("requires IO"))
            {
                error = true;
                SoundPlayer s = new SoundPlayer(Properties.Resources.Windows_XP_Critical_Stop);
                s.Play();
                Output.Items.Add("You must includ the IO Package to use these functions add 'requires IO' to your code");
                Output.Items.Add("Error Code 3");
            }
            else if (!error)
            {
                Thread.Sleep(1000);
                SoundPlayer s = new SoundPlayer(Properties.Resources.tada);
                s.Play();
                string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines)
                    {
                        if (line.ToLower().StartsWith("printe"))
                        {
                            Output.Items.Add(line.Remove(0, 7));
                        }
                        if (line.ToLower().StartsWith("printdebug"))
                        {
                        Output.Items.Add("Debbuger: " + line.Remove(0, 11));
                        }
                        if (line.ToLower().StartsWith("printerror"))
                        {
                        Output.Items.Add("ERROR: " + line.Remove(0, 11));
                        }
                        if (line.ToLower().StartsWith("inputprint"))
                        {
                        string result;
                        string text = line.Remove(0, 11);
                        result = Interaction.InputBox(text, text);
                        Output.Items.Add(result);
                        }
                        if(line.ToLower().StartsWith("mb"))
                        {
                        string g = line.Remove(0, 3);
                        MessageBox.Show(g, g);
                        } if (line.ToLower().StartsWith("new"))
                        {
                        try
                        {
                            File.Create(line.Remove(0, 4));
                        } catch { }
                        }
                        if(line.ToLower().StartsWith("dir"))
                        {
                        try
                        {
                            Directory.CreateDirectory(line.Remove(0, 4));
                        } catch { }
                        }
                        if(line.ToLower().StartsWith("dl"))
                        {
                        string pathd = line.Remove(0, 3);
                            if(File.Exists(pathd)) {
                            File.Delete(pathd);
                            } else if(Directory.Exists(pathd))
                            {
                            Directory.Delete(pathd);
                            } else
                            {
                            Output.Items.Add("Item " + pathd + " does not exist");
                            }
                        }
                        if(line.ToLower().StartsWith("info"))
                        {
                        DirectoryInfo directoryInfo = new DirectoryInfo(".");
                        foreach (DirectoryInfo d in directoryInfo.GetDirectories())
                        {
                            Output.Items.Add("Current dir contains dir " + d.Name);
                        }
                        foreach(FileInfo f in directoryInfo.GetFiles())
                        {
                            Output.Items.Add("Current dir contains file " + f.Name);
                        }
                        }
                    if (line.ToLower().StartsWith("sleep"))
                    {
                        int sleepytime;
                        try
                        {
                            sleepytime = Int32.Parse(line.Remove(0, 6));
                            Thread.Sleep(sleepytime * 1000);
                        } catch { }
                    }
                    if(line.ToLower().StartsWith("lastconsoleitem"))
                    {
                        Output.Items.Add(Output.Items[Output.Items.Count-1]);
                    }
                    if(line.ToLower().StartsWith("speak"))
                    {
                        string stringtospeak = line.Remove(0, 6);
                        speak(stringtospeak);
                    }
                    if(line.ToLower().StartsWith("inputspeak"))
                    {
                        string stringtospeak = line.Remove(0, 11);
                        speak(stringtospeak);
                        string result = Interaction.InputBox(stringtospeak, stringtospeak);
                        Output.Items.Add(result);
                    }
                    if(line.ToLower().StartsWith("sound"))
                    {
                        string pat;
                        if (File.Exists(line.Remove(0, 6)))
                        {
                            pat = line.Remove(0, 6);
                            SoundPlayer soundPlayer = new SoundPlayer(pat);
                        } else {
                        Output.Items.Add("File Not Found");
                        }
                        
                    }
                }
            }
        }
        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        public static void speak(string stringtospeak)
        {
            if(stringtospeak != "")
            {
                synthesizer.Dispose();
                synthesizer = new SpeechSynthesizer();
                synthesizer.SpeakAsync(stringtospeak);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Properties.Resources.Credits, "Credits");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Properties.Resources.Credits;
        }
    }
}
