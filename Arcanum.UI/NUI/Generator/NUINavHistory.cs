using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.UI.NUI.Generator;

public class NUINavHistory
{
   /// <summary>
   /// Creates a new <see cref="NUINavHistory"/> instance. <br/>
   /// Automatically registers the instance in <see cref="NUINavigation"/>. <br/>
   /// </summary>
   /// <param name="target"></param>
   /// <param name="generateSubViews"></param>
   /// <param name="root"></param>
   public NUINavHistory(INUI target,
                        bool generateSubViews,
                        ContentPresenter root)
   {
      Target = target;
      GenerateSubViews = generateSubViews;
      Root = root;
      
      NUINavigation.Instance.Navigate(this);
   }

   public INUI Target { get; }
   public bool GenerateSubViews { get; }
   public ContentPresenter Root { get; }
   
   public override bool Equals(object? obj)
   {
      if (obj is not NUINavHistory other)
         return false;

      return Target.Equals(other.Target) && GenerateSubViews == other.GenerateSubViews &&
             Root.Equals(other.Root);
   }
   
   public override int GetHashCode()
   {
      return HashCode.Combine(Target, GenerateSubViews, Root);
   }
}