/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 9/2/2014
 * Time: 2:49 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of FileHandler.
  /// </summary>
  public class FileHandler
  {
    public FileHandler()
      : this(@"C:\")
    {
      
    }
    
    public FileHandler( string path )
    {
      this.path = path;
    }
    
    public FileHandler( Entity.Store store )
    {
      this.GetFileServerProjectPath( store );
      
      this.store = store;
    }
    
    private Entity.Store store;
    
    private string path;
    
    public string Path
    {
      get { return this.path; }
    }
    
    public bool SaveAttachmentsToFolder ( Microsoft.Exchange.WebServices.Data.AttachmentCollection attachments )
    {
      if ( string.IsNullOrEmpty(this.path) ) {
        var argEx = new ArgumentNullException("File Path", "File path must be set prior to calling this method");
        
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Error(argEx);
        
        throw argEx;
      }
      
      var directory = new System.IO.DirectoryInfo(this.path);
      
      var xmlStorage = new Data.HashStorage(@"..\XmlStorage\hashTable.xml");
      
      xmlStorage.Read();
      
      var attachmentList = this.AttachmentFileHash(attachments).Result;
      
      var result = xmlStorage.MoveToProjectRfiFolder( this.store.ProjectNumber );
      
      this.SynchronizeHashStorage(directory, xmlStorage);
      
      if ( result == false ) {
        xmlStorage.MoveToProjectRfiFolder( this.store.ProjectNumber );
      }
      
      // test names and hashes
      
      foreach (var item in attachmentList) {
        var parser = new Utilities.Parser();
        
        var isValidName = parser.IsValidRfiAttachmentName(item.Key.Name);
        
        if (isValidName == false) {
          return false;
        }
        
        var comparisonResult = xmlStorage.Comparator(item);
        
        switch (comparisonResult) {
          case Enum.FileComparisonResult.NoMatch:
            var destination = directory.FullName + @"\" + item.Key.Name;
            
            xmlStorage.Write(directory, new List< KeyValuePair< FileInfo, byte[] > > {item}, this.store);
            
            var newFile = item.Key.CopyTo(destination );
            
            newFile.Refresh();
            
            item.Key.Delete();
            
            result = newFile.Exists;
            break;
          case Enum.FileComparisonResult.SameNames:
          case Enum.FileComparisonResult.SameNamesAndData:
            result = true;
            
            break;
          case Enum.FileComparisonResult.SameData:
            result = false;
            
            break;
          default:
            throw new Exception("Invalid value for FileComparisonResult");
        }
      }
      
      
      return result;
    }
    
    private void SynchronizeHashStorage (DirectoryInfo directory, Data.HashStorage storage)
    {
      if ( storage.CurrentElement == null ) {
        storage.Write(directory, DirectoryFileHash(directory).Result, this.store);
      } else {
        
        var hashCount = storage.CurrentElement.Attribute("count").Value;
        
        if ( hashCount == directory.GetFiles().Count().ToString() ) {
          // do nothing
        } else if ( hashCount == "0" ) {
          var contents = DirectoryFileHash(directory).Result;
          
          storage.Write(directory, contents, this.store);
        } else {
          
          var files = directory.GetFiles();
          
          var fileNodes = storage.CurrentElement.Elements();
          
          var fileSearchResult = files.Select( x => {
                                                var result = fileNodes.Select( y => ((string)y.Attribute("name") != x.Name) ).First();
                                                
                                                if (result) {
                                                  return x;
                                                } else {
                                                  return null;
                                                }
                                              }
                                             );
          var list = new List< KeyValuePair< FileInfo, byte[] > >();
          
          foreach (var item in fileSearchResult) {
            if ( item == null ) continue;
            
            var result = this.ComputeHash(item).Result;
            
            list.Add(new KeyValuePair< FileInfo, byte[] >(item, result));
          }
          
          storage.Write(directory, list, this.store);
        }
      }
    }
    
    public bool IsDuplicateFile ( System.IO.FileInfo file1, System.IO.FileInfo file2 )
    {
      // TODO: finish implementation
      return false;
    }
    
    public async System.Threading.Tasks.Task< List< KeyValuePair< FileInfo, byte[] > > > DirectoryFileHash ( DirectoryInfo directory )
    {
      List< KeyValuePair< FileInfo, byte[] > > hashList = new List< KeyValuePair< FileInfo, byte[] > >();
      
      foreach ( var file in directory.GetFiles() ) {
        var hash = await this.ComputeHash(file);
        
        hashList.Add(new KeyValuePair< FileInfo, byte[] >(file, hash));
      }
      
      return hashList;
    }
    
    public async System.Threading.Tasks.Task< List< KeyValuePair< FileInfo, byte[] > > > AttachmentFileHash ( Microsoft.Exchange.WebServices.Data.AttachmentCollection attachments )
    {
      List< KeyValuePair< FileInfo, byte[] > > hashList = new List< KeyValuePair< FileInfo, byte[] > >();
      
      foreach (FileAttachment attachment in attachments) {
        var tempPath = @"..\" + attachment.Name;
        
        var item = new FileInfo(tempPath);
        
        var fileStream = item.OpenWrite();
        
        attachment.Load(fileStream);
        
        fileStream.Close();
        fileStream.Dispose();
        
        var hash = await this.ComputeHash(item);
        
        hashList.Add(new KeyValuePair< FileInfo, byte[] >(item, hash));
      }
      
      return hashList;
    }
    
    private async System.Threading.Tasks.Task< byte[] > ComputeHash (System.IO.FileInfo file)
    {
      return await System.Threading.Tasks.Task.Run( () => {
                                                     var sha = SHA512Managed.Create();
                                                     
                                                     byte[] hash;
                                                     
                                                     try {
                                                       using (var stream = file.OpenRead()) {
                                                         hash = sha.ComputeHash(stream);
                                                       }
                                                       
                                                       return hash;
                                                     } catch (System.IO.IOException ex) {
                                                       
                                                       hash = new byte[] {1,2};
                                                       
                                                       return hash;
                                                     }
                                                     
                                                   });
    }
    
    public bool BitwiseFileComaparison ( System.IO.FileInfo file1, System.IO.FileInfo file2 )
    {
      // lengths have to be identical otherwise the files can't be the same
      var length = file1.Length;
      
      if ( length != file2.Length ) {
        return false;
      }
      
      var buffer1 = new byte[4096];
      
      var buffer2 = new byte[4096];
      
      // open both for reading and do a bit-wise comparison
      using ( var stream1 = file1.OpenRead() )
        using ( var stream2 = file2.OpenRead() ) {
        int bit1, bit2;
        
        while ( length > 0 ) {
          var toRead = buffer1.Length;
          
          if ( toRead > length ) {
            toRead = (int)length;
          }
          
          length -= toRead;
          
          bit1 = stream1.Read( buffer1, 0, toRead );
          
          bit2 = stream2.Read( buffer2, 0, toRead );
          
          for (int i = 0; i < toRead; i++) {
            if ( buffer1[i] != buffer2[i] ) {
              return false;
            }
          }
        }
      }
      
      return true;
    }
    
    public string GetFileServerProjectPath ( Entity.Store store )
    {
      var programMappedPath = Configuration.ConfigHandler.InstanceOf.FileServerMappings.Where(
        x => x.Key == store.Program);
      
      string result = "";
      
      foreach (var pair in programMappedPath) {
        var path = new System.IO.DirectoryInfo( pair.Value );
        
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Preparing to enumerate directories under {0} looking for {1}", path.FullName, store.ProjectNumber);
        
        try {
          var subFolders = path.EnumerateDirectories("*"+store.ProjectNumber+"*", System.IO.SearchOption.TopDirectoryOnly);
          
          if ( subFolders.Count() > 1 ) {
            
            
            foreach (var folder in subFolders) {
              result = this.GetFileServerProjectPath( store );
            }
            
          } else if ( subFolders.Count() < 1 ) {
            continue;
          } else {
            var folder = subFolders.First();
            
            var rfi = this.FindFileServerRfi( folder );
            
            result = rfi.FullName;
          }
        } catch ( Exception x) {
          Logger.LoggerAsync.InstanceOf.GeneralLogger.Error(x);
          
          throw x;
        }
      }
      
      this.path = result;
      
      return result;
    }
    
    private System.IO.DirectoryInfo FindFileServerRfi (System.IO.DirectoryInfo folder)
    {
      try {
        var rfiFolder = folder.EnumerateDirectories("*RFI*");
        
        if ( rfiFolder.Count() > 1 ) {
          var ex = new ArgumentOutOfRangeException("FindFileServerRfi", "There cannot be more than one RFI folder per project");
          
          Logger.LoggerAsync.InstanceOf.GeneralLogger.Error("Error at FindFileServerRfi {0}", ex);
          
          throw ex;
        }
        
        return rfiFolder.FirstOrDefault();
      } catch ( Exception x) {
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Error(x);
        
        throw x;
      }
    }
    
  }
}
