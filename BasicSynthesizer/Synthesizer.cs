using BasicSynthesizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

namespace BasicSynthesizerProject
{
    public class Synthesizer
    {
        private const int SAMPLE_RATE = 44100;
        private const short BITS_PER_SAMPLE = 16;
        private const short BlockAlign = BITS_PER_SAMPLE / 8;
        private const int subChunkTwoSize = SAMPLE_RATE * 1 * BlockAlign;
        private const int subChunkOneSize = 16;

        private SoundPlayer player;

        /// <summary>
        /// 
        /// </summary>
        public Synthesizer()
        {
            player = new SoundPlayer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oscillations"></param>
        /// <returns></returns>
        private short[] GenerateWave(IEnumerable<Oscillation> oscillations)
        {
            short[] wave = new short[SAMPLE_RATE];
            short tempSample;
            int waveCount = oscillations.Count();
            Random random = new Random();
            foreach (Oscillation o in oscillations)
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
            //SoundPlayer player = new(stream);
            player.Stream = stream;
            player.PlaySync();
        }

        /// <summary>
        /// Creates a WAVE format stream, based on documentation found here: http://soundfile.sapp.org/doc/WaveFormat/
        /// </summary>
        /// <param name="binaryWave"></param>
        /// <param name="writer"></param>
        /// <param name="BlockAlign"></param>
        /// <param name="subChunkTwoSize"></param>
        /// <param name="subChunkOneSize"></param>
        private void CreateWavStream(byte[] binaryWave, BinaryWriter writer, short BlockAlign, int subChunkTwoSize, int subChunkOneSize)
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
        public void GenerateSound(IEnumerable<Oscillation> oscillations)
        {
            short[] wave = null;
            byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];

            wave = GenerateWave(oscillations);
            Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
            PlaySound(binaryWave, BlockAlign, subChunkTwoSize, subChunkOneSize);
        }


    }
}