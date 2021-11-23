using System.Windows.Forms;
using System.Drawing;
using BasicSynthesizerProject;
using System;
using System.Linq;
using SynthesizerProject;

namespace BasicSynthesizerProject
{
    public class Oscillator : GroupBox
    {
        public Oscillator()
        {
            this.Size = new Size(300, 100);
            Point buttonPoint = new(10,15);
            this.Controls.Add(new Button()
            {
                Name = "Sine",
                Location = buttonPoint,
                Text = "Sine",
                BackColor = Color.Yellow
            });
            buttonPoint.X += 55;
            this.Controls.Add(new Button()
            {
                Name = "Square",
                Location = buttonPoint,
                Text = "Square"
            });
            buttonPoint.X += 55;
            this.Controls.Add(new Button()
            {
                Name = "Saw",
                Location = buttonPoint,
                Text = "Saw"
            });
            buttonPoint.X = 10;
            buttonPoint.Y = 50;
            this.Controls.Add(new Button()
            {
                Name = "Triangle",
                Location = buttonPoint,
                Text = "Triangle"
            });
            buttonPoint.X += 55;
            this.Controls.Add(new Button()
            {
                Name = "Noise",
                Location = buttonPoint,
                Text = "Noise"
            });
            foreach(Control c in this.Controls)
            {
                c.Size = new Size(50, 30);
                c.Font = new Font("Microsoft Sans Serif", 6.75f);
                c.Click += WaveButton_Click;
            }
            this.Controls.Add(new CheckBox()
            {
                Name = "OscillatorOn",
                Location = new Point(190, 10),
                Size = new Size(60,30),
                Text = "On",
                Checked = true
            });

            this.Controls.Add(new TrackBar()
            {
                Name = "Volume",
                Text = "Volume",
                Maximum = 100,
                Minimum = 0,
                Value = 100,
                Location = new Point(190, 50)
            }); 
        }

        public bool ON => ((CheckBox)this.Controls["OscillatorOn"]).Checked;
        public int Volume => ((TrackBar)this.Controls["Volume"]).Value;


        private void WaveButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            this.WaveForm = (WaveForm)Enum.Parse(typeof(WaveForm), button.Name);
            foreach (Button otherButtons in Controls.OfType<Button>().Where(b => !b.Equals(button)))
            {
                otherButtons.UseVisualStyleBackColor = true;
            }
            button.BackColor = Color.Yellow;
        }

        public WaveForm WaveForm { get; private set; }
    }
}
