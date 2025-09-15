using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Arcanum.UI.TutorialSystem.Data;

public class Step(
    string title,
    string description,
    Func<List<FrameworkElement>> getHighlightElements)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public bool IsCompleted { get; set; } = false;
    public Chapter? ParentChapter { get; set; } = null;

    public readonly Func<List<FrameworkElement>> GetHighlightElements = getHighlightElements;

    public virtual FrameworkElement? GetInteractiveElement() => null;
    
    public virtual bool IsInteractive => false;

    public virtual void SetUp(Action onComplete)
    {
    }

    public virtual void CleanUp()
    {
    }

    public virtual void Skip()
    {
    }
}

public abstract class InteractiveStep<T>(
    string title,
    string description,
    Func<List<FrameworkElement>> getHighlightElements,
    Func<T> getInteractiveElement)
    : Step(title, description, getHighlightElements) where T : FrameworkElement
{
    public override bool IsInteractive => true;
    protected T TargetControl { get; private set; } = null!;

    public override FrameworkElement GetInteractiveElement() => getInteractiveElement();

    public override void SetUp(Action onComplete)
    {
        TargetControl = getInteractiveElement();
    }
}

// Step for clicking a Button
public class ButtonStep(
    string title,
    string description,
    Func<List<FrameworkElement>> getHighlightElements,
    Func<Button> getInteractiveElement)
    : InteractiveStep<Button>(title, description, getHighlightElements, getInteractiveElement)
{
    private RoutedEventHandler? _handler;

    public override void SetUp(Action onComplete)
    {
        base.SetUp(onComplete);
        _handler = (_, _) => onComplete();
        TargetControl.Click += _handler;
    }

    public override void CleanUp()
    {
        Debug.Assert(_handler is not null);
        TargetControl.Click -= _handler;
    }

    public override void Skip()
    {
        TargetControl.RaiseEvent(new(ButtonBase.ClickEvent));
    }
}

// Step for entering text into a TextBox
public class TextBoxStep : InteractiveStep<TextBox>
{
    private TextChangedEventHandler? _handler;
    private readonly Predicate<string> _verifyText;
    private readonly string _exampleValidText;

    public TextBoxStep(string title,
        string description,
        Func<List<FrameworkElement>> getHighlightElements,
        Func<TextBox> getInteractiveElement,
        Predicate<string> verifyText, string exampleValidText) : base(title, description, getHighlightElements,
        getInteractiveElement)
    {
        if (verifyText(exampleValidText))
            throw new ArgumentException("The example valid text must pass the verification predicate.",
                nameof(exampleValidText));
        _verifyText = verifyText;
        _exampleValidText = exampleValidText;
    }

    public override void SetUp(Action onComplete)
    {
        base.SetUp(onComplete);
        _handler = (_, _) =>
        {
            if (_verifyText(TargetControl.Text))
            {
                onComplete();
            }
        };
        TargetControl.TextChanged += _handler;
    }

    public override void CleanUp()
    {
        Debug.Assert(_handler is not null);
        TargetControl.TextChanged -= _handler;
    }

    public override void Skip()
    {
        TargetControl.Text = _exampleValidText;
        if (_verifyText(TargetControl.Text))
        {
            _handler?.Invoke(this, null!);
        }
    }
}

// Step for selecting an item in a ComboBox
public class ComboBoxStep(
    string title,
    string description,
    Func<List<FrameworkElement>> getHighlightElements,
    Func<ComboBox> getInteractiveElement,
    int expectedItem = -1)
    : InteractiveStep<ComboBox>(title, description, getHighlightElements, getInteractiveElement)
{
    private SelectionChangedEventHandler? _handler;

    public override void SetUp(Action onComplete)
    {
        base.SetUp(onComplete);
        _handler = (_, _) =>
        {
            if (expectedItem == -1 || TargetControl.SelectedIndex == expectedItem)
            {
                onComplete();
            }
        };
        TargetControl.SelectionChanged += _handler;
    }

    public override void CleanUp()
    {
        Debug.Assert(_handler is not null);
        TargetControl.SelectionChanged -= _handler;
    }

    public override void Skip()
    {
        if (TargetControl.Items.Count <= 0) return;
        if (expectedItem != -1)
            TargetControl.SelectedIndex = expectedItem; // Select the first item
    }
}

// Step for checking/unchecking a CheckBox
public class CheckBoxStep(
    string title,
    string description,
    Func<List<FrameworkElement>> getHighlightElements,
    Func<CheckBox> getInteractiveElement,
    bool expectedState)
    : InteractiveStep<CheckBox>(title, description, getHighlightElements, getInteractiveElement)
{
    private RoutedEventHandler? _handler;

    public override void SetUp(Action onComplete)
    {
        base.SetUp(onComplete);
        _handler = (_, _) =>
        {
            if (TargetControl.IsChecked == expectedState)
            {
                onComplete();
            }
        };
        TargetControl.Checked += _handler;
        TargetControl.Unchecked += _handler;
    }

    public override void CleanUp()
    {
        Debug.Assert(_handler is not null);
        TargetControl.Checked -= _handler;
        TargetControl.Unchecked -= _handler;
    }

    public override void Skip()
    {
        TargetControl.IsChecked = expectedState;
        if (TargetControl.IsChecked == expectedState)
        {
            _handler?.Invoke(this, null!);
        }
    }
}