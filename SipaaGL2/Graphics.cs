using Cosmos.Core;
using Cosmos.Core.Multiboot;
using Cosmos.Core.Multiboot.Tags;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SipaaGL2
{
    public unsafe class Graphics
    {
        public static uint* Internal = null;

        static bool InternalInitialized = false;
        static uint _width = 800;
        static uint _height = 600;

        public uint Width
        {
            get { return _width; }
            set
            {
                if (!InternalInitialized)
                    Internal = (uint*)NativeMemory.Realloc(Internal, (value * _height) * 4);
                else
                    Internal = (uint*)NativeMemory.Alloc((value * _height) * 4);

                _width = value;
            }
        }

        public uint Height
        {
            get { return _height; }
            set
            {
                if (!InternalInitialized)
                    Internal = (uint*)NativeMemory.Realloc(Internal, (_width * value) * 4);
                else
                    Internal = (uint*)NativeMemory.Alloc((_width * value) * 4);

                _height = value;
            }
        }

        public uint Size
        {
            get => _width * _height;
        }

        public Graphics()
        {
            Internal = (uint*)NativeMemory.Alloc((_width * _height) * 4);
            InternalInitialized = true;
        }

        public void SetPixel(int x, int y, Color color)
        {
            var blendedcol = color;

            if (color.A > 0 && color.A < 255)
                blendedcol = Color.AlphaBlend(GetPixel(x, y), color);

            Internal[y * _width + x] = blendedcol.ARGB;
        }

        public Color GetPixel(int x, int y)
        {
            return Color.FromARGB(Internal[y * _width * x]);
        }

        /**public static void SetImage(LibASGImage image, int x, int y)
        {
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    UInt32 pixel = image.pixels[j * image.Width + i];
                    SetPixel(x + i, y + j, pixel);
                }
            }
        }**/

        public void set_round_rect(int x, int y, int width, int height, int radius, Color color)
        {
            for (int j = y; j < (y + height); j++)
            {
                for (int i = x; i < (x + width); i++)
                {
                    int dx = i - x;
                    int dy = j - y;
                    if ((dx < radius && dy < radius && Math.Pow(dx - radius, 2) + Math.Pow(dy - radius, 2) > Math.Pow(radius, 2)) ||
                        (dx < radius && dy > height - radius && Math.Pow(dx - radius, 2) + Math.Pow(dy - (height - radius), 2) > Math.Pow(radius, 2)) ||
                        (dx > width - radius && dy < radius && Math.Pow(dx - (width - radius), 2) + Math.Pow(dy - radius, 2) > Math.Pow(radius, 2)) ||
                        (dx > width - radius && dy > height - radius && Math.Pow(dx - (width - radius), 2) + Math.Pow(dy - (height - radius), 2) > Math.Pow(radius, 2)))
                    {
                    }
                    else
                    {
                        SetPixel(i, j, color);
                    }
                }
            }
        }

        public void DrawFilledRectangle(int x, int y, uint width, uint height, Color color)
        {
            for (int j = y; j < (y + height); j++)
            {
                for (int i = x; i < (x + width); i++)
                {
                    SetPixel(i, j, color);
                }
            }
        }

        public void ClearScreen(uint Color = 0x000000)
        {
            Cosmos.Core.MemoryOperations.Fill((byte*)Internal, (int)Color, (int)Size * 4);
        }

        protected void CopyTo(uint* address)
        {
            Buffer.MemoryCopy(Internal, address, Size * 4, Size * 4);
        }

        public virtual void Flush() { }
    }
}