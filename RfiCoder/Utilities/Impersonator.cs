/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 7/31/2014
 * Time: 4:00 PM
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
   #region Using directives.
   // ----------------------------------------------------------------------
 
   using System;
   using System.Security.Principal;
   using System.Runtime.InteropServices;
   using System.ComponentModel;
 
   // ----------------------------------------------------------------------
   #endregion
 
 
   /////////////////////////////////////////////////////////////////////////
   /// <summary>
   /// Impersonation of a user. Allows to execute code under another
   /// user context.
   /// Please note that the account that instantiates the Impersonator class
   /// needs to have the 'Act as part of operating system' privilege set.
   /// </summary>
   /// <remarks>       
   /// This class is based on the information in the Microsoft knowledge base
   /// article http://support.microsoft.com/default.aspx?scid=kb;en-us;Q306158
   ///
   /// Encapsulate an instance into a using-directive like e.g.:
   ///
   ///          ...
   ///          using ( new Impersonator( "myUsername", "myDomainname", "myPassword" ) )
   ///          {
   ///                  ...
   ///                  [code that executes under the new context]
   ///                  ...
   ///          }
   ///          ...
   ///
   /// Please contact the author Uwe Keim (mailto:uwe.keim@zeta-software.de)
   /// for questions regarding this class.
   /// </remarks>
   public class Impersonator : IDisposable
   {
      // ------------------------------------------------------------------
 
      /// <summary>
      /// Constructor. Starts the impersonation with the given credentials.
      /// Please note that the account that instantiates the Impersonator class
      /// needs to have the 'Act as part of operating system' privilege set.
      /// </summary>
      /// <param name="userName">The name of the user to act as.</param>
      /// <param name="domainName">The domain name of the user to act as.</param>
      /// <param name="password">The password of the user to act as.</param>
      public Impersonator(string userName, string domainName, string password)
      {
         ImpersonateValidUser(userName, domainName, password);
      }
 
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
 
      protected virtual void Dispose(bool disposing)
      {
         UndoImpersonation();
         if (impersonationContext != null)
         {
            impersonationContext.Dispose();
         }
      }
 
      /// <summary>
      /// Does the actual impersonation.
      /// </summary>
      /// <param name="userName">The name of the user to act as.</param>
      /// <param name="domainName">The domain name of the user to act as.</param>
      /// <param name="password">The password of the user to act as.</param>
      private void ImpersonateValidUser(string userName, string domainName, string password)
      {
         RevertToSelf();
         IntPtr token = IntPtr.Zero;
         try
         {
            token = LogOnUser(userName, domainName, password);
            CreateImpersonationContext(token);
         }
         finally
         {
            if (token != IntPtr.Zero)
            {
               NativeMethods.CloseHandle(token);
            }
         }
      }
 
      private static void RevertToSelf()
      {
         if (!NativeMethods.RevertToSelf())
         {
            throw new Win32Exception(Marshal.GetLastWin32Error());
         }
      }
 
      private static IntPtr LogOnUser(string userName, string domainName, string password)
      {
         IntPtr token = IntPtr.Zero;
         if (NativeMethods.LogonUser(userName, domainName, password, NativeMethods.LOGON32_LOGON_INTERACTIVE, NativeMethods.LOGON32_PROVIDER_DEFAULT, ref token) != 0)
         {
            return token;
         }
         throw new Win32Exception(Marshal.GetLastWin32Error());
      }
 
      private void CreateImpersonationContext(IntPtr token)
      {
         IntPtr tokenDuplicate = IntPtr.Zero;
         try
         {
            if (NativeMethods.DuplicateToken(token, 2, ref tokenDuplicate) == 0)
            {
               throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            using (var tempWindowsIdentity = new WindowsIdentity(tokenDuplicate))
            {
               impersonationContext = tempWindowsIdentity.Impersonate();
            }
         }
         finally
         {
            if (tokenDuplicate != IntPtr.Zero)
            {
               NativeMethods.CloseHandle(tokenDuplicate);
            }
         }
      }
 
      /// <summary>
      /// Reverts the impersonation.
      /// </summary>
      private void UndoImpersonation()
      {
         if (impersonationContext != null)
         {
            impersonationContext.Undo();
         }
      }
 
      private WindowsImpersonationContext impersonationContext = null;
 
      private static class NativeMethods
      {
         [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
         internal static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
 
         [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
         internal static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);
 
         [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
         [return: MarshalAs(UnmanagedType.Bool)]
         internal static extern bool RevertToSelf();
 
         [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
         [return: MarshalAs(UnmanagedType.Bool)]
         internal static extern bool CloseHandle(IntPtr handle);
 
         internal const int LOGON32_LOGON_INTERACTIVE = 2;
         internal const int LOGON32_PROVIDER_DEFAULT = 0;
      }
 
   }
}