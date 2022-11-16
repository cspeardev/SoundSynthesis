using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WaveGeneration;

namespace BasicSynthesizerProject
{
    public partial class BasicSynthesizer : Form
    {
        private Dictionary<Keys, float> KeyFrequencies = new()
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

            List<Oscillation> oscillations = ExtractOscillations(frequency);

            synth.AddOscillations(oscillations);
            synth.Play();
        }

        private List<Oscillation> ExtractOscillations(float frequency)
        {
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

            return oscillations;
        }

        private void BasicSynthesizer_KeyUp(object sender, KeyEventArgs e)
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

            var oscillations = ExtractOscillations(frequency);

            synth.RemoveOscillations(oscillations);
            synth.Stop();
        }

        private void BasicSynthesizer_Leave(object sender, EventArgs e)
        {
            synth.RemoveOscillations(null);
            synth.Stop();
        }
    }
}
