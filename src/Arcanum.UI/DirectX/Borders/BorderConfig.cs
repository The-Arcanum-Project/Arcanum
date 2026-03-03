namespace Arcanum.UI.DirectX.Borders;

public static class BorderConfig
{
    public const int BORDER_ATLAS_WIDTH = 128;
    public const int SLICE_SIZE = 16;
    public const int NUMBER_OF_BORDER_TYPES = 8;
    public const string BORDER_ATLAS_PATH = "Arcanum.UI.DirectX.Borders.BorderAtlas.png";
}

public enum BorderType
{
    Single,
    Alternating,
    Dotted,
    Bead,
    Thin,
    Double,
    DoubleDashed,
    Dashed,
}
