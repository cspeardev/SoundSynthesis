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
        private const short BLOCK_ALIGN = BITS_PER_SAMPLE / 8;
        private const int SUB_CHUNK_TWO_SIZE = SAMPLE_RATE * 1 * BLOCK_ALIGN;
        private const int SUB_CHUNK_ONE_SIZE = 16;
        private IEnumerable<Oscillation> CurrentSample;


        private SoundPlayer player;

        /// <summary>
        /// 
        /// </summary>
        public Synthesizer()
        {
            player = new SoundPlayer();
            player.StreamChanged += Player_StreamChanged;
        }

        private void Player_StreamChanged(object sender, EventArgs e)
        {
            player.Play();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oscillations"></param>
        /// <returns></returns>
        private short[] GenerateWave(IEnumerable<Oscillation> oscillations, int StartTime)
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
                            wave[i] += Convert.ToInt16(short.MaxValue * Math.Sin(Math.PI * 2 * o.Frequency / SAMPLE_RATE * (i + StartTime) ) / waveCount * o.Volume);
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
                        throw new Exception("Unknown WaveForm supplied.");
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


            CreateWavStream(writer, BlockAlign, subChunkTwoSize, subChunkOneSize);
            writer.Write(binaryWave);

            stream.Position = 0;
            player.Stream = stream;
        }

        /// <summary>
        /// Creates a WAVE format stream, based on documentation found here: http://soundfile.sapp.org/doc/WaveFormat/
        /// </summary>
        /// <param name="binaryWave"></param>
        /// <param name="writer"></param>
        /// <param name="BlockAlign"></param>
        /// <param name="subChunkTwoSize"></param>
        /// <param name="subChunkOneSize"></param>
        private void CreateWavStream( BinaryWriter writer, short BlockAlign, int subChunkTwoSize, int subChunkOneSize)
        {
            //ChunkID
            writer.Write("RIFF".ToCharArray());
            //ChunkSize
            writer.Write(36 + subChunkTwoSize);
            //Format
            writer.Write("WAVE".ToCharArray());
            //Subchunk1ID
            writer.Write("fmt ".ToCharArray());
            //Subchunk1Size
            writer.Write(subChunkOneSize);
            //AudioFormat
            writer.Write((short)1);
            //NumChannels
            writer.Write((short)1);
            //SampleRate
            writer.Write(SAMPLE_RATE);
            //ByteRate
            writer.Write(SAMPLE_RATE * BlockAlign);
            //BlockAlign
            writer.Write(BlockAlign);
            //BitsPerSample
            writer.Write(BITS_PER_SAMPLE);
            //SubChunk2ID
            writer.Write("data".ToCharArray());
            //SubChunk2Size
            writer.Write(subChunkTwoSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency"></param>
        public void GenerateSound(IEnumerable<Oscillation> oscillations)
        {
            if (CurrentSample == null || CurrentSample.SequenceEqual(oscillations))
            {
                CurrentSample = oscillations;
                short[] wave = null;
                byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
                wave = GenerateWave(oscillations, 0);
                Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
                PlaySound(binaryWave, BLOCK_ALIGN, SUB_CHUNK_TWO_SIZE, SUB_CHUNK_ONE_SIZE);
            }
        }
    }
}