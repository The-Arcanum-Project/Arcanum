using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Microsoft.Xaml.Behaviors.Core;
using Nexus.Core;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.UI.NUI.Generator;

public static class NUIViewGenerator
{
   private static int _index;

   #region Public API

   /// <summary>
   /// Generates a view for the given <see cref="NUINavHistory"/> and sets it as the content of the root ContentPresenter.
   /// This method is a convenience wrapper around <see cref="GenerateView(NUINavHistory)"/> that directly updates the UI.
   /// </summary>
   /// <param name="navHistory"></param>
   public static void GenerateAndSetView(NUINavHistory navHistory)
   {
      var sw = Stopwatch.StartNew();
      var view = GenerateView(navHistory);
      sw.Stop();
      Console.WriteLine($"[NUI] Generated view for {navHistory.PrimaryTarget.GetType().Name} in {sw.ElapsedMilliseconds} ms");
      sw.Restart();
      navHistory.Root.Content = view;
      sw.Stop();
      Console.WriteLine($"[NUI] Set view content in {sw.ElapsedMilliseconds} ms");
   }

   /// <summary>
   /// Generates a WPF UserControl view for the given <see cref="NUINavHistory"/>.
   /// The view includes navigation headers, property editors, and handles nested INUI objects and collections.
   /// Each generated view is wrapped in a BaseView control for consistent styling.
   /// </summary>
   /// <param name="navHistory"></param>
   /// <returns></returns>
   public static UserControl GenerateView(NUINavHistory navHistory)
   {
      var target = navHistory.Targets[0];
      var baseUI = new BaseView
      {
         Name = $"{target.NUISettings.Title}_{_index}", BaseViewBorder = { BorderThickness = new(0) },
      };
      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(40, GridUnitType.Pixel) } }, Margin = new(4) };

      var header = NavigationHeader(navHistory, target);

      header.FontSize = 24;
      header.Height = 32;
      header.HorizontalAlignment = HorizontalAlignment.Center;
      header.VerticalAlignment = VerticalAlignment.Top;
      baseGrid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);

      GenerateViewElement(navHistory, target, baseGrid);

      baseUI.BaseViewBorder.Child = baseGrid;
      _index++;
      return baseUI;
   }

   #endregion

   #region View Generation (High-Level Builders)

   private static void GenerateViewElement(NUINavHistory navHistory, INUI target, Grid baseGrid)
   {
      long embeddedSwTicks = 0;
      long collectionSwTicks = 0;
      var sw = Stopwatch.StartNew();
      var viewFields = navHistory.PrimaryTarget.NUISettings.ViewFields;
      if (!Config.Settings.NUIConfig.ListViewsInCustomOrder)
         viewFields = viewFields.OrderBy(f => f.ToString()).ToArray();

      for (var i = 0; i < target.NUISettings.ViewFields.Length; i++)
      {
         var nxProp = viewFields[i];
         FrameworkElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type) || typeof(INUI) == type)
         {
            // Detect if value has ref to target. --> 1 to n relationship.
            if (navHistory.GenerateSubViews)
            {
               if (navHistory.Targets.Count > 1)
               {
                  element = new TextBlock
                  {
                     Text = $"- Multiple selections for '{nxProp}' not supported -",
                     FontStyle = FontStyles.Italic,
                     Margin = new(6, 4, 0, 4),
                  };
               }
               else
               {
                  var embeddedSw = Stopwatch.StartNew();
                  INUI value = null!;
                  Nx.ForceGet(target, nxProp, ref value);
                  element = GetEmbeddedView(target, nxProp, value, navHistory);
                  embeddedSw.Stop();
                  embeddedSwTicks += embeddedSw.ElapsedTicks;
               }
            }
            else
               element = GenerateShortInfo(target, navHistory);
         }
         else
         {
            var collectionSw = Stopwatch.StartNew();
            element = BuildCollectionOrDefaultView(navHistory, type, navHistory.Targets.ToList(), nxProp);
            collectionSw.Stop();
            collectionSwTicks += collectionSw.ElapsedTicks;
         }

         element.VerticalAlignment = VerticalAlignment.Stretch;
         baseGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      sw.Stop();
      Console.WriteLine($"[NUI] View generation for {target.GetType().Name} took {sw.ElapsedMilliseconds} ms \n(Embedded: {new TimeSpan(embeddedSwTicks).TotalMilliseconds} ms \nCollections: {new TimeSpan(collectionSwTicks).TotalMilliseconds} ms)");
   }

   private static BaseEmbeddedView GetEmbeddedView<T>(INUI parent,
                                                      Enum property,
                                                      T target,
                                                      NUINavHistory navHistory) where T : INUI
   {
      var startExpanded = !Config.Settings.NUIConfig.StartEmbeddedFieldsCollapsed;
      var embeddedFields = target.NUISettings.EmbeddedFields;

      if (!Config.Settings.NUIConfig.ListViewsInCustomOrder)
         embeddedFields = embeddedFields.OrderBy(f => f.ToString()).ToArray();

      var isReadonlyProp = parent.IsPropertyReadOnly(property);

      var baseUI = new BaseEmbeddedView();
      var baseGrid = baseUI.ContentGrid;
      var initialVisibility = startExpanded ? Visibility.Visible : Visibility.Collapsed;
      var collapsibleElements = new List<FrameworkElement>();

      IEnumerable? allItems;
      if (target is IEu5Object eu5Obj)
         allItems = eu5Obj.GetGlobalItemsNonGeneric().Values;
      else
         allItems = new List<INUI>();

      var headerBlock = NavigationHeader(navHistory, target, property.ToString());

      var descriptionText = parent.GetDescription(property);
      if (!string.IsNullOrWhiteSpace(descriptionText))
      {
         var existingToolTip = headerBlock.ToolTip as string ?? string.Empty;
         headerBlock.ToolTip = string.IsNullOrWhiteSpace(existingToolTip)
                                  ? descriptionText
                                  : $"{existingToolTip}\n\nDescription: {descriptionText}";
      }

      baseGrid.RowDefinitions.Add(new() { Height = new(27, GridUnitType.Pixel) });

      var collapseButton = NEF.GetCollapseButton(startExpanded);

      if (!isReadonlyProp)
      {
         var objectSelector = NEF.ObjectSelector(target, allItems, _index, property);
         _index++;

         var binding = new Binding(property.ToString())
         {
            Source = parent,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
         };

         objectSelector.SetBinding(Selector.SelectedItemProperty, binding);

         objectSelector.SelectionChanged += (_, args) =>
         {
            if (args.AddedItems.Count > 0 && !args.AddedItems[0]!.Equals(target))
            {
               objectSelector.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
               GenerateAndSetView(navHistory);
            }
         };

         var headerGrid = NEF.CreateHeaderGrid<T>();

         headerGrid.Children.Add(headerBlock);
         Grid.SetRow(headerBlock, 0);
         Grid.SetColumn(headerBlock, 0);

         var inferActions = GenerateInferActions(parent,
                                                 navHistory,
                                                 property,
                                                 target.GetType(),
                                                 null,
                                                 null);

         if (inferActions != null)
         {
            headerGrid.Children.Add(inferActions);
            Grid.SetRow(inferActions, 0);
            Grid.SetColumn(inferActions, 0);
         }

         var itemType = target.GetType();
         if (!isReadonlyProp && EmptyRegistry.TryGet(itemType, out var emptyInstance))
         {
            var setEmptyButton = NEF.GetSetEmptyButton(itemType,
                                                       property,
                                                       parent,
                                                       navHistory,
                                                       emptyInstance,
                                                       parent.AllowsEmptyValue(property));

            headerGrid.Children.Add(setEmptyButton);
            Grid.SetRow(setEmptyButton, 0);
            Grid.SetColumn(setEmptyButton, 2);
         }

         headerGrid.Children.Add(objectSelector);
         Grid.SetRow(objectSelector, 0);
         Grid.SetColumn(objectSelector, 1);

         headerGrid.Children.Add(collapseButton);
         Grid.SetRow(collapseButton, 0);
         Grid.SetColumn(collapseButton, 3);

         baseGrid.Children.Add(headerGrid);
         Grid.SetRow(headerGrid, 0);
         Grid.SetColumn(headerGrid, 0);
      }
      else
      {
         NEF.CreateSimpleHeaderGrid(headerBlock, collapseButton, baseGrid);
      }

      var embedMarker = NEF.EmbedMarker(baseGrid);

      GenerateEmbeddedViewElements(target,
                                   navHistory,
                                   embeddedFields,
                                   baseGrid,
                                   initialVisibility,
                                   collapsibleElements);

      NEF.AddSpacerToGrid(baseGrid, embeddedFields, collapsibleElements);

      Grid.SetRowSpan(embedMarker, baseGrid.RowDefinitions.Count);

      collapseButton.Click +=
         CollapseButtonOnClick(startExpanded, collapsibleElements, collapseButton, embedMarker, baseGrid);
      return baseUI;
   }

   private static Grid BuildCollectionOrDefaultView(NUINavHistory navHistory,
                                                    Type type,
                                                    List<INUI> targets,
                                                    Enum nxProp,
                                                    int leftMargin = 0)
   {
      var exValue = targets[0];
      var itemType = exValue.GetNxItemType(nxProp);

      // if it is null we do not have a collection
      if (itemType == null)
         return GetTypeSpecificGrid(navHistory, targets, nxProp, leftMargin);

      object collectionObject = null!;
      Nx.ForceGet(targets[0], nxProp, ref collectionObject);

      // We need a modifiable list (IList) for the editor to work.
      if (collectionObject is not IList modifiableList)
         return GetTypeSpecificGrid(navHistory, targets.ToList(), nxProp, leftMargin);

      var grid = NEF.CreateDefaultGrid(leftMargin);

      if (targets.Count > 1)
      {
         var info = NEF.CreateInfoTextBlock(nxProp, leftMargin);
         grid.Children.Add(info);
         return grid;
      }

      var inuiItems = modifiableList.OfType<INUI>().ToList();
      var textBlock = NEF.CreateItemsCountTextBlock(nxProp, modifiableList);

      SetTooltipIsAny(targets[0], nxProp, textBlock);

      grid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
      GenerateCollectionItemPreview(navHistory, inuiItems, grid, modifiableList);
      var openButton = NEF.GetEyeButton();
      openButton.Margin = new(4, 0, 0, 0);

      var providerInterfaceType = typeof(ICollectionProvider<>).MakeGenericType(itemType);
      SetupCollectionEditorButton(navHistory,
                                  nxProp,
                                  providerInterfaceType,
                                  itemType,
                                  modifiableList,
                                  textBlock,
                                  grid,
                                  openButton);

      var inferActions = GenerateInferActions(targets[0],
                                              navHistory,
                                              nxProp,
                                              type,
                                              itemType,
                                              modifiableList);

      var headerStack = NEF.CreateHeaderStackPanel();
      headerStack.Children.Add(textBlock);
      headerStack.Children.Add(openButton);
      FrameworkElement header;

      if (inferActions != null)
      {
         var headerGrid = NEF.CreateHeaderGrid();

         headerGrid.Children.Add(headerStack);
         Grid.SetRow(headerStack, 0);
         Grid.SetColumn(headerStack, 0);

         headerGrid.Children.Add(inferActions);
         Grid.SetRow(inferActions, 0);
         Grid.SetColumn(inferActions, 1);
         header = headerGrid;
      }
      else
      {
         header = headerStack;
      }

      grid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);
      Grid.SetColumnSpan(header, 2);

      // --- Decorative Border ---
      if (grid.RowDefinitions.Count > 1)
      {
         var rect = NEF.CreateDashedBorderRectangle();
         grid.Children.Add(rect);
         Grid.SetRow(rect, 1);
         Grid.SetColumn(rect, 0);
         Grid.SetRowSpan(rect, grid.RowDefinitions.Count - 1);
      }

      return grid;
   }

   private static Grid GetTypeSpecificGrid(NUINavHistory navh,
                                           List<INUI> targets,
                                           Enum nxProp,
                                           int leftMargin = 0)
   {
      var target = targets[0];
      var type = Nx.TypeOf(target, nxProp);

      var propertyViewModel = new MultiSelectPropertyViewModel(targets, nxProp);

      var binding = new Binding(nameof(propertyViewModel.Value))
      {
         Source = propertyViewModel,
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };

      Control element;

      if (type == typeof(float))
         element = NEF.GetFloatUI(binding, 0f); // THIS BREAKS THE UI AS UI IS DEPRECATED
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
         Nx.ForceGet(targets[0], nxProp, ref temp);
         element = NEF.GetJominiColorUI(binding, temp, targets[0].IsPropertyReadOnly(nxProp));
      }
      else if (type == typeof(object))
         element = NEF.GetStringUI(binding);
      else
         throw new NotSupportedException($"Type {type} is not supported for property {nxProp}.");

      element.IsEnabled = !target.IsReadonly;
      element.VerticalAlignment = VerticalAlignment.Stretch;

      SetTooltipIsAny(target, nxProp, element);

      var desc = NEF.DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);

      SetTooltipIsAny(target, nxProp, desc);

      var inferActions = GenerateInferActions(target,
                                              navh,
                                              nxProp,
                                              type,
                                              null,
                                              null);

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

      if (inferActions != null)
      {
         grid.Children.Add(inferActions);
         Grid.SetRow(inferActions, 0);
         Grid.SetColumn(inferActions, 0);
      }

      return grid;
   }

   #endregion

   #region UI Component Helpers

   private static StackPanel GenerateShortInfo<T>(T value, NUINavHistory navh) where T : INUI
   {
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, MinHeight = 20 };
      object headerValue = null!;
      Nx.ForceGet(value, value.NUISettings.Title, ref headerValue);
      var shortInfoParts = new List<string>();
      foreach (var nxProp in value.NUISettings.ShortInfoFields)
      {
         object pVal = null!;
         Nx.ForceGet(value, nxProp, ref pVal);
         if (pVal is IEnumerable collection and not string)
         {
            var count = collection.Cast<object>().Count();
            shortInfoParts.Add($"{nxProp}: {count}");
         }
         else
         {
            object? val = null;
            Nx.ForceGet(value, nxProp, ref val);
            shortInfoParts.Add($"{val?.ToString() ?? "null"}");
         }
      }

      const int fontSize = 11;
      var headerBlock = NavigationHeader(navh,
                                         value,
                                         value.GetType().Name,
                                         fontSize,
                                         FontWeights.Normal);
      headerBlock.Margin = new(6, 0, 0, 0);
      var dashBlock = NEF.CreateDashBlock<T>(fontSize);
      var infoBlock = NEF.CreateShortInfoBlock<T>(shortInfoParts, fontSize);
      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(dashBlock);
      stackPanel.Children.Add(infoBlock);

      return stackPanel;
   }

   private static TextBlock NavigationHeader<T>(NUINavHistory navh,
                                                T value,
                                                string text = null!,
                                                int fontSize = 16,
                                                FontWeight? fontWeight = null) where T : INUI
   {
      var height = fontSize + 4;
      var header = NEF.CreateHeaderTextBlock<T>(fontSize, fontWeight, height);

      if (navh.Targets.Count > 1)
         header.Text = $"{navh.Targets.Count} {value.GetType().Name}s Selected";
      else
         header.Text =
            $"Left-Click to navigate to this object '{value.ToString()}' ({value.GetType().Name})\nRight-click for navigation options.";

      if (string.IsNullOrWhiteSpace(text))
      {
         object headerValue = null!;
         Nx.ForceGet(value, value.NUISettings.Title, ref headerValue);
         text = value.ToString() ?? "INVALID_VALUE_MUST_NOT_BE_NULL";
      }

      header.Text = text;

      var clickHandler = NUIEventHandlers.MouseButtonEventHandler(navh, value, header);
      header.MouseUp += clickHandler;
      header.Unloaded += (_, _) => { header.MouseUp -= clickHandler; };
      header.Cursor = Cursors.Hand;

      return header;
   }

   #endregion

   #region Feature-Specific Logic (e.g., Infer Actions, Collections)

   private static void SetTooltipIsAny(INUI target, Enum property, FrameworkElement element)
   {
      var tt = target.GetDescription(property);
      if (string.IsNullOrWhiteSpace(tt))
         return;

      element.ToolTip = tt;
   }

   /// <summary>
   /// Dynamically generates a StackPanel with map inference action buttons if the property's
   /// item type supports the IMapInferable contract.
   /// </summary>
   /// <param name="parent">The parent INUI object that owns the property.</param>
   /// <param name="navHistory"></param>
   /// <param name="property">The enum representing the property being displayed.</param>
   /// <param name="concreteItemType"></param>
   /// <param name="collection">The actual IList instance of the collection property.</param>
   /// <param name="nxPropType"></param>
   /// <returns>A StackPanel containing the action buttons, or null if the contract is not met.</returns>
   private static StackPanel? GenerateInferActions(INUI parent,
                                                   NUINavHistory navHistory,
                                                   Enum property,
                                                   Type nxPropType,
                                                   Type? concreteItemType,
                                                   IList? collection)
   {
      // Infer actions are disabled globally
      if (Config.Settings.NUIConfig.DisableNUIInferFromMapActions)
         return null;

      // Determine the target type for the IMapInferable<T> interface.
      // For collections, it's the item type. For single objects, it's the property type itself.
      var targetType = collection != null
                          ? concreteItemType
                          : nxPropType;

      Debug.Assert(targetType != null, "Targets type should not be null here.");

      var inferableInterfaceType = typeof(IMapInferable<>).MakeGenericType(targetType);
      if (!inferableInterfaceType.IsAssignableFrom(targetType))
         return null;

      // (Optional) Check for [DisableMapInferActions] attribute.
      var memberInfo = parent.GetType().GetMember(property.ToString()).FirstOrDefault();
      if (memberInfo?.IsDefined(typeof(DisableMapInferActionsAttribute), false) ?? false)
         return null;

      // --- Contract is met. Get reflection info. ---
      var getListMethod = targetType.GetMethod("GetInferredList", BindingFlags.Public | BindingFlags.Static);
      var getMapModeProp = targetType.GetProperty("GetMapMode", BindingFlags.Public | BindingFlags.Static);
      if (getListMethod == null || getMapModeProp == null)
         return null;

      var actionsPanel = new StackPanel
      {
         Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right,
      };

      // --- Button 1: Set Map Mode ---
      var mapMode = (IMapMode)getMapModeProp.GetValue(null)!;
      var mapModeButton = NEF.CreateMapModeButton(mapMode);
      mapModeButton.Click += (_, _) => { MapModeManager.Activate(mapMode.Type); };
      actionsPanel.Children.Add(mapModeButton);

      // --- Branching Logic: Generate buttons based on whether it's a collection or single item ---
      if (collection != null)
      {
         // --- Path A: It's a collection ---
         var addButton = NEF.CreateAddButton();
         addButton.Click += (_, _) =>
         {
            var selectedLocations = Selection.GetSelectedLocations;
            if (selectedLocations.Count == 0)
               return;

            var itemsToAdd = (IEnumerable)getListMethod.Invoke(null, [selectedLocations])!;

            // --- THIS IS THE ROBUST ADD LOGIC ---
            // Use reflection on the collection's ACTUAL runtime type to find AddRange.
            var addRangeMethod = collection.GetType().GetMethod("AddRange");

            if (addRangeMethod != null)
               // The fast path: The collection has an AddRange method.
               addRangeMethod.Invoke(collection, [itemsToAdd]);
            else
               // The safe fallback: Add items one by one.
               foreach (var item in itemsToAdd)
                  if (!collection.Contains(item))
                     collection.Add(item);

            GenerateAndSetView(navHistory);
         };
         actionsPanel.Children.Add(addButton);

         var removeButton = NEF.CreateRemoveButton();
         removeButton.Click += (_, _) =>
         {
            var selectedLocations = Selection.GetSelectedLocations;
            if (selectedLocations.Count == 0)
               return;

            var itemsToRemove = (IEnumerable)getListMethod.Invoke(null, [selectedLocations])!;
            foreach (var item in itemsToRemove)
            {
               collection.Remove(item);
               GenerateAndSetView(navHistory);
            }
         };
         actionsPanel.Children.Add(removeButton);
      }
      else
      {
         // --- Path B: It's a single value ---
         var setButton = NEF.CreateSetButton();
         setButton.Click += (_, _) =>
         {
            var selectedLocations = Selection.GetSelectedLocations;
            if (selectedLocations.Count == 0)
               return;

            var inferredList = (IEnumerable)getListMethod.Invoke(null, [selectedLocations])!;
            var firstItem = inferredList.Cast<object>().FirstOrDefault();

            if (firstItem != null)
            {
               Nx.ForceSet(firstItem, parent, property);
               GenerateAndSetView(navHistory);
            }
         };
         actionsPanel.Children.Add(setButton);
      }

      return actionsPanel;
   }

   private static void SetupCollectionEditorButton(NUINavHistory navHistory,
                                                   Enum nxProp,
                                                   Type providerInterfaceType,
                                                   Type itemType,
                                                   IList modifiableList,
                                                   TextBlock navHeader,
                                                   Grid grid,
                                                   BaseButton openButton)
   {
      if (providerInterfaceType.IsAssignableFrom(itemType))
      {
         RoutedEventHandler clickHandler = (_, _) =>
         {
            var methodInfo = itemType.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
               return;

            var allItems = (IEnumerable)methodInfo.Invoke(null, null)!;

            DualListSelector.CreateWindow(allItems, modifiableList, $"{nxProp} Editor").ShowDialog();

            navHeader.Text = $"{nxProp}: {modifiableList.Count} Items";
            GenerateCollectionItemPreview(navHistory, modifiableList.OfType<INUI>().ToList(), grid, modifiableList);
         };

         openButton.Click += clickHandler;
         openButton.Unloaded += (_, _) => openButton.Click -= clickHandler;
         openButton.ToolTip = "Open Collection Editor";
      }
      else
      {
         openButton.IsEnabled = true;
         openButton.Click += (_, _) =>
         {
            PrimitiveTypeListView.ShowDialog(modifiableList, modifiableList, $"{nxProp} Editor");
         };

         openButton.ToolTip = $"Open Primitive Collection Editor for {nxProp}({itemType.Name})";
      }
   }

   private static void GenerateCollectionItemPreview(NUINavHistory navHistory,
                                                     List<INUI> inuiItems,
                                                     Grid grid,
                                                     IList modifiableList)
   {
      if (inuiItems.Count == 0)
         return;

      for (var i = grid.Children.Count - 1; i >= 0; i--)
      {
         var child = grid.Children[i];
         if (Grid.GetRow(child) > 0 &&
             child.GetType() != typeof(Rectangle)) // Only remove children that are not in the header row
            grid.Children.RemoveAt(i);
      }

      // Clear all row definitions except the one for the header
      while (grid.RowDefinitions.Count > 1)
         grid.RowDefinitions.RemoveAt(1);

      foreach (var item in inuiItems.Take(Config.Settings.NUIConfig.MaxCollectionItemsPreviewed))
      {
         FrameworkElement shortInfo;
         if (item is ModValInstance instance)
            shortInfo = NEF.GetModValInstanceUI(instance);
         else
            shortInfo = GenerateShortInfo(item, navHistory);

         grid.RowDefinitions.Add(new() { Height = new(20, GridUnitType.Pixel) });
         grid.Children.Add(shortInfo);
         Grid.SetRow(shortInfo, grid.RowDefinitions.Count - 1);
         Grid.SetColumn(shortInfo, 0);
         Grid.SetColumnSpan(shortInfo, 2);
      }

      if (modifiableList.Count > Config.Settings.NUIConfig.MaxCollectionItemsPreviewed)
      {
         grid.RowDefinitions.Add(new() { Height = new(20, GridUnitType.Pixel) });
         grid.Children.Add(new TextBlock
         {
            Text = $"... and {modifiableList.Count - Config.Settings.NUIConfig.MaxCollectionItemsPreviewed} more items",
            FontStyle = FontStyles.Italic,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(6, 0, 0, 0),
         });
         Grid.SetRow(grid.Children[^1], grid.RowDefinitions.Count - 1);
         Grid.SetColumn(grid.Children[^1], 0);
         Grid.SetColumnSpan(grid.Children[^1], 2);
      }
   }

   private static RoutedEventHandler CollapseButtonOnClick(bool startExpanded,
                                                           List<FrameworkElement> collapsibleElements,
                                                           BaseButton collapseButton,
                                                           Border embedMarker,
                                                           Grid baseGrid)
   {
      return (_, _) =>
      {
         startExpanded = !startExpanded;
         var newVisibility = startExpanded ? Visibility.Visible : Visibility.Collapsed;

         foreach (var elem in collapsibleElements)
            elem.Visibility = newVisibility;

         var path = (Path)collapseButton.Content;
         var transform = (RotateTransform)path.LayoutTransform;
         var duration = new Duration(TimeSpan.FromMilliseconds(200));
         var rotationAnimation = new DoubleAnimation(startExpanded ? 180 : 0, duration);
         Grid.SetRowSpan(embedMarker, startExpanded ? baseGrid.RowDefinitions.Count : 1);
         transform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
      };
   }

   #endregion

   #region Reflection & Type Helpers

   private static void GenerateEmbeddedViewElements<T>(T target,
                                                       NUINavHistory navHistory,
                                                       Enum[] embeddedFields,
                                                       Grid baseGrid,
                                                       Visibility initialVisibility,
                                                       List<FrameworkElement> collapsibleElements) where T : INUI
   {
      for (var i = 0; i < embeddedFields.Length; i++)
      {
         var nxProp = embeddedFields[i];
         FrameworkElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            INUI value = null!;
            Nx.ForceGet(target, nxProp, ref value);
            if (value == null!)
               continue;

            element = GenerateShortInfo(value, navHistory);
         }
         else
         {
            element = BuildCollectionOrDefaultView(navHistory, type, [target], nxProp, 6);
         }

         baseGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);

         element.Visibility = initialVisibility;
         collapsibleElements.Add(element);
      }
   }

   /// <summary>
   /// Define the context menu for navigation options.
   /// Use <c>null</c> as a value in the <see cref="navigations"/> array to create a separator.
   /// </summary>
   /// <param name="navigations"></param>
   /// <param name="root"></param>
   /// <returns></returns>
   internal static ContextMenu GetContextMenu(INUINavigation?[] navigations, ContentPresenter root)
   {
      var contextMenu = new ContextMenu();
      foreach (var navigation in navigations)
      {
         if (navigation == null)
         {
            contextMenu.Items.Add(new Separator());
            continue;
         }

         var newItem = new MenuItem
         {
            FontSize = 12,
            FontWeight = FontWeights.Normal,
            Foreground = (Brush)Application.Current.FindResource("DefaultForeColorBrush")!,
            Header = navigation.ToolStripString,
            IsEnabled = navigation.IsEnabled,
         };

         if (navigation.Target != null)
            newItem.Command = new ActionCommand(() => { GenerateAndSetView(new(navigation.Target, true, root)); });
         else
            newItem.IsEnabled = false;

         contextMenu.Items.Add(newItem);
      }

      return contextMenu;
   }

   private static Type? GetCollectionItemType(Type collectionType)
   {
      if (collectionType == typeof(string) || !collectionType.IsGenericType)
         return null;

      var enumerableInterface = collectionType.GetInterfaces()
                                              .FirstOrDefault(i => i.IsGenericType &&
                                                                   i.GetGenericTypeDefinition() ==
                                                                   typeof(IEnumerable<>));

      return enumerableInterface?.GetGenericArguments()[0];
   }

   private static Type? GetArrayItemType(Type arrayType)
   {
      if (arrayType == typeof(string) || !arrayType.IsArray)
         return null;

      return arrayType.IsArray ? arrayType.GetElementType() : null;
   }

   /// <summary>
   /// Determines the concrete item type of a collection, even for empty collections where the
   /// declared item type is abstract or a generic parameter.
   /// </summary>
   /// <param name="collection">The collection instance.</param>
   /// <param name="ownerType">The concrete type of the object that owns the collection property.</param>
   /// <param name="propertyName">The name of the collection property on the owner.</param>
   /// <returns>The concrete type of the items, or null if it cannot be determined.</returns>
   private static Type? GetConcreteCollectionItemType(IEnumerable collection, Type ownerType, string propertyName)
   {
      // Strategy 1: The most reliable way is to check the first actual item.
      var firstItem = collection.Cast<object>().FirstOrDefault();
      if (firstItem != null)
         return firstItem.GetType();

      // Get the PropertyInfo for the property on the concrete owner type.
      // This gets us the most-derived version of the property (handles overrides).
      var propInfo = ownerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (propInfo == null)
         return null;

      // Get the item type from this property's type.
      var itemType = GetCollectionItemType(propInfo.PropertyType);
      if (itemType == null)
         return null;

      // If the item type is NOT a generic parameter (e.g., it's 'Location', not 'T'),
      // we've found our concrete type! This handles overrides correctly.
      if (!itemType.IsGenericTypeParameter)
         return itemType;

      // If we're here, the item type is a generic parameter (like 'T').
      // We need to find out what 'T' resolves to in our 'ownerType'.

      // Find the base class where this generic property was originally declared.
      var declaringType = propInfo.DeclaringType;
      if (declaringType is not { IsGenericType: true })
         return null; // Should not happen if itemType is a generic parameter.

      // Walk up the owner's inheritance chain to find the closed generic version of the declaring type.
      var currentType = ownerType;
      while (currentType != null && currentType != typeof(object))
      {
         if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == declaringType)
         {
            // We found the closed type, e.g., LocationCollection<Location>.
            // Now we map the generic parameter 'T' to its concrete type 'Location'.
            var genericArgsOnDef = declaringType.GetGenericArguments();
            var typeParamIndex = Array.IndexOf(genericArgsOnDef, itemType);

            if (typeParamIndex != -1)
               // Use the index to get the concrete type from the closed generic type.
               return currentType.GetGenericArguments()[typeParamIndex];
         }

         currentType = currentType.BaseType;
      }

      return null;
   }

   /// <summary>
   /// Gets a user-friendly string for an object. If the object has a custom
   /// ToString() override, it's used. Otherwise, the class's simple name is returned.
   /// Numeric types are formatted using invariant culture.
   /// </summary>
   private static string GetDisplayString(object? obj)
   {
      if (obj is null)
         return "null";

      if (obj is IConvertible convertible)
         switch (convertible.GetTypeCode())
         {
            case TypeCode.Single: // float
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Byte:
            case TypeCode.SByte:
               return convertible.ToString(CultureInfo.InvariantCulture);
         }

      var type = obj.GetType();

      if (type.GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType != typeof(object))
         return obj.ToString() ?? string.Empty;

      return $"<{type.Name}>";
   }

   #endregion
}