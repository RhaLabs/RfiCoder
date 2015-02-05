/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/3/2014
 * Time: 8:04 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace RfiCoder.Data
{
  /// <summary>
  /// Description of HashStorageInterface.
  /// </summary>
  public interface HashStorageInterface
  {
    bool Read ();
    
    bool Write ();
    
    bool WriteAndClose ();
    
    bool Close ();
  }
}
