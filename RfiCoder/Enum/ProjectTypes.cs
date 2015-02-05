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
  /// ProjectTypes enumeration for selecting various combinations of job types.
  /// <para>WalMart Realty projects begin at 1d to 8192d</para>
  /// <para>walmart storelayout projects begin at 16384d</para>
  /// </summary>
  [Flags]
  public enum ProjectTypes : uint //ushort max value 4294967295u
  {
    // VALUES FOR ANY NEW ENUM MUST BE A POWER OF 2
    /// <summary>
    /// default value, not to be used in bitwise operations
    /// </summary>
    None = 0,
    /// <summary>
    /// WalMart Realty projects begin at 1d to 8192d
    /// </summary>
    RemodelC = 1,
    Expansion = 2,
    TakeOver = 4,
    GroundUp = 8,
    OnSiteRelo = 16,
    Relo = 32,
    LandBank = 64,
    /* next = 128,
     * next = 256,
     * next = 512,
     * next = 1024,
     * next = 2048,
     * next = 4096
     * next = 8192,
     */
    /// <summary>
    /// walmart storelayout projects begin at 16384d to 2097152d
    /// </summary>
    Remodel = 16384,
    Fixturing = 32768,
    Tenant = 65536,
    Special = 131072,
      /* next = 262144,
       * next = 524288,
       * next = 1048576,
       * next = 2097152,*/
    /// <summary>
    /// ABR holdings begin at 4194304d to 8388608d
    /// </summary>
    ABR = 4194304,
    Resturant = 8388608,
       /* next = 16777216,
       * next = 33554432,
       * next = 67108864,
       * next = 134217728,
       * next = 268435456,
       * next = 536870912,
       * next = 1073741824,*/
    /// <summary>
    /// Gemeral project is the maximum value 2147483648d
    /// </summary>
    General = 2147483648

  }
}

