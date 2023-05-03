using Cosmos.Core;
using Cosmos.HAL;

namespace PrismGraphics.Extentions.VMWare;

/// <summary>
/// The VMWare SVGAII canvas class. Allows for fast(er) graphics.
/// </summary>
public unsafe class SVGAIICanvas : Display
{
	/// <summary>
	/// Creates a new instance of the <see cref="SVGAIICanvas"/> class.
	/// </summary>
	/// <param name="Width">Total width (in pixels) of the canvas.</param>
	/// <param name="Height">Total height (int pixels) of the canvas.</param>
	public SVGAIICanvas(ushort Width, ushort Height): base(0, 0) // Use 0 so no data is assigned.
	{
		Device = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);
		Device.EnableMemory(true);

		uint BasePort = Device.BaseAddressBar[0].BaseAddress;
		IndexPort = (ushort)(BasePort + (uint)IOPortOffset.Index);
		ValuePort = (ushort)(BasePort + (uint)IOPortOffset.Value);

		WriteRegister(Register.ID, (uint)ID.V2);
		if (ReadRegister(Register.ID) != (uint)ID.V2)
		{
			throw new NotSupportedException("Un-supported SVGAII device! Please consider updating.");
		}

		FIFOMemory = new MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
		Features = ReadRegister(Register.Capabilities);
		InitializeFIFO();

		this.Height = Height;
		this.Width = Width;
	}

	#region Properties

	public new ushort Height
	{
		get
		{
			return _Height;
		}
		set
		{
			// Memory resizing it already taken care of here.
			_Height = value;

			if (_Width != 0)
			{
				WriteRegister(Register.Width, Width);
				WriteRegister(Register.Height, Height);
				WriteRegister(Register.BitsPerPixel, 4);
				WriteRegister(Register.Enable, 1);
				InitializeFIFO();

				ScreenBuffer = (uint*)ReadRegister(Register.FrameBufferStart);
				Internal = ScreenBuffer + (Size * 4);
			}
		}
	}

	public new ushort Width
	{
		get
		{
			return _Width;
		}
		set
		{
			// Memory resizing it already taken care of here.
			_Width = value;

			if (_Height != 0)
			{
				WriteRegister(Register.Width, Width);
				WriteRegister(Register.Height, Height);
				WriteRegister(Register.BitsPerPixel, 4);
				WriteRegister(Register.Enable, 1);
				InitializeFIFO();

				ScreenBuffer = (uint*)ReadRegister(Register.FrameBufferStart);
				Internal = ScreenBuffer + (Size * 4);
			}
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Initialize FIFO.
	/// </summary>
	public void InitializeFIFO()
	{
		FIFOMemory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
		FIFOMemory[(uint)FIFO.Max] = FIFOMemory.Size;
		FIFOMemory[(uint)FIFO.NextCmd] = FIFOMemory[(uint)FIFO.Min];
		FIFOMemory[(uint)FIFO.Stop] = FIFOMemory[(uint)FIFO.Min];
		WriteRegister(Register.ConfigDone, 1);
	}

	/// <summary>
	/// Write register.
	/// </summary>
	/// <param name="register">A register.</param>
	/// <param name="value">A value.</param>
	public void WriteRegister(Register register, uint value)
	{
		IOPort.Write32(IndexPort, (uint)register);
		IOPort.Write32(ValuePort, value);
	}

	/// <summary>
	/// Read register.
	/// </summary>
	/// <param name="register">A register.</param>
	/// <returns>uint value.</returns>
	public uint ReadRegister(Register register)
	{
		IOPort.Write32(IndexPort, (uint)register);
		return IOPort.Read32(ValuePort);
	}

	/// <summary>
	/// Get FIFO.
	/// </summary>
	/// <param name="cmd">FIFO command.</param>
	/// <returns>uint value.</returns>
	public uint GetFIFO(FIFO cmd)
	{
		return FIFOMemory[(uint)cmd];
	}

	/// <summary>
	/// Set FIFO.
	/// </summary>
	/// <param name="cmd">Command.</param>
	/// <param name="value">Value.</param>
	/// <returns></returns>
	public uint SetFIFO(FIFO cmd, uint value)
	{
		return FIFOMemory[(uint)cmd] = value;
	}

	/// <summary>
	/// Wait for FIFO.
	/// </summary>
	public void WaitForFifo()
	{
		WriteRegister(Register.Sync, 1);
		while (ReadRegister(Register.Busy) != 0) { }
	}

	/// <summary>
	/// Write to FIFO.
	/// </summary>
	/// <param name="value">Value to write.</param>
	public void WriteToFifo(uint value)
	{
		if ((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4 && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) ||
			GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop))
			WaitForFifo();

		SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
		SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

		if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
			SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
	}

	/// <summary>
	/// A method that checks if the device has a specific feature.
	/// </summary>
	/// <param name="Feature">The feature to check for.</param>
	/// <returns>True if supported, otherwise false.</returns>
	public bool HasFeature(Capability Feature)
	{
		return (Features & (uint)Feature) != 0;
	}

	public new void DrawFilledRectangle(int X, int Y, ushort Width, ushort Height, ushort Radius, Color Color)
	{
		if (Radius == 0 && Color.A == 255 && HasFeature(Capability.RectFill))
		{
			WriteToFifo((uint)FIFOCommand.RECT_FILL);
			WriteToFifo(Color.ARGB);
			WriteToFifo((uint)X);
			WriteToFifo((uint)Y);
			WriteToFifo(Width);
			WriteToFifo(Height);
			WaitForFifo();
		}
		else
		{
			base.DrawFilledRectangle(X, Y, Width, Height, Radius, Color);
		}
	}

	public new void Clear(Color Color)
	{
		if (HasFeature(Capability.RectFill))
		{
			WriteToFifo((uint)FIFOCommand.RECT_FILL);
			WriteToFifo(Color.ARGB);
			WriteToFifo(0);
			WriteToFifo(0);
			WriteToFifo(Width);
			WriteToFifo(Height);
			WaitForFifo();
		}
		else
		{
			base.Clear(Color);
		}
	}

	public override bool DefineCursor(Graphics Graphics)
	{
		if (!HasFeature(Capability.AlphaCursor))
		{
			throw new NotSupportedException("This device does not have accelerated cursor support.");
		}

		WaitForFifo();
		WriteToFifo((uint)FIFOCommand.DEFINE_ALPHA_CURSOR);
		WriteToFifo(0); // ID
		WriteToFifo(0); // Hotspot X
		WriteToFifo(0); // Hotspot Y
		WriteToFifo(Graphics.Width); // Width
		WriteToFifo(Graphics.Height); // Height

		for (uint I = 0; I < Graphics.Size; I++)
		{
			WriteToFifo(Graphics[I].ARGB);
		}

		WaitForFifo();

        return true;
	}

	public override bool SetCursor(uint X, uint Y, bool IsVisible)
	{
		WriteRegister(Register.CursorOn, (uint)(IsVisible ? 1 : 0));
		WriteRegister(Register.CursorX, X);
		WriteRegister(Register.CursorY, Y);
		WriteRegister(Register.CursorCount, ReadRegister(Register.CursorCount) + 1);

        return true;
    }

	public override string GetName()
	{
		return nameof(SVGAIICanvas);
	}

	public override void Update()
	{
		MemoryOperations.Copy(ScreenBuffer, Internal, (int)Size * 4);

		WriteToFifo((uint)FIFOCommand.Update);
		WriteToFifo(0);
		WriteToFifo(0);
		WriteToFifo(Width);
		WriteToFifo(Height);
		WriteToFifo(Height);
		WaitForFifo();

		_Frames++;
	}

    /// <summary>
    /// Disable the SVGA driver
    /// </summary>
    public void Disable()
    {
        WriteRegister(Register.Enable, 0);
    }

    /// <summary>
    /// Enable the SVGA driver
    /// </summary>
    public void Enable()
    {
        WriteRegister(Register.Enable, 1);
    }

    #endregion

    #region Fields

    public readonly MemoryBlock FIFOMemory;
	public readonly PCIDevice Device;
	public readonly ushort IndexPort;
	public readonly ushort ValuePort;
	public readonly uint Features;
	public uint* ScreenBuffer;

	#endregion
}

public enum Register : ushort
{
    /// <summary>
    /// ID.
    /// </summary>
    ID = 0,
    /// <summary>
    /// Enabled.
    /// </summary>
    Enable = 1,
    /// <summary>
    /// Width.
    /// </summary>
    Width = 2,
    /// <summary>
    /// Height.
    /// </summary>
    Height = 3,
    /// <summary>
    /// Max width.
    /// </summary>
    MaxWidth = 4,
    /// <summary>
    /// Max height.
    /// </summary>
    MaxHeight = 5,
    /// <summary>
    /// Depth.
    /// </summary>
    Depth = 6,
    /// <summary>
    /// Bits per pixel.
    /// </summary>
    BitsPerPixel = 7,
    /// <summary>
    /// Pseudo color.
    /// </summary>
    PseudoColor = 8,
    /// <summary>
    /// Red mask.
    /// </summary>
    RedMask = 9,
    /// <summary>
    /// Green mask.
    /// </summary>
    GreenMask = 10,
    /// <summary>
    /// Blue mask.
    /// </summary>
    BlueMask = 11,
    /// <summary>
    /// Bytes per line.
    /// </summary>
    BytesPerLine = 12,
    /// <summary>
    /// Frame buffer start.
    /// </summary>
    FrameBufferStart = 13,
    /// <summary>
    /// Frame buffer offset.
    /// </summary>
    FrameBufferOffset = 14,
    /// <summary>
    /// VRAM size.
    /// </summary>
    VRamSize = 15,
    /// <summary>
    /// Frame buffer size.
    /// </summary>
    FrameBufferSize = 16,
    /// <summary>
    /// Capabilities.
    /// </summary>
    Capabilities = 17,
    /// <summary>
    /// Memory start.
    /// </summary>
    MemStart = 18,
    /// <summary>
    /// Memory size.
    /// </summary>
    MemSize = 19,
    /// <summary>
    /// Config done.
    /// </summary>
    ConfigDone = 20,
    /// <summary>
    /// Sync.
    /// </summary>
    Sync = 21,
    /// <summary>
    /// Busy.
    /// </summary>
    Busy = 22,
    /// <summary>
    /// Guest ID.
    /// </summary>
    GuestID = 23,
    /// <summary>
    /// Cursor ID.
    /// </summary>
    CursorID = 24,
    /// <summary>
    /// Cursor X.
    /// </summary>
    CursorX = 25,
    /// <summary>
    /// Cursor Y.
    /// </summary>
    CursorY = 26,
    /// <summary>
    /// Cursor on.
    /// </summary>
    CursorOn = 27,
    /// <summary>
    /// Cursor count.
    /// </summary>
    CursorCount = 0x0C,
    /// <summary>
    /// Host bits per pixel.
    /// </summary>
    HostBitsPerPixel = 28,
    /// <summary>
    /// Scratch size.
    /// </summary>
    ScratchSize = 29,
    /// <summary>
    /// Memory registers.
    /// </summary>
    MemRegs = 30,
    /// <summary>
    /// Number of displays.
    /// </summary>
    NumDisplays = 31,
    /// <summary>
    /// Pitch lock.
    /// </summary>
    PitchLock = 32,
    /// <summary>
    /// Indicates maximum size of FIFO Registers.
    /// </summary>
    FifoNumRegisters = 293
}

/// <summary>
/// ID values.
/// </summary>
public enum ID : uint
{
    /// <summary>
    /// Magic starting point.
    /// </summary>
    Magic = 0x900000,
    /// <summary>
    /// V0.
    /// </summary>
    V0 = Magic << 8,
    /// <summary>
    /// V1.
    /// </summary>
    V1 = (Magic << 8) | 1,
    /// <summary>
    /// V2.
    /// </summary>
    V2 = (Magic << 8) | 2,
    /// <summary>
    /// Invalid
    /// </summary>
    Invalid = 0xFFFFFFFF
}

/// <summary>
/// FIFO values.
/// </summary>
public enum FIFO : uint
{   // values are multiplied by 4 to access the array by byte index
    /// <summary>
    /// Min.
    /// </summary>
    Min = 0,
    /// <summary>
    /// Max.
    /// </summary>
    Max = 4,
    /// <summary>
    /// Next command.
    /// </summary>
    NextCmd = 8,
    /// <summary>
    /// Stop.
    /// </summary>
    Stop = 12
}

/// <summary>
/// FIFO command values.
/// </summary>
public enum FIFOCommand
{
    /// <summary>
    /// Update.
    /// </summary>
    Update = 1,
    /// <summary>
    /// Rectange fill.
    /// </summary>
    RECT_FILL = 2,
    /// <summary>
    /// Rectange copy.
    /// </summary>
    RECT_COPY = 3,
    /// <summary>
    /// Define bitmap.
    /// </summary>
    DEFINE_BITMAP = 4,
    /// <summary>
    /// Define bitmap scanline.
    /// </summary>
    DEFINE_BITMAP_SCANLINE = 5,
    /// <summary>
    /// Define pixmap.
    /// </summary>
    DEFINE_PIXMAP = 6,
    /// <summary>
    /// Define pixmap scanline.
    /// </summary>
    DEFINE_PIXMAP_SCANLINE = 7,
    /// <summary>
    /// Rectange bitmap fill.
    /// </summary>
    RECT_BITMAP_FILL = 8,
    /// <summary>
    /// Rectange pixmap fill.
    /// </summary>
    RECT_PIXMAP_FILL = 9,
    /// <summary>
    /// Rectange bitmap copy.
    /// </summary>
    RECT_BITMAP_COPY = 10,
    /// <summary>
    /// Rectange pixmap fill.
    /// </summary>
    RECT_PIXMAP_COPY = 11,
    /// <summary>
    /// Free object.
    /// </summary>
    FREE_OBJECT = 12,
    /// <summary>
    /// Rectangle raster operation fill.
    /// </summary>
    RECT_ROP_FILL = 13,
    /// <summary>
    /// Rectangle raster operation copy.
    /// </summary>
    RECT_ROP_COPY = 14,
    /// <summary>
    /// Rectangle raster operation bitmap fill.
    /// </summary>
    RECT_ROP_BITMAP_FILL = 15,
    /// <summary>
    /// Rectangle raster operation pixmap fill.
    /// </summary>
    RECT_ROP_PIXMAP_FILL = 16,
    /// <summary>
    /// Rectangle raster operation bitmap copy.
    /// </summary>
    RECT_ROP_BITMAP_COPY = 17,
    /// <summary>
    /// Rectangle raster operation pixmap copy.
    /// </summary>
    RECT_ROP_PIXMAP_COPY = 18,
    /// <summary>
    /// Define cursor.
    /// </summary>
    DEFINE_CURSOR = 19,
    /// <summary>
    /// Display cursor.
    /// </summary>
    DISPLAY_CURSOR = 20,
    /// <summary>
    /// Move cursor.
    /// </summary>
    MOVE_CURSOR = 21,
    /// <summary>
    /// Define alpha cursor.
    /// </summary>
    DEFINE_ALPHA_CURSOR = 22
}

/// <summary>
/// IO port offset.
/// </summary>
public enum IOPortOffset : byte
{
    /// <summary>
    /// Index.
    /// </summary>
    Index = 0,
    /// <summary>
    /// Value.
    /// </summary>
    Value = 1,
    /// <summary>
    /// BIOS.
    /// </summary>
    Bios = 2,
    /// <summary>
    /// IRQ.
    /// </summary>
    IRQ = 3
}

/// <summary>
/// Capability values.
/// </summary>
[Flags]
public enum Capability
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,
    /// <summary>
    /// Rectangle fill.
    /// </summary>
    RectFill = 1,
    /// <summary>
    /// Rectangle copy.
    /// </summary>
    RectCopy = 2,
    /// <summary>
    /// Rectangle pattern fill.
    /// </summary>
    RectPatFill = 4,
    /// <summary>
    /// Lecacy off screen.
    /// </summary>
    LecacyOffscreen = 8,
    /// <summary>
    /// Raster operation.
    /// </summary>
    RasterOp = 16,
    /// <summary>
    /// Cruser.
    /// </summary>
    Cursor = 32,
    /// <summary>
    /// Cursor bypass.
    /// </summary>
    CursorByPass = 64,
    /// <summary>
    /// Cursor bypass2.
    /// </summary>
    CursorByPass2 = 128,
    /// <summary>
    /// Eigth bit emulation.
    /// </summary>
    EigthBitEmulation = 256,
    /// <summary>
    /// Alpha cursor.
    /// </summary>
    AlphaCursor = 512,
    /// <summary>
    /// Glyph.
    /// </summary>
    Glyph = 1024,
    /// <summary>
    /// Glyph clipping.
    /// </summary>
    GlyphClipping = 0x00000800,
    /// <summary>
    /// Offscreen.
    /// </summary>
    Offscreen1 = 0x00001000,
    /// <summary>
    /// Alpha blend.
    /// </summary>
    AlphaBlend = 0x00002000,
    /// <summary>
    /// Three D.
    /// </summary>
    ThreeD = 0x00004000,
    /// <summary>
    /// Extended FIFO.
    /// </summary>
    ExtendedFifo = 0x00008000,
    /// <summary>
    /// Multi monitors.
    /// </summary>
    MultiMon = 0x00010000,
    /// <summary>
    /// Pitch lock.
    /// </summary>
    PitchLock = 0x00020000,
    /// <summary>
    /// IRQ mask.
    /// </summary>
    IrqMask = 0x00040000,
    /// <summary>
    /// Display topology.
    /// </summary>
    DisplayTopology = 0x00080000,
    /// <summary>
    /// GMR.
    /// </summary>
    Gmr = 0x00100000,
    /// <summary>
    /// Traces.
    /// </summary>
    Traces = 0x00200000,
    /// <summary>
    /// GMR2.
    /// </summary>
    Gmr2 = 0x00400000,
    /// <summary>
    /// Screen objects.
    /// </summary>
    ScreenObject2 = 0x00800000
}
