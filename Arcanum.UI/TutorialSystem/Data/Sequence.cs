namespace Arcanum.UI.TutorialSystem.Data;

//TODO delete Sequence... it will be replaced by Chapter only structure
public class Sequence(string title, string description, List<Chapter> chapters)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public readonly List<Chapter> Chapters = chapters;
}