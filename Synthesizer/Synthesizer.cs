using System.Media;

namespace WaveGeneration;

public abstract class SynthesizerState
{
    public Synthesizer Synthesizer { get; protected set; } = null!;
    protected static CancellationTokenSource source = new CancellationTokenSource();
    public abstract void Play();
    public abstract void Stop();
    public virtual void AddOscillations(IEnumerable<Oscillation> oscillations)
    {
        if (Synthesizer.CurrentOscillations == null || !Synthesizer.CurrentOscillations.SequenceEqual(oscillations))
        {
            foreach (Oscillation osc in oscillations)
            {
                if (!Synthesizer.CurrentOscillations.Contains(osc))
                {
                    Synthesizer.CurrentOscillations.Add(osc);
                }
            }
        }
    }
    public virtual void RemoveOscillations(IEnumerable<Oscillation> oscillations)
    {
        if (oscillations != null)
        {
            Synthesizer.CurrentOscillations.RemoveAll(oscillations.Contains);
        }
        else
        {
            Synthesizer.CurrentOscillations.Clear();
        }
    }
}

public class NotPlayingState : SynthesizerState
{
    public NotPlayingState(Synthesizer synth) => Synthesizer = synth;
    public override void Play()
    {
        source = new();
        Task.Run(() => Synthesizer.SynthesizerState = new GeneratingWaveState(Synthesizer), source.Token);
    }

    public override void Stop()
    {
        //Already not playing, don't worry about it.
        return;
    }
}

public class PlayingState : SynthesizerState
{

    private static SoundPlayer player = new();
    public PlayingState(Synthesizer synth, MemoryStream waveStream)
    {
        Synthesizer = synth;
        playSound(waveStream);
    }

    private void playSound(MemoryStream waveStream)
    {
        player.Stream = waveStream;
        player.PlaySync();
        Synthesizer.SynthesizerState = new GeneratingWaveState(Synthesizer);
        //Synthesizer.SynthesizerState = new NotPlayingState(Synthesizer);
    }


    public override void Play()
    {
        //Already playing, don't worry about it.
        return;
    }
    public override void Stop()
    {
        source.Cancel();
        Synthesizer.SynthesizerState = new NotPlayingState(Synthesizer);
    }
}

public class GeneratingWaveState : SynthesizerState
{
    private const int SAMPLE_RATE = 44100;
    private const short BITS_PER_SAMPLE = 16;
    private const short BLOCK_ALIGN = BITS_PER_SAMPLE / 8;
    private const int SUB_CHUNK_ONE_SIZE = 16;
    private static readonly WaveGenerator Generator = new(SAMPLE_RATE);

    public GeneratingWaveState(Synthesizer synth)
    {
        Synthesizer = synth;
        short[] wave = Generator.GenerateWave(Synthesizer.CurrentOscillations, 0);
        byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
        Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        var WaveDuration = binaryWave.Length / 2 / SAMPLE_RATE * 1000;
        CreateWavStream(writer, WaveDuration);
        writer.Write(binaryWave);
        stream.Position = 0;
        Synthesizer.SynthesizerState = new PlayingState(Synthesizer, stream);
    }
    public override void Play()
    {
        return;
    }
    public override void Stop()
    {
        source.Cancel();
        Synthesizer.SynthesizerState = new NotPlayingState(Synthesizer);
    }


    /// <summary>
    /// Creates a WAVE format stream, based on documentation found here: http://soundfile.sapp.org/doc/WaveFormat/
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="Length">Length of sample to generate, in milliseconds</param>
    private void CreateWavStream(BinaryWriter writer, int Length)
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
}

public class Synthesizer
{
    public SynthesizerState SynthesizerState { get; set; }
    internal List<Oscillation> CurrentOscillations = new();
    public Synthesizer()
    {
        SynthesizerState = new NotPlayingState(this);
    }

    public void Play() => SynthesizerState.Play();

    public void Stop() => SynthesizerState.Stop();

    public void AddOscillations(IEnumerable<Oscillation> oscillations) => SynthesizerState.AddOscillations(oscillations);
    public void RemoveOscillations(IEnumerable<Oscillation> oscillations) => SynthesizerState.RemoveOscillations(oscillations);
}