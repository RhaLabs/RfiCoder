/*
 * Created by SharpDevelop.
 * User: brian
 * Date: 5/17/2014
 * Time: 3:05 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of DateTime.
  /// </summary>
  public sealed class DateTime : IDisposable
  {
    private static System.Lazy< DateTime > lazyLoader =
      new System.Lazy< DateTime >(() => new DateTime());
    
    public static DateTime Instance {
      get {
        return lazyLoader.Value;
      }
    }
    
    private DateTime()
    {
    }
    
    public static System.DateTime AdjustForWeekend (System.DateTime weekDay)
    {
      switch (weekDay.DayOfWeek) {
        case DayOfWeek.Sunday:
          return weekDay.AddDays(1);
          // break;
          
        case DayOfWeek.Monday:
        case DayOfWeek.Tuesday:
        case DayOfWeek.Wednesday:
        case DayOfWeek.Thursday:
        case DayOfWeek.Friday:
          return weekDay;
          // break;
          
        case DayOfWeek.Saturday:
          return weekDay.AddDays(-1);
          // break;
          
        default:
          throw new Exception("Invalid value for DayOfWeek");
      }
      
    }
    
    public static System.Collections.Generic.HashSet< System.DateTime > GetHolidays (int year)
    {
      System.Collections.Generic.HashSet< System.DateTime > holidays = new System.Collections.Generic.HashSet< System.DateTime >();
      
      //NEW YEARS
      System.DateTime newYearsDate = AdjustForWeekend(new System.DateTime(year, 1, 1).Date);
      
      holidays.Add(newYearsDate);
      
      //MEMORIAL DAY  -- last monday in May
      System.DateTime memorialDay = new System.DateTime(year, 5, 31);
      
      DayOfWeek dayOfWeek = memorialDay.DayOfWeek;
      
      while (dayOfWeek != DayOfWeek.Monday) {
        memorialDay = memorialDay.AddDays(-1);
        
        dayOfWeek = memorialDay.DayOfWeek;
      }
      
      holidays.Add(memorialDay.Date);

      //INDEPENCENCE DAY
      System.DateTime independenceDay = AdjustForWeekend(new System.DateTime(year, 7, 4).Date);
      
      holidays.Add(independenceDay);

      //LABOR DAY -- 1st Monday in September
      System.DateTime laborDay = new System.DateTime(year, 9, 1);
      
      dayOfWeek = laborDay.DayOfWeek;
      
      while (dayOfWeek != DayOfWeek.Monday) {
        laborDay = laborDay.AddDays(1);
        
        dayOfWeek = laborDay.DayOfWeek;
      }
      
      holidays.Add(laborDay.Date);

      //THANKSGIVING DAY - 4th Thursday in November
      
      var thanksgiving = (from day in Enumerable.Range(1, 30)
                          where new System.DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday
                          select day).ElementAt(3);
      
      System.DateTime thanksgivingDay = new System.DateTime(year, 11, thanksgiving);
      
      holidays.Add(thanksgivingDay.Date);

      System.DateTime christmasDay = AdjustForWeekend(new System.DateTime(year, 12, 25).Date);
      
      holidays.Add(christmasDay);
      
      return holidays;
    }
    
    public static System.TimeSpan RemoveNonBusinesshours(System.DateTime date)
    {
      var timeReceived = date.TimeOfDay;
      
      var startOfBusiness = new System.TimeSpan(7, 30, 0);
      
      var endOfBusiness = new System.TimeSpan(17, 30, 0);
      
      var businessHours = new System.TimeSpan();
      
      if (timeReceived < startOfBusiness) {
        businessHours = timeReceived + new TimeSpan(6, 30, 0);
      }
      
      return businessHours;
    }
    
    public static System.DateTime DueDate(System.DateTime startDate, string interval)
    {
      using (var parser = new Parser()){
        var timeInterval = parser.GetTimeInterval(interval);
        
        var unit = timeInterval.Keys.First();
        
        var time = timeInterval[unit];
        
        var calendar = new System.Globalization.GregorianCalendar();
        
        System.DateTime dueDate;
        
        switch (unit) {
          case Enum.TimeInterval.None:
            goto default;
            // break;
          case Enum.TimeInterval.Seconds:
            dueDate = calendar.AddSeconds(startDate, time);
            break;
          case Enum.TimeInterval.Minutes:
            dueDate = calendar.AddMinutes(startDate, time);
            break;
          case Enum.TimeInterval.Hours:
            dueDate = calendar.AddHours(startDate, time);
            break;
          case Enum.TimeInterval.Days:
            dueDate = calendar.AddDays(startDate, time);
            break;
          case Enum.TimeInterval.Weeks:
            dueDate = calendar.AddWeeks(startDate, time);
            break;
          case Enum.TimeInterval.Months:
            dueDate = calendar.AddYears(startDate, time);
            break;
          default:
            throw new Exception("Invalid value for TimeInterval");
        }
        
        return AdjustForWeekend(dueDate);

      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateReceived"></param>
    /// <param name="dateDue"></param>
    /// <param name="timeElasped"></param>
    /// <returns>True if overdue; false otherwise</returns>
    public static bool IsOverdue(System.DateTime dateReceived, System.DateTime dateDue, out System.TimeSpan timeElasped)
    {
      var today = System.DateTime.Now;
      
      timeElasped = today.Subtract(dateDue);
      
      var result = today.CompareTo(dateDue);
      
      bool overdueOrNot;
      
      // see TimeSpan.CompareTo()
      if (result < 0) {
        overdueOrNot = false; // less than dateDue
      } else if (result == 0) {
        overdueOrNot = false; // is due Today
      } else {
        overdueOrNot = true; //greater than dateDue
      }
      
      
      return overdueOrNot;
    }
    
    public string TimeSpanToString (System.TimeSpan timeSpan)
    {
      return String.Format("{0:00} Hours, {1:00} Minutes, {3:00} Seconds",
                           timeSpan.Hours,
                           timeSpan.Minutes,
                           timeSpan.Seconds
                          );
    }
    
    public void Dispose() { }
  }
}
