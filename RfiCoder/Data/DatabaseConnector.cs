/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/14/2014
 * Time: 1:26 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using RfiCoder.Entity;

namespace RfiCoder.Data
{
  /// <summary>
  /// Description of DatabaseConnector.
  /// </summary>
  public class DatabaseConnector
  {
    //private ConfigLibrary.ConfigHandler configHandler;
    
    private MySqlConnection connection;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public DatabaseConnector()
    {
      /*configHandler = new ConfigLibrary.ConfigHandler(@".\\config.json");
      
      string connectionParameters = String.Format(
        "Server={0}; Database={1}; Port={2}; User Id={3}; Pwd={4};",
        configHandler.SqlHost,
        configHandler.SqlDatabase,
        configHandler.SqlPort,
        configHandler.SqlUser,
        configHandler.SqlPass
       );*/
      
      connection = new MySqlConnection(Configuration.ConfigHandler.InstanceOf.DatabaseConnectionParameters);
    }
    
    /// <summary>
    /// Fetches some store data from MySql
    /// </summary>
    /// <param name="number">Store number to find</param>
    /// <param name="sequence">Store Sequence number to find</param>
    /// <returns>A list of RfiCoder.Entity.Store objects</returns>
    public List< Store > GetStore (int number, int? sequence)
    {
      var queryBuilder = new System.Text.StringBuilder();
      
      queryBuilder.Append(@"SELECT s.id AS id, s.storeNumber AS number,
      p.Sequence AS sequence, p.canonicalName AS name,
      pt.name AS type,p.projectNumber AS projectNumber, sa.abbreviation AS state ");
      
      queryBuilder.Append(@"FROM store_information s
        JOIN store_projects sp ON s.id = sp.store_id
        JOIN project_information p ON p.id = sp.projects_id
        JOIN project_status status ON p.ProjectStatus_id = status.id
        JOIN project_type pt ON p.ProjectType_id = pt.id
        JOIN store_type st ON s.storeType_id = st.id
        JOIN state sa ON s.state_id = sa.id
        ");
      queryBuilder.Append("WHERE ");
      queryBuilder.Append(String.Format("s.storeNumber = {0} ", number.ToString()));
      
      if(sequence.HasValue) {
        queryBuilder.Append(string.Format("AND p.Sequence = {0}", sequence.ToString()));
      } else {
        queryBuilder.Append("AND status.name = 'ACTIVE'");
      }
      
      List< Store > stores = new List< Store >();
      
      using (MySqlDataReader reader = MySqlHelper.ExecuteReader(connection.ConnectionString, queryBuilder.ToString())) {
        if (reader.HasRows) {
          while (reader.Read()){
            
            var store = new Store();
            
            store.Id = reader.GetInt32("id");
            
            store.Number = reader.GetInt32("number");
            
            store.Sequence = reader.GetInt32("sequence");
            
            store.City = reader.GetString("name");
            
            store.Type = this.SetProjectType( reader.GetString("type") );
            
            store.Program = this.SetProgramType( reader.GetInt32("projectNumber") );
            
            store.ProjectNumber = reader.GetInt32("projectNumber");
            
            store.State = reader.GetString("state");
            
            stores.Add(store);
          }
        } else {
          throw new ArgumentNullException("No results found");
        }
      }
      
      return stores;
    }
    
    /// <summary>
    /// Fetches some store data from MySql
    /// </summary>
    /// <param name="number">Store number to find</param>
    /// <returns>A list of RfiCoder.Entity.Store objects</returns>
    public List< Store > GetStore (int number)
    {
      return this.GetStore(number, null);
    }
    
    /// <summary>
    /// Fetches contacts for a project data from MySql
    /// </summary>
    /// <param name="store">RfiCoder.Entity.Store object</param>
    /// <returns>A Dictionary of contacts for RFIs.  Keys are job roles</returns>
    public List< Person > GetProjectContacts (Store store)
    {
      var queryBuilder = new System.Text.StringBuilder();
      
      queryBuilder.Append(@"SELECT c0_.firstName, c0_.lastName, c0_.email, IF(c0_.directPhone IS NULL, '0', c0_.directPhone), j3_.jobRole ");
      queryBuilder.Append(@"FROM store_information s1_
        INNER JOIN store_projects s3_ ON s1_.id = s3_.store_id
        INNER JOIN project_information p1_ ON p1_.id = s3_.projects_id
        INNER JOIN project_contacts p2_ ON p1_.id = p2_.project_id
        INNER JOIN contacts c0_ ON p2_.contact_id = c0_.id
        INNER JOIN job_role j3_ ON p2_.jobrole_id = j3_.id
        JOIN store_type st ON s1_.storeType_id = st.id
        ");
      queryBuilder.Append("WHERE ");
      queryBuilder.Append(
        String.Format("p1_.id = {0} AND j3_.jobRole LIKE '%RFI%' ", this.GetProjectId(store))
       );
      
      var contacts = new List< Person >();
      
      using (MySqlDataReader reader = MySqlHelper.ExecuteReader(connection.ConnectionString, queryBuilder.ToString())) {
        if (reader.HasRows) {
          while (reader.Read()){
            
            var person = new Person();
            
            person.FirstName = reader.GetString(0);
            person.LastName = reader.GetString(1);
            person.Email = reader.GetString(2);
            person.PhoneNumber = reader.GetString(3);
            person.JobRole = reader.GetString(4);
            
            contacts.Add(person);
          }
        } else {
          throw new ArgumentNullException("No results found");
        }
      }
      
      return contacts;
    }
    
    /// <summary>
    ///Gets every store matching a city and state combination
    /// </summary>
    /// <param name="city">the city name</param>
    /// <param name="state">the state abbreviation</param>
    /// <returns>A list of store objects if there are matches, otherwise the list is empty.</returns>
    public List< Store > GetAllStoresByCityState (string city, string state)
    {
      var queryBuilder = new System.Text.StringBuilder();
      
      queryBuilder.Append(@"SELECT store.id AS id, store.storeNumber AS number, ci.name AS city,
st.abbreviation AS state, type.name AS type, a.address AS address,
si.name AS info, z.zipcode AS zip, di.name AS divison, re.name AS region,
store.storeNumber AS storeNumber,p.projectNumber AS projectNumber, p.Sequence AS sequence ");
      
      queryBuilder.Append(@"FROM store_information store
INNER JOIN store_type type ON store.storeType_id = type.id
INNER JOIN store_projects sp ON store.id = sp.store_id
INNER JOIN project_information p ON p.id = sp.projects_id
INNER JOIN address a ON store.address_id = a.id
INNER JOIN street_intersection si ON store.streetIntersection_id = si.id
INNER JOIN city ci ON store.city_id = ci.id
INNER JOIN zip z ON store.zip_id = z.id
INNER JOIN division di ON store.division_id = di.id
INNER JOIN region re ON store.region_id = re.id
INNER JOIN state st ON store.state_id = st.id
INNER JOIN project_status status ON p.ProjectStatus_id = status.id
        ");
      queryBuilder.Append("WHERE ");
      
      queryBuilder.Append(String.Format("ci.name = '{0}' ", city));
      
      queryBuilder.Append(String.Format("AND st.abbreviation = '{0}' ", state));
      
      queryBuilder.Append("AND status.name = 'ACTIVE'");
      
      List< Store > stores = new List< Store >();
      
      using (MySqlDataReader reader = MySqlHelper.ExecuteReader(connection.ConnectionString, queryBuilder.ToString())) {
        if (reader.HasRows) {
          while (reader.Read()){
            
            var store = new Store();
            
            store.Id = reader.GetInt32("id");
            
            store.Number = reader.GetInt32("number");
            
            store.Sequence = reader.GetInt32("sequence");
            
            store.City = reader.GetString("city");
            
            store.Type = this.SetProjectType( reader.GetString("type") );
            
            try {
              store.Program = this.SetProgramType( reader.GetInt32("projectNumber") );
              
              store.ProjectNumber = reader.GetInt32("projectNumber");
            } catch (System.Data.SqlTypes.SqlNullValueException exception) {
              store.Program = Enum.ProgramTypes.Generic;
            }
            store.State = reader.GetString("state");
            
            stores.Add(store);
          }
        } else {
          //throw new ArgumentNullException("No results found");
          // not throwing an exception here becuase i want to return an empty list instead
          return stores;
        }
      }
      
      return stores;
    }
    
    private int GetProjectId (Store store)
    {
      var queryBuilder = new System.Text.StringBuilder();
      
      queryBuilder.Append(@"SELECT p1_.id ");
      queryBuilder.Append(@"FROM store_information s0_
        LEFT JOIN store_projects s5_ ON s0_.id = s5_.store_id
        LEFT JOIN project_information p1_ ON p1_.id = s5_.projects_id
        LEFT JOIN project_status p7_ ON p1_.ProjectStatus_id = p7_.id
        LEFT JOIN store_type s4_ ON s0_.storeType_id = s4_.id
        ");
      queryBuilder.Append("WHERE ");
      queryBuilder.Append(
        String.Format("s0_.id = {0} ", store.Id));
      
      queryBuilder.Append("GROUP BY p1_.id");
      
      var results = new List< int >();
      
      using (MySqlDataReader reader = MySqlHelper.ExecuteReader(connection.ConnectionString, queryBuilder.ToString())) {
        if (reader.HasRows) {
          while (reader.Read()){
            
            results.Add(reader.GetInt32(0));
            
          }
        }
      }
      
      if (results.Count > 1) {
        throw new FormatException("Too many results returned.");
      }
      
      return results[0];
    }
    
    private Enum.ProjectTypes SetProjectType (string type)
    {
      switch (type) {
        case "NEW":
          
          return Enum.ProjectTypes.GroundUp;
        case "T/O":
          return  Enum.ProjectTypes.TakeOver;
          
        case "OSR":
          return Enum.ProjectTypes.OnSiteRelo;
          
        case "EXP":
          return  Enum.ProjectTypes.Expansion;
          
        case "REL":
          return Enum.ProjectTypes.Relo;
          
        default:
          return Enum.ProjectTypes.None;
      }
    }
    
    private Enum.ProgramTypes SetProgramType (int projectNumber)
    {
      var parser = new Utilities.Parser();
      
      var programNumber = parser.ExtractRhaProgramNumber(projectNumber);
      
      var programMapping = Configuration.ConfigHandler.InstanceOf.ProgramMappings;
      
      if ( programMapping.ContainsKey(programNumber) ) {
        return programMapping[programNumber];
      } else {
        return Enum.ProgramTypes.Generic;
      }
    }
  }
}
