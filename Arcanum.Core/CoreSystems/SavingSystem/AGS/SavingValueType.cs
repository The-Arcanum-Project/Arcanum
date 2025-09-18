namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// The types of values that can be saved in an AGS file.
/// </summary>
public enum SavingValueType
{
   /// <summary>
   /// An object implementing <see cref="IAgs"/>. <br/>
   /// This indicates that the property is a complex object that should be saved using its own saving logic.
   /// </summary>
   IAgs,

   /// <summary>
   /// Infers the type automatically on runtime (based on the property type). <br/>
   /// This is the default option and should be used unless there is a specific reason to enforce another type.
   /// </summary>
   Auto,

   /// <summary>
   /// A string value encapsulated in quotes. <br/>
   /// Example: "Hello, World!"
   /// </summary>
   String,

   /// <summary>
   /// An integer value. <br/>
   /// Example: 42
   /// </summary>
   Int,

   /// <summary>
   /// A floating-point number. <br/>
   /// Example: 3.14
   /// </summary>
   Float,

   /// <summary>
   /// A boolean value represented as "yes" or "no". <br/>
   /// Example: yes
   /// </summary>
   Bool,

   /// <summary>
   /// A double-precision floating-point number. <br/>
   /// Example: 2.71828
   /// </summary>
   Double,

   /// <summary>
   /// An identifier, typically used for names or keys without quotes. <br/>
   /// Example: arcanum_great
   /// </summary>
   Identifier,

   /// <summary>
   /// An <see cref="Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor.JominiColor"/> value in a specific format. <br/>
   /// Example: rgb = { 255 0 0 } ; hsv = { 0 100 100 } ; hsv360 = { 0.4 0.5 0.5 }
   /// </summary>
   Color,

   /// <summary>
   /// An enumeration value represented by its name. <br/>
   /// Example: high
   /// </summary>
   Enum,

   /// <summary>
   /// A flags enumeration value represented by a combination of names. <br/>
   /// Example: key = flag1 <br/>
   ///          key = flag2 <br/>
   ///          key = flag3 
   /// </summary>
   FlagsEnum,

   /// <summary>
   /// A modifiers instance <see cref="Arcanum.Core.CoreSystems.Jomini.ModifierSystem"/>
   /// </summary>
   Modifier,
}