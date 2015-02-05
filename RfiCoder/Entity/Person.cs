/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/15/2014
 * Time: 11:26 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Entity
{
  /// <summary>
  /// Description of Person.
  /// </summary>
  public class Person
  {
    private string firstname;
    
    private string lastname;
    
    private string email;
    
    private string phone;
    
    private string jobRole;
    
    /// <summary>
    /// Gets or Sets the first name
    /// </summary>
    public string FirstName
    {
      get { return this.firstname; }
      set { this.firstname = value; }
    }
    
    /// <summary>
    /// Gets or Sets the last name
    /// </summary>
    public string LastName
    {
      get { return this.lastname; }
      set { this.lastname = value; }
    }
    
    /// <summary>
    /// Gets the full name i.e. First Last
    /// </summary>
    public string FullName
    {
      get { return string.Format("{0} {1}",this.firstname, this.lastname); }
    }
    
    /// <summary>
    /// Gets or Sets the email
    /// </summary>
    public string Email
    {
      get { return this.email; }
      set { this.email = value; }
    }
    
    /// <summary>
    /// Gets or Sets the phone number
    /// </summary>
    public string PhoneNumber
    {
      get { return this.phone; }
      set { this.phone = value; }
    }
    
    /// <summary>
    /// Gets or Sets the Job Role
    /// </summary>
    public string JobRole
    {
      get { return this.jobRole; }
      set { this.jobRole = value; }
    }
  }
}
