using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public interface IAgs : INexus
{
   /// <summary>
   /// The settings that control how this IAgs instance is saved. <br/>
   /// Should ling to a settings object in <c>Config.Settings.Ags.'objectName'AgsSettings</c>/>
   /// </summary>
   public AgsSettings Settings { get; }

   /// <summary>
   /// A list of properties that can be saved for this IAgs instance.
   /// </summary>
   [IgnoreModifiable]
   public IReadOnlyList<PropertySavingMetadata> SaveableProps { get; }

   /// <summary>
   /// Returns a new AgsObjectSavingContext initialized for this IAgs instance.
   /// </summary>
   /// <returns></returns>
   public AgsObjectSavingContext ToAgsContext(string commentChar = "#")
      => new(this, commentChar); //TODO fix this by using FileInformation

   /// <summary>
   /// Returns the metadata for the class implementing this IAgs instance. <br/>
   /// This includes information such as the class name, namespace, and any relevant attributes.
   /// </summary>
   public ClassSavingMetadata ClassMetadata { get; }

   /// <summary>
   /// The key used to identify this object in the saved file. <br/>
   /// Typically corresponds to the object's type name or a unique identifier. <br/>
   /// Example: "Player", "Enemy", "InventoryItem"
   /// </summary>
   public string SavingKey { get; }
}