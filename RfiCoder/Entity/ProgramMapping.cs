/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 8/11/2014
 * Time: 1:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Entity
{
  /// <summary>
  /// Description of ProgramMapping.
  /// </summary>
  public class ProgramMapping
  {
    public ProgramMapping()
    {
      programMappings = new System.Collections.Generic.Dictionary< int, Enum.ProgramTypes >();
    }
    
    protected System.Collections.Generic.Dictionary< int, Enum.ProgramTypes > programMappings;
    
    protected System.Collections.Generic.List< System.Collections.Generic.KeyValuePair< Enum.ProgramTypes, string > > fileServerMappings;
    
    public System.Collections.Generic.Dictionary< int, Enum.ProgramTypes > ProgramMappings
    {
      get { return this.programMappings; }
      set { this.programMappings = value; }
    }
    
    public System.Collections.Generic.List< System.Collections.Generic.KeyValuePair< Enum.ProgramTypes, string > > FileServerMappings
    {
      get { return this.fileServerMappings; }
      set { this.fileServerMappings = value; }
    }
  }
}
