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
        public BasicSynthesizer()
        {
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



            GenerateSound(frequency);
        }


        private const int SAMPLE_RATE = 44100;
        private const short BITS_PER_SAMPLE = 16;

        /// <summary>
        /// Creates a WAVE format stream, based on documentation found here: http://soundfile.sapp.org/doc/WaveFormat/
        /// </summary>
        /// <param name="binaryWave"></param>
        /// <param name="writer"></param>
        /// <param name="BlockAlign"></param>
        /// <param name="subChunkTwoSize"></param>
        /// <param name="subChunkOneSize"></param>
        private static void CreateWavStream(byte[] binaryWave, BinaryWriter writer, short BlockAlign, int subChunkTwoSize, int subChunkOneSize)
        {
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + subChunkTwoSize);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(subChunkOneSize);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SAMPLE_RATE);
            writer.Write(SAMPLE_RATE * BlockAlign);
            writer.Write(BlockAlign);
            writer.Write(BITS_PER_SAMPLE);
            writer.Write("data".ToCharArray());
            writer.Write(subChunkTwoSize);
            writer.Write(binaryWave);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency"></param>
        private void GenerateSound(float frequency)
        {
            short[] wave = null;
            byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
            const short BlockAlign = BITS_PER_SAMPLE / 8;
            const int subChunkTwoSize = SAMPLE_RATE * 1 * BlockAlign;
            const int subChunkOneSize = 16;

            List<Oscillation> oscillations = new();

            foreach(Oscillator o in Controls.OfType<Oscillator>().Where(o => o.ON))
            {
                oscillations.Add(new Oscillation()
                {
                    WaveForm = o.WaveForm,
                    Volume = (Convert.ToDouble(o.Volume))/100,
                    Frequency = frequency
                }) ;
            }
            wave = GenerateWave(oscillations);
            Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
            PlaySound(binaryWave, BlockAlign, subChunkTwoSize, subChunkOneSize);
        }

        private short[] GenerateWave(IEnumerable<Oscillation> oscillations)
        {
            short[] wave = new short[SAMPLE_RATE];
            short tempSample;
            int waveCount = oscillations.Count();
            Random random = new Random();
            foreach(Oscillation o in oscillations)
            {
                int samplesPerWaveLength = (int)(SAMPLE_RATE / o.Frequency);
                short ampStep = (short)((short.MaxValue * 2) / samplesPerWaveLength);

                switch (o.WaveForm)
                {
                    case WaveForm.Sine:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            wave[i] += Convert.ToInt16(short.MaxValue * Math.Sin(Math.PI * 2 * o.Frequency / SAMPLE_RATE * i) / waveCount * o.Volume);
                        }
                        break;
                    case WaveForm.Square:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            wave[i] += Convert.ToInt16((short.MaxValue * Math.Sign(Math.Sin((Math.PI * 2 * o.Frequency) / SAMPLE_RATE * i))) / waveCount * o.Volume);
                        }
                        break;
                    case WaveForm.Saw:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            tempSample = -short.MaxValue;
                            for (int j = 0; j < samplesPerWaveLength && i < SAMPLE_RATE; j++)
                            {
                                tempSample += ampStep;
                                wave[i++] += Convert.ToInt16(tempSample / waveCount * o.Volume);
                            }
                            i--;
                        }
                        break;
                    case WaveForm.Triangle:
                        tempSample = -short.MaxValue;
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            if (Math.Abs(tempSample + ampStep) > short.MaxValue)
                            {
                                ampStep = (short)-ampStep;
                            }
                            tempSample += ampStep;
                            wave[i] += Convert.ToInt16(tempSample / waveCount * o.Volume);
                        }
                        break;
                    case WaveForm.Noise:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            wave[i] += Convert.ToInt16(random.Next(-short.MaxValue, short.MaxValue) / waveCount * o.Volume);
                        }
                        break;
                    default:
                        throw new Exception();
                }

            }
            
            return wave;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryWave"></param>
        /// <param name="BlockAlign"></param>
        /// <param name="subChunkTwoSize"></param>
        /// <param name="subChunkOneSize"></param>
        private void PlaySound(byte[] binaryWave, short BlockAlign, int subChunkTwoSize, int subChunkOneSize)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream);
            CreateWavStream(binaryWave, writer, BlockAlign, subChunkTwoSize, subChunkOneSize);
            stream.Position = 0;
            SoundPlayer player = new(stream);

            player.Play();
        }


    }
}
