﻿/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 8/7/2014
 * Time: 4:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace RfiCoder.Entity
{
  /// <summary>
  /// Description of MailEntity.
  /// </summary>
  public class MailEntity : ConfigLibrary.Entity.MailEntity
  {
    public MailEntity()
    {
      mailBoxes = new System.Collections.Generic.List< Entity.MailBox >();
    }
  }
}
