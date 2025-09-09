using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.GlobalStates.BackingClasses;

#if DEBUG
public partial class TestINUI : INUI, ICollectionProvider<TestINUI>
{
   static TestINUI()
   {
      Globals.TestNUIObjects.Add(new()
      {
         TestInt = 12,
         TestString = "First",
         TestDouble = 1.23,
         TestBool = false,
      });
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
      Globals.TestNUIObjects.Add(new());
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = new(Field.TestInt,
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray(),
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray(),
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray());
   public INUINavigation[] Navigations { get; } = [];

   public int TestInt { get; set; } = 56;
   public string TestString { get; set; } = "Hello, World!";
   public double TestDouble { get; set; } = 3.14;
   public bool TestBool { get; set; } = true;
   public DayOfWeek TestEnum { get; set; } = DayOfWeek.Wednesday;
   public static IEnumerable<TestINUI> GetGlobalItems() => Globals.TestNUIObjects;
}
#endif