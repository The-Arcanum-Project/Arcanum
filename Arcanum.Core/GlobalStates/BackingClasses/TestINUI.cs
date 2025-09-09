using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.GlobalStates.BackingClasses;

#if DEBUG
public partial class TestINUI : INUI, ICollectionProvider<TestINUI>
{
   public static ObservableRangeCollection<EmbeddedObject> EmbeddedObjects { get; } =
   [
      new(), new()
      {
         SomeInt = 7,
         SomeString = "Embedded",
         SomeDouble = 0.57721,
      }
   ];

   static TestINUI()
   {
      Globals.TestNUIObjects.Add(new()
      {
         Embedded = EmbeddedObjects[1],
         TestInt = 12,
         TestString = "First",
         TestDouble = 1.23,
         TestFloat = 1.23f,
         TestBool = false,
         TestEnum = Field.TestDouble,
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

   public EmbeddedObject Embedded { get; set; } = EmbeddedObjects[0];
   public int TestInt { get; set; } = 56;
   public float TestFloat { get; set; } = 2.71f;
   public string TestString { get; set; } = "Hello, World!";
   public double TestDouble { get; set; } = 3.14;
   public bool TestBool { get; set; } = true;
   public Field TestEnum { get; set; } = Field.TestBool;
   public static IEnumerable<TestINUI> GetGlobalItems() => Globals.TestNUIObjects;
}

public partial class EmbeddedObject : INUI, ICollectionProvider<EmbeddedObject>
{
   public int SomeInt { get; set; } = 42;
   public string SomeString { get; set; } = "The answer";
   public double SomeDouble { get; set; } = 2.71828;
   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = new(Field.SomeInt,
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray(),
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray(),
                                             Enum.GetValues<Field>().Cast<Enum>().ToArray());
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<EmbeddedObject> GetGlobalItems() => TestINUI.EmbeddedObjects;
   
   public override string ToString() => string.Empty;
}
#endif