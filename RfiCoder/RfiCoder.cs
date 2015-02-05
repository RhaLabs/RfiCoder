/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/13/2014
 * Time: 12:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Exchange.WebServices.Authentication;
using RfiCoder.Utilities;

namespace RfiCoder
{
  public class RfiCoder : ServiceBase
  {
    public const string MyServiceName = "RfiCoder";
    
    private Utilities.Impersonator impersonator;
    
    private System.Diagnostics.EventLog eventLog;
    
    private StreamingSubscription emailSubscription;
    
    //    private StreamingSubscription GroundUpRfiBoxSubscription;
//
    //    private StreamingSubscription ExistingRfiBoxSubscription;
    
    private StreamingSubscriptionConnection emailSubscriptionConnection;
    
    public RfiCoder()
    {
      System.IO.Directory.SetCurrentDirectory(
        System.AppDomain.CurrentDomain.BaseDirectory
       );
      
      // Impersonation probably isn't the best way
      // what we should do is have a starter service which loads the rfi service
      //impersonator = new Impersonator(Utilities.Configuration.InstanceOf.LogonName, "rha", Utilities.Configuration.InstanceOf.LogonPassword);
      
      InitializeComponent();
    }
    
    private void InitializeComponent()
    {
      this.ServiceName = MyServiceName;
      
      this.eventLog = new EventLog();
      
      this.eventLog.Source = this.ServiceName;
      
      ((System.ComponentModel.ISupportInitialize)(this.eventLog)).BeginInit();
      
      emailSubscription = EmailHelper.InstanceOf.GetEmailService.SubscribeToStreamingNotifications(
        new FolderId[] {
          EmailHelper.InstanceOf.RfiInboxFolder
        },
        EventType.NewMail);
      
      //      GroundUpRfiBoxSubscription = EmailHelper.InstanceOf.GetEmailService.SubscribeToStreamingNotifications(
      //        new FolderId[] {
      //          EmailHelper.InstanceOf.NewStoresInboxFolder
      //        },
      //        EventType.NewMail,
      //        EventType.Created,
      //        EventType.Deleted,
      //        EventType.Moved,
      //        EventType.Copied);
//
      //      ExistingRfiBoxSubscription = EmailHelper.InstanceOf.GetEmailService.SubscribeToStreamingNotifications(
      //        new FolderId[] {
      //          EmailHelper.InstanceOf.ExistingStoresInboxFolder
      //        },
      //        EventType.NewMail,
      //        EventType.Created,
      //        EventType.Deleted,
      //        EventType.Moved,
      //        EventType.Copied);
      
      emailSubscriptionConnection = new StreamingSubscriptionConnection(
        EmailHelper.InstanceOf.GetEmailService,
        30);
      
      ((System.ComponentModel.ISupportInitialize)(this.eventLog)).EndInit();
    }
    
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      // TODO: Add cleanup code here (if required)
      base.Dispose(disposing);
    }
    
    /// <summary>
    /// Start this service.
    /// </summary>
    protected override void OnStart(string[] args)
    {
      this.eventLog.WriteEntry("RfiCoder is starting", EventLogEntryType.Information);
      Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("RfiCoder is starting: {0}", EventLogEntryType.Information);
      
      this.launchSubscriptions();
      
      this.eventLog.WriteEntry("RfiCoder started successfully", EventLogEntryType.Information);
      
      var thread = new System.Threading.Thread(this.ProcessUnhandledMail);
      
      thread.Start();
    }
    
    /// <summary>
    /// Stop this service.
    /// </summary>
    protected override void OnStop()
    {
      try {
        emailSubscriptionConnection.Close();
      } catch ( Microsoft.Exchange.WebServices.Data.ServiceLocalException localException) {
        
      }
      
      emailSubscriptionConnection.RemoveSubscription(emailSubscription);
      
      emailSubscriptionConnection.Dispose();
      
      Data.RfiIndexTable.Index.Dispose();
      
      Data.SpamIndexTable.Index.Dispose();
      
      //impersonator.Dispose();
      
      this.eventLog.WriteEntry("RfiCoder has stopped", EventLogEntryType.Information);
      Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("RfiCoder has stopped: {0}", EventLogEntryType.Information);
    }

    private void emailSubscriptionConnection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
    {
      this.eventLog.WriteEntry(args.Exception.Message, EventLogEntryType.Error);
      Logger.LoggerAsync.InstanceOf.GeneralLogger.Error("RfiCoder had a subscription error: {0}", args.Exception);
      
      if (emailSubscriptionConnection.IsOpen) {
        emailSubscriptionConnection.Close();
      }
      
      var timer = new System.Timers.Timer( 1000d);
      
      timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
      
    }


    private void emailSubscriptionConnection_OnDisconnect(object sender, SubscriptionErrorEventArgs args)
    {
      if (emailSubscriptionConnection.IsOpen == false) {
        emailSubscriptionConnection.Open();
      }
    }

    private void emailSubscriptionConnection_OnNotificationEvent(object sender, NotificationEventArgs args)
    {
      try {
        foreach (var notificationEvent in args.Events) {
          if (notificationEvent is ItemEvent) { // event was an item event
            var itemEvent = (ItemEvent)notificationEvent;
            
            var message = EmailMessage.Bind(EmailHelper.InstanceOf.GetEmailService,
                                            itemEvent.ItemId,
                                            EmailHelper.EmailProperties
                                           );
            
            var parentFolder = itemEvent.ParentFolderId;
            
            switch (itemEvent.EventType) {
              case EventType.Status:
                
                break;
              case EventType.NewMail:
                var newMailThread = new System.Threading.Thread(x => ProcessNewMail(message));
                
                newMailThread.Start();


                break;
              case EventType.Deleted:
                
                break;
              case EventType.Modified:
                
                break;
              case EventType.Moved:
                
                // break;
              case EventType.Copied:
                
                
                //EmailHelper.InstanceOf.RewriteSender(message);
                
                break;
              case EventType.Created:
                
                break;
              case EventType.FreeBusyChanged:
                
                break;
              default:
                throw new Exception("Invalid value for EventType");
            }
          } else { // the event was a folder event
            var folderEvent = (FolderEvent)notificationEvent;
            
            switch (folderEvent.EventType) {
              case EventType.Status:
                
                //break;
              case EventType.NewMail:

                //break;
              case EventType.Deleted:
                
                //break;
              case EventType.Modified:
                
                //break;
              case EventType.Moved:
                
                //break;
              case EventType.Copied:
                
                //break;
              case EventType.Created:
                
                //break;
              case EventType.FreeBusyChanged:
                
                break;
              default:
                throw new Exception("Invalid value for EventType");
            }
          }
        }
      } catch (Exception exception) {
        eventLog.WriteEntry(string.Format("{0}\r\nSource: {1}\r\nStack: {2}", exception.Message, exception.Source, exception.StackTrace),
                            EventLogEntryType.Error);
      }
    }

    private void launchSubscriptions()
    {
      emailSubscriptionConnection.AddSubscription(emailSubscription);
      
      //emailSubscriptionConnection.AddSubscription(GroundUpRfiBoxSubscription);
      
      //emailSubscriptionConnection.AddSubscription(ExistingRfiBoxSubscription);
      
      emailSubscriptionConnection.OnNotificationEvent += new StreamingSubscriptionConnection.NotificationEventDelegate(emailSubscriptionConnection_OnNotificationEvent);
      
      emailSubscriptionConnection.OnDisconnect += new StreamingSubscriptionConnection.SubscriptionErrorDelegate(emailSubscriptionConnection_OnDisconnect);
      
      emailSubscriptionConnection.OnSubscriptionError += new StreamingSubscriptionConnection.SubscriptionErrorDelegate(emailSubscriptionConnection_OnSubscriptionError);
      
      emailSubscriptionConnection.Open();
    }

    private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try{
        this.launchSubscriptions();
        
        var timer = (System.Timers.Timer)sender;
        
        timer.Stop();
      } catch (Exception ex) {
        this.eventLog.WriteEntry("RfiCoder is trying to setup failed subscriptions", EventLogEntryType.Information);
      }
    }
    
    /// <summary>
    ///Processes any unhandled mail upon startup
    /// </summary>
    private void ProcessUnhandledMail ()
    {
      var view = new ItemView(int.MaxValue);
      
      var items = EmailHelper.InstanceOf.GetEmailService.FindItems(EmailHelper.InstanceOf.RfiInboxFolder, view);
      
      foreach (var item in items) {
        var message = EmailMessage.Bind(EmailHelper.InstanceOf.GetEmailService,
                                        item.Id,
                                        EmailHelper.EmailProperties
                                       );
        
        var result = EmailHelper.InstanceOf.HandleEmailItem(message);
      }
    }
    
    private void ProcessNewMail (EmailMessage message)
    {
      var result = EmailHelper.InstanceOf.HandleEmailItem(message);
    }

  }
}
