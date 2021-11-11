using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using BasicSynthesizer;

namespace BasicSynthesizerProject
{
    public partial class BasicSynthesizer : Form
    {
        private Dictionary<Keys, float> KeyFrequencies = new Dictionary<Keys, float>
        {
            {Keys.Z,65.4f},
            {Keys.X,138.59f},
            {Keys.C,261.62f},
            {Keys.V,523.25f},
            {Keys.B,1046.5f},
            {Keys.N,2093f},
            {Keys.M,4185.01f}
        };
        private Synthesizer synth;
        public BasicSynthesizer()
        {
            synth = new Synthesizer()
            {

            };
            InitializeComponent();
            
        }

        private void BasicSynthesizer_KeyDown(object sender, KeyEventArgs e)
        {
            float frequency;
            if (KeyFrequencies.ContainsKey(e.KeyCode))
            {
                frequency = KeyFrequencies[e.KeyCode];
            }
            else
            {
                return;
            }

            List<Oscillation> oscillations = new();
            foreach (Oscillator o in Controls.OfType<Oscillator>().Where(o => o.ON))
            {
                oscillations.Add(new Oscillation()
                {
                    WaveForm = o.WaveForm,
                    Volume = (Convert.ToDouble(o.Volume)) / 100,
                    Frequency = frequency
                });
            }

            synth.GenerateSound(oscillations);
        }



        

        
        

    }
}
