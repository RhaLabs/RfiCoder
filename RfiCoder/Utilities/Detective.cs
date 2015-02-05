/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 7/25/2014
 * Time: 4:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// The bayesian detector class
  /// </summary>
  public class Detective : IDisposable
  {
    /// <summary>
    /// default constructor
    /// </summary>
    /// <param name="index1">The first index to look in</param>
    /// <param name="index2">The second index to look in</param>
    public Detective(nBayes.FileIndex index1, nBayes.FileIndex index2)
    {
      this.index1 = index1;
      
      this.index2 = index2;
    }
    
    /// <summary>
    /// constructor with search string intialization
    /// </summary>
    /// <param name="haystack">The string to parse</param>
    /// <param name="index1">The first index to look in</param>
    /// <param name="index2">The seconf index to look in</param>
    public Detective(string  haystack, nBayes.FileIndex index1, nBayes.FileIndex index2)
      : this(index1, index2)
    {
      this.search = haystack;
    }
    
    private string search;
    
    private nBayes.FileIndex index1;
    
    private nBayes.FileIndex index2;
    
    /// <summary>
    /// Sets the string to search
    /// </summary>
    public Utilities.Detective SearchString (string value)
    {
      this.search = value;
      
      return this;
    }
    
    public nBayes.CategorizationResult GetResult()
    {
      if ( String.IsNullOrWhiteSpace(this.search) ) {
        throw new NullReferenceException("Search string must be set prior to calling GetResult()");
      }
      
      var analyzer = new nBayes.Analyzer();
      
      var document = nBayes.Entry.FromString(this.search);
      
      var result = analyzer.Categorize(document, index1, index2);
      
      switch (result) {
        case nBayes.CategorizationResult.First:
          index1.AddAndSave(document);
          
          break;
        case nBayes.CategorizationResult.Undetermined:
          break;
        case nBayes.CategorizationResult.Second:
          index2.AddAndSave(document);
          
          break;
        default:
          throw new Exception("Invalid value for CategorizationResult");
      }
      
      return result;
    }
    
    private bool disposed = false;
    
    public void Dispose()
    {
      Dispose(true);
      
      GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed) {
        if (disposing) {
          // manged resources
          // no global managed resources.
        }
        
        // no unmanaged resources
        
        disposed = true;
      }
    }
    
    /// <summary>
    ///Destructor
    /// </summary>
    ~Detective()
    {
      Dispose(false);
    }
  }
}
