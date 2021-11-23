using SynthesizerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesizerProject
{
    internal class WaveGenerator
    {
        private readonly int SAMPLE_RATE;
        private int sineLast = 0;
        private readonly Random random = new();
        public WaveGenerator(int SampleRate)
        {
            SAMPLE_RATE = SampleRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oscillations"></param>
        /// <returns></returns>
        internal short[] GenerateWave(IEnumerable<Oscillation> oscillations, int StartTime, int Duration = 1000)
        {

            int SampleLength = (int)((double)Duration / 1000 * SAMPLE_RATE);
            short[] wave = new short[SampleLength];
            short tempSample;
            int waveCount = oscillations.Count();


            foreach (Oscillation o in oscillations)
            {
                int samplesPerWaveLength = (int)(SampleLength / o.Frequency);
                short ampStep = (short)((short.MaxValue * 2) / samplesPerWaveLength);
                switch (o.WaveForm)
                {
                    case WaveForm.Sine:
                        for (int i = 0; i < SampleLength; i++)
                        {
                            wave[i] += Convert.ToInt16(short.MaxValue * Math.Sin(Math.PI * 2 * o.Frequency / SampleLength * (i + sineLast)) / waveCount * o.Volume);
                        }
                        break;
                    case WaveForm.Square:
                        for (int i = 0; i < SampleLength; i++)
                        {
                            wave[i] += Convert.ToInt16((short.MaxValue * Math.Sign(Math.Sin((Math.PI * 2 * o.Frequency) / SAMPLE_RATE * i))) / waveCount * o.Volume);
                        }
                        break;
                    case WaveForm.Saw:
                        for (int i = 0; i < SampleLength; i++)
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
                        for (int i = 0; i < SampleLength; i++)
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
                        for (int i = 0; i < SampleLength; i++)
                        {
                            wave[i] += Convert.ToInt16(random.Next(-short.MaxValue, short.MaxValue) / waveCount * o.Volume);
                        }
                        break;
                    default:
                        throw new Exception("Unknown WaveForm supplied.");
                }

            }

            sineLast += SampleLength;
            return wave;
        }

    }
}
