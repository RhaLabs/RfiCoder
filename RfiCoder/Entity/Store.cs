/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/15/2014
 * Time: 10:32 AM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Entity
{
  /// <summary>
  /// Description of Store.
  /// </summary>
  public class Store
  {
    private int id;
    
    private int number;

    private int sequence;

    private string city;

    private string state;

    private Enum.ProjectTypes type;
    
    private Enum.ProgramTypes program;
    
    private int projectNumber;

    /// <summary>
    /// Gets or Sets the store number
    /// </summary>
    public int Id
    {
      get { return this.id; }
      set { this.id = value; }
    }

    /// <summary>
    /// Gets or Sets the store number
    /// </summary>
    public int Number
    {
      get { return this.number; }
      set { this.number = value; }
    }

    /// <summary>
    /// Gets or Sets the store sequence number
    /// </summary>
    public int Sequence
    {
      get { return this.sequence; }
      set { this.sequence = value; }
    }

    /// <summary>
    /// Gets or Sets the store's city
    /// </summary>
    public string City
    {
      get { return this.city; }
      set { this.city = value; }
    }

    /// <summary>
    /// Gets or Sets the store's state
    /// </summary>
    public string State
    {
      get { return this.state; }
      set { this.state = value; }
    }

    /// <summary>
    /// Gets or Sets the store's type
    /// </summary>
    public Enum.ProjectTypes Type
    {
      get { return this.type; }
      set { this.type = value; }
    }
    
    /// <summary>
    /// Gets or Sets the store's program
    /// </summary>
    public Enum.ProgramTypes Program
    {
      get { return this.program; }
      set {this.program = value; }
    }
    
    /// <summary>
    /// Gets or Sets the store's project number
    /// </summary>
    public int ProjectNumber
    {
      get { return this.projectNumber; }
      set {this.projectNumber = value; }
    }
  }
}
