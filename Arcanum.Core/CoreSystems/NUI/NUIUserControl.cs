using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace Arcanum.Core.CoreSystems.NUI;

public abstract class NUIUserControl(ViewType type = ViewType.View) : UserControl
{
   public NUIUserControl(INUI parent, ViewType type, List<INavigate> navigations) : this(type)
   {
      Navigations = navigations;
      Parent = parent;
   }

   public NUIUserControl(INUI parent, ViewType type = ViewType.View) : this(parent, type, [])
   {
   }

   /// <summary>
   /// The type of view this control represents.
   /// </summary>
   public ViewType Type { get; } = type;

   /// <summary>
   /// The root of the NUI hierarchy this control belongs to.
   /// This is used to navigate between different views and controls.
   /// </summary>
   public NUIRoot Root { get; set; } = null!;

   /// <summary>
   /// The parent of this control in the NUI hierarchy.
   /// </summary>
   public INUI Parent { get; set; }

   /// <summary>
   /// An optional list of elements that can be navigated to from this MainItems RMB
   /// </summary>
   public List<INavigate> Navigations { get; } = [];

   /// <summary>
   /// This should trigger when the main item of this control is clicked, like a title bar or header.
   /// Should also be part of the same control the <see cref="OnMainItemMouseUp"/> is bound to.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void OnMainItemClick(object sender, MouseButtonEventArgs e)
   {
      Root.SetView(Parent.GetObjectView());
   }

   /// <summary>
   /// Opens a context menu with all <see cref="Navigations"/> of this <see cref="NUIUserControl"/> <br/><br/>
   /// This should be bound to whatever UI element is the main item in this control.
   /// like a title bar, header, or main content area. <br/><br/>
   /// Should be part of the same control the <see cref="OnMainItemClick"/> is bound to.
   /// We use MouseUp so that the click can handle the default action
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public virtual void OnMainItemMouseUp(object sender, MouseButtonEventArgs e)
   {
      if (e.ChangedButton == MouseButton.Right)
      {
         var contextMenu = new ContextMenu();
         foreach (var navigation in Navigations)
            contextMenu.Items.Add(new MenuItem
            {
               Header = navigation.ToolStripString, Command = navigation.Command,
            });

         if (contextMenu.Items.Count > 0)
         {
            contextMenu.PlacementTarget = sender as UIElement ?? this;
            contextMenu.IsOpen = true;
            e.Handled = true;
         }
      }
   }
}