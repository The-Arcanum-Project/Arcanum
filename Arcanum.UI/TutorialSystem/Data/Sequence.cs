namespace Arcanum.UI.TutorialSystem.Data;

public class Sequence(string title, string description, List<Chapter> chapters)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public readonly List<Chapter> Chapters = chapters;
}