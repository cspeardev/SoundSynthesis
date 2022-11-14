using System.Media;

namespace WaveGeneration;

public class Synthesizer
{
    private const int SAMPLE_RATE = 44100;
    private const short BITS_PER_SAMPLE = 16;
    private const short BLOCK_ALIGN = BITS_PER_SAMPLE / 8;
    private const int SUB_CHUNK_ONE_SIZE = 16;
    private List<Oscillation> CurrentOscillations = new();
    private readonly WaveGenerator Generator;
    public bool Playing { get; set; }

    private SoundPlayer player;

    /// <summary>
    /// 
    /// </summary>
    public Synthesizer()
    {
        player = new SoundPlayer();
        player.StreamChanged += Player_StreamChanged;
        Generator = new(SAMPLE_RATE);
    }

    private void Player_StreamChanged(object sender, EventArgs e)
    {
        //player.Play();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="binaryWave"></param>
    private void PlayWave(byte[] binaryWave)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        var WaveDuration = binaryWave.Length / 2 / SAMPLE_RATE * 1000;
        CreateWavStream(writer, WaveDuration);
        writer.Write(binaryWave);
        stream.Position = 0;
        player.Stream = stream;
    }

    private async Task PlaySound()
    {
        short[] wave = Generator.GenerateWave(CurrentOscillations, 0);
        byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
        Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
        PlayWave(binaryWave);
    }

    /// <summary>
    /// Creates a WAVE format stream, based on documentation found here: http://soundfile.sapp.org/doc/WaveFormat/
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="Length">Length of sample to generate, in milliseconds</param>
    private static void CreateWavStream(BinaryWriter writer, int Length)
    {
        double SampleModifierLength = (double)Length / 1000;
        int subChunkTwoSize = (int)(SAMPLE_RATE * SampleModifierLength * BLOCK_ALIGN);
        //ChunkID
        writer.Write("RIFF".ToCharArray());
        //ChunkSize
        writer.Write(36 + subChunkTwoSize);
        //Format
        writer.Write("WAVE".ToCharArray());
        //Subchunk1ID
        writer.Write("fmt ".ToCharArray());
        //Subchunk1Size
        writer.Write(SUB_CHUNK_ONE_SIZE);
        //AudioFormat
        writer.Write((short)1);
        //NumChannels
        writer.Write((short)1);
        //SampleRate
        writer.Write(SAMPLE_RATE);
        //ByteRate
        writer.Write(SAMPLE_RATE * BLOCK_ALIGN);
        //BlockAlign
        writer.Write(BLOCK_ALIGN);
        //BitsPerSample
        writer.Write(BITS_PER_SAMPLE);
        //SubChunk2ID
        writer.Write("data".ToCharArray());
        //SubChunk2Size
        writer.Write(subChunkTwoSize);
    }

    /// <summary>
    /// Adds oscillations
    /// </summary>
    /// <param name="frequency"></param>
    public void AddOscillations(IEnumerable<Oscillation> oscillations)
    {
        if (CurrentOscillations == null || !CurrentOscillations.SequenceEqual(oscillations))
        {
            foreach (Oscillation osc in oscillations)
            {
                if (!CurrentOscillations.Contains(osc))
                {
                    CurrentOscillations.Add(osc);
                }
            }
            if (!Playing)
            {
                Playing = true;
                _ = Task.Run(() => PlaySound());
            }
            else
            {

            }
        }
    }

    /// <summary>
    /// Removes oscillations 
    /// </summary>
    /// <param name="oscillations"></param>
    public void RemoveOscillations(List<Oscillation> oscillations)
    {
        if (oscillations != null)
        {
            CurrentOscillations.RemoveAll(o => oscillations.Contains(o));
        }
        else
        {
            CurrentOscillations.Clear();
        }

        if (CurrentOscillations.Count == 0) Playing = false;
    }
}