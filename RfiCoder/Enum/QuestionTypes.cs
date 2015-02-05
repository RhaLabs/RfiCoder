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
  /// QuestionTypes enumeration for selecting various combinations of questions.
  /// </summary>
  [Flags]
  public enum QuestionTypes : uint //ushort max value 4294967295u
  {
    // VALUES FOR ANY NEW ENUM MUST BE A POWER OF 2
    /// <summary>
    /// default value, none, not to be used in bitwise operations
    /// Success should be returned for successful question finds
    /// </summary>
    None = 0,
    Success = 1,
    /// <summary>
    /// Question types begin at 2d
    /// </summary>
    RequestForInformation = 2,
    BidQuestion = 4,
    VendorQuestion = 8,
    ProtoQuestion = 16,
    GenericQuestion = 32,
    /* next = 64,
     * next = 128,
     * next = 256,
     * next = 512,
     * next = 1024,
     * next = 2048,
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

