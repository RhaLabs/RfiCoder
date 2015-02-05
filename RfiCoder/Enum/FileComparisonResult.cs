/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/4/2014
 * Time: 5:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Enum
{
  public enum FileComparisonResult : byte
  {
    NoMatch,
    SameNames,
    SameData,
    SameNamesAndData
  }
}
