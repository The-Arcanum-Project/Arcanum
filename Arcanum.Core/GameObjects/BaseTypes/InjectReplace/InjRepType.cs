namespace Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

public enum InjRepType
{
   /// <summary>
   /// Default Value  
   /// </summary>
   None = 0,

   /// <summary>
   /// Properties are being injected into an already existing object from vanilla <br/>
   /// Throws an error if no existing object is found
   /// </summary>
   Inject = 1,

   /// <summary>
   /// Tries to inject into an existing object, if none is found, does not throw an error
   /// </summary>
   TryInject = 2,

   /// <summary>
   /// If an existing object to inject into is found, it injects into it, otherwise a new object is created
   /// </summary>
   InjectOrCreate = 3,

   /// <summary>
   /// An existing object is replaced by the defined one <br/>
   /// Throws an error if no existing object is found
   /// </summary>
   Replace = 4,

   /// <summary>
   /// Tries to replace an existing object, if none is found, does not throw an error
   /// </summary>
   TryReplace = 5,

   /// <summary>
   /// If an existing object is found, it is replaced, otherwise a new object is created
   /// </summary>
   ReplaceOrCreate = 6,
}