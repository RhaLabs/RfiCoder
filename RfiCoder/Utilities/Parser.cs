/*
 * Created by SharpDevelop.
 * User: brian
 * Date: 5/15/2014
 * Time: 10:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of Parser.
  /// </summary>
  public class Parser : IDisposable
  {
    public Parser()
    {
    }
    
    /// <summary>
    /// Finds in the haystack what may appear to be a store-sequence number
    /// </summary>
    /// <param name="haystack"></param>
    /// <returns></returns>
    public Dictionary< string, int? > GetStoreNumberAndSequenceFromHaystack ( string haystack )
    {
      // Regex with named group capture
      var builder = new System.Text.StringBuilder();
      
      // begin match on word boundary, match 3 to 4 numbers and capture in group number
      builder.Append(@"\b(?<number>\d{3,4})");
      
      // match a delimited between the number which is either a full stop or a hyphen matched only 1 time and captured into a group called delimiter
      builder.Append(@"(?<delimiter>[-.]{1})");//
      
      // match 1 to 3 numbers and capture into a group called sequence
      builder.Append(@"(?<sequence>\d{1,3})");
      
      // do not match anything that matched the content in group delimiter
      builder.Append(@"(?!\k<delimiter>)\b");
      
      var regex = new Regex(
        builder.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      var result = regex.Match(haystack);
      
      var store = new Dictionary< string, int? >();
      
      // loop as long as we have matching groups
      if(result.Success){
        var g = result.Groups;
        
        // use the captured group names as keys
        var names = regex.GetGroupNames();
        
        foreach (var name in names) {
          int number;
          
          if (int.TryParse(g[name].Value, out number)) {
            store.Add(name, number);
          }
        }
        
      } else {
        builder.Clear();
        
        builder.Append(@"\b(?<number>\d{3,4})\b");
        
        var regex2 = new Regex(
          builder.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        result = regex2.Match(haystack);
        
        int number;
        
        if (int.TryParse(result.Groups["number"].Value, out number)) {
          store.Add("number", number);
          store.Add("sequence", (int?)null);
        }
      }
      
      return store;
    }
    
    public string GetAttachmentExtension (string name)
    {
      var extensionBeginning = name.LastIndexOf('.');
      
      var extension = name.Substring(extensionBeginning);
      
      return extension;
    }
    
    public string GetRfiNumberWithPadding(string haystack)
    {
      // Regex with named group capture
      var regex = new Regex(
        @"\b(?:rfi){1}\b\W*(?:number)?\b\W*(?<number>\d+)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      var result = regex.Match(haystack);
      
      // loop as long as we have matching groups
      if(result.Success) {
        int number;
        
        var g = result.Groups;
        
        if (int.TryParse(g["number"].Value, out number)) {
          return number.ToString("D3");
        } else {
          return string.Empty;
        }
        
      } else {
        string rfiCode;
        
        if (this.TryVendorRfi(haystack, out rfiCode)) {
          return rfiCode;
        } else {
          return string.Empty;
        }
      }
    }
    
    public bool IsBidQuestion(string search)
    {
      // Regex with named group capture
      var regex = new Regex(
        @"\b(?:bid){1}\b.*(?:question)?\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      return regex.IsMatch(search);
    }
    
    public bool IsRfiQuestion(string search)
    {
      // Regex with named group capture
      var regex = new Regex(
        @"\b((?:request for information)|(?:rfi)){1}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      return regex.IsMatch(search);
    }
    
    public bool IsReverseRfi(string search)
    {
      // Regex with named group capture
      var regex = new Regex(
        @"\b(?:reverse){1}\b.*(?:rfi)?\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      return regex.IsMatch(search);
    }
    
    public Dictionary< Enum.TimeInterval, int > GetTimeInterval(string search)
    {
      var regex = new Regex(
        @"\b(?<value>\d+)\s*(?<unit>\w+)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase
       );
      
      var result = regex.Match(search);
      
      if(result.Success){
        var g = result.Groups;
        
        var interval = new Dictionary< Enum.TimeInterval, int >();
        
        switch (g["unit"].Value) {
          case "Seconds":
          case "Minutes":
          case "Hours":
          case "Days":
          case "Weeks":
          case "Months":
            interval.Add(
              (Enum.TimeInterval)Enum.TimeInterval.Parse(
                typeof(Enum.TimeInterval),
                g["unit"].Value,
                true),
              int.Parse(g["value"].Value)
             );
            return interval;
          default:
            throw new NotSupportedException("The interval unit given is not supported");
            // break;
        }
        
      }
      
      return new Dictionary< Enum.TimeInterval, int >();
    }
    
    private bool TryVendorRfi(string haystack, out string code)
    {
      // Regex with named group capture
      var regex = new Regex(
        @"\b(?:rfi){1}\b\W*(?:number)?\b\W*(?<number>v{1}\d+-?\d*-?\d*)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      var result = regex.Match(haystack);
      
      // loop as long as we have matching groups
      if(result.Success) {
        
        var g = result.Groups;
        
        code =  g["number"].Value;
        
        return true;
      } else {
        code = string.Empty;
        
        return false;
      }
    }
    
    /// <summary>
    ///This method only tries to get a City, State combination
    /// </summary>
    /// <param name="haystack">the string to search</param>
    /// <param name="values">a System.Collections.Generic.List&lt; Dictionary&lt; string, string &gt;&gt; which might have the city and state.
    ///The keys will be "city" and "state".  There can be multiple "city" keys; one for each potential match.</param>
    /// <returns>true for any match, false otherwise</returns>
    public bool TryCityState ( string haystack, out System.Collections.Generic.List< Dictionary< string, string> > values)
    {
      // 
      var regex = new Regex(
        @"\b(?<city>\w+\s*\w*),{1}\s*(?<state>\w{2})\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      values = new List< Dictionary< string, string > >();
      
      if ( regex.IsMatch(haystack) ) {
        var result = regex.Matches(haystack);
        
        foreach (Match match in result) {
          var group = match.Groups;

          var dic = new Dictionary< string, string >();

          dic.Add("city", group["city"].Value);
          
          dic.Add("state", group["state"].Value);
          
          values.Add(dic);
        }
        
        return true;
        
      } else {
        return false;
      }

    }
    
    public bool TryEmailContacts ( string haystack, out List< string > addresses, List< string > names)
    {
      if ( string.IsNullOrWhiteSpace(haystack) ) {
        throw new NullReferenceException("haystack must not be empty, null, or only whitespace");
      }
      
      var dictionary = this.parseEmailFields(haystack);
      
      //TODO: iterate the dictionary and sort by names or addresses
      addresses = new List<string>();
      
      addresses.AddRange( dictionary["to_addresses"] );
      
      names.AddRange( dictionary["to_names"] );
      
      return false;
    }
    
    private string truncateStringStartsWith (string search, string start)
    {
      // TODO: finish method
      return "";
    }
    
    /// <summary>
    ///slice up the input (email body) by putting the To: From: CC: strings all together
    /// </summary>
    /// <param name="haystack">the input the search, i.e. email body</param>
    /// <returns>System.Collections.Generic.Dictionary&lt; string, System.Collections.Generic.List&lt; string &gt;&gt;</returns>
    private Dictionary< string, List< string > > parseEmailFields (string haystack)
    {
      // slice up the input (email body) by putting the To: From: CC: strings all together
      var splitStrings = haystack.Split( new string[]{ "\r\n", "\n" }, StringSplitOptions.None );
      
      var to = new System.Text.StringBuilder();
      
      var from = new System.Text.StringBuilder();
      
      var cc = new System.Text.StringBuilder();
      
      var fields = new Dictionary< string, List< string > >();
      
      foreach (var stringToCheck in splitStrings) {
        var start = stringToCheck.Substring(0, stringToCheck.IndexOf(':') ).ToLowerInvariant();
        
        switch (start) {
          case "to":
            
            break;
          case "from":
          
            break;
          case "cc":
          
            break;
          default:
            
            break;
        }
      }
      
      return fields;
    }
    
    /// <summary>
    ///Extracts the RHA Program number from the RHA project number
    /// <para>The RHA project number is a specific sequence of 7 numbers</para>
    /// <example>A project may look like this:<code>1204003</code>
    /// <para>The number breaks down into groups <code>(12)(04)(003)</code> where the first group is the last 2 digits of the year when the project was created</para>
    /// <para>The second group is the digits for the program.  The last group is the unique identifier</para>
    /// </example>
    /// <para></para>
    /// </summary>
    /// <param name="projectNumber">The full RHA project number</param>
    /// <returns>The integer representing the program</returns>
    public int ExtractRhaProgramNumber ( int projectNumber )
    {
      /// <summary>
      /// Implementation details
      /// loop over the projectNumber and divide by 10 on each pass through the loop.
      /// dividing by 10 will move the decimal point to the left one position.
      /// example:  start with the number 7704003 and after one pass through the loop we have the number 770400.3
      /// </summary>

      projectNumber = Math.Abs(projectNumber);

      // Get to the year part. meaning first two digits are less than 100 becuase max value can only be 99
      var yearPart = projectNumber;
      
      // TODO: implement a loop which does not fail when there are leading zeros in the project number
//      // have to start at 1
//      int divisionAmount = 1;
//      
//      // yearPArt should equal 77 once this loop is finished
//      while (yearPart >= 100) {
//        yearPart /= 10;
//        divisionAmount *= 10;
//      }
      
      // somtime there are leading zeros in the project number.  we need to divide by 100000
      int divisionAmount = 100000;
      
      yearPart = projectNumber / divisionAmount;
      
      // convert the 2 digit yearPart into the RHA projectnumber format
      // we should have 7700000 
      var yearToProjectNumberFormat = yearPart * divisionAmount;
      
      // we should be able to subtract the yearToProjectNumberFormat and projectNumber
      // to get the number 04003
      var projectNumberWithoutYear = projectNumber - yearToProjectNumberFormat;
      
      // TODO: fix this to be dynamic
      // We can have two kinds of numbers at this point 4003 or 17065
      // either way we need to remove the thousands component
      var programNumber = projectNumberWithoutYear / 1000;
      
      return (int)programNumber;
    }
    
    public bool IsAnsweredQuestion ( string search )
    {
     // Regex with named group capture
      var regex = new Regex(
        @"\b(?:Request for Information Answered){1}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
      return regex.IsMatch(search);
    }
    
    public bool IsValidRfiAttachmentName (string search)
    {
      var regex = new Regex(
        @"^\d{1,4}[-_](RFI|v\d+).*(attachment)", System.Text.RegularExpressions.RegexOptions.IgnoreCase
       );
       
      return regex.IsMatch(search);
    }
    
    public void Dispose() { }
    
  }
}
