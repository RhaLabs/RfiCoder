/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 8/7/2014
 * Time: 4:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RfiCoder.Entity
{
  /// <summary>
  /// Description of MailBox.
  /// </summary>
  public class MailBox : ConfigLibrary.Entity.MailBox
  {
    public MailBox()
    {
      folders = new System.Collections.Generic.List< string >();
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    protected Enum.ProjectTypes projectType;
    
    [JsonConverter(typeof(StringEnumConverter))]
    public Enum.ProjectTypes ProjectType
    {
      get { return this.projectType; }
      set { this.projectType = value; }
    }

  }
}
