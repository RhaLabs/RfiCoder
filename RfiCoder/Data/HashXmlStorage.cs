/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/3/2014
 * Time: 8:17 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace RfiCoder.Data
{
  /// <summary>
  /// Description of HashXmlStorage.
  /// </summary>
  public abstract class HashXmlStorage : HashStorageInterface
  {
    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="path">fully qualified path to the xml file.  If the file does not exist it will be created</param>
    public HashXmlStorage(string path)
    {
      var demarcation = path.LastIndexOf('\\');
      
      this.path = path.Substring( 0, demarcation );
      
      this.fileName = path.Substring(demarcation+1);
      
      this.init();
    }
    
    private void init ()
    {
      if ( !Directory.Exists(this.path) ) {
        Directory.CreateDirectory(path);
      }
      
      this.fullPathName = this.path + "\\" + this.fileName;
      
      if ( !File.Exists(fullPathName) ) {
        var document = new XDocument(
          new XComment("This file contains the file hashes for the RFI directories"),
          new XElement("root")
         );
        
        document.Declaration = new XDeclaration("1.0", "utf-16", "true");
        
        document.Save(fullPathName);
      }
    }
    
    private string path;
    
    private string fileName;
    
    private string fullPathName;
    
    private XElement xmlDocument;
    
    protected string Path
    {
      get { return this.path; }
    }
    
    protected string FileName
    {
      get { return this.fileName; }
    }
    
    protected string FullPathName
    {
      get { return this.fullPathName; }
    }
    
    protected XElement XmlDocument
    {
      get { return this.xmlDocument; }
      set { this.xmlDocument = value; }
    }
    
    public virtual bool Read ()
    {
      this.xmlDocument = XElement.Load(this.fullPathName);
      
      return true;
    }
    
    public virtual bool Write ()
    {
      //TODO: implement
      return true;
    }
    
    public virtual bool WriteAndClose ()
    {
      var result = this.Write();
      
      if (result) {
        result = this.Close();
      }
      
      return result;
    }
    
    public virtual bool Close ()
    {
      //TODO: implement
      return true;
    }
  }
}
