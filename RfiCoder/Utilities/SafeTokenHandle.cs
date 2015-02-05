/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 7/31/2014
 * Time: 4:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of SafeTokenHandle.
  /// </summary>
  public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    private SafeTokenHandle()
      : base(true)
    {
    }

    [DllImport("kernel32.dll")]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    protected override bool ReleaseHandle()
    {
      return CloseHandle(handle);
    }
  }
}
