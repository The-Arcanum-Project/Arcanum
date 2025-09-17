// Tutorial/UI/TutorialWindow.xaml.cs

using System.Windows;
using System.Windows.Input;
using Arcanum.UI.TutorialSystem.Data;

namespace Arcanum.UI.TutorialSystem.UI;

public partial class TutorialWindow
{
    // Events for the TutorialManager to subscribe to
    public event Action? TutorialStarted;
    public event Action? RequestPrevious;
    public event Action? RequestNext;
    public event Action? RequestSkipChapter;
    public event Action? RequestFinish;

    public TutorialWindow()
    {
        InitializeComponent();
#if DEBUG
        // Define a simple command
        var myCommand = new RoutedUICommand();

        // Add the InputBinding (Ctrl+M for example)
        InputBindings.Add(
            new KeyBinding(myCommand, new KeyGesture(Key.E, ModifierKeys.Control))
        );

        // Attach the command handler
        CommandBindings.Add(
            new(myCommand, (_, _) => OpenEditor())
        );
#endif
    }

#if DEBUG
    private void OpenEditor()
    {
    }
#endif

    /// <summary>
    /// Configures the window to show the initial tutorial overview.
    /// </summary>
    public void ShowStartView(Chapter parentChapter)
    {
        // Set content
        Title = parentChapter.Title;
        SequenceTitleTextBlock.Text = parentChapter.Title;
        SequenceDescriptionTextBlock.Text = parentChapter.Description;
        ChaptersListBox.Items.Clear();
        foreach (var chapter in parentChapter.SubChapters)
        {
            ChaptersListBox.Items.Add(chapter.Title);
        }

        // Switch visibility
        StartViewGrid.Visibility = Visibility.Visible;
        StepViewGrid.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Configures the window to show a specific step's details.
    /// </summary>
    public void ShowStepView(Step step, int currentStepNum, int totalSteps)
    {
        // Switch visibility
        if (StartViewGrid.Visibility == Visibility.Visible)
        {
            StartViewGrid.Visibility = Visibility.Collapsed;
            StepViewGrid.Visibility = Visibility.Visible;
            // Adjust window size for the step view if desired
            this.Height = 280;
            this.Width = 350;
        }

        // Set content
        this.Title = $"Tutorial ({currentStepNum}/{totalSteps})";
        ChapterTitleTextBlock.Text = step.ParentChapter.Title ?? "Tutorial";
        StepTitleTextBlock.Text = step.Title;
        StepDescriptionTextBlock.Text = step.Description;

        // Update button states
        NextButton.IsEnabled = !step.IsInteractive;
        PreviousButton.IsEnabled = currentStepNum > 1;
        FinishButton.Visibility = (currentStepNum == totalSteps) ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Visibility = (currentStepNum < totalSteps) ? Visibility.Visible : Visibility.Collapsed;
    }

    // --- Event Invokers ---
    private void StartButton_Click(object sender, RoutedEventArgs e) => TutorialStarted?.Invoke();
    private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();
    private void PreviousButton_Click(object sender, RoutedEventArgs e) => RequestPrevious?.Invoke();
    private void NextButton_Click(object sender, RoutedEventArgs e) => RequestNext?.Invoke();
    private void SkipChapterButton_Click(object sender, RoutedEventArgs e) => RequestSkipChapter?.Invoke();
    private void FinishButton_Click(object sender, RoutedEventArgs e) => RequestFinish?.Invoke();
}