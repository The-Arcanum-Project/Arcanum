namespace Arcanum.UI.TutorialSystem.Data;

public class Chapter(string title, string description, List<Step> steps, List<Chapter> subChapters)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public readonly List<Step> Steps = steps;
    public List<Chapter> SubChapters = subChapters;
}