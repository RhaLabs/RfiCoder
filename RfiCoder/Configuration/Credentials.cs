/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 8/20/2014
 * Time: 2:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Configuration
{
  /// <summary>
  /// Description of Credentials.
  /// </summary>
  public class Credentials : EvocoWebCrawler.Credentials.CredentialInterface
  {
    public string Username { get; set; }

    public string Password { get; set; }

    public bool UseCookies { get; set; }
  }
}
