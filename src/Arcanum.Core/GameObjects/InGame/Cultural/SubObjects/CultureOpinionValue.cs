using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.GameObjects.InGame.Cultural.SubObjects;

[ObjectSaveAs(savingMethod: "SaveIAgsEnumKvp")]
#pragma warning disable ARC002
public partial class CultureOpinionValue : IEmpty<CultureOpinionValue>, IIagsEnumKvp<Culture, Opinion>
#pragma warning restore ARC002
{
   [SuppressAgs]
   [DefaultValue(null)]
   [Description("The culture this opinion is about.")]
   public Culture Key { get; set; } = Culture.Empty;

   [SuppressAgs]
   [DefaultValue(Opinion.Neutral)]
   [Description("The opinion value.")]
   public Opinion Value { get; set; } = Opinion.Neutral;

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.OpinionValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.OpinionValue;
   public string SavingKey => string.Empty;
   public static CultureOpinionValue Empty { get; } = new() { Key = Culture.Empty, Value = Opinion.Neutral };

   #endregion

   #region Equality Members

   protected bool Equals(CultureOpinionValue other) => Key.Equals(other.Key) && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((CultureOpinionValue)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(Key, Value);

   #endregion

   public override string ToString() => $"{Key.UniqueId}: {Value}";
}