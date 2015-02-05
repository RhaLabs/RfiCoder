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
  /// ProgramTypes enumeration for selecting various combinations of store programs.
  /// </summary>
  [Flags]
  public enum ProgramTypes : uint //ushort max value 4294967295u
  {
    // VALUES FOR ANY NEW ENUM MUST BE A POWER OF 2
    /// <summary>
    /// default value, none, not to be used in bitwise operations
    /// </summary>
    None = 0,
    NewStores = 1,
    TakeOvers = 2,
    Remodels = 4,
    Resturants = 8,
    Expansions = 16,
    FuelStations = 32,
    Special = 64,
    Generic = 128,
    DallasCommunityCollege = 256,
    ABRHoldings = 512,
    Express = 1024,
     /* next = 2048,
     * next = 4096
     * next = 8192,
     * next = 16384,
     * next  = 32768,
     * next = 65536,
     * next = 131072
     * next = 262144,
     * next = 524288,
     * next = 1048576,
     * next = 2097152,
     * next = 4194304,
     * next = 8388608,
     * next = 16777216,
     * next = 33554432,
     * next = 67108864,
     * next = 134217728,
     * next = 268435456,
     * next = 536870912,
     * next = 1073741824,
     * next = 2147483648,
     */
  }
}

