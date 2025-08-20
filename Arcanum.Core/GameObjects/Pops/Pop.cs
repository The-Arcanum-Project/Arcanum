using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.NUIUserControls;
using Common.UI.NUI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.Pops;

public partial class Pop(PopType type,
                         float size,
                         string culture,
                         string religion)
   : INUI
{
   public PopType Type { get; set; } = type;
   public float Size { get; set; } = size;
   public string Culture { get; set; } = culture;
   public string Religion { get; set; } = religion;

   public override string ToString()
   {
      return $"{Type.Name} ({Size})";
   }

   public override bool Equals(object? obj)
   {
      if (obj is Pop other)
         return Type == other.Type && Size.Equals(other.Size) && Culture == other.Culture && Religion == other.Religion;
      return false;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Type, Size, Culture, Religion);
   }

   public static bool operator ==(Pop? left, Pop? right)
   {
      if (left is null && right is null)
         return true;

      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(Pop? left, Pop? right)
   {
      return !(left == right);
   }

   public NUIUserControl GetObjectView(Enum[]? e = null)
   {
      var fields = e ?? [Field.Culture, Field.Type];
      foreach (var field in fields)
      {
         var fd = (Field)field;
         var type = Nx.TypeOf(this, fd);
      }
      
      throw new NotImplementedException($"GetObjectView for {this.GetType().Name} is not implemented yet.");
   }

   public NUIUserControl GetEmbeddedView(Enum[] e) => throw new NotImplementedException();

   public NUIUserControl GetEmbeddedEditorView(Enum[]? e = null) => throw new NotImplementedException();

   public NUIUserControl GetShortInfoView(Enum[]? e = null) => throw new NotImplementedException();
   public KeyValuePair<string, string> GetTitleAndSubTitle() => new(Type.Name, Culture);

   public NUIUserControl GetBaseUI(ViewType view)
   {
      return new DefaultNUI(this, view)
      {
         DataContext = this,
      };
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}