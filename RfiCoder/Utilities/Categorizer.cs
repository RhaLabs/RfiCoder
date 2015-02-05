/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/20/2014
 * Time: 4:52 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RfiCoder.Data;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of Categorizer.
  /// </summary>
  public sealed class Categorizer
  {
    private static volatile Categorizer instance;
    
    private static object sync = new Object();

    private Parser parser;

    private DatabaseConnector connector;

    private Categorizer()
    {
      parser = new Parser();

      connector = new DatabaseConnector();
    }

    public static Categorizer InstanceOf
    {
      get {
        if (instance == null) {
          lock (sync) {
            if (instance == null) {
              instance = new Categorizer();
            }
          }
        }
        
        return instance;
      }
    }
    
    /// <summary>
    /// Attempts to provide a category based upon some input string.
    /// This method looks for store and sequennce numbers.
    /// </summary>
    /// <param name="guess">the string to attempt to parse</param>
    /// <param name="category">Contains the category string if function returns true.
    /// <para>If function returns false this value will be String.Empty</para>
    /// </param>
    /// <returns>boolean true or false</returns>
    public Enum.QuestionTypes TryCategorizationFromString(string guess, out string category, out Entity.Store store)
    {
      var guessStore = this.parser.GetStoreNumberAndSequenceFromHaystack(guess);

      System.Collections.Generic.List< Entity.Store > storeList;
      
      if (guessStore["sequence"].HasValue) {
        storeList = this.connector.GetStore(guessStore["number"].Value, guessStore["sequence"].Value);
      } else {
        storeList = this.connector.GetStore(guessStore["number"].Value);
      }
      

      store = storeList[0];

      if (typeof(Entity.Store) != store.GetType()) { throw new ArgumentNullException("No store found"); }

      if (storeList.Count > 1) {
        category = string.Empty;

        return Enum.QuestionTypes.None;
      }

      var rfiNumber = this.parser.GetRfiNumberWithPadding(guess);

      var builder = new System.Text.StringBuilder();

      builder.AppendFormat("{0} {1}-{2} ",
                           store.City,
                           store.Number.ToString(),
                           store.Sequence.ToString());

      if (rfiNumber != string.Empty) {
        builder.AppendFormat("RFI: {0}", rfiNumber);
      } else if (this.parser.IsBidQuestion(guess)) {
        builder.Append("Bid Question");
      } else if (this.parser.IsRfiQuestion(guess)) {
        builder.AppendFormat("RFI");
        
        category = builder.ToString();
        
        return Enum.QuestionTypes.RequestForInformation;
      } else {
        category = string.Empty;
        
        return Enum.QuestionTypes.None;
      }

      category = builder.ToString();

      return Enum.QuestionTypes.Success;
    }

    public Enum.QuestionTypes TryCategorizationFromText(string guess, Entity.Store store, out string category)
    {
      var searchString = new System.Text.StringBuilder(guess);
      
      searchString.AppendFormat(" {0}-{1}", store.Number, store.Sequence);
      
      return this.TryCategorizationFromString(searchString.ToString(), out category, out store);
    }

    /// <summary>
    /// Attempts to discover a walmart project based on content and contacts in the email
    /// </summary>
    /// <param name="message">Microsoft.Exchange.WebServices.Data.EmailMessage</param>
    /// <param name="category">Contains the category string if function returns true.
    /// <para>If function returns false this value will be String.Empty</para></param>
    /// <param name="store">System.Collections.Generic.List&lt; Entity.Store &gt;</param>
    /// <returns>RfiCoder.Enum.QuestionTypes</returns>
    public Enum.QuestionTypes DiscoverProject(Microsoft.Exchange.WebServices.Data.EmailMessage message, out string category, out System.Collections.Generic.List< Entity.Store > store)
    {
      // step one try to get the city, state combination from the email.
      var parser = new Parser();
      
      var search = String.Format("{0} {1}",message.Subject, message.Body);
      
      System.Collections.Generic.List< System.Collections.Generic.Dictionary< string, string> > matches;
      
      var isMatch = parser.TryCityState(search, out matches);
      
      if ( isMatch ){
        // let's try to find some stores
        var connector = new Data.DatabaseConnector();
        
        var stores = new System.Collections.Generic.List< Entity.Store >();
        
        foreach (var match in matches) {
          var storeList = connector.GetAllStoresByCityState(match["city"], match["state"]);
          
          if (storeList.Count != 0) {
            stores.AddRange( storeList );
          }
        }
      }

      // get email contacts
      var contacts = message.CcRecipients;
      
      contacts.Add(message.Sender);
      
      // some (actually alot) email are automatically forwarded from Amy.  These email
      // don't have any CC recipients.  They are listed in the body of the email.
      // what we'll do is check the message to see if it was sent from Amy but only
      // to the RFI inbox.
      
      if ( message.Sender.Address == "amills@rhaaia.com"
          && message.ToRecipients[0].Address == "rfi@rhaaia.com" ) {
        // becuase this message is an autoforward we need to clear the email addresses we've stored
        contacts.Clear();
        
        
      }
      
      store = new System.Collections.Generic.List<Entity.Store>();
      
      category = "";
      
      return Enum.QuestionTypes.RequestForInformation;
    }

  }
}
