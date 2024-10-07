using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Xorwow random number generator
// random, fast and flexible

    public class Randomer
    {
        private uint x, y, z, w, v, addend;

        private Randomer(uint seed1, uint seed2)
        {
            x = seed1;
            y = seed2;
            z = w = 0;
            v = ~seed1;
            addend = (seed1 << 10) ^ (seed2 >> 4);
            for (int i = 0; i < 64; ++i) nextUInt();
        }

        public Randomer(ulong seed): this((uint) seed, (uint) (seed >> 32)) {}

        public Randomer() : this((ulong) DateTime.Now.Ticks) {}

        // uniform[0, 0xFFFF_FFFFu]
        public uint nextUInt()
        {
            uint t = x;
            t ^= t >> 2;
            x = y;
            y = z;
            z = w;
            uint v0 = v;
            w = v0;
            t = t ^ (t << 1) ^ v0 ^ (v0 << 4);
            v = t;
            addend += 362437;
            return t + addend;
        }

        // uniform[0x8000_0000u, 0x7FFF_FFFFu]
        public int nextInt() => (int) nextUInt();

        // uniform[0, 0xFFFF_FFFFu >> (32 - bits)]
        public uint nextBits(int bits) => nextUInt() >> (32 - bits);

        // uniform[0.0, 1.0)
        public float nextFloat() => (float) nextBits(24) / (1 << 24);
        
        public float nextNormal(float mean=0,float sigma = 1)
    {
        float u1 = nextFloat(), u2 = nextFloat();
        return (float)(Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2)*sigma+mean);
    }
        // uniform[0, until)
        public uint nextUInt(uint until)
        {
            switch (until)
            {
                case 0:
                    throw new ArgumentException("'until' cannot be 0");
                case 1:
                    return 0;
                default:
                    uint rnd;
                    int bits = CeilLog2(until);
                    do
                    {
                        rnd = nextBits(bits);
                    } while (rnd >= until);
                    return rnd;
            }
        }

        // uniform[from, until)
        public uint nextUInt(uint from, uint until)
        {
            if (until <= from) throw new ArgumentException("'until' must be greater than 'from'");
            return from + nextUInt(until - from);
        }

        // uniform[from, until)
        public uint nextUInt((uint from, uint until) range) => nextUInt(range.from, range.until);

        // uniform[0, until)
        public int nextInt(int until)
        {
            if (until <= 0) throw new ArgumentException("'until' must be positive");
            return (int) nextUInt((uint) until);
        }

        // uniform[from, until)
        public int nextInt(int from, int until)
        {
            if (until <= from) throw new ArgumentException("'until' must be greater than 'from'");
            return @from + nextInt(until - @from);
        }
    
        // uniform[from, until)
        public int nextInt((int from, int until) range) => nextInt(range.from, range.until);

        // uniform[false, true]
        public bool nextBool() => nextBits(1) != 0;
    
        // bit operations
        // magic, do not touch
    
        [StructLayout(LayoutKind.Explicit)]
        private struct Union
        {
            [FieldOffset(0)] public ulong asLong;
            [FieldOffset(0)] public double asDouble;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CeilLog2(uint x) // requires x >= 2
            => (int)((new Union{asDouble = x - 1}.asLong >> 52) + 2) & 0xFF;
    }

