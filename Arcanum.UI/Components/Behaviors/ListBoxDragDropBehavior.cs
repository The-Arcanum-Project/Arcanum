using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.UI.Helpers;
using Microsoft.Xaml.Behaviors;

namespace Arcanum.UI.Components.Behaviors;

public class ListBoxDragDropBehavior : Behavior<ListBox>
{
    private object? _draggedItem;
    private bool _isDragging;
    private ScrollViewer? _scrollViewer;
    // Dependency Property to specify which control types should NOT start a drag.
    public static readonly DependencyProperty DragHandleExclusionTypeProperty =
        DependencyProperty.Register(
            nameof(DragHandleExclusionType),
            typeof(Type),
            typeof(ListBoxDragDropBehavior),
            new PropertyMetadata(null));
    
    public static readonly DependencyProperty ScrollAreaHeightProperty =
        DependencyProperty.Register(
            nameof(ScrollAreaHeight),
            typeof(int),
            typeof(ListBoxDragDropBehavior),
            new PropertyMetadata(50)); // Default scroll area height
    
    public Type DragHandleExclusionType
    {
        get => (Type)GetValue(DragHandleExclusionTypeProperty);
        set => SetValue(DragHandleExclusionTypeProperty, value);
    }
    
    public int ScrollAreaHeight
    {
        get => (int)GetValue(ScrollAreaHeightProperty);
        set => SetValue(ScrollAreaHeightProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        // Subscribe to the events on the ListBox this behavior is attached to.
        AssociatedObject.AllowDrop = true;
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.Drop += OnDrop;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.QueryContinueDrag += OnQueryContinueDrag;
        AssociatedObject.Loaded += (s, e) => _scrollViewer = TreeTraversal.FindVisualChild<ScrollViewer>(AssociatedObject);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        // Unsubscribe to prevent memory leaks.
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.Drop -= OnDrop;
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.QueryContinueDrag -= OnQueryContinueDrag;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // If an exclusion type is set (e.g., Button), check if the click was on it.
        if (DragHandleExclusionType != null)
        {
            var element = e.OriginalSource as DependencyObject;
            if (TreeTraversal.FindVisualParent(element, DragHandleExclusionType) != null)
            {
                // Click was on an excluded control, so do nothing.
                _draggedItem = null;
                return;
            }
        }
        e.Handled = true;
        //e.Handled = true;
        _draggedItem = GetItemUnderMouse(e.GetPosition(AssociatedObject));
    }
    
    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (_scrollViewer == null) return;

        const double scrollTolerance = 10; // Proximity to edge to start scrolling
        Point mousePosition = e.GetPosition(AssociatedObject);

        if (mousePosition.Y < scrollTolerance)
        {
            _scrollViewer.LineUp();
        }
        else if (mousePosition.Y > AssociatedObject.ActualHeight - scrollTolerance)
        {
            _scrollViewer.LineDown();
        }
    }
    
    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedItem != null && e.LeftButton == MouseButtonState.Pressed && !_isDragging)
        {
            _isDragging = true;
            DragDrop.DoDragDrop(AssociatedObject, _draggedItem, DragDropEffects.Move);
        }
        e.Handled = true;
    }
    
    private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        if (e.Action == DragAction.Cancel || e.Action == DragAction.Drop)
        {
            e.Handled = true;
        }

        // Always reset after drag ends (cancel or drop), regardless of target.
        _isDragging = false;
        _draggedItem = null;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        _isDragging = false;
        var droppedData = e.Data.GetData(e.Data.GetFormats()[0]);
        var targetItem = GetItemUnderMouse(e.GetPosition(AssociatedObject));

        if (droppedData == null || targetItem == null || ReferenceEquals(droppedData, targetItem))
        {
            return;
        }

        if (AssociatedObject.ItemsSource is IList list)
        {
            int targetIndex = list.IndexOf(targetItem);
            if (targetIndex < 0) return;

            list.Remove(droppedData);
            list.Insert(targetIndex, droppedData);
        }
    }
    
    
    
    private object? GetItemUnderMouse(Point position)
    {
        var element = AssociatedObject.InputHitTest(position) as DependencyObject;
        var listBoxItem = TreeTraversal.FindVisualParent<ListBoxItem>(element);
        return (listBoxItem != null) ? AssociatedObject.ItemContainerGenerator.ItemFromContainer(listBoxItem) : null;
    }
}