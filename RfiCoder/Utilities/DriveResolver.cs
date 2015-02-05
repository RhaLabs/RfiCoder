/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/15/2014
 * Time: 4:44 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Management;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of DriveResolver.
  /// </summary>
  public static class DriveResolver
  {

    /// <summary>Resolves the given path to a full UNC path, or full local drive path.</summary>
    /// <param name="pPath"></param>
    /// <returns></returns>
    public static string ResolveToUNC(string pPath) {
      if (pPath.StartsWith(@"\\")) { return pPath; }

      string root = ResolveToRootUNC(pPath);

      if (pPath.StartsWith(root)) {
        return pPath; // Local drive, no resolving occurred
      } else {
        return pPath.Replace(GetDriveLetter(pPath), root);
      }
    }

    /// <summary>Resolves the given path to a root UNC path, or root local drive path.</summary>
    /// <param name="pPath"></param>
    /// <returns>\\server\share OR C:\</returns>
    public static string ResolveToRootUNC(string pPath) {
      using ( ManagementObject mo = new ManagementObject() ){

        if (pPath.StartsWith(@"\\")) { return Directory.GetDirectoryRoot(pPath); }

        // Get just the drive letter for WMI call
        string driveletter = GetDriveLetter(pPath);
        
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Trying to get root UNC of {0}",driveletter);

        mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));

        // Get the data we need
        try {
          uint DriveType = Convert.ToUInt32(mo["DriveType"]);

          // Return the root UNC path if network drive, otherwise return the root path to the local drive
          if (DriveType == 4) {
            try {
              string NetworkRoot = Convert.ToString(mo["ProviderName"]);
              
              return NetworkRoot;
            } catch (Exception x) {
              Logger.LoggerAsync.InstanceOf.GeneralLogger.Error("Error getting \"ProviderName\"",x);
              
              throw x;
            }

          } else {
            return driveletter + Path.DirectorySeparatorChar;
          }
          
        } catch (Exception ex) {
          Logger.LoggerAsync.InstanceOf.GeneralLogger.Error("Error getting \"DriveType\"", ex);
          
          throw ex;
        }
      }
    }

    /// <summary>Checks if the given path is on a network drive.</summary>
    /// <param name="pPath"></param>
    /// <returns></returns>
    public static bool isNetworkDrive(string pPath) {
      using ( ManagementObject mo = new ManagementObject() ) {

        if (pPath.StartsWith(@"\\")) { return true; }

        // Get just the drive letter for WMI call
        string driveletter = GetDriveLetter(pPath);

        mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));

        // Get the data we need
        uint DriveType = Convert.ToUInt32(mo["DriveType"]);

        return DriveType == 4;
      }
    }

    /// <summary>Given a path will extract just the drive letter with volume separator.</summary>
    /// <param name="pPath"></param>
    /// <returns>C:</returns>
    public static string GetDriveLetter(string pPath) {
      if (pPath.StartsWith(@"\\")) { throw new ArgumentException("A UNC path was passed to GetDriveLetter"); }
      return Directory.GetDirectoryRoot(pPath).Replace(Path.DirectorySeparatorChar.ToString(), "");
    }

  }
}