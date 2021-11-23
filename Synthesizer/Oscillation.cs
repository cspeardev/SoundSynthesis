
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesizerProject
{
    public class Oscillation : IEquatable<Oscillation>
    {
        public WaveForm WaveForm { get; set; }
        public double Volume { get; set; }
        public float Frequency { get; set; }

        public bool Equals(Oscillation other)
        {
            if(ReferenceEquals(this, other)) return true;
            if(WaveForm != other.WaveForm) return false;
            if(Volume != other.Volume) return false;   
            if(Frequency != other.Frequency) return false; 
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Oscillation);
        }
    }
}
