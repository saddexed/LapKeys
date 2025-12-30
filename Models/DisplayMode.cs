namespace LapKeys.Models;

/// <summary>
/// Represents a display mode with resolution and refresh rate.
/// </summary>
public class DisplayMode
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRate { get; set; }
    public int BitsPerPixel { get; set; }

    public override string ToString()
    {
        return $"{Width}x{Height} @ {RefreshRate}Hz";
    }

    public override bool Equals(object? obj)
    {
        if (obj is DisplayMode other)
        {
            return Width == other.Width &&
                   Height == other.Height &&
                   RefreshRate == other.RefreshRate &&
                   BitsPerPixel == other.BitsPerPixel;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height, RefreshRate, BitsPerPixel);
    }
}
