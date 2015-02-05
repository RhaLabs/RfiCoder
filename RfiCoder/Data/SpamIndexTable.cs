/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 7/24/2014
 * Time: 12:32 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nBayes;

namespace RfiCoder.Data
{
  /// <summary>
  /// Description of SpamIndexTable.
  /// </summary>
  public sealed class SpamIndexTable  : FileIndex, IFilter
  {

    // with the static keyword on the constructor this should
    // keep the object in memory for the lifetime of the application
    private static readonly Lazy<SpamIndexTable> lazy =
      new Lazy<SpamIndexTable>(() => new SpamIndexTable());

    private static object synclock = new object();

    public static SpamIndexTable Index { get { return lazy.Value; } }

    public override void Add(Entry document)
    {
      lock (synclock)
        base.Add(document);
    }

    public override int EntryCount
    {
      get { lock (synclock)
        { return base.EntryCount; }
      }
    }

    public override int GetTokenCount(string token)
    {
      lock (synclock)
        return base.GetTokenCount(token);
    }

    public void Save()
    {
      lock (synclock)
        base.Save();
    }
    
        public new void AddAndSave(Entry document)
    {
      lock (synclock) {
        base.Add(document);
        
        base.Save();
      }
    }

    public void Dispose()
    {
      lock (synclock)
        base.Save();
      // GC.ReRegisterForFinalize(this);
    }

    private SpamIndexTable()
      : base(Configuration.ConfigHandler.InstanceOf.SpamIndex)
    {
      lock (synclock)
        Open();
      /* 
       * Per MSDN SuppressFinalize exmepts objects from finalization
       * this means that during shutdown the deconstructor won't be called
       */
      //  GC.SuppressFinalize(this);
    }

    ~SpamIndexTable()
    {
      Dispose();
    }
  }
}
