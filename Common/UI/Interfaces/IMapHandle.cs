using System.Runtime.Intrinsics;

namespace Common.UI.Interfaces;

public interface IMapHandle
{
    public void NotifyMapLoaded();

    public void SetColor(int[] colors);
}