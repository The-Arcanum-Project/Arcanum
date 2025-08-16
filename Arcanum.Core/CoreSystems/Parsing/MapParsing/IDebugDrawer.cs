namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public interface IDebugDrawer
{
    public void DrawLine(int x1, int y1, int x2, int y2);

    public void DrawNode(int x, int y, int color = unchecked((int)0xFFFF0000));
}