using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.GraphDisplay;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Common.UI;
using Common.UI.MBox;
using Nexus.Core;
using Binding = System.Windows.Data.Binding;
using Control = System.Windows.Controls.Control;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public static class Eu5UiGen
{
   public readonly static Dictionary<Type, Func<object, Enum, int, int, FrameworkElement>> CustomShortInfoGenerators =
      new() { { typeof(ModValInstance), Nui2Gen.CustomShortInfoGenerators.GetModValInstanceShortInfo }, };

   public readonly static Dictionary<Type, RoutedEventHandler> CustomCollectionEditors = new();

   public readonly static Dictionary<Type, Func<object, Enum, FrameworkElement>> CustomTypeButtons = new();

   public readonly static Dictionary<Type, Func<object, Enum, FrameworkElement>> CustomItemTypeButtons = new();

   public readonly static Dictionary<Type, Func<Binding, int, int, Control>> CustomUiGenerators = new();

   public static readonly Dictionary<Enum, bool> IsExpandedCache = new();

   public static void GenerateAndSetView(NavH navh, List<Enum>? markedProps = null!, bool hasHeader = true)
   {
      navh.Root.Content = GenerateView(navh, markedProps ?? [], hasHeader);
   }

   public static BaseView GenerateView(NavH navh,
                                       List<Enum> markedProps,
                                       bool hasHeader = true,
                                       bool allowReadOnlyEditing = false)
   {
      var primary = navh.Targets[0];
      var view = ControlFactory.GetBaseView();
      view.BaseViewBorder.BorderThickness = new(0);
      var mainGrid = ControlFactory.GetMainGrid();
      view.BaseViewBorder.Child = mainGrid;

      if (hasHeader)
      {
         SetStatusEllipse(mainGrid, primary, 0, 0, 0);
         GridManager.SetPureHeader(mainGrid, primary, navh.Targets.Count, 0, 0, 3);
      }

      GenerateViewElements(navh, mainGrid, primary, markedProps, allowReadOnlyEditing, hasHeader ? 1 : 0);

      return view;
   }

   // Depending on the editing state we will put in a green, yellow or red ellipse on the left vertically centered
   private static void SetStatusEllipse(Grid mainGrid, IEu5Object primary, int row, int column, int columnSpan)
   {
      var state = SaveMaster.GetState(primary);

      var ellipse = new Ellipse
      {
         Width = 8,
         Height = 8,
         VerticalAlignment = VerticalAlignment.Center,
         HorizontalAlignment = HorizontalAlignment.Left,
         Margin = new(4, 0, 0, 0),
      };

      ellipse.ToolTip = state switch
      {
         ObjState.Unchanged => $"{primary.UniqueId} is Unchanged",
         ObjState.Modified => $"{primary.UniqueId} is Modified",
         ObjState.New => $"{primary.UniqueId} is New",
         _ => "Unknown State",
      };

      ellipse.Fill = state switch
      {
         ObjState.Unchanged => Brushes.Green,
         ObjState.Modified => Brushes.Yellow,
         ObjState.New => Brushes.Blue,
         _ => Brushes.Gray
      };

      GridManager.AddToGrid(mainGrid, ellipse, row, column, columnSpan, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GenerateViewElements(NavH navH,
                                            Grid mainGrid,
                                            IEu5Object primary,
                                            List<Enum> markedProps,
                                            bool allowReadOnlyEditing,
                                            int startRow = 1)
   {
      var viewFields = SortViewFieldsByConfig(primary, markedProps);

      for (var i = 0; i < viewFields.Length; i++)
      {
         var nxProp = viewFields[i];
         var nxPropType = primary.GetNxPropType(nxProp);

         var isMarked = markedProps.Contains(nxProp);

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
                  GenerateEmbeddedView(navH, primary, mainGrid, nxProp, i + startRow, isMarked);
               }
            }
            else
            {
               GenerateShortInfo(navH, primary, mainGrid, isMarked);
            }
         }
         else
         {
            BuildCollectionViewOrDefault(navH,
                                         primary,
                                         mainGrid,
                                         nxProp,
                                         i + startRow,
                                         isMarked: isMarked,
                                         allowReadOnlyEditing: allowReadOnlyEditing);
         }
      }
   }

   private static Enum[] SortViewFieldsByConfig(IEu5Object primary, List<Enum> markedProps)
   {
      var viewFields = primary.NUISettings.ViewFields.Except(markedProps);
      if (Config.Settings.NUIConfig.ListViewsInCustomOrder)
         viewFields = viewFields.OrderBy(f => f.ToString()).ToArray();

      if (markedProps.Count <= 0)
         return viewFields.ToArray();

      markedProps = markedProps.OrderBy(f => f.ToString()).ToList();
      viewFields = markedProps.Concat(viewFields);

      return viewFields.ToArray();
   }

   private static void GenerateEmbeddedView(NavH navH,
                                            IEu5Object primary,
                                            Grid mainGrid,
                                            Enum nxProp,
                                            int rowIndex,
                                            bool isMarked)
   {
      var pevm = new PropertyEditorViewModel(nxProp, navH, primary);
      var ebv = new EmbeddedView(pevm)
      {
         MinHeight = ControlFactory.EMBEDDED_VIEW_HEIGHT, Margin = new(0, 4, 0, 4),
      };
      GridManager.AddToGrid(mainGrid, ebv, rowIndex, 0, 2, ControlFactory.EMBEDDED_VIEW_HEIGHT);
      if (IsExpandedCache.TryGetValue(nxProp, out var isExpanded) && isExpanded)
         ebv.ViewModel.IsExpanded = true;

      object em = null!;
      Nx.ForceGet(primary, nxProp, ref em);
      var embedded = em as IEu5Object ??
                     throw new InvalidOperationException($"Property {nxProp} is not an embedded IEu5Object.");

      var header = GridManager.GetNavigationHeader(embedded,
                                                   navH,
                                                   nxProp.ToString(),
                                                   ControlFactory.SHORT_INFO_FONT_SIZE + 2,
                                                   ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                   true);

      if (isMarked)
         header.Background = ControlFactory.MarkedBrush;

      ebv.TitleDockPanel.Children.Add(header);
      DockPanel.SetDock(header, Dock.Left);

      var setButton = NEF.CreateSetButton();
      ebv.TitleDockPanel.Children.Add(setButton);
      DockPanel.SetDock(setButton, Dock.Right);
      setButton.ToolTip = $"Set the '{nxProp}' property for all selected objects to the value inferred from the map.";

      AddMapModeButtonToPanel(embedded, ebv.TitleDockPanel, embedded.GetType(), Dock.Right);
      AddCustomButtonToPanel(embedded, nxProp, ebv.TitleDockPanel, embedded.GetType(), Dock.Right);
      CreateGraphViewerButton(embedded, navH, ebv.TitleDockPanel);
      AddCreateNewEu5ObjectButton(primary, nxProp, ebv.TitleDockPanel, Dock.Right);

      RoutedEventHandler setClick = (_, _) =>
      {
         var inferred = MapInferrableRegistry.GetInferredList(primary.GetType(), Selection.SelectedLocations);
         if (inferred == null)
            return;

         if (inferred.Count < 1)
            return;

         Nx.ForceSet(inferred[0], primary, nxProp);
         // We don't? need this as the PropertyEditorViewModel handles updating all targets.
         // foreach (var obj in navH.Targets)
         //    Nx.Set(obj, nxProp, inferred[0]);
         if (inferred.Count > 1)
            UIHandle.Instance.PopUpHandle
                    .ShowMBox($"Multiple inferred values found for {nxProp}. Using the first one ({inferred[0]}).",
                              "Multiple inferred values");
      };

      setButton.Click += setClick;
      setButton.Unloaded += (_, _) => setButton.Click -= setClick;
   }

   public static void PopulateEmbeddedGrid(Grid grid, NavH navH, IEu5Object embedded, Enum parentProp)
   {
      for (var index = 0; index < embedded.NUISettings.EmbeddedFields.Length; index++)
      {
         var nxProp = embedded.NUISettings.EmbeddedFields[index];
         var nxPropType = embedded.GetNxPropType(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(nxPropType) || typeof(IEu5Object) == nxPropType)
         {
            object embeddedValue = null!;
            Nx.ForceGet(embedded, nxProp, ref embeddedValue);
            var ui = Nui2Gen.CustomShortInfoGenerators.GenerateEu5ShortInfo(new(embedded, false, navH.Root),
                                                                            (IEu5Object)embeddedValue,
                                                                            nxProp,
                                                                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                                            ControlFactory.SHORT_INFO_FONT_SIZE,
                                                                            0,
                                                                            2);

            GridManager.AddToGrid(grid, ui, 1 + index, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
            continue;
         }

         BuildCollectionViewOrDefault(navH, embedded, grid, nxProp, index + 1, parentProp: parentProp, isMarked: false);
      }
   }

   private static void BuildCollectionViewOrDefault(NavH navH,
                                                    IEu5Object primary,
                                                    Grid mainGrid,
                                                    Enum nxProp,
                                                    int rowIndex,
                                                    bool isMarked,
                                                    bool allowReadOnlyEditing = false,
                                                    Enum? parentProp = null)
   {
      var itemType = primary.GetNxItemType(nxProp);

      if (itemType == null)
      {
         // We have a default property, not a collection.
         GetTypeSpecificUI(navH,
                           primary,
                           nxProp,
                           mainGrid,
                           rowIndex,
                           isMarked: isMarked,
                           embeddedPropertyTargets: parentProp,
                           leftMargin: 6,
                           allowReadOnlyEditing: allowReadOnlyEditing);
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
         GetTypeSpecificUI(navH,
                           primary,
                           nxProp,
                           mainGrid,
                           rowIndex,
                           isMarked: isMarked,
                           embeddedPropertyTargets: parentProp,
                           allowReadOnlyEditing: allowReadOnlyEditing);
         return;
      }

      var margin = 8;
      var collectionGrid = ControlFactory.GetCollectionGrid();
      SetCollectionHeaderPanel(nxProp, itemType, modifiableList, collectionGrid, primary, 0, margin, isMarked, navH);
      GetCollectionPreview(navH,
                           primary,
                           collectionGrid,
                           nxProp,
                           modifiableList,
                           1,
                           margin);

      if (modifiableList.Count > 0)
         GetCollectionLiner(collectionGrid, margin);
      GridManager.AddToGrid(mainGrid, collectionGrid, rowIndex, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void SetCollectionHeaderPanel(Enum nxProp,
                                                Type itemType,
                                                IList modifiableList,
                                                Grid mainGrid,
                                                IEu5Object primary,
                                                int row,
                                                int leftMargin,
                                                bool isMarked,
                                                NavH navh)
   {
      var headerPanel = NEF.PropertyTitlePanel(leftMargin);

      GetCollectionTitleTextBox(modifiableList.Count,
                                nxProp,
                                headerPanel,
                                primary,
                                leftMargin,
                                isMarked: isMarked,
                                fontSize: 12);
      GetCollectionEditorButton(primary, nxProp, itemType, modifiableList, headerPanel);
      GetInferActionButtons(primary, nxProp, primary.GetNxPropType(nxProp), itemType, headerPanel, navh);

      var marker = GetPropertyMarker(primary, nxProp);

      GridManager.AddToGrid(mainGrid, headerPanel, row, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
      GridManager.AddToGrid(mainGrid, marker, row, 0, 1, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GetInferActionButtons(IEu5Object primary,
                                             Enum nxProp,
                                             Type nxPropType,
                                             Type? nxItemType,
                                             DockPanel panel,
                                             NavH navh)
   {
      // Infer actions are disabled globally
      if (Config.Settings.NUIConfig.DisableNUIInferFromMapActions)
         return;

      var targetType = nxItemType ?? nxPropType;
      Debug.Assert(targetType != null, "targetType != null");

      var inferableInterface = typeof(IMapInferable<>).MakeGenericType(targetType);
      if (!inferableInterface.IsAssignableFrom(targetType))
         return;

      // Infer actions for a collection
      if (nxItemType != null)
      {
         var addButton = NEF.CreateAddButton();
         var removeButton = NEF.CreateRemoveButton();

         RoutedEventHandler addClick = (_, _) =>
         {
            var enumerable = MapInferrableRegistry.GetInferredList(nxItemType, Selection.SelectedLocations);
            Debug.Assert(enumerable != null, "enumerable != null");
            foreach (var obj in enumerable)
               Nx.AddToCollection(primary, nxProp, obj);
         };

         RoutedEventHandler removeClick = (_, _) =>
         {
            var enumerable = MapInferrableRegistry.GetInferredList(nxItemType, Selection.SelectedLocations);
            Debug.Assert(enumerable != null, "enumerable != null");
            foreach (var obj in enumerable)
               Nx.RemoveFromCollection(primary, nxProp, obj);
         };

         panel.Children.Add(addButton);
         panel.Children.Add(removeButton);
         DockPanel.SetDock(addButton, Dock.Right);
         DockPanel.SetDock(removeButton, Dock.Right);

         addButton.Click += addClick;
         removeButton.Click += removeClick;
         addButton.Unloaded += (_, _) => addButton.Click -= addClick;
         removeButton.Unloaded += (_, _) => removeButton.Click -= removeClick;

         if (CustomItemTypeButtons.TryGetValue(nxItemType, out var customButtonFunc))
         {
            var customButton = customButtonFunc(primary, nxProp);
            panel.Children.Add(customButton);
            DockPanel.SetDock(customButton, Dock.Right);
         }
      }

      if (!EmptyRegistry.TryGet(targetType, out var item) && !typeof(IEu5Object).IsAssignableFrom(targetType))
         return;

      // Infer actions for a single embedded object or property
      AddMapModeButtonToPanel((IEu5Object)item, panel, targetType, Dock.Right);
      CreateGraphViewerButton((IEu5Object)item, navh, panel);
      AddCustomButtonToPanel((IEu5Object)item, nxProp, panel, targetType, Dock.Right);
   }

   private static void AddCustomButtonToPanel(IEu5Object primary,
                                              Enum nxProp,
                                              DockPanel panel,
                                              Type targetType,
                                              Dock dock)
   {
      if (CustomTypeButtons.TryGetValue(targetType, out var customTypeButtonFunc))
      {
         var customButton = customTypeButtonFunc(primary, nxProp);
         panel.Children.Add(customButton);
         DockPanel.SetDock(customButton, dock);
      }
   }

   private static void AddCreateNewEu5ObjectButton(IEu5Object primary, Enum nxProp, DockPanel panel, Dock dock)
   {
      var createNewButton = NEF.GetCreateNewButton();

      RoutedEventHandler createNewClick = (_, _) =>
      {
         Type type;
         if (primary.IsCollection(nxProp))
         {
            type = primary.GetNxItemType(nxProp) ??
                   throw new
                      InvalidOperationException($"Property {nxProp} does not have an item type but is a collection.");
         }
         else
         {
            type = primary.GetNxPropType(nxProp) ??
                   throw new InvalidOperationException($"Property {nxProp} does not have a type.");
         }

         Eu5ObjectCreator.ShowPopUp(type,
                                    newObj =>
                                    {
                                       if (primary.IsCollection(nxProp))
                                          Nx.AddToCollection(primary, nxProp, newObj);
                                       else
                                          Nx.ForceSet(newObj, primary, nxProp);

                                       // Find the first parent of type EmbeddedView of the createNewButton and refresh its selector
                                       var parent = VisualTreeHelper.GetParent(createNewButton);
                                       while (parent != null && parent is not EmbeddedView)
                                          parent = VisualTreeHelper.GetParent(parent);

                                       if (parent is EmbeddedView ev)
                                          ev.RefreshSelector();
                                    });
      };

      createNewButton.Click += createNewClick;
      createNewButton.Unloaded += (_, _) => createNewButton.Click -= createNewClick;
      createNewButton.ToolTip =
         $"Create a new {primary.GetNxItemType(nxProp)?.Name ?? primary.GetNxPropType(nxProp).Name} and set it to the '{nxProp}' property.";

      panel.Children.Add(createNewButton);
      DockPanel.SetDock(createNewButton, dock);
   }

   private static void AddMapModeButtonToPanel(IEu5Object primary, DockPanel panel, Type targetType, Dock dock)
   {
      if (typeof(IMapMode).IsAssignableFrom(targetType))
      {
         var mapModeButton = GetMapModeButton(primary);
         panel.Children.Add(mapModeButton);
         DockPanel.SetDock(mapModeButton, dock);
      }
   }

   public static BaseButton GetMapModeButton(IEu5Object primary)
   {
      var mmType = ((IMapMode)primary).Type;
      var mapModeButton = new BaseButton
      {
         Content = "M",
         ToolTip = $"Set to '{mmType.ToString()}' Map Mode",
         Margin = new(1),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
      };
      RoutedEventHandler mapModeButtonClick = (_, _) => { MapModeManager.Activate(mmType); };
      mapModeButton.Click += mapModeButtonClick;
      mapModeButton.Unloaded += (_, _) => mapModeButton.Click -= mapModeButtonClick;
      return mapModeButton;
   }

   private static void GetCollectionEditorButton(IEu5Object parent,
                                                 Enum property,
                                                 Type nxItemType,
                                                 IList collection,
                                                 DockPanel panel)
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

      panel.Children.Add(eyeButton);
   }

   private static void GetCollectionLiner(Grid grid, int margin)
   {
      var border = new Border
      {
         BorderBrush = ControlFactory.AccentBrush,
         BorderThickness = new(1.2, 0, 0, 1),
         Margin = new(margin - 2, 6, 0, -2),
         CornerRadius = new(0, 0, 0, 4),
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
                                            int rowIndex,
                                            int margin)
   {
      var itemType = primary.GetNxItemType(nxProp);

      if (itemType == null || modifiableList.Count == 0)
         return;

      var maxPreviewCount = Config.Settings.NUIConfig.MaxCollectionItemsPreviewed;
      var itemsToPreview = modifiableList.Cast<object>().Take(maxPreviewCount);

      PopulateCollectionGridWithItems(navH, primary, grid, nxProp, itemsToPreview, rowIndex, margin);

      if (modifiableList.Count <= Config.Settings.NUIConfig.MaxCollectionItemsPreviewed)
         return;

      var moreText = ControlFactory.GetHeaderTextBlock(ControlFactory.SHORT_INFO_FONT_SIZE,
                                                       false,
                                                       $"  ... and {modifiableList.Count - Config.Settings.NUIConfig.MaxCollectionItemsPreviewed} more",
                                                       height: ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                       alignment: HorizontalAlignment.Left);
      moreText.FontStyle = FontStyles.Italic;
      moreText.Cursor = Cursors.Hand;
      moreText.TextDecorations = TextDecorations.Underline;
      moreText.Foreground = Brushes.CornflowerBlue;
      moreText.ToolTip = "Click to expand the full collection";

      MouseButtonEventHandler clickHandler = null!;
      clickHandler = (_, _) =>
      {
         moreText.MouseLeftButtonUp -= clickHandler;
         ClearCollectionPreview(grid, rowIndex);
         PopulateCollectionGridWithItems(navH, primary, grid, nxProp, modifiableList, rowIndex, margin);
      };

      moreText.MouseLeftButtonUp += clickHandler;

      GridManager.AddToGrid(grid,
                            moreText,
                            rowIndex + Config.Settings.NUIConfig.MaxCollectionItemsPreviewed,
                            0,
                            0,
                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                            leftMargin: margin + 4);
   }

   private static void ClearCollectionPreview(Grid grid, int startingRow)
   {
      for (var i = grid.Children.Count - 1; i >= 0; i--)
      {
         var child = grid.Children[i];
         if (Grid.GetRow(child) >= startingRow)
            grid.Children.RemoveAt(i);
      }
   }

   private static void PopulateCollectionGridWithItems(NavH navH,
                                                       IEu5Object primary,
                                                       Grid grid,
                                                       Enum nxProp,
                                                       IEnumerable items,
                                                       int rowIndex,
                                                       int margin)
   {
      var itemType = primary.GetNxItemType(nxProp);
      if (itemType == null)
         return;

      var i = 0;
      foreach (var item in items)
      {
         if (item is IEu5Object eu5Obj)
         {
            var ui = Nui2Gen.CustomShortInfoGenerators.GenerateEu5ShortInfo(navH,
                                                                            eu5Obj,
                                                                            nxProp,
                                                                            ControlFactory.SHORT_INFO_ROW_HEIGHT - 4,
                                                                            ControlFactory.SHORT_INFO_FONT_SIZE,
                                                                            margin + 6,
                                                                            0);
            GridManager.AddToGrid(grid, ui, rowIndex + i, 0, 0, ControlFactory.SHORT_INFO_ROW_HEIGHT);
         }
         else
         {
            if (CustomShortInfoGenerators.TryGetValue(itemType, out var generator))
            {
               var ui = generator(item!,
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

         i++;
      }
   }

   private static void GetCollectionTitleTextBox(int count,
                                                 Enum nxProp,
                                                 DockPanel panel,
                                                 IEu5Object primary,
                                                 int margin,
                                                 bool isMarked,
                                                 int fontSize = ControlFactory.SHORT_INFO_FONT_SIZE)
   {
      var text = $"{nxProp.ToString()} ({count})";
      var tb = ControlFactory.GetHeaderTextBlock(fontSize,
                                                 false,
                                                 text,
                                                 height: ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                 alignment: HorizontalAlignment.Left,
                                                 leftMargin: margin);

      if (isMarked)
         tb.Background = ControlFactory.MarkedBrush;

      panel.Children.Add(tb);
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

   private static void GetTypeSpecificUI(NavH navH,
                                         IEu5Object primary,
                                         Enum nxProp,
                                         Grid mainGrid,
                                         int rowIndex,
                                         bool isMarked,
                                         Enum? embeddedPropertyTargets = null,
                                         int leftMargin = 0,
                                         bool allowReadOnlyEditing = false)
   {
      var type = primary.GetNxPropType(nxProp);

      MultiSelectPropertyViewModel propertyViewModel;

      if (embeddedPropertyTargets != null)
      {
         List<IEu5Object> targets = [];
         foreach (var obj in navH.Targets)
         {
            object value = null!;
            Nx.ForceGet(obj, embeddedPropertyTargets, ref value);
            Debug.Assert(value is IEu5Object, "value is IEu5Object");
            targets.Add((IEu5Object)value);
         }

         propertyViewModel = new(targets, nxProp, allowReadOnlyEditing);
      }
      else
         propertyViewModel = new(navH.Targets, nxProp, allowReadOnlyEditing);

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
      else if (type == typeof(JominiDate))
         element = NEF.GetJominiDateUI(binding);
      else if (type == typeof(object))
         element = NEF.GetStringUI(binding);
      else if (CustomUiGenerators.TryGetValue(type, out var generator))
         element = generator(binding,
                             ControlFactory.SHORT_INFO_ROW_HEIGHT,
                             ControlFactory.SHORT_INFO_FONT_SIZE);
      else
      {
         UIHandle.Instance.PopUpHandle.ShowMBox($"Type {type} is not supported for property {nxProp}.",
                                                "Type Not Supported",
                                                MBoxButton.OK,
                                                MessageBoxImage.Warning);
         return;
         // throw new NotSupportedException($"Type {type} is not supported for property {nxProp}.");
      }

      if (!allowReadOnlyEditing && primary.IsReadonly)
         element.IsEnabled = false;
      element.VerticalAlignment = VerticalAlignment.Stretch;
      element.Height = ControlFactory.SHORT_INFO_ROW_HEIGHT;

      SetTooltipIsAny(primary, nxProp, element);

      var desc = NEF.DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);
      if (isMarked)
         desc.Background = ControlFactory.MarkedBrush;

      SetTooltipIsAny(primary, nxProp, desc);

      GetInferActionButtons(primary, nxProp, type, primary.GetNxItemType(nxProp), new(), navH);

      var line = NEF.GenerateDashedLine(leftMargin);
      RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

      var dockPanel = NEF.PropertyTitlePanel(leftMargin);
      dockPanel.Children.Add(desc);

      var propertyMarker = GetPropertyMarker(primary, nxProp);

      GridManager.AddToGrid(mainGrid, dockPanel, rowIndex, 0, 1, ControlFactory.SHORT_INFO_ROW_HEIGHT);
      GridManager.AddToGrid(mainGrid,
                            line,
                            rowIndex,
                            0,
                            1,
                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                            leftMargin: leftMargin);
      GridManager.AddToGrid(mainGrid, element, rowIndex, 1, 1, ControlFactory.SHORT_INFO_ROW_HEIGHT);
      GridManager.AddToGrid(mainGrid, propertyMarker, rowIndex, 0, 1, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static Ellipse GetPropertyMarker(IEu5Object primary, Enum nxProp)
   {
      var value = Nx.ForceGetAs<object>(primary, nxProp);
      var defaultValue = primary.GetDefaultValue(nxProp);

      var isSaved = !Equals(value, defaultValue);

      if (!primary.AgsSettings.SkipDefaultValues && isSaved)
         isSaved = false;

      if (primary.AgsSettings.WriteEmptyCollectionHeader &&
          primary.IsCollection(nxProp) &&
          value is IList { Count: 0 })
         isSaved = true;

      var ellipse = new Ellipse
      {
         Width = 4,
         Height = 4,
         VerticalAlignment = VerticalAlignment.Center,
         HorizontalAlignment = HorizontalAlignment.Left,
         Margin = new(-2, 0, 0, 0),
         Fill = isSaved ? Brushes.Green : Brushes.Transparent,
         ToolTip = isSaved
                      ? $"{nxProp} is set to a non-default value."
                      : $"{nxProp} is set to its default value.",
      };

      return ellipse;
   }

   private static void SetTooltipIsAny(IAgs iAgs, Enum nxProp, UIElement element)
   {
   }

   private static void GenerateShortInfo(NavH navH, IEu5Object primary, Grid mainGrid, bool isMarked)
   {
   }

   public static void CreateGraphViewerButton(IEu5Object primary, NavH navh, DockPanel panel)
   {
      if (Nx.GetGraphableProperties(primary).Length == 0)
         return;

      var graphButton = NEF.GetGraphButton();
      graphButton.ToolTip = "Open Graph Viewer for this object and its relations.";
      RoutedEventHandler graphClick = (_, _) =>
      {
         var graph = primary.CreateGraph();
         foreach (var node in graph.Nodes)
            node.NavigationHandler = EventHandlers.GetSimpleNavigationHandler(navh, primary);

         GraphWindow.ShowWindow(graph);
      };

      graphButton.Click += graphClick;
      graphButton.Unloaded += (_, _) => graphButton.Click -= graphClick;

      panel.Children.Add(graphButton);
      DockPanel.SetDock(graphButton, Dock.Right);
   }
}