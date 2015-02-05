/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 8/7/2014
 * Time: 4:29 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace RfiCoder.Configuration
{
  /// <summary>
  /// Description of ConfigHandler.
  /// </summary>
  public class ConfigHandler : ConfigLibrary.ConfigHandler
  {
    public static ConfigHandler InstanceOf
    {
      get {
        if (instance == null) {
          lock (sync) {
            if (instance == null) {
              instance = new ConfigHandler();
            }
          }
        }
        
        return instance;
      }
    }
    
    private static object sync = new Object();
    
    private static ConfigHandler instance;
    
    private Configuration config;
    
    private ConfigHandler()
      : base(@".\\config.json")
    {
      config = JsonConvert.DeserializeObject< Configuration >(jsonString);

      configuration = config;
    }
    
    public string DatabaseConnectionParameters
    {
      get { return String.Format(
        "Server={0}; Database={1}; Port={2}; User Id={3}; Pwd={4};",
        this.SqlHost,
        this.SqlDatabase,
        this.SqlPort,
        this.SqlUser,
        this.SqlPass
       ); }
    }

    public string SpamIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["spamIndex"];
      }
    }
    
    public string RfiIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["rfiIndex"];
      }
    }
    
    public string WalmartIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["walmartIndex"];
      }
    }
    
    public string GeneralIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["generalIndex"];
      }
    }
    
    public string QuestionIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["questionIndex"];
      }
    }
    
    public string NotQuestionIndex
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["notQuestionIndex"];
      }
    }
    
    public string LogonPassword
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["logonpassword"];
      }
    }
    
    public string LogonName
    {
      get
      {
        var dictionary = this.configuration.collection.List;
        
        return dictionary["logonname"];
      }
    }
    
    public System.Collections.Generic.Dictionary< int, Enum.ProgramTypes > ProgramMappings
    {
      get { return this.config.ProgramMappings; }
      set { this.ProgramMappings = value; }
    }
    
    public System.Collections.Generic.List< System.Collections.Generic.KeyValuePair< Enum.ProgramTypes, string > > FileServerMappings
    {
      get { return this.config.FileServerMappings; }
      set { this.config.FileServerMappings = value; }
    }
    
    public Credentials GetCredentials
    {
      get { return this.config.GetCredentials; }
    }
  }
}