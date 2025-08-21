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
   [IgnoreModifiable]
   public bool IsReadonly { get; }
   [IgnoreModifiable]
   public NUISetting Settings { get; }

   /// <summary>
   /// An optional list of elements that can be navigated to from this MainItems RMB
   /// </summary>
   [IgnoreModifiable]
   public INUINavigation[] Navigations { get; }
}