namespace Arcanum.API.UtilServices.Search.SearchableSetting;

[AttributeUsage(AttributeTargets.Property)]
public class IconPathSettingsAttribute(string path) : Attribute
{
   public string IconPath { get; } = path;
}