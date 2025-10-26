using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.Core.CoreSystems.History.Dtos;

public static class IEu5ObjectDtoConverter
{
   public static Eu5Dto[] ToDtoArray(IEnumerable<IEu5Object> objs) => objs.Select(ToDto).ToArray();
   public static Eu5Dto ToDto(IEu5Object obj) => new(obj.UniqueId, obj.GetType().Name);

   public static IEu5Object[] FromDtoArray(IEnumerable<Eu5Dto> dtos) => dtos.Select(FromDto<IEu5Object>).ToArray();

   public static T FromDto<T>(Eu5Dto dto) where T : IEu5Object
   {
      if (!EmptyRegistry.TryGetEmpty(dto.TypeName, out var empty))
         throw new InvalidOperationException("EmptyRegistry is not initialized!");

      var globals = ((IEu5Object)empty).GetGlobalItemsNonGeneric();
      if (!globals.Contains(dto.Key))
         throw new KeyNotFoundException($"Object with key '{dto.Key}' of type '{dto.TypeName}' not found in globals.");

      return (T)globals[dto.Key]!;
   }
}

public class Eu5Dto(string key, string type)
{
   public string Key { get; } = key;
   public string TypeName { get; } = type;
}