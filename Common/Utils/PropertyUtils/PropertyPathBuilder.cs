using System.Reflection;

namespace Common.Utils.PropertyUtils;

public static class PropertyPathBuilder
{
   public static string[] GetPathToProperty(object root, PropertyInfo target)
   {
      var path = new List<string>();
      if (TryBuild(root, target, path))
         return path.ToArray();

      return [];
   }

   private static bool TryBuild(object current, PropertyInfo target, List<string> path)
   {
      var type = current.GetType();

      foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
         // skip indexers
         if (prop.GetIndexParameters().Length > 0)
            continue;

         path.Add(prop.Name);

         if (prop.MetadataToken == target.MetadataToken 
             && prop.Module == target.Module)
            return true;

         var value = prop.GetValue(current);
         if (value != null && TryBuild(value, target, path))
            return true;

         path.RemoveAt(path.Count - 1);
      }

      return false;
   }
}