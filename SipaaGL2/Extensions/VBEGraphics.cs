using Cosmos.Core.Multiboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaGL2.Extensions
{
    public unsafe class VBEGraphics : Graphics
    {
        public VBEGraphics() : base()
        {
            Width = Multiboot2.Framebuffer->Width;
            Height = Multiboot2.Framebuffer->Height;
        }

        public override void Flush()
        {
            CopyTo((uint*)Multiboot2.Framebuffer->Address);
        }
    }
}
