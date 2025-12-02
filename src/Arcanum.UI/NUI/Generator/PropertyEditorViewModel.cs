using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.NUI.UserControls.BaseControls;

namespace Arcanum.UI.NUI.Generator;

public sealed class PropertyEditorViewModel
   : INotifyPropertyChanged, IDisposable
{
   private bool _isExpanded;
   private readonly bool _isInline;
   private Grid? _expandableContentGrid;
   private readonly BaseView _view;

   public PropertyEditorViewModel(BaseView view, Enum nxProp, NavH navH, IEu5Object target, bool isInline)
   {
      _view = view;
      view.RegisterDisposable(this);
      _isInline = isInline;
      NxProp = nxProp;
      NavH = navH;
      Target = target;
      var embedded = Target._getValue(nxProp);
      Debug.Assert(embedded is IEu5Object, "EmbeddedView can only display IEu5Object values.");
      Embedded = (IEu5Object)embedded;

      var type = target.GetNxPropType(nxProp);
      Debug.Assert(type != null, "type != null");
      IsExpanded = !Config.Settings.NUIConfig.StartEmbeddedFieldsCollapsed;
   }

   public Enum NxProp { get; }
   public NavH NavH { get; }
   public IEu5Object Target { get; set; }
   public IEu5Object Embedded { get; }
   private bool _hasRefreshedContent;

   public bool IsExpanded
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged(nameof(IsExpanded));

         if (field && !_hasRefreshedContent)
            RefreshContent();

         Eu5UiGen.IsExpandedCache[NxProp] = field;
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
      _expandableContentGrid?.Children.Clear();

      var newGrid = new Grid
      {
         Margin = new(4, 4, 0, 4),
         ColumnDefinitions =
         {
            new() { Width = new(2, GridUnitType.Star) }, new() { Width = new(3, GridUnitType.Star) },
         },
      };

      var targets = NavH.Targets;

      if (_isInline)
      {
         targets = [];
         foreach (var t in NavH.Targets)
            targets.Add(t._getValue(NxProp) as IEu5Object ?? throw new InvalidOperationException());
      }

      Eu5UiGen.PopulateEmbeddedGrid(_view,
                                    newGrid,
                                    NavH,
                                    _isInline ? targets : targets.Select(x => (IEu5Object)x._getValue(NxProp)).ToList(),
                                    (IEu5Object)Target._getValue(NxProp),
                                    NxProp);

      ExpandableContentGrid = newGrid;
      _hasRefreshedContent = true;
   }

   public void Dispose()
   {
      if (_expandableContentGrid == null)
         return;

      _expandableContentGrid.Children.Clear();
      _expandableContentGrid = null;
   }

   public event PropertyChangedEventHandler? PropertyChanged;
   private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
}