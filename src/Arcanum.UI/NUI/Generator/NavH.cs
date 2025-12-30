using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.NUI.Generator;

public class NavH
{
   /// <summary>
   /// Creates a new <see cref="NavH"/> instance. <br/>
   /// Automatically registers the instance in <see cref="NUINavigation"/>. <br/>
   /// </summary>
   public NavH(List<IEu5Object> targets,
               bool generateSubViews,
               ContentPresenter root,
               bool navigate)
   {
      if (targets == null || targets.Count == 0)
         throw new ArgumentException("Target list cannot be null or empty.", nameof(targets));

#if DEBUG
      var firstType = targets[0].GetType();
      if (targets.Any(t => t.GetType() != firstType))
         throw new ArgumentException("All targets in a multi-select view must be of the same type.", nameof(targets));
#endif

      Targets = targets;
      GenerateSubViews = generateSubViews;
      Root = root;

      if (navigate)
         NUINavigation.Instance.Navigate(this);
   }

   public NavH(IEu5Object target, bool generateSubViews, ContentPresenter root, bool navigate)
      : this([target], generateSubViews, root, navigate)
   {
   }

   public List<IEu5Object> Targets { get; }
   public bool GenerateSubViews { get; }
   public ContentPresenter Root { get; }

   public INUINavigation[] GetNavigations()
   {
      if (Targets.Count == 1)
         return Targets[0].Navigations!;

      List<INUINavigation> combinedNavigations = [];
      foreach (var target in Targets)
         combinedNavigations.Add(new Core.CoreSystems.NUI.NUINavigation(target, target.ToString() ?? "??object"));

      return combinedNavigations.Distinct().ToArray();
   }

   public override bool Equals(object? obj)
   {
      if (obj is not NavH other)
         return false;

      return Targets.SequenceEqual(other.Targets) && Root.Equals(other.Root);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Targets, GenerateSubViews, Root);
   }

   public void NavigateTo(IEu5Object? navTarget, bool subViews = true)
   {
      if (navTarget == null)
         return;

      var nav = new NavH(navTarget, subViews, Root, true);
      NUINavigation.GenerateUi(nav);
   }

   public override string ToString() => $"NavH: {string.Join(", ", Targets.Select(t => t.ToString()))}";
}