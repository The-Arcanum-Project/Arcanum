using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// This interface is for objects that are embedded within <see cref="IEu5Object{T}"/>. <br/>
/// They can be saved/loaded and provide collections of themselves. <br/>
/// They also support NUI for editing in the editor. <br/>
/// The following interfaces are inherited: <br/>
/// - <see cref="INUI"/>: For NUI support <br/>
/// - <see cref="IAgs"/>: For AGS saving/loading support <br/>
/// - <see cref="ICollectionProvider{T}"/>: To provide collections of themselves <br/>
/// The following constraints apply to T: <br/>
/// - T must implement <see cref="IEu5EmbeddedObject{T}"/> (this interface) <br/>
/// - T must have a public parameterless constructor (<c>new()</c>) <br/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEu5EmbeddedObject<T> : INUI, IAgs, ICollectionProvider<T>
   where T : IEu5EmbeddedObject<T>, new();