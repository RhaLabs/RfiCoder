/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/3/2014
 * Time: 8:20 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace RfiCoder.Data
{
  /// <summary>
  /// Description of HashStorage.
  /// </summary>
  public class HashStorage : HashXmlStorage
  {
    public HashStorage(string path)
      : base (path)
    {
    }
    
    private XElement currentElement;
    
    public XElement CurrentElement
    {
      get { return this.currentElement; }
    }
    
    public bool MoveToFirstElementAttribute ( string attribute, string attributeValue )
    {
      try {
        currentElement = ( from elements in this.XmlDocument.Elements()
                          where (string)elements.Attribute(attribute) == attributeValue
                          select elements ).First();
        
        return true;
      } catch {
        return false;
      }
    }
    
    public bool MoveToProjectRfiFolder ( int projectNumber )
    {
      var number = projectNumber.ToString();
      
      var result = this.MoveToFirstElementAttribute("number", number);
      
      if (result) {
        currentElement = currentElement.Element("folder");
      }
      
      return result;
    }
    
    public override bool Write()
    {
      return base.Write();
    }
    
    public bool Write( DirectoryInfo directory, List< KeyValuePair< FileInfo, byte[] > > files, Entity.Store store)
    {
      var projects = from elements in this.XmlDocument.Elements("project")
        where (int)elements.Attribute("number") == store.ProjectNumber
        select elements;
      
      var count = projects.Count();
      
      XElement project;
      
      if ( count < 1 ) {
        project = new XElement(
          "project",
          new XAttribute("number", store.ProjectNumber),
          new XAttribute("name", store.City)
         );
        
        this.XmlDocument.Add(project);
      } else if ( count == 1 ) {
        project = projects.First();
      } else {
        throw new ArgumentOutOfRangeException("XmlData:projects", "Project entry must be unique");
      }
      
      var directories = from elements in projects.Elements()
        where (string)elements.Attribute("name") == directory.Name
        select elements;
      
      count = directories.Count();
      
      XElement folder;
      
      if ( count < 1 ) {
        folder = new XElement(
          "folder",
          new XAttribute("name", directory.Name),
          new XAttribute("count", 0),
          new XAttribute("date-modified", directory.LastWriteTime.ToString("u"))
         );
        
        project.Add(folder);
      } else if ( count == 1 ) {
        folder = directories.First();
      } else {
        throw new ArgumentOutOfRangeException("XmlData:directories", "Directories must be unique for each prject");
      }
      
      var items = folder.Elements();
      
      foreach (var pair in files) {
        var item = items.Where( x => (string)x.Attribute("name") == pair.Key.Name);
        
        if (item.Count() == 0) {
          folder.Add(
            new XElement(
              "item",
              new XAttribute("name", pair.Key.Name),
              new XAttribute("size", pair.Key.Length),
              new XAttribute("date-modified", pair.Key.LastWriteTime.ToString("u")),
              new XElement(
                "hash",
                this.ByteConverter(pair.Value),
                new XAttribute("method", "sha512")
               )
             )
           );
          
          var filesInFolder = int.Parse( folder.Attribute("count").Value );
          
          filesInFolder ++;
          
          folder.Attribute("count").Value = filesInFolder.ToString();
        }
      }
      
      this.XmlDocument.Save(FullPathName);
      
      return true;
    }
    
    public string ByteConverter (byte[] array)
    {
      return System.Text.Encoding.UTF32.GetString(array);
    }
    
    /// <summary>
    /// compares file hash with known file hashes from a store
    /// </summary>
    /// <param name="keyPairs"></param>
    /// <returns>Enum.FileComparisonResult depending on the result</returns>
    public Enum.FileComparisonResult Comparator ( KeyValuePair< FileInfo, byte[] > keyPairs )
    {
      if (this.currentElement.Name != "folder") {
        throw new ArgumentException("CurrentElement must be set at a folder element prior to calling the Comparator");
      }
      
      var names = from elements in this.currentElement.Elements()
        where (string)elements.Attribute("name") == keyPairs.Key.Name
        select elements;
      
      Enum.FileComparisonResult result;
      
      switch (names.Count()) {
        case 0:
          return Enum.FileComparisonResult.NoMatch;
          //break;
        default:
          result = Enum.FileComparisonResult.SameNames;
          break;
      }
      
      var hashes = from elements in names.Elements("hash")
        where (string)elements.Value == this.ByteConverter(keyPairs.Value)
        select elements;
      
      switch (hashes.Count()) {
        case 0:
          return result;
          //break;
        default:
          return Enum.FileComparisonResult.SameNamesAndData;
          break;
      }
    }

  }
}
