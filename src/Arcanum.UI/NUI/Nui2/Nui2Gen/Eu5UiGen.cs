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
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.GraphDisplay;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.Converters;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Common.Logger;
using Common.UI;
using Common.UI.MBox;
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

   public readonly static Dictionary<Type, Func<Enum, FrameworkElement>> CustomTypeButtons = new();

   public readonly static Dictionary<Type, Func<object, Enum, FrameworkElement>> CustomItemTypeButtons = new();

   public readonly static Dictionary<Type, Func<Binding, int, int, Control>> CustomUiGenerators = new();

   public static readonly Dictionary<Enum, bool> IsExpandedCache = new();

   public static void GenerateAndSetView(NavH navh, List<Enum>? markedProps = null!, bool hasHeader = true)
   {
      if (navh.Targets.Count > 0)
      {
         var empty = EmptyRegistry.Empties[navh.Targets[0].GetType()];
         if (navh.Targets.Any(t => t.Equals(empty)))
         {
            // We do not want to show empties
            navh.Root.Content = null;
            return;
         }
      }

      navh.Root.Content = GenerateView(navh, markedProps ?? [], hasHeader);
   }

   public static BaseView GenerateView(NavH navh,
                                       List<Enum> markedProps,
                                       bool hasHeader = true,
                                       bool allowReadOnlyEditing = false)
   {
      var view = ControlFactory.GetBaseView();
      if (navh.Targets.Count < 1)
         return view;

      var primary = navh.Targets[0];
      view.BaseViewBorder.BorderThickness = new(0);
      var mainGrid = ControlFactory.GetMainGrid();
      view.BaseViewBorder.Child = mainGrid;

      if (hasHeader)
      {
         SetStatusEllipse(mainGrid, primary, 0, 0, 0);
         GridManager.SetPureHeader(mainGrid, primary, navh.Targets.Count, 0, 0, 3);
      }

      GenerateViewElements(navh,
                           navh.Targets,
                           mainGrid,
                           primary,
                           markedProps,
                           allowReadOnlyEditing,
                           false,
                           hasHeader ? 1 : 0);

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
                                            List<IEu5Object> targets,
                                            Grid mainGrid,
                                            IEu5Object primary,
                                            List<Enum> markedProps,
                                            bool allowReadOnlyEditing,
                                            bool isInlineCall,
                                            int startRow = 1)
   {
      var viewFields = SortViewFieldsByConfig(primary, markedProps);

      for (var i = 0; i < viewFields.Length; i++)
      {
         var nxProp = viewFields[i];
         var nxPropType = primary.GetNxPropType(nxProp);

         var isMarked = markedProps.Contains(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(nxPropType) || typeof(IEu5Object) == nxPropType)
            if (navH.GenerateSubViews)
               if (primary.IsPropertyInlined(nxProp))
                  GenerateInlinedView(navH, primary, mainGrid, nxProp, i + startRow);
               else
                  GenerateEmbeddedView(navH, targets, primary, mainGrid, nxProp, i + startRow, isMarked, isInlineCall);
            else
               GenerateShortInfo(navH, primary, nxProp, mainGrid, isMarked);
         else
            BuildCollectionViewOrDefault(navH,
                                         targets,
                                         primary,
                                         mainGrid,
                                         nxProp,
                                         i + startRow,
                                         isMarked: isMarked,
                                         allowReadOnlyEditing: allowReadOnlyEditing);
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

   private static void GenerateInlinedView(NavH navH,
                                           IEu5Object primary,
                                           Grid mainGrid,
                                           Enum nxProp,
                                           int startRow)
   {
      var inlineProp = primary._getValue(nxProp);

      // Instead of standard embedded view we generate a nice header, then just add type specific grid elements
      // and at the end we add another border to mark that it ends there.
      var inlineGrid = ControlFactory.GetMainGrid();
      Debug.Assert(inlineProp != null, "inlineProp != null");
      Debug.Assert(inlineProp is IEu5Object, "inlineProp is IEu5Object");
      var inlineObj = (IEu5Object)inlineProp;

      GridManager.AddToGrid(inlineGrid, NEF.InlineBorderMarker(6, 2), 0, 0);
      GridManager.AddToGrid(inlineGrid, NEF.InlineHeaderPanel(nxProp, inlineObj.GetType().Name, 2), 0, 0);

      var inlineTargets = navH.Targets.Select(target => (IEu5Object)target._getValue(nxProp)).ToList();

      // TODO @Minnator: https://github.com/users/Minnator/projects/2/views/2?pane=issue&itemId=135643593&issue=Minnator%7CArcanum%7C61
      var inlineNavH = new NavH(inlineTargets, navH.GenerateSubViews, navH.Root, true);

      // If we have an empty inlined objects, we only add a + button to create a new one.
      // When this button is pressed we then generate the full inlined view.
      var empty = EmptyRegistry.Empties[primary.GetNxPropType(nxProp)];
      if (inlineProp == empty)
      {
         var createButton = NEF.GetCreateNewButton();
         createButton.HorizontalAlignment = HorizontalAlignment.Center;

         RoutedEventHandler createClick = (_, _) =>
         {
            var newObj = (IEu5Object)Activator.CreateInstance(inlineObj.GetType())!;
            newObj.Source = Eu5FileObj.Empty;
            newObj.UniqueId = primary.UniqueId;
            primary._setValue(nxProp, newObj);

            NUINavigation.Instance.InvalidateUi(primary);
         };
         createButton.Click += createClick;
         createButton.Unloaded += (_, _) => createButton.Click -= createClick;

         GridManager.AddToGrid(inlineGrid, createButton, 1, 0, columnSpan: 2);
      }
      else
      {
         GenerateViewElements(inlineNavH, inlineTargets, inlineGrid, inlineObj, [], false, true);
      }

      var bottomBorderMarker = NEF.InlineBorderMarker(6, 2);
      bottomBorderMarker.Margin = new(0, 0, 0, 6);
      GridManager.AddToGrid(inlineGrid, bottomBorderMarker, inlineGrid.RowDefinitions.Count, 0);
      GridManager.AddToGrid(mainGrid, inlineGrid, startRow, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GenerateEmbeddedView(NavH navH,
                                            List<IEu5Object> targets,
                                            IEu5Object primary,
                                            Grid mainGrid,
                                            Enum nxProp,
                                            int rowIndex,
                                            bool isMarked,
                                            bool isInline)
   {
      var pevm = new PropertyEditorViewModel(nxProp, navH, primary, isInline);
      var mspvm = new MultiSelectPropertyViewModel(targets, nxProp);
      var ebv = new EmbeddedView(pevm, mspvm)
      {
         MinHeight = ControlFactory.EMBEDDED_VIEW_HEIGHT, Margin = new(0, 4, 0, 4),
      };
      
      ebv.Unloaded += (_, _) =>
      {
         mspvm.Dispose();
         pevm.Dispose();
      };
      
      GridManager.AddToGrid(mainGrid, ebv, rowIndex, 0, 2, ControlFactory.EMBEDDED_VIEW_HEIGHT);
      if (IsExpandedCache.TryGetValue(nxProp, out var isExpanded) && isExpanded)
         ebv.ViewModel.IsExpanded = true;

      var em = mspvm.Value;
      var embeddedObjectBinding = new Binding(nameof(MultiSelectPropertyViewModel.Value)) { Source = mspvm };

      var header = GridManager.GetNavigationHeader(navH,
                                                   nxProp.ToString(),
                                                   ControlFactory.SHORT_INFO_FONT_SIZE + 2,
                                                   ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                   true);

      // Make the header update if the value changes in multi-select
      header.SetBinding(FrameworkElement.TagProperty, embeddedObjectBinding);

      if (isMarked)
         header.Background = ControlFactory.MarkedBrush;

      ebv.TitleDockPanel.Children.Add(header);
      DockPanel.SetDock(header, Dock.Left);

      var setButton = NEF.CreateSetButton();
      ebv.TitleDockPanel.Children.Add(setButton);
      DockPanel.SetDock(setButton, Dock.Right);
      setButton.ToolTip = $"Set the '{nxProp}' property for all selected objects to the value inferred from the map.";

      if (em is not IEu5Object embedded)
      {
         // We don't have a valid embedded object, so we can't add buttons that operate on it.
      }
      else
      {
         var targetType = embedded.GetType();

         AddMapModeButtonToPanel(embedded, ebv.TitleDockPanel, targetType, Dock.Right);
         if (AddCustomButtonToPanel(nxProp, ebv.TitleDockPanel, targetType, Dock.Right) is { } customButton)
            customButton.SetBinding(FrameworkElement.TagProperty, embeddedObjectBinding);
      }

      var visibilityBinding = new Binding(nameof(MultiSelectPropertyViewModel.Value))
      {
         Source = mspvm, Converter = new ObjectToGraphButtonVisibilityConverter(),
      };
      if (CreateGraphViewerButton(navH, ebv.TitleDockPanel) is { } graphButton)
      {
         graphButton.SetBinding(FrameworkElement.TagProperty, embeddedObjectBinding);
         graphButton.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
      }

      AddCreateNewEu5ObjectButton(mspvm, primary, nxProp, ebv.TitleDockPanel, Dock.Right);

      RoutedEventHandler setClick = (_, _) =>
      {
         if (mspvm.Targets.Length < 1)
            return;

         var inferred =
            SelectionManager.GetInferredObjectsForLocations(Selection.GetSelectedLocations,
                                                            mspvm.Targets[0].GetNxPropType(nxProp));

         if (inferred == null || inferred.Count < 1)
            return;

         mspvm.Value = inferred[0];
         if (inferred.Count > 1)
            UIHandle.Instance.PopUpHandle
                    .ShowMBox($"Multiple inferred values found for {nxProp}. Using the first one ({inferred[0]}).",
                              "Multiple inferred values");
      };

      setButton.Click += setClick;
      setButton.Unloaded += (_, _) => setButton.Click -= setClick;
   }

   public static void PopulateEmbeddedGrid(Grid grid,
                                           NavH navH,
                                           List<IEu5Object> targets,
                                           IEu5Object embedded,
                                           Enum parentProp)
   {
      for (var index = 0; index < embedded.NUISettings.EmbeddedFields.Length; index++)
      {
         var nxProp = embedded.NUISettings.EmbeddedFields[index];
         var nxPropType = embedded.GetNxPropType(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(nxPropType) || typeof(IEu5Object) == nxPropType)
         {
            object embeddedValue = null!;
            Nx.ForceGet(embedded, nxProp, ref embeddedValue);
            var ui = Nui2Gen.CustomShortInfoGenerators.GenerateEu5ShortInfo(new(embedded, false, navH.Root, true),
                                                                            (IEu5Object)embeddedValue,
                                                                            nxProp,
                                                                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                                            ControlFactory.SHORT_INFO_FONT_SIZE,
                                                                            0,
                                                                            2);

            GridManager.AddToGrid(grid, ui, 1 + index, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
            continue;
         }

         BuildCollectionViewOrDefault(navH,
                                      targets,
                                      embedded,
                                      grid,
                                      nxProp,
                                      index + 1,
                                      parentProp: parentProp,
                                      isMarked: false);
      }
   }

   private static void BuildCollectionViewOrDefault(NavH navH,
                                                    List<IEu5Object> targets,
                                                    IEu5Object primary,
                                                    Grid mainGrid,
                                                    Enum nxProp,
                                                    int rowIndex,
                                                    bool isMarked,
                                                    bool allowReadOnlyEditing = false,
                                                    Enum? parentProp = null)
   {
      var itemType = primary.GetNxItemType(nxProp);
      var propertyViewModel = new MultiSelectPropertyViewModel(targets, nxProp, allowReadOnlyEditing);

      if (itemType == null)
      {
         // We have a default property, not a collection.
         GetTypeSpecificUI(navH,
                           primary,
                           nxProp,
                           mainGrid,
                           rowIndex,
                           isMarked,
                           propertyViewModel,
                           parentProp,
                           17,
                           allowReadOnlyEditing);
         return;
      }

      // We have collections from multiple objects which are not identical.
      if (primary._getValue(nxProp) is not ICollection)
      {
         // We have a collection property, but it's not a list we can iterate with our current implementation.
         // List and HashSet are supported.
         GetTypeSpecificUI(navH,
                           primary,
                           nxProp,
                           mainGrid,
                           rowIndex,
                           mspvm: propertyViewModel,
                           isMarked: isMarked,
                           embeddedPropertyTargets: parentProp,
                           allowReadOnlyEditing: allowReadOnlyEditing);
         return;
      }

      const int margin = 19;
      var collectionGrid = ControlFactory.GetCollectionGrid();

      SetCollectionHeaderPanel(nxProp,
                               itemType,
                               collectionGrid,
                               primary,
                               0,
                               margin,
                               isMarked,
                               navH,
                               propertyViewModel);

      Action rebuildPreview;
      if (propertyViewModel.Value is ICollection modifiableList)
      {
         rebuildPreview = () =>
         {
            ClearCollectionPreview(collectionGrid);
            GetCollectionPreview(navH, primary, collectionGrid, nxProp, modifiableList, 1, margin);
         };

         if (modifiableList.Count > 0)
            GetCollectionLiner(collectionGrid, margin);
      }
      else
      {
         rebuildPreview = () => { GetDifferingCollectionsNotPrevieable(collectionGrid, 1, margin); };
      }

      rebuildPreview();

      EventHandler collectionChangedHandler = (_, _) => { Application.Current.Dispatcher.Invoke(rebuildPreview); };

      propertyViewModel.CollectionContentChanged += collectionChangedHandler;

      collectionGrid.Unloaded += (_, _) =>
      {
         propertyViewModel.CollectionContentChanged -= collectionChangedHandler;
         propertyViewModel.Dispose();
      };

      GridManager.AddToGrid(mainGrid, collectionGrid, rowIndex, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void ClearCollectionPreview(Grid grid)
   {
      for (var i = grid.Children.Count - 1; i >= 0; i--)
      {
         var child = grid.Children[i];
         if (Grid.GetRow(child) > 0)
            grid.Children.RemoveAt(i);
      }
   }

   private static void SetCollectionHeaderPanel(Enum nxProp,
                                                Type itemType,
                                                Grid mainGrid,
                                                IEu5Object primary,
                                                int row,
                                                int leftMargin,
                                                bool isMarked,
                                                NavH navh,
                                                MultiSelectPropertyViewModel propertyViewModel)
   {
      var headerPanel = NEF.PropertyTitlePanel(leftMargin);

      var tb = GetCollectionTitleTextBox(nxProp,
                                         headerPanel,
                                         primary,
                                         leftMargin,
                                         isMarked,
                                         propertyViewModel,
                                         fontSize: 12,
                                         height: ControlFactory.SHORT_INFO_ROW_HEIGHT - 4);

      SetUpPropertyContextMenu(primary, nxProp, tb, propertyViewModel);

      GetCollectionEditorButton(navh, primary, nxProp, itemType, headerPanel);
      GetInferActionButtons(propertyViewModel, nxProp, primary.GetNxPropType(nxProp), itemType, headerPanel, navh);

      var marker = GetPropertyMarker(propertyViewModel);

      GridManager.AddToGrid(mainGrid, headerPanel, row, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
      GridManager.AddToGrid(mainGrid, marker, row, 0, 1, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   private static void GetInferActionButtons(MultiSelectPropertyViewModel mspvm,
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

      if (!typeof(IMapInferable).IsAssignableFrom(targetType))
         return;

      // Infer actions for a collection
      if (nxItemType != null)
      {
         var addButton = NEF.CreateAddButton();
         var removeButton = NEF.CreateRemoveButton();

         RoutedEventHandler addClick = (_, _) =>
         {
            if (mspvm.Value == null || mspvm.Targets.Length < 1)
               return;

            var itemType = mspvm.Targets[0].GetNxItemType(nxProp)!;

            var enumerable = SelectionManager.GetInferredObjectsForLocations(Selection.GetSelectedLocations, itemType);

            Debug.Assert(enumerable != null, "enumerable != null");
            foreach (var obj in enumerable)
               foreach (var target in mspvm.Targets)
                  Nx.AddToCollection(target, nxProp, obj);
         };

         RoutedEventHandler removeClick = (_, _) =>
         {
            if (mspvm.Value == null || mspvm.Targets.Length < 1)
               return;

            var itemType = mspvm.Targets[0].GetNxItemType(nxProp)!;

            var enumerable = SelectionManager.GetInferredObjectsForLocations(Selection.GetSelectedLocations, itemType);
            Debug.Assert(enumerable != null, "enumerable != null");

            foreach (var obj in enumerable)
               foreach (var target in mspvm.Targets)
                  Nx.RemoveFromCollection(target, nxProp, obj);
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
            var customButton = customButtonFunc(mspvm, nxProp);
            panel.Children.Add(customButton);
            DockPanel.SetDock(customButton, Dock.Right);
         }
      }

      if (!EmptyRegistry.TryGet(targetType, out var item) && !typeof(IEu5Object).IsAssignableFrom(targetType))
         return;

      var valueBinding = new Binding(nameof(MultiSelectPropertyViewModel.Value)) { Source = mspvm };
      var visibilityBinding = new Binding(nameof(MultiSelectPropertyViewModel.Value))
      {
         Source = mspvm, Converter = new ObjectToGraphButtonVisibilityConverter(),
      };

      // Infer actions for a single embedded object or property
      AddMapModeButtonToPanel((IEu5Object)item, panel, targetType, Dock.Right);
      if (CreateGraphViewerButton(navh, panel) is { } graphButton)
      {
         graphButton.SetBinding(FrameworkElement.TagProperty, valueBinding);
         graphButton.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
      }

      if (AddCustomButtonToPanel(nxProp, panel, targetType, Dock.Right) is { } fe)
         fe.SetBinding(FrameworkElement.TagProperty, valueBinding);
   }

   private static FrameworkElement? AddCustomButtonToPanel(Enum nxProp, DockPanel panel, Type targetType, Dock dock)
   {
      if (!CustomTypeButtons.TryGetValue(targetType, out var customTypeButtonFunc))
         return null;

      // The implementer MUST write event handlers that get the current IEu5Object
      // from the sender's Tag.
      var customButton = customTypeButtonFunc(nxProp);

      panel.Children.Add(customButton);
      DockPanel.SetDock(customButton, dock);
      return customButton;
   }

   private static void AddCreateNewEu5ObjectButton(MultiSelectPropertyViewModel mspvm,
                                                   IEu5Object primary,
                                                   Enum nxProp,
                                                   DockPanel panel,
                                                   Dock dock)
   {
      var createNewButton = NEF.GetCreateNewButton();

      RoutedEventHandler createNewClick = (_, _) =>
      {
         Type type;
         if (primary.IsCollection(nxProp))
            type = primary.GetNxItemType(nxProp) ??
                   throw new
                      InvalidOperationException($"Property {nxProp} does not have an item type but is a collection.");
         else
            type = primary.GetNxPropType(nxProp) ??
                   throw new InvalidOperationException($"Property {nxProp} does not have a type.");

         Eu5ObjectCreator.ShowPopUp(type,
                                    newObj =>
                                    {
                                       // Find the first parent of type EmbeddedView of the createNewButton and refresh its selector
                                       var parent = VisualTreeHelper.GetParent(createNewButton);
                                       while (parent != null && parent is not EmbeddedView)
                                          parent = VisualTreeHelper.GetParent(parent);

                                       if (parent is EmbeddedView ev)
                                          ev.RefreshSelector();

                                       // Has to happen after refreshing the selector to avoid adding the command twice to history
                                       mspvm.Value = newObj;
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
      if (typeof(IMapInferable).IsAssignableFrom(targetType))
      {
         var mapModeButton = GetMapModeButton((IMapInferable)primary);
         panel.Children.Add(mapModeButton);
         DockPanel.SetDock(mapModeButton, dock);
      }
   }

   public static BaseButton GetMapModeButton(IMapInferable primary)
   {
      var mmType = primary.GetMapMode;
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

   private static void GetCollectionEditorButton(NavH navh,
                                                 IEu5Object parent,
                                                 Enum property,
                                                 Type nxItemType,
                                                 DockPanel panel)
   {
      var eyeButton = NEF.GetEyeButton();
      eyeButton.Margin = new(4, 0, 0, 0);

      ICollection allItems = null!;
      if (EmptyRegistry.Empties.TryGetValue(nxItemType, out var value))
      {
         if (value is not IEu5Object eu5Object)
         {
            ArcLog.WriteLine("UIG",
                             LogLevel.CRT,
                             $"Tried to get global items for type {nxItemType.Name} but the empty is not an IEu5Object.");
            return;
         }

         allItems = eu5Object.GetGlobalItemsNonGeneric().Values;
      }

      RoutedEventHandler clickHandler = (_, _) =>
      {
         var owner = Window.GetWindow(navh.Root)!;
         var lists = navh.Targets.Select(x => (IList)x._getValue(property));
         var result =
            MultiCollectionEditor.ShowDialogN(owner,
                                              $"Edit {property} - {parent.UniqueId}",
                                              nxItemType,
                                              lists,
                                              allItems);

         dynamic dynamicResult = result;

         if (dynamicResult.Canceled)
            return;

         Array toAddPerCollection = dynamicResult.ToAddPerCollection;
         Array toRemovePerCollection = dynamicResult.ToRemovePerCollection;

         for (var i = 0; i < navh.Targets.Count; i++)
         {
            var itemsToAdd = (Array)toAddPerCollection.GetValue(i)!;
            var itemsToRemove = (Array)toRemovePerCollection.GetValue(i)!;

            foreach (var item in itemsToAdd)
               Nx.AddToCollection(navh.Targets[i], property, item);

            foreach (var item in itemsToRemove)
               Nx.RemoveFromCollection(navh.Targets[i], property, item);
         }
      };

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

   private static void GetDifferingCollectionsNotPrevieable(Grid grid,
                                                            int rowIndex,
                                                            int margin)
   {
      var infoText = ControlFactory.GetHeaderTextBlock(ControlFactory.SHORT_INFO_FONT_SIZE,
                                                       false,
                                                       "  (Collections differ between selected objects - no preview)",
                                                       height: ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                       alignment: HorizontalAlignment.Left);
      GridManager.AddToGrid(grid,
                            infoText,
                            rowIndex,
                            0,
                            0,
                            ControlFactory.SHORT_INFO_ROW_HEIGHT,
                            leftMargin: margin + 4);
   }

   private static void GetCollectionPreview(NavH navH,
                                            IEu5Object primary,
                                            Grid grid,
                                            Enum nxProp,
                                            ICollection modifiableList,
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
      moreText.Unloaded += (_, _) => moreText.MouseLeftButtonUp -= clickHandler;

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
               var text = ControlFactory.PureHeaderTextBlock(false);
               text.Text = item?.ToString() ?? "null";
               text.FontSize = ControlFactory.SHORT_INFO_FONT_SIZE;
               text.Height = 14;
               text.VerticalAlignment = VerticalAlignment.Top;
               GridManager.AddToGrid(grid,
                                     text,
                                     rowIndex + i,
                                     0,
                                     0,
                                     ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                     leftMargin: margin + 4);
            }
         }

         i++;
      }
   }

   private static TextBlock GetCollectionTitleTextBox(Enum nxProp,
                                                      DockPanel panel,
                                                      IEu5Object primary,
                                                      int margin,
                                                      bool isMarked,
                                                      MultiSelectPropertyViewModel propertyViewModel,
                                                      int fontSize = ControlFactory.SHORT_INFO_FONT_SIZE,
                                                      int height = ControlFactory.SHORT_INFO_ROW_HEIGHT)
   {
      var tb = ControlFactory.GetHeaderTextBlock(fontSize,
                                                 false,
                                                 string.Empty,
                                                 height: height,
                                                 alignment: HorizontalAlignment.Left,
                                                 leftMargin: margin);

      if (isMarked)
         tb.Background = ControlFactory.MarkedBrush;

      var multiBinding = new MultiBinding { Converter = new PropertyNameAndCountConverter() };
      multiBinding.Bindings.Add(new Binding { Source = nxProp });

      multiBinding.Bindings.Add(new Binding(nameof(MultiSelectPropertyViewModel.CollectionCount))
      {
         Source = propertyViewModel,
      });
      tb.SetBinding(TextBlock.TextProperty, multiBinding);

      panel.Children.Add(tb);
      SetCollectionToolTip(primary, nxProp, tb);
      return tb;
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

   private static void GetTypeSpecificUI(NavH navH,
                                         IEu5Object primary,
                                         Enum nxProp,
                                         Grid mainGrid,
                                         int rowIndex,
                                         bool isMarked,
                                         MultiSelectPropertyViewModel mspvm,
                                         Enum? embeddedPropertyTargets = null,
                                         int leftMargin = 0,
                                         bool allowReadOnlyEditing = false)
   {
      var type = primary.GetNxPropType(nxProp);

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

         mspvm = new(targets, nxProp, allowReadOnlyEditing);
      }

      var binding = new Binding(nameof(mspvm.Value))
      {
         Source = mspvm,
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };

      Control element;
      var val = Nx.ForceGetAs<object>(primary, nxProp);

      if (type == typeof(float))
         element = NEF.GetFloatUI(binding, (float)val);
      else if (type == typeof(string))
         element = NEF.GetStringUI(binding);
      else if (type == typeof(bool))
         element = NEF.GetBoolUI(binding);
      else if (type.IsEnum)
         element = NEF.GetEnumUI(type, binding);
      else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
         element = NEF.GetIntUI(binding, (int)val);
      else if (type == typeof(double) || type == typeof(decimal))
         element = NEF.GetDoubleUI(binding, (decimal)val);
      else if (type == typeof(JominiColor))
         element = NEF.GetJominiColorUI(binding, primary.IsPropertyReadOnly(nxProp));
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

      var desc = NEF.DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);

      SetUpPropertyContextMenu(primary, nxProp, desc, mspvm);

      if (isMarked)
         desc.Background = ControlFactory.MarkedBrush;

      SetTooltipIsAny(primary, nxProp, desc);

      GetInferActionButtons(mspvm, nxProp, type, primary.GetNxItemType(nxProp), new(), navH);

      var line = NEF.GenerateDashedLine(leftMargin);
      RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

      var dockPanel = NEF.PropertyTitlePanel(leftMargin);
      dockPanel.Children.Add(desc);

      dockPanel.Unloaded += (_, _) => { mspvm.Dispose(); };

      var propertyMarker = GetPropertyMarker(mspvm);

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

   private static void SetUpPropertyContextMenu(IEu5Object primary,
                                                Enum nxProp,
                                                TextBlock uiTarget,
                                                MultiSelectPropertyViewModel propertyViewModel)
   {
      if (primary.IsPropertyReadOnly(nxProp))
         return;

      var defaultValue = primary.GetDefaultValue(nxProp);
      MouseButtonEventHandler mouseUpHandler = (_, _) =>
      {
         if (uiTarget.ContextMenu == null)
            uiTarget.ContextMenu = new();
         else
         {
            foreach (var item in uiTarget.ContextMenu.Items.OfType<MenuItem>())
               if (item.Header.ToString() == "Reset to Default")
                  return;

            uiTarget.ContextMenu.Items.Add(new Separator());
         }

         var resetItem = new MenuItem
         {
            Name = "ResetToDefaultMenuItem",
            Header = "Reset to Default",
            DataContext = propertyViewModel,
         };

         resetItem.SetBinding(UIElement.IsEnabledProperty,
                              new Binding(nameof(MultiSelectPropertyViewModel.IsNonDefaultValue)));

         RoutedEventHandler clickEvent = (_, _) =>
         {
            if (primary.IsCollection(nxProp))
            {
               Nx.ClearCollection(primary, nxProp);
               propertyViewModel.Refresh();
            }
            else
               propertyViewModel.Value = defaultValue;
         };
         resetItem.Click += clickEvent;
         resetItem.Unloaded += (_, _) => resetItem.Click -= clickEvent;

         if (uiTarget.ContextMenu.Items.Count > 0 && uiTarget.ContextMenu.Items[^1] is not Separator)
            uiTarget.ContextMenu.Items.Add(new Separator());

         uiTarget.ContextMenu.Items.Add(resetItem);
      };

      uiTarget.MouseRightButtonUp += mouseUpHandler;
      uiTarget.Unloaded += (_, _) => uiTarget.MouseRightButtonUp -= mouseUpHandler;
   }

   private static Image GetPropertyMarker(MultiSelectPropertyViewModel vm)
   {
      var image = new Image
      {
         Width = 16,
         Height = 16,
         VerticalAlignment = VerticalAlignment.Center,
         HorizontalAlignment = HorizontalAlignment.Left,
         Margin = new(0, 0, 0, 0),
         SnapsToDevicePixels = true,
         DataContext = vm,
      };

      var sourceBinding = new Binding(nameof(vm.IsNonDefaultValue))
      {
         Converter = new IsNonDefaultToImageSourceConverter(),
      };

      var tooltipBinding = new Binding(nameof(vm.IsNonDefaultValue))
      {
         Converter = new IsNonDefaultToTooltipConverter(),
      };

      image.SetBinding(Image.SourceProperty, sourceBinding);
      image.SetBinding(FrameworkElement.ToolTipProperty, tooltipBinding);

      return image;
   }

   private static void SetTooltipIsAny(IEu5Object primary, Enum nxProp, FrameworkElement element)
   {
      var desc = primary.GetDescription(nxProp);
      if (string.IsNullOrWhiteSpace(desc))
         return;

      element.ToolTip = new ToolTip
      {
         Content = desc, MaxWidth = 400,
      };
   }

   private static void GenerateShortInfo(NavH navH, IEu5Object primary, Enum nxProp, Grid mainGrid, bool isMarked)
   {
      var si = Nui2Gen.CustomShortInfoGenerators.GenerateEu5ShortInfo(new(primary, false, navH.Root, true),
                                                                      Nx.ForceGetAs<IEu5Object>(primary, nxProp),
                                                                      nxProp,
                                                                      ControlFactory.SHORT_INFO_ROW_HEIGHT,
                                                                      ControlFactory.SHORT_INFO_FONT_SIZE,
                                                                      0,
                                                                      2);
      if (isMarked)
         si.Background = ControlFactory.MarkedBrush;

      GridManager.AddToGrid(mainGrid, si, mainGrid.RowDefinitions.Count, 0, 2, ControlFactory.SHORT_INFO_ROW_HEIGHT);
   }

   public static FrameworkElement CreateGraphViewerButton(NavH navh, DockPanel panel)
   {
      var graphButton = NEF.GetGraphButton();
      graphButton.ToolTip = "Open Graph Viewer for this object and its relations.";

      // Dynamic click handler
      RoutedEventHandler graphClick = (sender, _) =>
      {
         if (sender is not FrameworkElement { Tag: IEu5Object currentTarget })
            return;

         var graph = currentTarget.CreateGraph();
         foreach (var node in graph.Nodes)
            node.NavigationHandler = EventHandlers.GetSimpleNavigationHandler(navh, node.LinkedObject);
         GraphWindow.ShowWindow(graph);
      };

      graphButton.Click += graphClick;
      graphButton.Unloaded += (_, _) => graphButton.Click -= graphClick;

      panel.Children.Add(graphButton);
      DockPanel.SetDock(graphButton, Dock.Right);
      return graphButton;
   }
}