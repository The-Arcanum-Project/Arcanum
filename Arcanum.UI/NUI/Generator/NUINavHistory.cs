using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

public class NUINavHistory
{
   /// <summary>
   /// Creates a new <see cref="NUINavHistory"/> instance. <br/>
   /// Automatically registers the instance in <see cref="NUINavigation"/>. <br/>
   /// </summary>
   /// <param name="targets"></param>
   /// <param name="generateSubViews"></param>
   /// <param name="root"></param>
   public NUINavHistory(IReadOnlyList<INUI> targets,
                        bool generateSubViews,
                        ContentPresenter root)
   {
      if (targets == null || targets.Count == 0)
         throw new ArgumentException("Target list cannot be null or empty.", nameof(targets));

      var firstType = targets[0].GetType();
      if (targets.Any(t => t.GetType() != firstType))
         throw new ArgumentException("All targets in a multi-select view must be of the same type.", nameof(targets));

      Targets = targets;
      GenerateSubViews = generateSubViews;
      Root = root;

      NUINavigation.Instance.Navigate(this);
   }

   public NUINavHistory(INUI target, bool generateSubViews, ContentPresenter root)
      : this([target], generateSubViews, root)
   {
   }

   public IReadOnlyList<INUI> Targets { get; }
   public bool GenerateSubViews { get; }
   public ContentPresenter Root { get; }
   public INUI PrimaryTarget => Targets[0];

   public INUINavigation[] GetNavigations()
   {
      if (Targets.Count == 1)
         return Targets[0].Navigations;

      List<INUINavigation> combinedNavigations = [];
      foreach (var target in Targets)
         combinedNavigations.Add(new Core.CoreSystems.NUI.NUINavigation(target, target.ToString() ?? "??object"));
      
      return combinedNavigations.Distinct().ToArray();
   }

   public override bool Equals(object? obj)
   {
      if (obj is not NUINavHistory other)
         return false;

      return Targets.SequenceEqual(other.Targets) && Root.Equals(other.Root);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Targets, GenerateSubViews, Root);
   }
}