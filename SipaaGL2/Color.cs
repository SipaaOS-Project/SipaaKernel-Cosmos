using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaGL2
{
    public class Color
    {
        public byte A { get; set; } = 255;
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;

        public uint ARGB { get { return ToARGB(A, R, G, B); } }

        public Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static uint ToARGB(byte A, byte R, byte G, byte B)
        {
            return (uint)(A << 24 | R << 16 | G << 8 | B);
        }

        public static Color FromARGB(uint argb)
        {
            return new((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        }

        public static Color AlphaBlend(Color Source, Color NewColor)
        {
            if (NewColor.A == 255)
            {
                return NewColor;
            }
            if (NewColor.A == 0)
            {
                return Source;
            }

            return new(
                (byte)((Source.A * (255 - NewColor.A) / 255) + NewColor.A),
                (byte)((Source.R * (255 - NewColor.A) / 255) + NewColor.R),
                (byte)((Source.G * (255 - NewColor.A) / 255) + NewColor.G),
                (byte)((Source.B * (255 - NewColor.A) / 255) + NewColor.B));
        }
    }
}
