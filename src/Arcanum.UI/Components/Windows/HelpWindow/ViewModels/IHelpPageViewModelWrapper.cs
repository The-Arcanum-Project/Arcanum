#region

using Arcanum.Core.ApplicationContext;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public interface IHelpPageViewModelWrapper : IAppContext
{
   /// <summary>
   /// Activates the feature tab for the given feature. If the feature is not found, it does nothing.
   /// </summary>
   /// <param name="feature">The feature to focus</param>
   public void ActivateFeatureTabFor(FeatureDoc feature);

   /// <summary>
   /// Shows the next tip in the dashboard view. If there are no tips, it does nothing.
   /// </summary>
   public void ShowNextTip();

   /// <summary>
   /// Shows the previous tip in the dashboard view. If there are no tips, it does nothing.
   /// </summary>
   public void ShowPreviousTip();
}