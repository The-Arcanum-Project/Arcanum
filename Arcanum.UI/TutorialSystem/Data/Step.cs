using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.TutorialSystem.Data;

public abstract class Step(string title, string description, FrameworkElement targetControl, string targetControlName = "")
{
    public readonly string Title = title;
    public readonly string Description = description;
    public Chapter ParentChapter { get; set; }
    public readonly string TargetControlName = targetControlName;
    protected internal FrameworkElement? TargetControl = targetControl;

    public bool IsInteractive => this is not InfoStep;

    public abstract void Execute(Action onComplete);
    public abstract void CleanUp();
}

public class InfoStep(string title, string description, FrameworkElement targetControl, string targetControlName = "") : Step(title, description, targetControl, targetControlName)
{
    public override void Execute(Action onComplete)
    {
        /* Non-interactive, user clicks Next */
    }

    public override void CleanUp()
    {
        /* Nothing to clean */
    }
}

// Step for clicking a Button
public class ButtonStep(string title, string description, FrameworkElement targetControl, string targetControlName = "") : Step(title, description, targetControl, targetControlName)
{
    private RoutedEventHandler? _handler;

    public override void Execute(Action onComplete)
    {
        if (TargetControl is not Button button) return;
        _handler = (_, _) => onComplete();
        button.Click += _handler;
    }

    public override void CleanUp()
    {
        if (TargetControl is Button button && _handler is not null)
        {
            button.Click -= _handler;
        }
    }
}

// Step for entering text into a TextBox
public class TextBoxStep(string title, string description, FrameworkElement targetControl, string requiredText, string targetControlName = "") : Step(title, description, targetControl, targetControlName)
{
    private readonly string _requiredText = requiredText;
    private TextChangedEventHandler? _handler;

    public override void Execute(Action onComplete)
    {
        if (TargetControl is not TextBox textBox) return;
        _handler = (_, _) =>
        {
            if (string.IsNullOrEmpty(_requiredText) || textBox.Text.Equals(_requiredText))
            {
                onComplete();
            }
        };
        textBox.TextChanged += _handler;
    }

    public override void CleanUp()
    {
        if (TargetControl is TextBox textBox && _handler is null)
        {
            textBox.TextChanged -= _handler;
        }
    }
}

// Step for selecting an item in a ComboBox
public class ComboBoxStep(string title, string description, FrameworkElement targetControl, string requiredItemText, string targetControlName = "") : Step(title, description, targetControl, targetControlName)
{
    private readonly string _requiredItemText = requiredItemText;
    private SelectionChangedEventHandler? _handler;

    public override void Execute(Action onComplete)
    {
        if (TargetControl is ComboBox comboBox)
        {
            _handler = (_, _) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem item && (item.Content.ToString() == _requiredItemText || string.IsNullOrEmpty(_requiredItemText)))
                {
                    onComplete();
                }
            };
            comboBox.SelectionChanged += _handler;
        }
    }

    public override void CleanUp()
    {
        if (TargetControl is ComboBox comboBox && _handler is not null)
        {
            comboBox.SelectionChanged -= _handler;
        }
    }
}

// Step for checking/unchecking a CheckBox
public class CheckBoxStep(string title, string description, FrameworkElement targetControl, bool requiredState, string targetControlName = "") : Step(title, description, targetControl, targetControlName)
{
    private readonly bool _requiredState = requiredState;
    private RoutedEventHandler? _handler;

    public override void Execute(Action onComplete)
    {
        if (TargetControl is not CheckBox checkBox) return;
        _handler = (_, _) =>
        {
            if (checkBox.IsChecked == _requiredState)
            {
                onComplete();
            }
        };
        checkBox.Checked += _handler;
        checkBox.Unchecked += _handler;
    }

    public override void CleanUp()
    {
        if (TargetControl is not CheckBox checkBox || _handler is null) return;
        checkBox.Checked -= _handler;
        checkBox.Unchecked -= _handler;
    }
}