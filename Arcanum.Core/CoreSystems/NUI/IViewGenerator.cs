
namespace Arcanum.Core.CoreSystems.NUI;

public interface IViewGenerator
{
   public NUIUserControl GenerateView(INUI target,
                                      Enum title,
                                      Enum subTitle,
                                      Enum[] nxProps,
                                      bool generateSubViews);
}