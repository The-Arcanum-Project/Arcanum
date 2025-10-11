using System.ComponentModel;
using Point = System.Windows.Point;

namespace Arcanum.UI.WpfTesting;

public partial class ExampleWindow
{
   public ExampleWindow()
   {
      InitializeComponent();
   }
}

public enum SomeValues
{
   Value1,
   Value2,
   Value3,
   Value4,
   Value5
}

public enum SomeOtherValues
{
   OptionA,
   OptionB,
   OptionC,
   OptionD,
   OptionE
}

public class UserSettings
{
   public string FavoriteColor { get; set; } = "Blue";
   public string ProfilePicturePath { get; set; } = "/Assets/profile_picture.png";
   public string Bio { get; set; } = "This is a sample bio for the user.";
   public string Email { get; set; } = "this.example@web.de";
   public string PhoneNumber { get; set; } = "+1234567890";
   [Description("The user's account balance in USD.")]
   public double AccountBalance { get; set; } = 1000.50;
   //public DateTime DateOfBirth { get; set; } = new(1993, 5, 15); // We will have our own date implementation in the future so no need to do this one here.
   public List<string> Hobbies { get; set; } = ["Reading", "Gaming", "Traveling", "Cooking"];

   [Description("This is a list of Point objects.")]
   public List<Point> Coordinates { get; set; } = [new(1.0, 2.0), new(3.0, 4.0), new(5.0, 6.0)];
   [Description("This is a list of Point objects with a description.")]
   public List<List<Point>> NestedCoordinates { get; set; } =
   [
      new() { new(1.0, 2.0), new(3.0, 4.0) }, new() { new(5.0, 6.0), new(7.0, 8.0) }
   ];

   [Description("This is a complex object containing various properties.")]
   public ComplexObject ComplexData { get; set; } = new();
}

public class ComplexObject
{
   public string Line { get; set; } = "===============================";
   public Point[] ArrayCoordinates { get; set; } = [new(1.0, 2.0), new(3.0, 4.0), new(5.0, 6.0)];
   public Point[][] NestedArrayCoordinates { get; set; } =
   [
      new[] { new Point(1.0, 2.0), new Point(3.0, 4.0) }, new[] { new Point(5.0, 6.0), new Point(7.0, 8.0) }
   ];
   public string SLine { get; set; } = "===============================";
   [Description("This is a list of Point arrays.")]
   public List<Point[]> ListOfPointArrays { get; set; } =
   [
      new[] { new Point(1.0, 2.0), new Point(3.0, 4.0) }, new[] { new Point(5.0, 6.0), new Point(7.0, 8.0) }
   ];
   [Description("The user's height in meters.")]
   public float Height { get; set; } = 1.75f; // in meters
   [Description("The user's weight in kilograms.")]
   public SomeValues SelectedValue { get; set; } = SomeValues.Value2;
   public SomeOtherValues SelectedOption { get; set; } = SomeOtherValues.OptionB;
   [Description("The user's username.")]
   public string Username { get; set; } = "User123";
   public bool IsAdmin { get; set; } = true;
   [Description("The user's age in years.")]
   public int Age { get; set; } = 30;
   [Description("The user's account balance in USD.")]
   public double AccountBalance { get; set; } = 1000.50;
}