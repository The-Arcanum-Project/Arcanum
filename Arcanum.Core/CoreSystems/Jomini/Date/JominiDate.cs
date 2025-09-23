using System.ComponentModel;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Jomini.Date;

[ObjectSaveAs(savingMethod: "JominiDate")]
#pragma warning disable ARC002
public partial class JominiDate : INUI, IAgs, IEmpty<JominiDate>, IComparable<JominiDate>
#pragma warning restore ARC002
{
   #region Properties

   [SuppressAgs]
   [DefaultValue(0)]
   [Description("The year of this date.")]
   public int Year
   {
      get => TimeStamp / 365;
      set => SetJominiDate(value, Month, Day);
   }

   [SuppressAgs]
   [DefaultValue(1)]
   [Description("The month of this date.")]
   public int Month
   {
      get => GetGregorian().month;
      set => SetJominiDate(Year, value, Day);
   }

   [SuppressAgs]
   [DefaultValue(1)]
   [Description("The day of this date.")]
   public int Day
   {
      get => GetGregorian().day;
      set => SetJominiDate(Year, Month, value);
   }

   /// <summary>
   /// Each day is one tick
   /// Every year is 365 days
   /// The valid range is from -10.000 to 10.000
   /// 0 is the year 0 and 0.0.0 is the first day of the year 0: 1.1.0
   /// Months are 30, 31 or 28 days
   /// </summary>
   [SuppressAgs]
   [IgnoreModifiable]
   public int TimeStamp
   {
      get => _timeStamp;
      private set
      {
         _timeStamp = value;
         OnJominiDateChanged.Invoke(this, this);
      }
   }

   #endregion

   #region Defines

   private static readonly int[] StartJominiDateOfMonth = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
   private int _timeStamp;

   [IgnoreModifiable]
   public readonly EventHandler<JominiDate> OnJominiDateChanged = delegate { };

   public static JominiDate MinValue => new(int.MinValue);

   public static JominiDate MaxValue => new(int.MaxValue);

   public static JominiDate Empty => new(1, 1, 1);

   public static string GetNameOfMonth(int month)
   {
      return month switch
      {
         1 => "January",
         2 => "February",
         3 => "March",
         4 => "April",
         5 => "May",
         6 => "June",
         7 => "July",
         8 => "August",
         9 => "September",
         10 => "October",
         11 => "November",
         12 => "December",
         _ => "Oink",
      };
   }

   #endregion

   #region Constructors

   public JominiDate(int year, int month, int day) => SetJominiDate(year, month, day);

   public JominiDate(int timeStamp) => TimeStamp = timeStamp;

   public JominiDate()
   {
   }

   #endregion

   #region Utility Methods

   public (int year, int month, int day) GetGregorian()
   {
      var (year, remainder) = Math.DivRem(_timeStamp, 365);
      var month = 12;
      var day = remainder;
      for (var i = 1; i < StartJominiDateOfMonth.Length; i++)
      {
         var temp = remainder - StartJominiDateOfMonth[i];
         if (temp < 0)
         {
            month = i;
            break;
         }

         day = temp;
      }

      return (year, month, day + 1);
   }

   public void AddDays(int days)
   {
      TimeStamp += days;
   }

   public void AddMonths(int months)
   {
      var (year, month, day) = GetGregorian();
      var newMonth = month + months;

      while (newMonth > 12)
      {
         newMonth -= 12;
         year++;
      }

      while (newMonth < 1)
      {
         newMonth += 12;
         year--;
      }

      SetJominiDate(year, newMonth, day);
   }

   public void AddYears(int years)
   {
      TimeStamp += years * 365;
   }

   public void SetJominiDate(JominiDate jominiDate) => TimeStamp = jominiDate.TimeStamp;
   public void SetJominiDateSilent(JominiDate jominiDate) => _timeStamp = jominiDate.TimeStamp;

   public void SetJominiDate(int year, int month, int day)
   {
      Debug.Assert(month >= 1 && month <= 12, $"The month {month} is not a valid month");
      Debug.Assert(day >= 1 && day <= DaysInMonth(month), $"The day {day} is not a valid day in month {month}");

      TimeStamp = year * 365 + StartJominiDateOfMonth[month - 1] + day - 1;
   }

   public JominiDate Copy() => new(TimeStamp);
   public static JominiDate Copy(JominiDate jominiDate) => new(jominiDate.Year, jominiDate.Month, jominiDate.Day);
   public int DaysBetween(JominiDate jominiDate) => jominiDate.TimeStamp - TimeStamp;

   public static int DaysInMonth(int month)
   {
      return month switch
      {
         2 => 29,
         4 or 6 or 9 or 11 => 30,
         _ => 31,
      };
   }

   public override string ToString()
   {
      var (year, month, day) = GetGregorian();
      return $"{year}.{month}.{day}";
   }

   #endregion

   #region Operators and Equality

   public override bool Equals(object? obj) => obj is JominiDate jominiDate && TimeStamp == jominiDate.TimeStamp;

   protected bool Equals(JominiDate other)
   {
      return TimeStamp == other.TimeStamp;
   }

   public override int GetHashCode()
   {
      return TimeStamp;
   }

   public int CompareTo(JominiDate? other)
   {
      return TimeStamp.CompareTo(other?.TimeStamp);
   }

   public static bool operator ==(JominiDate left, JominiDate right)
   {
      return Equals(left, right);
   }

   public static bool operator !=(JominiDate left, JominiDate right)
   {
      return !Equals(left, right);
   }

   public static bool operator >(JominiDate left, JominiDate right)
   {
      return left.TimeStamp > right.TimeStamp;
   }

   public static bool operator <(JominiDate left, JominiDate right)
   {
      return left.TimeStamp < right.TimeStamp;
   }

   public static bool operator >=(JominiDate left, JominiDate right)
   {
      return left.TimeStamp >= right.TimeStamp;
   }

   public static bool operator <=(JominiDate left, JominiDate right)
   {
      return left.TimeStamp <= right.TimeStamp;
   }

   public static JominiDate operator +(JominiDate jominiDate, int days)
   {
      var newJominiDate = new JominiDate(jominiDate.TimeStamp);
      newJominiDate.AddDays(days);
      return newJominiDate;
   }

   public static JominiDate operator -(JominiDate jominiDate, int days)
   {
      var newJominiDate = new JominiDate(jominiDate.TimeStamp);
      newJominiDate.AddDays(-days);
      return newJominiDate;
   }

   public static int operator +(JominiDate jominiDate, JominiDate other)
   {
      return jominiDate.TimeStamp + other.TimeStamp;
   }

   public static int operator -(JominiDate jominiDate, JominiDate other)
   {
      return jominiDate.TimeStamp - other.TimeStamp;
   }

   public static JominiDate operator ++(JominiDate jominiDate)
   {
      jominiDate.AddDays(1);
      return jominiDate;
   }

   public static JominiDate operator --(JominiDate jominiDate)
   {
      jominiDate.AddDays(-1);
      return jominiDate;
   }

   public static implicit operator string(JominiDate jominiDate) => jominiDate.ToString();

   #endregion

   #region NUI and Ags

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.JominiDateSettings;
   public INUINavigation[] Navigations { get; } = [];
   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.JominiDateSettings;
   public string SavingKey => "date";

   #endregion
}