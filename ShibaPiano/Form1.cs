using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ShibaPiano.Services;
//using NAudio.Lame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShibaPiano
{
    public partial class Form1 : Form
    {
        UserActivityHook actHook;

        CancellationTokenSource cts = new CancellationTokenSource();
        int indexOfParens = 0;

        string SheetsFile = Directory.GetCurrentDirectory() + "/Sheets/";

        string InputDirName = Directory.GetCurrentDirectory() + "/Sounds/";
        //string OutputDirName = Directory.GetCurrentDirectory() + "/Combine/";

        List<int> KeyPress = new List<int>();
        Dictionary<int, int> ChangesKey = new Dictionary<int, int>()
        {
            [48] = -1,  //0
            [49] = 33,  //1
            [50] = 64,  //2
            [51] = -1,  //3
            [52] = 36,  //4
            [53] = 37,  //5
            [54] = 94,  //6
            [55] = -1,  //7
            [56] = 42,  //8
            [57] = 40,  //9

            [65] = 97,  //A
            [66] = 98,  //B
            [67] = 99,  //C
            [68] = 100,  //D
            [69] = 101,  //E
            [70] = 102,  //F
            [71] = 103,  //G
            [72] = 104,  //H
            [73] = 105,  //I
            [74] = 106,  //J
            [75] = 107,  //K
            [76] = 108,  //L
            [77] = 109,  //M
            [78] = 110,  //N
            [79] = 111,  //O
            [80] = 112,  //P
            [81] = 113,  //Q
            [82] = 114,  //R
            [83] = 115,  //S
            [84] = 116,  //T
            [85] = 117,  //U
            [86] = 118,  //V
            [87] = 119,  //W
            [88] = 120,  //X
            [89] = 121,  //Y
            [90] = 122,  //Z
        };

        public Form1()
        {
            InitializeComponent();

            actHook = new UserActivityHook(); // crate an instance

            // hang on events
            //actHook.OnMouseActivity += new MouseEventHandler(MouseMoved);
            actHook.KeyDown += new KeyEventHandler(MyKeyDown);
            actHook.KeyUp += new KeyEventHandler(MyKeyUp);
            //actHook.KeyPress += new KeyPressEventHandler(MyKeyPress);

            this.LoadSheets();
        }

        //public void MyKeyPress(object sender, KeyPressEventArgs e)
        //{
        //    lblCode.Text = ((int)e.KeyChar).ToString();
        //}

        public void MyKeyDown(object sender, KeyEventArgs e)
        {
            var keyValue = e.KeyValue;
            
            if (KeyPress.Contains(keyValue))
                return;

            KeyPress.Add(keyValue);

            var typeOfKey = "a";

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                typeOfKey = "b";

            if (typeOfKey == "a")
            {
                switch (keyValue)
                {
                    case 96: keyValue = 48; break;
                    case 97: keyValue = 49; break;
                    case 98: keyValue = 50; break;
                    case 99: keyValue = 51; break;
                    case 100: keyValue = 52; break;
                    case 101: keyValue = 53; break;
                    case 102: keyValue = 54; break;
                    case 103: keyValue = 55; break;
                    case 104: keyValue = 56; break;
                    case 105: keyValue = 57; break;
                }
            }
            else if (typeOfKey == "b")
            {
                switch (keyValue)
                {
                    case 45: keyValue = 48; break;
                    case 35: keyValue = 49; break;
                    case 40: keyValue = 50; break;
                    case 34: keyValue = 51; break;
                    case 37: keyValue = 52; break;
                    case 12: keyValue = 53; break;
                    case 39: keyValue = 54; break;
                    case 36: keyValue = 55; break;
                    case 38: keyValue = 56; break;
                    case 33: keyValue = 57; break;
                }
            }



            var soundName = typeOfKey + keyValue;
            lblCode.Text = keyValue.ToString();

            var path = this.InputDirName + $"{soundName}.mp3";

            var paths = new List<string>();
            paths.Add(path);

            this.PlaySound(paths);

            //txtNotes.Text += " ";
            //txtNotes.SelectionStart = txtNotes.Text.Length;
            //txtNotes.SelectionLength = 0;
        }

        public void MyKeyUp(object sender, KeyEventArgs e)
        {
            if (KeyPress.Contains(e.KeyValue))
            {
                KeyPress.Remove(e.KeyValue);
            }
        }
        //void keyboard_KeyBoardKeyPressed(object sender, EventArgs e)
        //{
        //    var temp = sender.GetHashCode();
        //}

        //void mouse_MouseMoved(object sender, EventArgs e)
        //{
        //    mouseTime.Content = FormatDateTime(DateTime.Now);
        //}

        private void BtExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtPlay_Click(object sender, EventArgs e)
        {
            if (this.btPlay.Text == "Play")
            {
                this.PlayNotes(this.cts.Token);
                this.btPlay.Text = "Stop";
            }
            else if (this.btPlay.Text == "Stop")
            {
                this.cts.Cancel(true);
                this.btPlay.Text = "Play";
                this.cts = new CancellationTokenSource();
            }
        }

        private void PlaySound(List<string> paths)
        {
            if (paths.Count == 0)
                return;

            var readers = new List<AudioFileReader>();

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                    continue;

                readers.Add(new AudioFileReader(path));
            }

            if (readers.Count == 0)
                return;

            var mixer = new MixingSampleProvider(readers.ToArray());
            //WaveFileWriter.CreateWaveFile16("mixed.wav", mixer);
            var wo = new WaveOutEvent();
            wo.Init(mixer);
            wo.Play();
        }

        public void PlayNotes(CancellationToken token)
        {
            var notes = txtNotes.Text;
            txtNotes.Focus();

            Task.Run(() =>
            {
                indexOfParens = 0;
                var multiple = false;
                var paths = new List<string>();
                var checkNote = true;

                foreach (char note in notes)
                {
                    // do some work
                    if (!token.IsCancellationRequested)
                    {

                        if (note == '[')
                        {
                            multiple = true;
                            indexOfParens++;
                            continue;
                        }

                        if (note == ']')
                        {
                            multiple = false;
                            checkNote = false;
                        }

                        if (note == ' ')
                        {
                            indexOfParens++;
                            continue;
                        }

                        this.Invoke(new Action(() =>
                        {
                            txtNotes.SelectionStart = indexOfParens;
                            txtNotes.SelectionLength = 1;
                        }));

                        if (checkNote)
                        {
                            var typeOfKeye = "a";
                            var value = (int)note;

                            if (ChangesKey.ContainsKey(value))
                            {
                                typeOfKeye = "b";
                                //value = ChangesKey.FirstOrDefault(x => x.Value == value).Key;
                                value = ChangesKey[value];

                                if (value == -1)
                                {
                                    indexOfParens++;
                                    continue;
                                }
                            }

                            var soundName = typeOfKeye + value;
                            var path = this.InputDirName + $"{soundName}.mp3";

                            paths.Add(path);
                        }

                        if (multiple)
                        {
                            indexOfParens++;
                            continue;
                        }

                        this.PlaySound(paths);

                        paths = new List<string>();
                        checkNote = true;

                        Thread.Sleep((int)numTimeout.Value);

                        indexOfParens++;
                    }
                }

                this.Invoke(new Action(() =>
                {
                    this.btPlay.Text = "Play";
                }));

            }, token);
        }

        private void CmbSheets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 0)
            {
                txtNotes.Text = "";
                return;
            }

            string selected = ((ComboBox)sender).GetItemText(((ComboBox)sender).SelectedItem);
            var path = SheetsFile + selected + ".txt";
            using (StreamReader sr = new StreamReader(path))
            {
                String line = sr.ReadToEnd();
                txtNotes.Text = line;
            }
        }

        private void ChbEnterText_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                txtNotes.Focus();
                btSave.Enabled = true;
            }
            else
            {
                btSave.Enabled = false;
            }
        }

        private void TxtNotes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!chbEnterText.Checked)
            {
                e.Handled = true;
            }
        }

        private void BtSave_Click(object sender, EventArgs e)
        {
            //var saved = false;
            if (string.IsNullOrWhiteSpace(txtNotes.Text))
            {
                MessageBox.Show("Please enter notes in text box!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fileName = cmbSheets.Text;
            if (string.IsNullOrWhiteSpace(fileName) || fileName == "- Select a sample music sheet to autoplay -")
            {
                MessageBox.Show("Please enter name for save this sheet into the sheets dropdown!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var path = SheetsFile + fileName + ".txt";
            if (File.Exists(path))
            {
                DialogResult dr = MessageBox.Show($"This name has already been used! {Environment.NewLine}Do you want To replace?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                //if (dr == DialogResult.Yes)
                //{
                //    using (var writer = new StreamWriter(path, true))
                //    {
                //        writer.WriteLine(txtNotes.Text);
                //        saved = true;
                //    }
                //}
                //else
                //{
                //    return;
                //}

                if (dr == DialogResult.No)
                {
                    return;
                }
            }

            //if(!saved)
            File.WriteAllText(path, txtNotes.Text);

            this.LoadSheets();
            chbEnterText.Checked = false;
        }

        private void LoadSheets()
        {
            cmbSheets.Items.Clear();
            var filePaths = Directory.GetFiles(SheetsFile, "*.txt");
            cmbSheets.Items.Add("- Select a sample music sheet to autoplay -");
            if (filePaths.Count() > 0)
            {
                foreach (var item in filePaths)
                {
                    cmbSheets.Items.Add(Path.GetFileName(item).Replace(".txt", ""));
                }
            }
            cmbSheets.SelectedIndex = 0;
        }
    }
}
