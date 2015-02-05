/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/20/2014
 * Time: 2:45 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Configuration
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public sealed class Configuration : ConfigLibrary.Configuration
  {
    private Entity.ProgramMapping programMappings;
    
    private Credentials creds;
    
    public Configuration()
    {
      database = new Entity.DataBaseEntity();
      
      mail = new Entity.MailEntity();
      
      list = new Entity.CollectionEntity();
      
      programMappings = new Entity.ProgramMapping();
      
      creds = new Credentials();
    }
    
    public System.Collections.Generic.Dictionary< int, Enum.ProgramTypes > ProgramMappings
    {
      get { return this.programMappings.ProgramMappings; }
      set { this.programMappings.ProgramMappings = value; }
    }
    
    public System.Collections.Generic.List< System.Collections.Generic.KeyValuePair< Enum.ProgramTypes, string > > FileServerMappings
    {
      get { return this.programMappings.FileServerMappings; }
      set { this.programMappings.FileServerMappings = value; }
    }
    
    public Credentials GetCredentials
    {
      get { return this.creds; }
    }
  }
}
