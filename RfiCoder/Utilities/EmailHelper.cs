/*
 * Created by SharpDevelop.
 * User: bcrawford
 * Date: 5/21/2014
 * Time: 8:42 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Microsoft.Exchange.WebServices.Authentication;
using Microsoft.Exchange.WebServices.Data;
using System.Linq;

namespace RfiCoder.Utilities
{
  /// <summary>
  /// Description of EmailHelper.
  /// </summary>
  public sealed class EmailHelper
  {
    private static EmailHelper instance = new EmailHelper();

    private Data.DatabaseConnector connector;

    private ExchangeService emailService;

    private Mailbox rfiMailBox;

    private FolderId rfiInboxFolder;

    private FolderId rfiDraftsFolder;

    private FolderId rfiDeletedItemsFolder;
    
    private FolderId rfiNewStoresFolder;
    
    private FolderId rfiExistingStoresFolder;
    
    private FolderId rfiRemodelFolder;
//
    //    private Mailbox newStoresMailbox;
//
    //    private FolderId newStoresInboxFolder;
//
    //    private FolderId newStoresDraftsFolder;
//
    //    private Mailbox existingStoresMailBox;
//
    //    private FolderId existingStoresInboxFolder;
//
    //    private FolderId existingStoresDraftsFolder;

    public static EmailHelper InstanceOf
    {
      get { return instance; }
    }

    private EmailHelper()
    {
      #if !DEBUG

      emailService = Service.ConnectToService(UserData.CreateUserData(
        Configuration.ConfigHandler.InstanceOf.MailUser
       ));

      #elif DEBUG

      emailService = Service.ConnectToService(UserData.CreateUserData(
        Configuration.ConfigHandler.InstanceOf.MailUser),new LogTraceListener());

      #endif

      rfiMailBox = new Mailbox( Configuration.ConfigHandler.InstanceOf.MailBoxes.ToList().Find( x => x.Address == "rfi@rhaaia.com" ).Address );

      rfiInboxFolder = new FolderId(WellKnownFolderName.Inbox, rfiMailBox);

      rfiDraftsFolder = new FolderId(WellKnownFolderName.Drafts, rfiMailBox);

      rfiDeletedItemsFolder = new FolderId(WellKnownFolderName.DeletedItems, rfiMailBox);

      //      newStoresMailbox = new Mailbox(Configuration.InstanceOf.MailBoxes.Find( x => x.Address == "P4-5@rhaaia.com" ).Address);
//
      //      newStoresInboxFolder = new FolderId(WellKnownFolderName.Inbox, newStoresMailbox);
//
      //      newStoresDraftsFolder = new FolderId(WellKnownFolderName.Drafts, newStoresMailbox);
//
      //      existingStoresMailBox = new Mailbox(Configuration.InstanceOf.MailBoxes.Find( x => x.Address == "P1-7-17@rhaaia.com" ).Address);
//
      //      existingStoresInboxFolder = new FolderId(WellKnownFolderName.Inbox, existingStoresMailBox);
//
      //      existingStoresDraftsFolder = new FolderId(WellKnownFolderName.Drafts, existingStoresMailBox);

      connector = new Data.DatabaseConnector();
      
      this.ConfigureSubFolders();
    }

    public ExchangeService GetEmailService
    {
      get { return this.emailService; }
    }

    public Mailbox RfiMailBox
    {
      get { return this.rfiMailBox; }
    }

    public FolderId RfiInboxFolder
    {
      get { return this.rfiInboxFolder; }
    }

    public FolderId RfiDraftsFolder
    {
      get { return this.rfiDraftsFolder; }
    }
    
    public FolderId NewStoresFolder
    {
      get { return this.rfiNewStoresFolder; }
    }
    
    public FolderId ExistingStoresFolder
    {
      get { return this.rfiExistingStoresFolder; }
    }

    //    public Mailbox NewStoresMailBox
    //    {
    //      get { return this.newStoresMailbox; }
    //    }
//
    //    public FolderId NewStoresInboxFolder
    //    {
    //      get { return this.newStoresInboxFolder; }
    //    }
//
    //    public FolderId NewStoresDraftsFolder
    //    {
    //      get { return this.newStoresDraftsFolder; }
    //    }
//
    //    public Mailbox ExistingStoresMailBox
    //    {
    //      get { return this.existingStoresMailBox; }
    //    }
//
    //    public FolderId ExistingStoresInboxFolder
    //    {
    //      get { return this.existingStoresInboxFolder; }
    //    }
//
    //    public FolderId ExistingStoresDraftsFolder
    //    {
    //      get { return this.existingStoresDraftsFolder; }
    //    }

    public static PropertySet EmailProperties
    {
      get { return new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments); }
    }
    
    private void ConfigureSubFolders ()
    {
      var folderView = new FolderView(10);
      
      folderView.PropertySet = new PropertySet(BasePropertySet.IdOnly);
      
      folderView.PropertySet.Add( FolderSchema.DisplayName );
      
      folderView.Traversal = FolderTraversal.Shallow;
      
      var found = this.emailService.FindFolders(this.rfiInboxFolder, folderView);
      
      var mailBox = Configuration.ConfigHandler.InstanceOf.MailBoxes.ToList().Find(
        x => x.Address == "rfi@rhaaia.com" );
      
      var query = found.Where( x => mailBox.Folders.Exists( y => y == x.DisplayName ));
      
      foreach (var folder in query) {
        switch (folder.DisplayName) {
          case "New Stores":
            rfiNewStoresFolder = folder.Id;
            
            break;
          case "Program 1-7-17":
            rfiExistingStoresFolder = folder.Id;
            
            break;
          case "Program 3-14":
            rfiRemodelFolder = folder.Id;
            
            break;
          default:
            
            break;
        }
      }
    }

    public bool AssignCategory (EmailMessage message, out Entity.Store store)
    {
      string category;

      bool isSuccess = false;

      var emailSubject = message.Subject;

      try {
        var result = Utilities.Categorizer.InstanceOf.TryCategorizationFromString(emailSubject, out category, out store);

        isSuccess = this.setCategory(result, message, category, store);
        
        try {
          // TODO: make this test smarter, we only want to check and download attachments on the very first email
          if (message.Sender.Name == "Amy Mills" && message.ToRecipients[0].Name == "RFI") {
            var success = this.TryGetAndAttachDocuments(message, store, category);
            
            if (success) {
              // disable saving attachments as it currently does not find the server paths correctly
              /*var fileHandler = new Utilities.FileHandler(store);
              
              fileHandler.SaveAttachmentsToFolder(message.Attachments);*/
            }
          }
        } catch {
          // for now we'll forget about the attachments.
        }

        //this.AddToRfiIndex(message.Body.Text);

      } catch (Exception exception) {
        // test if the message is an RFI
        if (IsRfi(message)) {
          // the message is an RFI
          // test if it is a walmart job
          var isWalmart = this.IsWalmartJob(message);

          if (isWalmart) {
            System.Collections.Generic.List< Entity.Store > storeList;
            
            // is a walmart job.  we need to figure out which store it is
            var result = Utilities.Categorizer.InstanceOf.DiscoverProject(message, out category, out storeList);
            
            store = storeList[0]; //TODO: iterate the store list
            
            isSuccess = this.setCategory(result, message, category, store);
          } else {
            // not a walmart job.
            // FIXME: figure out what kind of job it is and give the store object the correct type.
            // For now we'll just move the email to the general rfi box.
            store = new Entity.Store(); // return an empty store object becuase we have to.

            store.Type = Enum.ProjectTypes.General;
          }
        } else {
          // message is not an RFI.  so we'll ignore it.
          store = new Entity.Store();

          store.Type = Enum.ProjectTypes.None;

          isSuccess = false;
        }

      }

      return isSuccess;
    }

    public bool HandleEmailItem (EmailMessage message)
    {
      Entity.Store store;

      var result = this.AssignCategory(message, out store);

      if (result) {
        message.Update(ConflictResolutionMode.AlwaysOverwrite);

        result = this.SetDueDate(message);

        result = this.MoveEmailItemInOutlook(message, store);
      }

      return result;
    }

    public bool RewriteSender(EmailMessage message)
    {
      var email = new EmailAddress("rfi@rhaaia.com");

      message.From = email;

      try {
        message.Update(ConflictResolutionMode.AlwaysOverwrite);

        return true;
      } catch {
        return false;
      }
    }

    public bool MoveEmailItemInOutlook (EmailMessage message, Entity.Store store)
    {
      //var forwardEmail = this.CreateDraftEmail(message, store);
      
      Item movedMessage;
      
      switch (store.Program) {
        case Enum.ProgramTypes.None:
        case Enum.ProgramTypes.Generic:

          break;
        case Enum.ProgramTypes.NewStores:
          movedMessage = message.Move(this.rfiNewStoresFolder);
          
          //this.ArchiveAnsweredRfi(movedMessage, store);
          
          break;
        case Enum.ProgramTypes.TakeOvers:
        case Enum.ProgramTypes.Resturants:
        case Enum.ProgramTypes.Expansions:
        case Enum.ProgramTypes.Special:
        case Enum.ProgramTypes.DallasCommunityCollege:
        case Enum.ProgramTypes.ABRHoldings:
        case Enum.ProgramTypes.Express:
          movedMessage = message.Move(this.rfiExistingStoresFolder);
          
          //this.ArchiveAnsweredRfi(movedMessage, store);
          
          break;
        case Enum.ProgramTypes.FuelStations:
        case Enum.ProgramTypes.Remodels:
            movedMessage = message.Move(this.rfiRemodelFolder);
            
            break;
        default:
          throw new Exception("Invalid value for ProgramTypes");
      }
      


      return false;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="store"></param>
    /// <param name="category"></param>
    /// <returns>true if we were able to set attachments; false otherwise</returns>
    public bool TryGetAndAttachDocuments (EmailMessage message, Entity.Store store, string category)
    {
      var regex = new System.Text.RegularExpressions.Regex(@"RFI:\s(?<rfi>\d+)\b");
      
      var match = regex.Match(category);
      
      var group = match.Groups;
      
      var sid = group["rfi"].Value;
      
      int rfiNumber;
      
      if ( int.TryParse(sid, out rfiNumber) ) {
        var creds = Configuration.ConfigHandler.InstanceOf.GetCredentials;
        
        var crawler = new EvocoWebCrawler.HttpClient(creds);
        
        System.IO.Stream docStream;
        
        if ( crawler.TryNavigateStoreRfi(store.Number, store.Sequence, rfiNumber, out docStream) ) {
          var rfiAttachments = crawler.GetHttpFileAttachment(docStream);
          
          int count = 1;
          
          foreach (var item in rfiAttachments) {
            //we're going to rename the rfi attachments to match the RHA convention:
            // {storenumber}_RFI {rfinumber,D3}_Attachment {ordinal,D2}.{extension}
            // example: 6188_RFI 044_Attachment 01.pdf
            var parser = new Parser();
            
            var extension = parser.GetAttachmentExtension(item.Key);
            
            var name = string.Format( "{0}_RFI {1}_Attachment {2}{3}", store.Number.ToString(), rfiNumber.ToString("D3"), count.ToString("D2"), extension );
            
            message.Attachments.AddFileAttachment(name, item.Value);
            
            Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Found and attached file: ", name);
            
            count ++;
          }
          
          return true;
        }
      }
      
      return false;
    }
    
    public FindItemsResults< Item > GetMessagesOfSameCategory (EmailMessage message)
    {
      var category = message.Categories.ToList();
      
      if ( category.Count > 1 ) {
        // FIXME: bug when more than one category
      }
      
      var itemView = new ItemView(int.MaxValue);
      
      itemView.PropertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.Categories);
      
      var filter = new SearchFilter.IsEqualTo(EmailMessageSchema.Categories, category[0]);
      
      var results = EmailHelper.InstanceOf.GetEmailService.FindItems(message.ParentFolderId, filter, itemView);
      
      Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Found {0} emails to archive", results.Items.Count );
      
      return results;
    }
    
    public FolderId GetOutlookProjectPath (Entity.Store store)
    {
      //find the program folder first then search it for the project folder
      var parentView = new FolderView(int.MaxValue);
      
      parentView.PropertySet = new PropertySet(BasePropertySet.IdOnly, FolderSchema.DisplayName);
      
      var programNumber = Configuration.ConfigHandler.InstanceOf.ProgramMappings.Where(
        x => x.Value.Equals(store.Program))
        .Select(y => y.Key).Distinct();
      
      var taskResult = new System.Collections.Generic.List< System.Threading.Tasks.Task< FolderId > >();
      
      foreach ( var element in programNumber) {
        var filter = new SearchFilter.ContainsSubstring( FolderSchema.DisplayName, element.ToString("D2") + " " );

        var result = this.GetEmailService.FindFolders(WellKnownFolderName.PublicFoldersRoot, filter, parentView);
        
        var programFolder = result.Where( y => typeof(Microsoft.Exchange.WebServices.Data.ContactsFolder) != y.GetType() );

        taskResult.Add( this.FindProjectPath( programFolder.First(), 0, 255, "("+store.ProjectNumber.ToString("D7")+")", 0 ) );
      }
      
      var asyncResults = System.Threading.Tasks.Task.WhenAll(taskResult);
      
      var folders = asyncResults.Result;
      
      return folders.Single(x => x != null);
    }
    
    private async System.Threading.Tasks.Task< FolderId > FindProjectPath (Folder parentFolder, int start, int offset, string projectNumber, int recursionCount)
    {
      var view = new FolderView(int.MaxValue);
      
      view.PropertySet =  new PropertySet(BasePropertySet.IdOnly, FolderSchema.DisplayName);
      
      var filter = new SearchFilter.ContainsSubstring( FolderSchema.DisplayName, projectNumber );
      
      var results = await System.Threading.Tasks.Task.Run( () => {
                                                            return this.GetEmailService.FindFolders(parentFolder.Id, filter, view);
                                                          });
      
      //var folders = parentFolder.FindFolders(filter, view);
      
      if ( results.Folders.Count <= 0 ) {
        //return await this.FindProjectPath(parentFolder, offset, offset + 255, projectNumber, recursionCount ++);
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("FindProjectPath for {0} returned null after searching in {1}", projectNumber, parentFolder.DisplayName);
        
        return null;
      } else if (recursionCount > 5) {
        throw new TimeoutException("recursion too deep");
      } else {
        var projectFolder = results.Folders[0].Id;
        
        Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("FindProjectPath returned a project folder {0} after searching in {1}", projectFolder.FolderName, parentFolder.DisplayName);
        
        return projectFolder;
      }
    }
    
    public FolderId GetProjectRfiFolder (FolderId folderId)
    {
      var folder = Folder.Bind( this.GetEmailService, folderId, new PropertySet(BasePropertySet.IdOnly, FolderSchema.DisplayName) );
      
      var folderView = new FolderView(int.MaxValue);
      
      folderView.PropertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.Categories);
      
      var filter = new SearchFilter.ContainsSubstring( FolderSchema.DisplayName, "RFI" );
      
      var result = folder.FindFolders(filter, folderView);
      
      return result.Folders[0].Id;
    }

    public bool SetDueDate (EmailMessage message)
    {
      var conversationId = message.ConversationId;

      /* GetConversationItems is valid only on Exchange Server 2013 or later
var foldersToIgnore = new System.Collections.Generic.List< FolderId >() {
rfiDeletedItemsFolder, rfiDraftsFolder
};

var conversation = this.GetEmailService.GetConversationItems(conversationId, EmailProperties, null, foldersToIgnore, ConversationSortOrder.DateOrderAscending);


var searchView = new ConversationIndexedItemView(100, 0, OffsetBasePoint.Beginning);

// FindConversationResults

var conversations = this.GetEmailService.FindConversation(searchView, rfiInboxFolder);

foreach (var node in conversations) {
if (node.Topic == message.ConversationTopic && node.MessageCount == 1) {

message.Flag.DueDate = Utilities.DateTime.DueDate(message.DateTimeReceived, "72 Hours");

message.Update(ConflictResolutionMode.AlwaysOverwrite);
break;
}
}*/

      return true;
    }

    public ResponseMessage CreateDraftEmail (EmailMessage originalEmail, Entity.Store store)
    {
      var forwardEmail = originalEmail.CreateForward();

      try {
        var contacts = connector.GetProjectContacts(store);

        foreach (var contact in contacts) {
          forwardEmail.ToRecipients.Add(contact.Email);
        }
      } catch (ArgumentNullException ex) {

      }

      var body = @"Please see request for information below and respond as soon as possible.

PLEASE DO NOT MODIFY SUBJECT LINE
";
      forwardEmail.BodyPrefix = body;

      return forwardEmail;
    }

    private bool setCategory (Enum.QuestionTypes result, EmailMessage message, string category, Entity.Store store)
    {
      bool isSuccess = false;

      // check to see if the subject line had all of the information needed, otherwise look through the email body
      if (result == Enum.QuestionTypes.Success) {
        message.Categories.Clear();

        message.Categories.Add(category);

        isSuccess = true;
      } else if (result == Enum.QuestionTypes.RequestForInformation) {
        result = Utilities.Categorizer.InstanceOf.TryCategorizationFromText(message.Body, store, out category);

        // if after searching the body we figured out the RFI # then great, add it to the category
        switch (result) {
          case Enum.QuestionTypes.None:

            break;
          case Enum.QuestionTypes.Success:

            // break;
          case Enum.QuestionTypes.RequestForInformation:
            message.Categories.Clear();

            message.Categories.Add(category);

            isSuccess = true;
            break;
          case Enum.QuestionTypes.BidQuestion:

            break;
          case Enum.QuestionTypes.VendorQuestion:

            break;
          case Enum.QuestionTypes.ProtoQuestion:

            break;
          case Enum.QuestionTypes.GenericQuestion:

            break;
          default:
            throw new Exception("Invalid value for QuestionTypes");
        }
      }

      return isSuccess;
    }

    /// <summary>
    ///Decides if an email is an RFI
    /// </summary>
    /// <param name="email">Microsoft.Exchange.WebServices.Data.EmailMessage</param>
    /// <returns>True if it is an RFI, false otherwise</returns>
    public bool IsRfi (EmailMessage email)
    {
      var detector = new Utilities.Detective(
        Data.RfiIndexTable.Index, Data.SpamIndexTable.Index);
      
      var search = string.Format("{0} {1}", email.Subject, email.Body);
      
      var result = detector.SearchString(search).GetResult();
      
      switch (result) {
        case nBayes.CategorizationResult.First:
          return true;
          //break;
        case nBayes.CategorizationResult.Undetermined:
          return false;
          //break;
        case nBayes.CategorizationResult.Second:
          return false;
          //break;
        default:
          throw new Exception("Invalid value for CategorizationResult");
      }
      
    }

    public bool IsWalmartJob (EmailMessage email)
    {
      var detector = new Utilities.Detective(
        Data.WalmartIndexTable.Index, Data.GeneralIndexTable.Index);
      
      var search = string.Format("{0} {1}", email.Subject, email.Body);
      
      var result = detector.SearchString(search).GetResult();
      
      switch (result) {
        case nBayes.CategorizationResult.First:
          return true;
          //break;
        case nBayes.CategorizationResult.Undetermined:
          return false;
          //break;
        case nBayes.CategorizationResult.Second:
          return false;
          //break;
        default:
          throw new Exception("Invalid value for CategorizationResult");
      }
    }
    
    public void AddToRfiIndex (string content)
    {
      var document = nBayes.Entry.FromString(content);
      
      Data.RfiIndexTable.Index.AddAndSave(document);
    }
    
    public bool ArchiveAnsweredRfi(Microsoft.Exchange.WebServices.Data.Item email, Entity.Store store)
    {
      var message = Microsoft.Exchange.WebServices.Data.EmailMessage.Bind(this.GetEmailService,
                                                                          email.Id,
                                                                          EmailProperties
                                                                         );
      
      var subject = message.Subject;
      
      var parser = new Parser();
      
      var success = false;
      
      if ( parser.IsAnsweredQuestion(subject) ) {
        if (message.Sender.Name == "Amy Mills" && message.ToRecipients[0].Name == "RFI") {
          Logger.LoggerAsync.InstanceOf.GeneralLogger.Warn("Going to archive answered RFIs for {0}", message.Categories[0] );
          
          var results = this.GetMessagesOfSameCategory( message );
          
          success = this.MoveEmailsToFolder(results, store);
        }
      }
      
      return success;
    }
    
    private bool MoveEmailsToFolder ( FindItemsResults< Item > items, Entity.Store store )
    {
      var folderId = this.GetOutlookProjectPath(store);
      
      var rfiFolderId = this.GetProjectRfiFolder(folderId);
      
      var success = true;
      
      foreach (var item in items) {
        var message = Microsoft.Exchange.WebServices.Data.Item.Bind(this.GetEmailService,
                                                                    item.Id,
                                                                    EmailProperties
                                                                   );

        if (message.HasAttachments) {
          success = this.MoveAttachmentsToFolder(message, store);
          
          Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Moved attachments to folder for project {0} {1}", store.ProjectNumber, store.City);
        }
        
        if (success) {
          try {
            if (message.Flag.FlagStatus == ItemFlagStatus.Flagged) {
              message.Flag.FlagStatus = ItemFlagStatus.Complete;
            }
            
            message.Update(ConflictResolutionMode.AlwaysOverwrite);
            
            var newItem = message.Move(rfiFolderId);
            
            success = true;
            
            Logger.LoggerAsync.InstanceOf.GeneralLogger.Info("Moved email to folder for project {0} {1}", store.ProjectNumber, store.City);
          } catch (Exception x) {
            success = false;
            
            Logger.LoggerAsync.InstanceOf.GeneralLogger.Error("Moving email to folder failed: {0}", x );
          }
        }
      }
      
      return success;
    }
    
    private bool MoveAttachmentsToFolder ( Microsoft.Exchange.WebServices.Data.Item item, Entity.Store store)
    {
      var fileHandler = new Utilities.FileHandler(store);
      
      if (item.HasAttachments) {
        var success = fileHandler.SaveAttachmentsToFolder(item.Attachments);
        
        if (success) {
          item.Attachments.Clear();
          
          item.Update(ConflictResolutionMode.AlwaysOverwrite);
        }
      }
      
      return false;
    }

  }
}
