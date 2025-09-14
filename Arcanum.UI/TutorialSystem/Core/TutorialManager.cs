// Tutorial/Core/TutorialManager.cs

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Arcanum.UI.TutorialSystem.Data;
using Arcanum.UI.TutorialSystem.UI;

namespace Arcanum.UI.TutorialSystem.Core;

public class TutorialManager
{
    private readonly Window _mainWindow;
    private readonly FrameworkElement _adornedElement;
    private readonly AdornerLayer _adornerLayer;

    private Sequence? _currentSequence;
    private List<Step>? _flatSteps;
    private int _currentStepIndex = -1;

    private TutorialWindow? _tutorialWindow;
    private TutorialAdorner? _currentAdorner;

    public TutorialManager(Window mainWindow, FrameworkElement elementToAdorn)
    {
        _mainWindow = mainWindow;
        _adornedElement = elementToAdorn;
        _adornerLayer = AdornerLayer.GetAdornerLayer(_adornedElement) ?? throw new InvalidOperationException();
        if (_adornerLayer == null)
            throw new InvalidOperationException("Could not find AdornerLayer.");
    }

    public void Start(Sequence sequence)
    {
        if (_tutorialWindow != null && _tutorialWindow.IsLoaded) return;

        _currentSequence = sequence;

        _tutorialWindow = new TutorialWindow { Owner = _mainWindow };

        // Subscribe to all events from the window
        _tutorialWindow.TutorialStarted += OnTutorialStarted;
        _tutorialWindow.RequestNext += GoToNextStep;
        _tutorialWindow.RequestPrevious += GoToPreviousStep;
        _tutorialWindow.RequestSkipChapter += SkipChapter;
        _tutorialWindow.RequestFinish += Finish;
        _tutorialWindow.Closed += (s, e) => Finish(); // Ensure cleanup if user closes window with 'X'

        // Tell the window to display the initial start view
        _tutorialWindow.ShowStartView(sequence);
        _tutorialWindow.Show();
    }

    private void OnTutorialStarted()
    {
        if (_currentSequence == null) return;
        _flatSteps = FlattenChapters(_currentSequence.Chapters);
        if (_flatSteps.Count == 0)
        {
            Finish();
            return;
        }

        _currentStepIndex = 0;
        ShowStep(_currentStepIndex);
    }

    private void ShowStep(int index)
    {
        // Clean up the previous step
        if (_currentStepIndex >= 0 && _currentStepIndex < _flatSteps.Count)
        {
            _flatSteps[_currentStepIndex].CleanUp();
        }

        RemoveAdorner();

        _currentStepIndex = index;
        var currentStep = _flatSteps[_currentStepIndex];

        // Find the target control by its name at runtime
        currentStep.TargetControl ??= FindControlByName(_adornedElement, currentStep.TargetControlName);

        _tutorialWindow?.ShowStepView(currentStep, _currentStepIndex + 1, _flatSteps.Count);

        if (currentStep.TargetControl == null) return;
        // Add new adorner
        _currentAdorner = new TutorialAdorner(_adornedElement, currentStep.TargetControl, currentStep.IsInteractive);
        _adornerLayer.Add(_currentAdorner);

        // Start listening for the required action
        currentStep.Execute(GoToNextStep);
    }

    private void GoToNextStep() => Dispatcher.CurrentDispatcher.Invoke(() =>
    {
        if (_currentStepIndex < _flatSteps.Count - 1) ShowStep(_currentStepIndex + 1);
        else Finish();
    });

    private void GoToPreviousStep()
    {
        if (_currentStepIndex > 0) ShowStep(_currentStepIndex - 1);
    }

    private void SkipChapter()
    {
        var currentChapter = _flatSteps[_currentStepIndex].ParentChapter;
        int nextIndex = _flatSteps.FindIndex(_currentStepIndex + 1, s => s.ParentChapter != currentChapter);

        if (nextIndex != -1) ShowStep(nextIndex);
        else Finish();
    }

    private void AfterFinish()
    {
        MessageBox.Show("Tutorial Complete!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Finish()
    {
        if (_currentStepIndex >= 0)
            _flatSteps[_currentStepIndex].CleanUp();
        if (_currentAdorner != null)
        {
            RemoveAdorner();
            AfterFinish();
        }

        if (_tutorialWindow != null)
        {
            // Unsubscribe from events to prevent memory leaks
            _tutorialWindow.TutorialStarted -= OnTutorialStarted;
            _tutorialWindow.RequestNext -= GoToNextStep;
            _tutorialWindow.RequestPrevious -= GoToPreviousStep;
            _tutorialWindow.RequestSkipChapter -= SkipChapter;
            _tutorialWindow.RequestFinish -= Finish;

            _tutorialWindow.Close();
            _tutorialWindow = null;
        }
    }

    private void RemoveAdorner()
    {
        if (_currentAdorner != null)
        {
            _adornerLayer.Remove(_currentAdorner);
            _currentAdorner = null;
        }
    }

    private List<Step> FlattenChapters(List<Chapter> chapters)
    {
        var steps = new List<Step>();
        foreach (var chapter in chapters)
        {
            AddChapterSteps(chapter, steps);
        }

        return steps;
    }

    private void AddChapterSteps(Chapter chapter, List<Step> steps)
    {
        foreach (var step in chapter.Steps)
        {
            step.ParentChapter = chapter;
            steps.Add(step);
        }

        foreach (var subChapter in chapter.SubChapters)
        {
            AddChapterSteps(subChapter, steps);
        }
    }

    private FrameworkElement FindControlByName(DependencyObject parent, string name)
    {
        if (parent == null || string.IsNullOrEmpty(name)) return null;

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            var result = FindControlByName(child, name);
            if (result != null) return result;
        }

        return null;
    }
}