using Cosmos.Core.Multiboot;

namespace PrismGraphics.Extentions.VESA;

/// <summary>
/// The VBE canvas extention class.
/// </summary>
public unsafe class VBECanvas : Display
{
	/// <summary>
	/// Creates a new instance of the <see cref="VBECanvas"/> class.
	/// </summary>
	public VBECanvas() : base((ushort)Multiboot2.Framebuffer->Width, (ushort)Multiboot2.Framebuffer->Height) { }

	#region Methods

	public override bool DefineCursor(Graphics Graphics)
	{
		return false;
	}

	public override bool SetCursor(uint X, uint Y, bool IsVisible)
	{
		return false;
	}

	public override string GetName()
	{
		return nameof(VBECanvas);
	}

	public override unsafe void Update()
	{
		CopyTo((uint*)Multiboot2.Framebuffer->Address);
		_Frames++;
	}

	#endregion
}