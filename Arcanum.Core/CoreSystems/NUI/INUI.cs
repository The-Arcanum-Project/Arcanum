using System.ComponentModel;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Usage: <br/>
/// Every object which is displayable in Arcanum's NUI system must implement this interface. <br/>
/// With all objects using <see cref="INUI"/> we can explore all fields and if it is an object which
/// in return also implements <see cref="INUI"/> we display the appropriate view for it.
/// If it does not implements <see cref="INexus"/> we dynamically generate an interface base on the types.<br/>
/// Following this we can display any kind of object in the NUI system,
/// as long as it implements <see cref="INUI"/> and <see cref="INexus"/>.<br/>
/// </summary>
public interface INUI : INexus, INotifyPropertyChanged
{
   /// <summary>
   /// The enum defines the <see cref="INexus"/> fields which should be displayed.
   /// Default is null in which the settings will be pulled and fields displayed accordingly.
   /// </summary>
   /// <param name="e"></param>
   /// <returns></returns>
   public NUIUserControl GetObjectView(Enum[]? e = null);

   public NUIUserControl GetEmbeddedView(Enum[] e);
   public NUIUserControl GetEmbeddedEditorView(Enum[]? e = null);
   public NUIUserControl GetShortInfoView(Enum[]? e = null);

   public KeyValuePair<string, string> GetTitleAndSubTitle();
   public NUIUserControl GetBaseUI(ViewType view);
}