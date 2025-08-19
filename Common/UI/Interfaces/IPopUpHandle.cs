namespace Common.UI.Interfaces;

public interface IPopUpHandle
{
   /// <summary>
   /// Opens a new settings window and navigates to the specified property.
   /// </summary>
   /// <returns></returns>
   public void NavigateToSetting(string[] path);
}