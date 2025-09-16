namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// Automatic Generated Saving settings.
/// </summary>
public class AgsSettings
{
   /// <summary>
   /// If true, the saving order will follow the order specified in SaveOrder. <br/>
   /// If false, the default alphabetical order will be used.
   /// </summary>
   public bool CustomSaveOrder { get; set; } = false;

   /// <summary>
   /// If true, a comment will be added before the object
   /// </summary>
   public bool HasSavingComment { get; set; } = true;

   /// <summary>
   /// If true, empty collections will still write their header (e.g., "MyList = { \n\n }")
   /// </summary>
   public bool WriteEmptyCollectionHeader { get; set; } = true;

   /// <summary>
   /// The order in which properties will be saved if CustomSaveOrder is true. <br/>
   /// The list should contain Enum values corresponding to the properties to be saved. 
   /// </summary>
   public List<Enum> SaveOrder { get; set; }

   /// <summary>
   /// The format to use when saving. <br/>
   /// Default is <see cref="SavingFormat.Default"/>
   /// </summary>
   public SavingFormat Format { get; set; } = SavingFormat.Default;
}