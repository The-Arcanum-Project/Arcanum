using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Nexus.Core;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class Eu5UiGen
{
   public static Dictionary<Type, Func<object, Enum, int, int, FrameworkElement>> CustomShortInfoGenerators = new()
   {
      { typeof(ModValInstance), Nui2Gen.CustomShortInfoGenerators.GetModValInstanceShortInfo },
   };

   public static Dictionary<Type, RoutedEventHandler> CustomCollectionEditors = new() { };

   public static void GenerateAndSetView(NavH navh)
   {
      navh.Root.Content = GenerateView(navh);
   }

   public static BaseView GenerateView(NavH navh)
   {
      var primary = navh.Targets[0];

      var view = ControlFactory.GetBaseView();
      view.BaseViewBorder.BorderThickness = new(0);
      var mainGrid = ControlFactory.GetMainGrid();
      view.BaseViewBorder.Child = mainGrid;

      GridManager.SetPureHeader(mainGrid, primary, navh.Targets.Count, 0, 0, 3);

      GenerateViewElements(navh, mainGrid, primary);

      return view;
   }

   private static void GenerateViewElements(NavH navH, Grid mainGrid, IEu5Object primary)
   {
      var viewFields = primary.NUISettings.ViewFields;
      if (Config.Settings.NUIConfig.ListViewsInCustomOrder)
         viewFields = viewFields.OrderBy(f => f.ToString()).ToArray();

      for (var i = 0; i < viewFields.Length; i++)
      {
         var nxProp = viewFields[i];
         var nxPropType = primary.GetNxPropType(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(nxPropType) || typeof(IEu5Object) == nxPropType)
         {
            if (navH.GenerateSubViews)
            {
               if (navH.Targets.Count > 1)
               {
                  // TODO: @Minnator not supported Yet.
               }
               else
               {
                  GenerateEmbeddedView(navH, primary, mainGrid, nxProp, i + 1);
               }
            }
            else
            {
               GenerateShortInfo(navH, primary, mainGrid);
            }
         }
         else
         {
            BuildCollectionViewOrDefault(navH, primary, mainGrid, nxProp, nxPropType, i + 1);
         }
      }
   }

   private static void BuildCollectionViewOrDefault(NavH navH,
                                                    IEu5Object primary,
                                                    Grid mainGrid,
                                                    Enum nxProp,
                                                    Type nxPropType,
                                                    int rowIndex)
   {
      var itemType = primary.GetNxItemType(nxProp);
      var nxType = primary.GetNxPropType(nxProp);

      if (itemType == null)
      {
         // We have a default property, not a collection.
         GetTypeSpecificGrid(navH, primary, nxProp, mainGrid, rowIndex);
         return;
      }

      if (navH.Targets.Count > 1)
      {
         GetNotSupportedForMultiSelect(nxProp, mainGrid);
         return;
      }

      object collection = null!;
      Nx.ForceGet(primary, nxProp, ref collection);

      if (collection is not IList modifiableList)
      {
         // We have a collection property, but it's not a list we can iterate with our current implementation.
         GetTypeSpecificGrid(navH, primary, nxProp, mainGrid, rowIndex);
         return;
      }

      var collectionGrid = ControlFactory.GetCollectionGrid();
      SetCollectionHeaderPanel(nxProp, itemType, modifiableList, collectionGrid, primary, 0);
      GetCollectionPreview(navH,
                           primary,
                           collectionGrid,
                           nxProp,
                           modifiableList,
                           rowIndex);

      GetCollectionLiner(collectionGrid);
      GridManager.AddToGrid(mainGrid, collectionGrid, rowIndex, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void SetCollectionHeaderPanel(Enum nxProp,
                                                Type itemType,
                                                IList modifiableList,
                                                Grid mainGrid,
                                                IEu5Object primary,
                                                int row)
   {
      var headerGrid = new Grid
      {
         Margin = new(0, 0, 0, 2),
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         ColumnDefinitions =
         {
            new() { Width = GridLength.Auto },
            new() { Width = GridLength.Auto },
            new() { Width = new(1, GridUnitType.Star) },
            new() { Width = GridLength.Auto },
         },
      };

      GetCollectionTitleTextBox(modifiableList.Count, nxProp, headerGrid, primary, 0, 14, column: 0);
      GetCollectionEditorButton(primary, nxProp, itemType, modifiableList, headerGrid, 0, column: 1);
      GetInferActionButtons(primary, nxProp, primary.GetNxPropType(nxProp), itemType, modifiableList, headerGrid, 0, 3);

      GridManager.AddToGrid(mainGrid, headerGrid, row, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GetInferActionButtons(IEu5Object primary,
                                             Enum nxProp,
                                             Type nxPropType,
                                             Type? nxItemType,
                                             IList collection,
                                             Grid grid,
                                             int row,
                                             int column)
   {
      // Infer actions are disabled globally
      if (Config.Settings.NUIConfig.DisableNUIInferFromMapActions)
         return;

      var targetType = nxItemType ?? nxPropType;
      Debug.Assert(targetType != null, "targetType != null");

      var inferableInterface = typeof(IMapInferable<>).MakeGenericType(targetType);
      if (!inferableInterface.IsAssignableFrom(targetType))
         return;

      if (!EmptyRegistry.TryGet(targetType, out var empty))
         return;

      var actionsPanel = new StackPanel
      {
         Orientation = Orientation.Horizontal,
         HorizontalAlignment = HorizontalAlignment.Right,
         VerticalAlignment = VerticalAlignment.Center,
         Margin = new(0, 0, 4, 0),
      };
   }

   private static void GetCollectionEditorButton(IEu5Object parent,
                                                 Enum property,
                                                 Type nxItemType,
                                                 IList collection,
                                                 Grid grid,
                                                 int rowIndex,
                                                 int column)
   {
      var eyeButton = NEF.GetEyeButton();
      eyeButton.Margin = new(4, 0, 0, 0);
      RoutedEventHandler clickHandler;

      if (nxItemType.IsAssignableFrom(typeof(IEu5Object)))
      {
         clickHandler = (_, _) =>
         {
            var allItems = ((IEu5Object)EmptyRegistry.Empties[nxItemType]).GetGlobalItemsNonGeneric().Values;
            DualListSelector.CreateWindow(allItems, collection, $"Edit {property} - {parent.UniqueId}").ShowDialog();
         };
      }
      else
      {
         if (CustomCollectionEditors.TryGetValue(nxItemType, out var customHandler))
            clickHandler = customHandler;
         else
            clickHandler = (_, _) =>
            {
               PrimitiveTypeListView.ShowDialog(collection, collection, $"Edit {property} - {parent.UniqueId}");
            };
      }

      eyeButton.Click += clickHandler;
      eyeButton.Unloaded += (_, _) => eyeButton.Click -= clickHandler;
      eyeButton.ToolTip = $"Open collection editor for {property}";

      GridManager.AddToGrid(grid, eyeButton, rowIndex, column, 0, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GetCollectionLiner(Grid grid)
   {
      var border = new Border
      {
         BorderBrush = ControlFactory.AccentBrush,
         BorderThickness = new(2, 0, 0, 2),
         Margin = new(-2, 5, 0, 0),
         Width = 15,
         VerticalAlignment = VerticalAlignment.Stretch,
         HorizontalAlignment = HorizontalAlignment.Left,
      };
      grid.Children.Add(border);
      Grid.SetRow(border, 0);
      Grid.SetColumn(border, 0);
      Grid.SetRowSpan(border, int.MaxValue);
   }

   private static void GetCollectionPreview(NavH navH,
                                            IEu5Object primary,
                                            Grid grid,
                                            Enum nxProp,
                                            IList modifiableList,
                                            int rowIndex)
   {
      var itemType = primary.GetNxItemType(nxProp);

      if (itemType == null || modifiableList.Count == 0)
         return;

      for (var i = 0; i < Math.Min(modifiableList.Count, Config.Settings.NUIConfig.MaxCollectionItemsPreviewed); i++)
      {
         if (modifiableList[i] is IEu5Object eu5Obj)
         {
            var ui = Nui2Gen.CustomShortInfoGenerators.GenerateEu5ShortInfo(navH,
                                                                            nxProp,
                                                                            eu5Obj,
                                                                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                                            ControlFactory.SHORT_INFO_FONT_SIZE);
            GridManager.AddToGrid(grid, ui, rowIndex + i, 0, 0, ControlFactory.SHORT_INFO_ROW_HEIGHT);
         }
         else
         {
            if (CustomShortInfoGenerators.TryGetValue(itemType, out var generator))
            {
               var ui = generator(modifiableList[i]!,
                                  nxProp,
                                  ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                  ControlFactory.SHORT_INFO_FONT_SIZE);
               GridManager.AddToGrid(grid, ui, rowIndex + i, 0, 0, ControlFactory.SHORT_INFO_ROW_HEIGHT);
            }
            else
            {
               // TODO: @Minnator Fallback window
            }
         }
      }

      if (modifiableList.Count > Config.Settings.NUIConfig.MaxCollectionItemsPreviewed)
      {
         var moreText = ControlFactory.GetHeaderTextBlock(ControlFactory.SHORT_INFO_FONT_SIZE,
                                                          false,
                                                          $"  ... and {modifiableList.Count - Config.Settings.NUIConfig.MaxCollectionItemsPreviewed} more",
                                                          height: ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                          alignment: HorizontalAlignment.Left);
         moreText.FontStyle = FontStyles.Italic;
         GridManager.AddToGrid(grid,
                               moreText,
                               rowIndex + Config.Settings.NUIConfig.MaxCollectionItemsPreviewed,
                               0,
                               0,
                               ControlFactory.SHORT_INFO_ROW_HEIGHT);
      }
   }

   private static void GetCollectionTitleTextBox(int count,
                                                 Enum nxProp,
                                                 Grid grid,
                                                 IEu5Object primary,
                                                 int rowIndex,
                                                 int fontSize = ControlFactory.SHORT_INFO_FONT_SIZE,
                                                 int column = 0)
   {
      var text = $"{nxProp.ToString()} ({count})";
      var tb = ControlFactory.GetHeaderTextBlock(fontSize,
                                                 false,
                                                 text,
                                                 height: ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                 alignment: HorizontalAlignment.Left);

      GridManager.AddToGrid(grid, tb, rowIndex, column, 0, ControlFactory.SHORT_INFO_ROW_HEIGHT);

      SetCollectionToolTip(primary, nxProp, tb);
   }

   private static void SetCollectionToolTip(IEu5Object primary, Enum nxProp, TextBlock tb)
   {
      var desc = primary.GetDescription(nxProp);
      if (string.IsNullOrWhiteSpace(desc))
         return;

      var toolTip = new ToolTip
      {
         Content = desc, MaxWidth = 400,
      };
      tb.ToolTip = toolTip;
   }

   private static void GetNotSupportedForMultiSelect(Enum nxProp, Grid mainGrid)
   {
      var text = $"{nxProp} (Not supported in multi-select views)";
   }

   private static void GetTypeSpecificGrid(NavH navH,
                                           IEu5Object primary,
                                           Enum nxProp,
                                           Grid mainGrid,
                                           int rowIndex,
                                           int leftMargin = 0)
   {
      var type = primary.GetNxPropType(nxProp);

      var propertyViewModel = new MultiSelectPropertyViewModel(navH.Targets, nxProp);

      var binding = new Binding(nameof(propertyViewModel.Value))
      {
         Source = propertyViewModel,
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };

      Control element;

      if (type == typeof(float))
         element = NEF.GetFloatUI(binding);
      else if (type == typeof(string))
         element = NEF.GetStringUI(binding);
      else if (type == typeof(bool))
         element = NEF.GetBoolUI(binding);
      else if (type.IsEnum)
         element = NEF.GetEnumUI(type, binding);
      else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
         element = NEF.GetIntUI(binding);
      else if (type == typeof(double) || type == typeof(decimal))
         element = NEF.GetDoubleUI(binding);
      else if (type == typeof(JominiColor))
      {
         var temp = JominiColor.Empty;
         Nx.ForceGet(primary, nxProp, ref temp);
         element = NEF.GetJominiColorUI(binding, temp);
      }
      else if (type == typeof(object))
         element = NEF.GetStringUI(binding);
      else
         throw new NotSupportedException($"Type {type} is not supported for property {nxProp}.");

      element.IsEnabled = !primary.IsReadonly;
      element.VerticalAlignment = VerticalAlignment.Stretch;
      element.Height = ControlFactory.SHORT_INFO_ROW_HEIGHT;

      SetTooltipIsAny(primary, nxProp, element);

      var desc = NEF.DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);

      SetTooltipIsAny(primary, nxProp, desc);

      GetInferActionButtons(primary, nxProp, type, primary.GetNxItemType(nxProp), new List<string>(), new(), 0, 1);

      var line = NEF.GenerateDashedLine(leftMargin);
      RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

      var grid = NEF.CreateGridForProperty();
      grid.Children.Add(desc);
      Grid.SetRow(desc, 0);
      Grid.SetColumn(desc, 0);

      grid.Children.Add(line);
      Grid.SetRow(line, 0);
      Grid.SetColumn(line, 0);

      grid.Children.Add(element);
      Grid.SetRow(element, 0);
      Grid.SetColumn(element, 1);
      //
      // if (inferActions != null)
      // {
      //    grid.Children.Add(inferActions);
      //    Grid.SetRow(inferActions, 0);
      //    Grid.SetColumn(inferActions, 0);
      // }

      GridManager.AddToGrid(mainGrid, grid, rowIndex, 0, 3, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void SetTooltipIsAny(IAgs iAgs, Enum nxProp, UIElement element)
   {
   }

   private static void GenerateEmbeddedView(NavH navH, IEu5Object primary, Grid mainGrid, Enum nxProp, int rowIndex)
   {
   }

   private static void GenerateShortInfo(NavH navH, IEu5Object primary, Grid mainGrid)
   {
   }
}