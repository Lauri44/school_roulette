using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Roulette
{
    public partial class Form1 : Form
    {
        private delegate void SafeCallDelegate(string points);

        // inits lists for control elements
        private List<Button> buttons = new List<Button>();
        private List<Button> selection = new List<Button>();
        private List<List<String>> spcBtn = new List<List<String>>(); // 2D matrix for buttons
        private List<List<int>> btnM = new List<List<int>>();

        private List<RadioButton> radioButtons = new List<RadioButton>();

        private String[] invCtrls = {"btnSpin", "btnBreak"};
        
        // inits threads
        private Thread mainThread;
        private Boolean threadIsAlreadyActive;

        private int cCount = 0; // cCount counts the selections that were made
        private int value = 25;

        public Form1()
        {
            InitializeComponent();

            mainThread = Thread.CurrentThread; // <- this is not usefull for this program

            threadIsAlreadyActive = false;

            this.BackColor = System.Drawing.Color.Green;

            radioButtons.AddRange(this.Controls.OfType<RadioButton>().ToList());

            foreach(RadioButton radioButton in radioButtons) { radioButton.GotFocus += new EventHandler(SetValue); }

            rdb1.Checked = true;

            // adds all buttons to a list
            buttons.AddRange(this.Controls.OfType<Button>().ToList());

            foreach (Button button in buttons) {
                button.Click += new EventHandler(SetAction);   // maby restructure the code - adding seperate EvH for spcBtns
                button.Tag = "false";

                if(button.Name.StartsWith("btn_"))
                {
                    if (Convert.ToInt32(button.Text) % 2 == 0)
                    {
                        button.BackColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        button.BackColor = System.Drawing.Color.Black;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                }
                else
                {
                    if (button.Name.EndsWith("Hr"))
                    {
                        button.BackColor = System.Drawing.Color.Red;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                    else if(button.Name.EndsWith("Hb"))
                    {
                        button.BackColor = System.Drawing.Color.Black;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        button.BackColor = System.Drawing.Color.Green;
                        button.ForeColor = System.Drawing.Color.White;
                    }

                    if (!button.Name.EndsWith("Spin"))
                    {
                        List<String> spc = new List<string>();
                        spc.Add(button.Name);
                        spcBtn.Add(spc);
                    }
                }
            }

            #region ======================================> Jar Jar Binks <===========================================

            // The following code in this region eppeard to be useless (just like Jar Jar Binks from Star Wars)
            // but i didn't delete it because i've spend two hourse on that - caused by some kind of stupidety -, isn't it!?

            // this code had the task to sort all the buttons by its row they are in
            // or which row they stand for
            // and put them in the right place in a nested List

            //for (int i = 0; i <= 2; i++)
            //{
            //    List<Button> row = new List<Button>();
            //    foreach (Button button in buttons)
            //    {
            //        if (button.Name.StartsWith($"row{i + 1}"))
            //        {
            //            row.Insert(0, button);
            //        }
            //        else
            //        {
            //            for (int x = 0; x <= 11; x++)
            //            {
            //                if (button.Name.EndsWith($"_{i + 1 + 3 * x}"))
            //                {
            //                    row.Add(button);
            //                }
            //            }
            //        }
            //    }
            //    spcBtn.Add(row);
            //}

            // This code appeard to be mutch more usefull then our Jar Jar above
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].Name.StartsWith($"btn_"))
                {         
                    List<int> onBtnSet = new List<int>();
                    onBtnSet.Add(Convert.ToInt32(buttons[i].Text));
                    btnM.Add(onBtnSet);
                }
            }
            
            #endregion
        }

        // this method will be executed when the click event happened
        private void SetAction(Object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (isCtrlInv(button)) { return; }

            if (button.Tag.ToString() == "true")
            {
                if (!button.Name.StartsWith("btn_"))
                {
                    if (button.Name.EndsWith("Hr"))
                    {
                        button.BackColor = System.Drawing.Color.Red;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                    else if(button.Name.EndsWith("Hb"))
                    {
                        button.BackColor = System.Drawing.Color.Black;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        button.BackColor = System.Drawing.Color.Green;
                        button.ForeColor = System.Drawing.Color.White;
                    }
                }
                else if (Convert.ToInt32(button.Text) % 2 == 0)
                {
                    button.BackColor = System.Drawing.Color.Red;
                }
                else
                {
                    button.BackColor = System.Drawing.Color.Black;
                    button.ForeColor = System.Drawing.Color.White;
                }

                selection.Remove(button);
                button.Tag = "false";

                rmBtn(button);
                rmVisSet(button);

                cCount--;
            }
            else
            {
                if (cCount == 4 || Convert.ToInt32(lblPoints.Text) - value < 0) { return; } // potential function to check for the count and its neighbours

                button.BackColor = System.Drawing.Color.Gray;
                button.ForeColor = System.Drawing.Color.Black;

                selection.Add(button);
                button.Tag = "true";

                addBtn(button);
                visSet(button);

                cCount++;
            }
        }

        private void SetValue(object sender, EventArgs e)
        {
            RadioButton rBtn = (RadioButton)sender;

            switch (rBtn.Name.Last())
            {
                case '1':
                    value = 25;
                    break;
                case '2':
                    value = 50;
                    break;
                case '3':
                    value = 100;
                    break;
            }
        }

        private void btnSpin_Click(object sender, EventArgs e)
        {
            List<Button> btnL = new List<Button>();

            foreach(Button button in buttons)
            {
                if (button.Name.StartsWith("btn_"))
                {
                    btnL.Add(button);
                }
                
            }

            btnL.Reverse();

            if (!threadIsAlreadyActive)
            {
                Thread spinThread = new Thread(() => Roulette(btnL))
                { IsBackground = true };
                spinThread.Start();
                threadIsAlreadyActive = true;
            }
        }

        private void addBtn(Button button)
        {
            if (button.Name.StartsWith("btn_"))
            {
                foreach (List<int> num in btnM)
                {
                    if (num[0] == Convert.ToInt32(button.Text))
                    {
                        num.Add(value);
                    }
                }
            }
            else
            {
                foreach (List<String> spc in spcBtn)
                {
                    if (spc[0] == button.Name)
                    {
                        spc.Add(value.ToString());
                    }
                }
            }

            int points = Convert.ToInt32(lblPoints.Text) - value;
            lblPoints.Text = points.ToString();
        }

        private void rmBtn(Button button)
        {
            int points = 0;
            if (button.Name.StartsWith("btn_"))
            {
                foreach (List<int> num in btnM)
                {
                    if (num[0] == Convert.ToInt32(button.Text))
                    {
                        points += num.Last();
                        num.Remove(num.Last());
                    }
                }
            }
            else
            {
                foreach (List<String> spc in spcBtn)
                {
                    if (spc[0] == button.Name)
                    {
                        points += Convert.ToInt32(spc.Last());
                        spc.Remove(spc.Last());
                    }
                }
            }

            points += Convert.ToInt32(lblPoints.Text);
            lblPoints.Text = points.ToString();
        }

        private void visSet(Button button)
        {
            // visSet() [visualise set points]

            int x = button.Location.X + button.Size.Width / 2 - 8;
            int y = button.Location.Y + button.Size.Height / 3 * 2;

            Label lblSetPoints = new Label();
            lblSetPoints.Location = new Point(x, y);
            lblSetPoints.Name = "lblSetPoints";
            lblSetPoints.Tag = button.Name;
            lblSetPoints.AutoSize = true;
            lblSetPoints.BackColor = button.BackColor;
            lblSetPoints.ForeColor = Color.White;
            lblSetPoints.Text = value.ToString();

            this.Controls.Add(lblSetPoints);

            lblSetPoints.BringToFront();
        }

        private void rmVisSet(Button button)
        {
            // rmVisSet() [remove visualisation of set points]

            foreach(Control control in this.Controls.OfType<Label>())
            {
                if (control.Tag ==  button.Name)
                {
                    this.Controls.Remove(control);
                }
            }
        }

        private bool isCtrlInv(Button button)
        {
            // isCtrlInv() [is control invalid]
            // it is a validation check of the given control element

            if (invCtrls.Contains(button.Name))
            {
                return true;
            }
            return false;
        }

        private void Roulette(List<Button> btnL) // not ready
        {
            Random rand = new Random();
            int x=0; // <= Contains the exponent for the for-loop
            int index = rand.Next(0, 36);
            double OUTCOME = rand.NextDouble() / 100;
            OUTCOME += (OUTCOME < 0.001) ? 1.001 : 1.000;

            // in this for-loop i used the exponential function a * b^x
            // to simulate the down slowing effect through enrgy loss

            for (double i = 1; i <= rand.Next(1000, 1500); i *= Math.Pow(OUTCOME, x))
            {
                if (i > 1 && index < btnL.Count - 1) { index++; } else { index = 0; }

                Color bColor = btnL[index].BackColor;
                Color fColor = btnL[index].ForeColor;

                btnL[index].BackColor = Color.Yellow;
                btnL[index].ForeColor = Color.Black;

                Thread.Sleep(Convert.ToInt32(i));

                btnL[index].BackColor = bColor;
                btnL[index].ForeColor = fColor;
                
                x++;
            }
            Console.WriteLine(btnL[index].Name);
            Won(btnL[index]);
            threadIsAlreadyActive = false;
        }

        private void Won(Button button)
        {
            // checks if the player won a round or not

            bool isSet = false;
            bool isInformed = false;

            int newValue = Convert.ToInt32(lblPoints.Text);

            foreach(List<int> btn in btnM)
            {
                if(btn.Count == 2 && btn[0].Equals(Convert.ToInt32(button.Text)))
                {
                    MessageBox.Show("You Won");
                    newValue += btn.Last()*35;

                    isSet = true;
                    isInformed = true;
                }
            }
            
            foreach(List<string> btn in spcBtn)
            {
                if(btn.Count == 2) //this is a part of the reward system but its not finished yet...
                {
                    if((btn[0].StartsWith("btnQ") || btn[0].StartsWith("btnR")) && isSet)
                    {
                        newValue += Convert.ToInt32(btn.Last())*3;
                    }
                    else if (btn[0].StartsWith("btnH"))
                    {
                        newValue += Convert.ToInt32(btn.Last()) * 2;
                    }
                }
            }

            safeCall(newValue.ToString());
        }

        private void safeCall(string points)
        {
            // this function is required to call the controll element lblPoints
            // and change its text without crashing!!!

            if (lblPoints.InvokeRequired)
            {
                var t = new SafeCallDelegate(safeCall); // <= creating an instance of the SafeCallDelegante and calling the method in its self
                lblPoints.Invoke(t, new object[] { points });
            }
            else
            {
                lblPoints.Text = points;
            }
        }
    }
}
