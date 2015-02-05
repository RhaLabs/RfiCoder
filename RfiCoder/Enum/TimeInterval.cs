/*
 * Created by SharpDevelop.
 * User: brian
 * Date: 5/18/2014
 * Time: 2:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Enum
{
  /// <summary>
  /// Description of Class1.
  /// </summary>
  public enum TimeInterval : byte
  {
    None = 0,
    Seconds = 1,
    Minutes = 2,
    Hours = 4,
    Days = 8,
    Weeks = 16,
    Months = 32
  }
}
