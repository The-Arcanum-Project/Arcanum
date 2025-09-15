using System.Windows;
using System.Windows.Forms;

namespace Arcanum.UI.TutorialSystem.Data;

public abstract class Chapter(string title, string description, List<Chapter> subChapters)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public readonly List<Chapter> SubChapters = subChapters;
}

public class InteractiveChapter : Chapter
{
    public InteractiveChapter(string title, string description, List<Step> steps) : base(title, description, [])
    {
        Steps = steps;
        foreach (var step in steps)
            step.ParentChapter = this;
    }

    public List<Step> Steps { get; }
}

public class StructureChapter : Chapter
{
    public readonly Step HighlightStep;
    
    public StructureChapter(List<Chapter> subChapters, string title, string description, List<FrameworkElement> highlightElements)
        : this([], new (title, description, () => highlightElements))
    {
        
    }

    public StructureChapter(List<Chapter> subChapters,
        Step step) : base(step.Title, step.Description, subChapters)
    {
        HighlightStep = step;
        step.ParentChapter = this;
    }
}