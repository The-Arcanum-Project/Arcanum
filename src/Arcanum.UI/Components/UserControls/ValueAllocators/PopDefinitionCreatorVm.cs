using System.ComponentModel;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using Arcanum.UI.Components.Windows.PopUp;
using CommunityToolkit.Mvvm.Input;
using Culture = Arcanum.Core.GameObjects.InGame.Cultural.Culture;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using PopDefinition = Arcanum.Core.GameObjects.InGame.Pops.PopDefinition;
using PopType = Arcanum.Core.GameObjects.InGame.Pops.PopType;
using Religion = Arcanum.Core.GameObjects.InGame.Religious.Religion;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public class PopDefinitionCreatorVm : ViewModelBase
{
   public RelayCommand CreatePopDefCommand { get; } = null!;
   public RelayCommand CreatePopDefFromSampeCommand { get; } = null!;

   public PopType PopType
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public Religion Religion
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public Culture Culture
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public int Value
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public float RandomOffsetPercentage
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = Config.Settings.SpecializedEditorSettings.PopEditorSettings.PopCreationRandomOffsetPercentage;

   public PopDefinition.Field Condition1
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = PopDefinition.Field.Culture;

   public PopDefinition.Field Condition2
   {
      get;
      set
      {
         if (value.Equals(field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = PopDefinition.Field.Religion;

   public IEu5Object Condition1Value
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = Culture.Empty;

   public IEu5Object Condition2Value
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = Religion.Empty;

   public IEu5Object[] Condition1PossibleValues
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = null!;

   public IEu5Object[] Condition2PossibleValues
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   } = null!;

   public static PopDefinition.Field[] SamplePopDefinitionSources => [PopDefinition.Field.PopType, PopDefinition.Field.Culture, PopDefinition.Field.Religion];

   private readonly Location _targetLocation;
   private readonly AllocatorViewModel _avm;

   public PopDefinitionCreatorVm(Location targetLocation, AllocatorViewModel avm)
   {
      _targetLocation = targetLocation;
      _avm = avm;

      PopType = CalculateItemWithMaxPop<PopType>(_targetLocation.Pops, PopDefinition.Field.PopType) ?? PopType.Empty;
      Religion = CalculateItemWithMaxPop<Religion>(_targetLocation.Pops, PopDefinition.Field.Religion) ?? Religion.Empty;
      Culture = CalculateItemWithMaxPop<Culture>(_targetLocation.Pops, PopDefinition.Field.Culture) ?? Culture.Empty;

      var allOfType = _targetLocation.Pops.Where(p => p.PopType == PopType).ToList();
      if (allOfType.Count == 0)
      {
         Value = Config.Settings.SpecializedEditorSettings.PopEditorSettings.DefaultPopSize;
         return;
      }

      var num = (allOfType.Sum(p => p.Size) / allOfType.Count * 1000d) *
                Config.Settings.SpecializedEditorSettings.PopEditorSettings.PopCreationRandomOffsetPercentage;
      Value = (int)num;

      CreatePopDefCommand = new(CreateNewPops);
      CreatePopDefFromSampeCommand = new(CreatePopDefFromSample);

      UpdateCondition1();
      UpdateCondition2();

      PropertyChanged += OnPropertyChanged;
   }

   private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if (e.PropertyName is nameof(Condition1))
         UpdateCondition1();
      else if (e.PropertyName is nameof(Condition2))
         UpdateCondition2();
   }

   private void UpdateCondition1()
   {
      Condition1PossibleValues = GetPossibleValuesForField(Condition1);
      Condition1Value = GetCurrent(Condition1);
   }

   private void UpdateCondition2()
   {
      Condition2PossibleValues = GetPossibleValuesForField(Condition2);
      Condition2Value = GetCurrent(Condition2);
   }

   private IEu5Object GetCurrent(PopDefinition.Field field)
   {
      return field switch
      {
         PopDefinition.Field.PopType => PopType,
         PopDefinition.Field.Culture => Culture,
         PopDefinition.Field.Religion => Religion,
         _ => throw new ArgumentOutOfRangeException(nameof(field)),
      };
   }

   private static IEu5Object[] GetPossibleValuesForField(PopDefinition.Field field)
   {
      return field switch
      {
         PopDefinition.Field.PopType => Globals.PopTypes.Values.Cast<IEu5Object>().ToArray(),
         PopDefinition.Field.Culture => Globals.Cultures.Values.Cast<IEu5Object>().ToArray(),
         PopDefinition.Field.Religion => Globals.Religions.Values.Cast<IEu5Object>().ToArray(),
         _ => throw new ArgumentOutOfRangeException(nameof(field)),
      };
   }

   private void CreatePopDefFromSample()
   {
      List<PopDefinition> samplePops = [];

      foreach (var loc in Globals.Locations.Values)
         foreach (var pop in loc.Pops)
         {
            var condition1Val = (IEu5Object)pop._getValue(Condition1);
            var condition2Val = (IEu5Object)pop._getValue(Condition2);
            if (Equals(condition1Val, Condition1Value) && Equals(condition2Val, Condition2Value))
               samplePops.Add(pop);
         }

      if (samplePops.Count == 0)
      {
         MBox.Show("No sample pops found with the specified conditions.", "No Sample Pops");
         return;
      }

      var totalSize = samplePops.Sum(p => p.Size) * 1000;
      var avgSize = totalSize / samplePops.Count;
      var offset = (int)(avgSize * (RandomOffsetPercentage));
      var randomizedSize = avgSize + Random.Shared.Next(Math.Min(-offset, 0), Math.Max(0, offset + 1));

      var popDef = new PopDefinition();
      switch (Condition1)
      {
         case PopDefinition.Field.PopType:
            popDef.PopType = (PopType)Condition1Value;
            break;
         case PopDefinition.Field.Culture:
            popDef.Culture = (Culture)Condition1Value;
            break;
         case PopDefinition.Field.Religion:
            popDef.Religion = (Religion)Condition1Value;
            break;
         default:
            throw new ArgumentOutOfRangeException();
      }

      switch (Condition2)
      {
         case PopDefinition.Field.PopType:
            popDef.PopType = (PopType)Condition2Value;
            break;
         case PopDefinition.Field.Culture:
            popDef.Culture = (Culture)Condition2Value;
            break;
         case PopDefinition.Field.Religion:
            popDef.Religion = (Religion)Condition2Value;
            break;
         default:
            throw new ArgumentOutOfRangeException();
      }

      popDef.Size = Math.Max(0, randomizedSize) / 1000d;

      PopDefinition.Field missingField;

      if (popDef.PopType == PopType.Empty)
         missingField = PopDefinition.Field.PopType;
      else if (popDef.Culture == Culture.Empty)
         missingField = PopDefinition.Field.Culture;
      else if (popDef.Religion == Religion.Empty)
         missingField = PopDefinition.Field.Religion;
      else
      {
         MBox.Show("All fields are already set from the sample conditions.", "No Missing Fields");
         return;
      }

      var possibleValues = GetPossibleValuesForField(missingField);
      popDef._setValue(missingField, possibleValues[Random.Shared.Next(possibleValues.Length)]);

      AddPopToVms(popDef);
   }

   private static T? CalculateItemWithMaxPop<T>(IList<PopDefinition> pops, Enum property) where T : IEu5Object
   {
      Dictionary<T, double> totals = new();

      foreach (var pop in pops)
      {
         var val = (T)pop._getValue(property);
         totals.TryAdd(val, 0);
         totals[val] += pop.Size;
      }

      if (totals.Count == 0)
         return default;

      return totals.OrderByDescending(x => x.Value).FirstOrDefault().Key;
   }

   private void CreateNewPops()
   {
      var popDef = new PopDefinition()
      {
         PopType = PopType,
         Religion = Religion,
         Culture = Culture,
         Size = Value / 1000d,
      };
      AddPopToVms(popDef);
   }

   private void AddPopToVms(PopDefinition popDef)
   {
      _targetLocation.Pops.Add(popDef); // TODO call the global EventDispatcher
      _avm.AddAllocationItemForPopDefinition(popDef);
   }
}