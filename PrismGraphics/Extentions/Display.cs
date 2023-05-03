﻿//using PrismGraphics.Extentions.VMWare;
using PrismGraphics.Extentions.VESA;
using Cosmos.System;
using Cosmos.Core.Multiboot;
using PrismGraphics.Extentions.VMWare;

namespace PrismGraphics.Extentions;

/// <summary>
/// The generic display interface. Used to abstract driver classes and get display output.
/// </summary>
public abstract class Display : Graphics
{
	public Display(ushort Width, ushort Height) : base(Width, Height)
	{
		// Setup the FPS counter timer.
		Timer T = new((O) => { _FPS = _Frames; _Frames = 0; }, null, 1000, 0);
	}

	#region Methods

	/// <summary>
	/// Sets the image of the hardware accelerated cursor.
	/// </summary>
	/// <param name="Graphics">The image to use as the cursor.</param>
	public abstract bool DefineCursor(Graphics Graphics);

	/// <summary>
	/// Sets the position of the hardware accelerated cursor on the screen.
	/// </summary>
	/// <param name="X">The X-axis position.</param>
	/// <param name="Y">The Y-axis position.</param>
	public abstract bool SetCursor(uint X, uint Y, bool IsVisible);

	/// <summary>
	/// Gets a display output, the best mode is automatically chosen.
	/// The Width and Height arguments may not always be used.
	/// </summary>
	/// <param name="Width">The requested Width of the display.</param>
	/// <param name="Height">The requested Height of the display.</param>
	/// <returns>An instance of the display class.</returns>
	public static Display GetDisplay(ushort Width, ushort Height)
	{
		if (Multiboot2.IsVBEAvailable)
		{
			return new VBECanvas();
		}
	    if (VMTools.IsVMWare)
		{
			return new SVGAIICanvas(Width, Height);
		}
		

		throw new NotImplementedException("No display is available!");
	}

	/// <summary>
	/// Gets the display driver's name.
	/// </summary>
	/// <returns>the display name.</returns>
	public abstract string GetName();

	/// <summary>
	/// Coppies the second buffer to the primary display buffer.
	/// </summary>
	public abstract void Update();

	/// <summary>
	/// Gets the FPS measurment of the display.
	/// </summary>
	/// <returns>The FPS as a uint number.</returns>
	public uint GetFPS()
	{
		return _FPS;
	}

    /// <summary>
    /// Disable the display
    /// </summary>
    public virtual void Disable() { }

    /// <summary>
    /// Enable the display
    /// </summary>
    public virtual void Enable() { }
    
	#endregion

    #region Fields

    /// <summary>
    /// The internal frame counter, used for FPS calculation.
    /// </summary>
    internal uint _Frames;

	/// <summary>
	/// The internal FPS value, returned from <see cref="GetFPS()"/>.
	/// </summary>
	internal uint _FPS;

	#endregion
}