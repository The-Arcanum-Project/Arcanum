using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Nexus.Core;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

using System.ComponentModel;
using System.Windows.Controls;

public class PropertyEditorViewModel
   : INotifyPropertyChanged
{
   private bool _isExpanded;
   private Grid? _expandableContentGrid;

   public PropertyEditorViewModel(Enum nxProp, NavH navH, IEu5Object target)
   {
      NxProp = nxProp;
      NavH = navH;
      Target = target;
      object embedded = null!;
      Nx.ForceGet(target, nxProp, ref embedded);
      Debug.Assert(embedded is IEu5Object, "EmbeddedView can only display IEu5Object values.");
      Embedded = (IEu5Object)embedded;

      var type = target.GetNxPropType(nxProp);
      Debug.Assert(type != null, "type != null");

      var itemObj = (IEu5Object)EmptyRegistry.Empties[type];

      IsExpanded = !Config.Settings.NUIConfig.StartEmbeddedFieldsCollapsed;
   }

   public Enum NxProp { get; }
   public NavH NavH { get; }
   public IEu5Object Target { get; set; }
   public IEu5Object Embedded { get; }
   private bool _hasRefreshedContent;

   public bool IsExpanded
   {
      get => _isExpanded;
      set
      {
         if (_isExpanded == value)
            return;

         _isExpanded = value;
         OnPropertyChanged(nameof(IsExpanded));

         if (_isExpanded && !_hasRefreshedContent)
            RefreshContent();

         Eu5UiGen.IsExpandedCache[NxProp] = _isExpanded;
      }
   }

   public Grid? ExpandableContentGrid
   {
      get => _expandableContentGrid;
      set
      {
         _expandableContentGrid = value;
         OnPropertyChanged(nameof(ExpandableContentGrid));
      }
   }

   public void RefreshContent()
   {
      var newGrid = new Grid
      {
         Margin = new(4, 4, 0, 4),
         ColumnDefinitions =
         {
            new() { Width = new(2, GridUnitType.Star) }, new() { Width = new(3, GridUnitType.Star) },
         },
      };

      object value = null!;
      Nx.ForceGet(Target, NxProp, ref value);
      Debug.Assert(value is IEu5Object, "EmbeddedView can only display IEu5Object values.");

      Eu5UiGen.PopulateEmbeddedGrid(newGrid, NavH, (IEu5Object)value, NxProp);

      ExpandableContentGrid = newGrid;
      _hasRefreshedContent = true;
   }

   public event PropertyChangedEventHandler? PropertyChanged;
   protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
}