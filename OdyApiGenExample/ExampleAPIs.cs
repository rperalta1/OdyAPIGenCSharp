/**************************************

    Copyright (C) 2018  
    Judicial Information Division,
    Administrative Office of the Courts,
    State of New Mexico

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

***************************************/
using System;
using System.IO;
using generated.com.tylertech.xsdbindings.loadcase;
using generated.com.tylertech.xsdbindings.loadcaseresult;
using gov.nmcourts.webservices.exception;
using gov.nmcourts.webservices.odyssey;

namespace OdyApiGenExample
{
    class ExampleAPIs
    {
        static void Main(string[] args)
        {
            ExampleAPIs exampleAPIs = new ExampleAPIs();
            exampleAPIs.Run();
        }

        public void Run()
        {
            OdysseyWebServiceInvoker odysseyWebServiceInvoker =
                new OdysseyWebServiceInvoker("http://dev-config-all.nmcourts.gov/WebServices/APIWebService.asmx?WSDL", "DEVCONFIG");
            String referenceNumber = "TEST_REFNUM_1234";
            String source = "TEST_SOURCE";
            String userID = "4847";
            String nodeID = "1";
            String odysseyCaseNumber = "D307NH9700003";
            
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine("Testing FindCaseByCaseNumber API -- finding caseId and node Id:");
            Console.WriteLine("---------------------------------------------------------------");
            GetResults results = FindCaseByCaseNumber(odysseyWebServiceInvoker, odysseyCaseNumber, nodeID, userID,
                                                        referenceNumber, source);
            
            // GetResult holds caseID and nodeID needed for testing other APIs
            Console.WriteLine(results);

            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("Testing GetOdysseyReleaseLevel -- verifiyingthe release and current patch of an Odyssey server:");
            Console.WriteLine("-------------------------------------------------------------------------");
            GetOdysseyReleaseLevel(odysseyWebServiceInvoker, userID, referenceNumber, source);

            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine("Testing LoadCase API -- loading case events from a case number:");
            Console.WriteLine("---------------------------------------------------------------");
            LoadCaseCaseEvent[] loadEvents = LoadCase(odysseyWebServiceInvoker, results.CaseID, results.NodeID,
                                userID, referenceNumber, source);
            for (int i = 0; i < loadEvents.Length; i++)
            {
                Console.WriteLine(loadEvents[i].EventID + ">>" + loadEvents[i].Type + ">>" + loadEvents[i].Date + ">>" + loadEvents[i].Comment);
            }

            Console.WriteLine("---------------------------------------------------------------------------");
            Console.WriteLine("Testing GetDocumentInfoByEntity -- finding documents in loaded case events:");
            Console.WriteLine("---------------------------------------------------------------------------");

            int lastCaseEventWithDocument = -1;
            GetDocumentResults getDocumentsResults = null;
            for (int index = 0; index < loadEvents.Length; index++)
            {
                getDocumentsResults = GetDocumentInfoByEntity(odysseyWebServiceInvoker, loadEvents[index].EventID,
                        userID, referenceNumber, source);
                if (getDocumentsResults != null)
                {
                    lastCaseEventWithDocument = index;
                }
            }
            if (lastCaseEventWithDocument < 0)
            {
                Console.WriteLine("Test was not completed since a case event with a document was not found within the case number");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Last document:" + getDocumentsResults);
            }
            
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Testing GetDocument -- downloading document:");
            Console.WriteLine("--------------------------------------------");

            // File extension will be added by the API
            
            String fileName = "c:\\tmp\\downtest";
            GetDocument(odysseyWebServiceInvoker, getDocumentsResults.DocumentVersionID, fileName, "0",
                    userID, referenceNumber, source);
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Testing AddCaseEvent & LinkDocument APIs -- duplicating last event:");
            Console.WriteLine("-------------------------------------------------------------------");
            String newCaseEventID = AddCaseEvent(odysseyWebServiceInvoker, results.CaseID,
                    loadEvents[lastCaseEventWithDocument].Type,
                    DateTime.Now.ToString("yyyy-MM-dd"), "Comment for the new case event", results.NodeID,
                    userID, referenceNumber, source);

            LinkDocument(odysseyWebServiceInvoker,
                    getDocumentsResults.DocumentID,
                    newCaseEventID,
                    results.NodeID,
                    userID, referenceNumber, source);

            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Testing EditCaseEvent API -- case event comment has been changed:");
            Console.WriteLine("-----------------------------------------------------------------");
            EditCaseEvent(odysseyWebServiceInvoker, newCaseEventID, "Comment has been changed by EditCaseEvent API",
                    results.NodeID, userID, referenceNumber, source);

            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("Testing AddCaseCrossReferenceNumber API -- adding one CCR to case number:");
            Console.WriteLine("-------------------------------------------------------------------------");
            String newCCR = "CCR" + DateTime.Now.ToString(" dd-MM-yy") + DateTime.Now.ToString(" hh:mm:ss");
            AddCaseCrossReferenceNumber(odysseyWebServiceInvoker, results.CaseID,
                    newCCR, "SE", results.NodeID, userID, referenceNumber, source);
        }

        private GetResults FindCaseByCaseNumber(OdysseyWebServiceInvoker odyInvoker, String caseNumber, String nodeID,
                                          String userID, String referenceNumber, String source)
        {
            generated.com.tylertech.xsdbindings.findcasebycasenumber.Message message =
                new generated.com.tylertech.xsdbindings.findcasebycasenumber.Message
                {
                    CaseNumber = caseNumber,
                    MessageType = generated.com.tylertech.xsdbindings.findcasebycasenumber.LOCALFINDCASEBYCASENUMBER.FindCaseByCaseNumber,
                    NodeID = nodeID,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source
                };

            generated.com.tylertech.xsdbindings.findcasebycasenumberresult.Result reply =
                    (generated.com.tylertech.xsdbindings.findcasebycasenumberresult.Result)
                        odyInvoker.Invoker(message, "FindCaseByCaseNumberResult", 
                            typeof(generated.com.tylertech.xsdbindings.findcasebycasenumberresult.Result));

            Console.WriteLine("Case Id: " + reply.CaseID + " node Id: " + reply.NodeID);

            GetResults getResults = new GetResults
            {
                CaseID = reply.CaseID,
                NodeID = reply.NodeID
            };
            return getResults;
        }

        private LoadCaseCaseEvent[] LoadCase(OdysseyWebServiceInvoker odyInvoker, String caseID, String nodeID, String userID, 
            String referenceNumber, String source)
        {
            generated.com.tylertech.xsdbindings.loadcase.Message message = 
                new generated.com.tylertech.xsdbindings.loadcase.Message
            {
                NodeID = nodeID,
                UserID = userID,
                ReferenceNumber = referenceNumber,
                Source = source,
                CaseID = caseID,
                MessageType = generated.com.tylertech.xsdbindings.loadcase.LOCALLOADCASE.LoadCase
            };

            LoadEntitiesCollection loadEntities = new LoadEntitiesCollection
            {
                CaseEvents = "true",
                CaseCrossReferences = "false",
                CaseFlags = "false",
                CaseParties = "true",
                CaseStatuses = "false",
                CausesOfAction = "false",
                Charges = "false"
            };
            message.LoadEntities = loadEntities;

            generated.com.tylertech.xsdbindings.loadcaseresult.Result reply =
                (generated.com.tylertech.xsdbindings.loadcaseresult.Result)
                odyInvoker.Invoker(message, "LoadCaseResult", typeof(generated.com.tylertech.xsdbindings.loadcaseresult.Result));
            return reply.Case.Events;
        }

        private GetDocumentResults GetDocumentInfoByEntity(OdysseyWebServiceInvoker odyInvoker, String entityId,
            String userID, String referenceNumber, String source)
        {

            generated.com.tylertech.xsdbindings.getdocumentinfobyentity.Message message =
                new generated.com.tylertech.xsdbindings.getdocumentinfobyentity.Message
                {
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    EntityType = generated.com.tylertech.xsdbindings.getdocumentinfobyentity.DocumentByEntityEntityType.Event,
                    EntityID = entityId,
                    IncludeObsolete = "false",
                    MessageType = generated.com.tylertech.xsdbindings.getdocumentinfobyentity.DocumentByEntityMessageType.GetDocumentInfoByEntity,
                    NodeID = generated.com.tylertech.xsdbindings.getdocumentinfobyentity.BASEREQUIREDZERO.Item0
                };

            generated.com.tylertech.xsdbindings.getdocumentinfobyentityresult.Result reply =
                (generated.com.tylertech.xsdbindings.getdocumentinfobyentityresult.Result)
                odyInvoker.Invoker(message, "GetDocumentInfoByEntityResult", 
                    typeof(generated.com.tylertech.xsdbindings.getdocumentinfobyentityresult.Result));

            generated.com.tylertech.xsdbindings.getdocumentinfobyentityresult.GETDOCINFODOCUMENT document = null;
            try
            {
                document = reply.Documents[0];
            }
            catch (Exception)
            {
                return null;
            }
            GetDocumentResults getDocumentsResults = new GetDocumentResults
            {
                DocumentID = document.DocumentID,
                DocumentVersionID = document.CurrentDocumentVersionID
            };
            return getDocumentsResults;
        }
        
        private void GetDocument(OdysseyWebServiceInvoker odyInvoker, String documentVersionID, String fileName,
            String nodeID, String userID, String referenceNumber, String source)
        {
            generated.com.tylertech.xsdbindings.getdocument.Message message =
                new generated.com.tylertech.xsdbindings.getdocument.Message
                {
                    NodeID = generated.com.tylertech.xsdbindings.getdocument.BASEREQUIREDZERO.Item0,  
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    VersionID = documentVersionID,
                    MessageType = generated.com.tylertech.xsdbindings.getdocument.GetDocumentMessageType.GetDocument,
                    Item = "true",
                    ItemElementName = generated.com.tylertech.xsdbindings.getdocument.ItemChoiceType.IncludeEmbeddedDocument
                };

            generated.com.tylertech.xsdbindings.getdocumentresult.Result reply =
                (generated.com.tylertech.xsdbindings.getdocumentresult.Result)
                odyInvoker.Invoker(message, "GetDocumentResult", typeof(generated.com.tylertech.xsdbindings.getdocumentresult.Result));

            String dataDocument = reply.EmbeddedDocument[0].Document;
            byte[] decodedValue = Convert.FromBase64String(dataDocument);

            String extension = reply.EmbeddedDocument[0].Extension.ToString();
            fileName = fileName + "." + extension;

            FileStream file;
            try
            {
                file = new FileStream(fileName, FileMode.Create);
            }
            catch (FileNotFoundException e)
            {
                throw new OdysseyWebServiceException("Unable to create file", e);
            }
            try
            {
                file.Write(decodedValue, 0, decodedValue.Length);
                file.Close();
            }
            catch (IOException e)
            {
                throw new OdysseyWebServiceException("Unable to write data into a file", e);
            }
            Console.WriteLine("File was downloaded: " + fileName + " downloaded for document version Id " + documentVersionID);
        }
        
        private String AddCaseEvent(OdysseyWebServiceInvoker odyInvoker, String caseID,
                        String eventType, String dateEvent, String comment,
                        String nodeID, String userID, String referenceNumber, String source)
        {

            generated.com.tylertech.xsdbindings.addcaseevent.Message message =
                new generated.com.tylertech.xsdbindings.addcaseevent.Message
                {
                    CaseID = caseID,
                    CaseEventType = eventType,
                    Date = dateEvent,
                    Comment = comment,
                    NodeID = nodeID,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    MessageType = generated.com.tylertech.xsdbindings.addcaseevent.LOCALADDCASEEVENT.AddCaseEvent
                };

            generated.com.tylertech.xsdbindings.addcaseeventresult.Result reply =
                (generated.com.tylertech.xsdbindings.addcaseeventresult.Result)
                    odyInvoker.Invoker(message, "AddCaseEventResult", typeof(generated.com.tylertech.xsdbindings.addcaseeventresult.Result));

            Console.WriteLine("New case eventId: " + reply.CaseEventID);
            return reply.CaseEventID;
        }

        private void LinkDocument(OdysseyWebServiceInvoker odyInvoker, String docId, String eventID,
                String nodeID, String userID, String referenceNumber, String source)
        {

            generated.com.tylertech.xsdbindings.linkdocument.Message message =
                new generated.com.tylertech.xsdbindings.linkdocument.Message
                {
                    DocumentID = docId,
                    MessageType = generated.com.tylertech.xsdbindings.linkdocument.LOCALLINKDOCUMENT.LinkDocument,
                    NodeID = nodeID,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source
                };

            generated.com.tylertech.xsdbindings.linkdocument.MessageEntity entity =
                new generated.com.tylertech.xsdbindings.linkdocument.MessageEntity
                {
                    EntityID = eventID,
                    EntityType = generated.com.tylertech.xsdbindings.linkdocument.EntityTypeCode.Event
                };

            generated.com.tylertech.xsdbindings.linkdocument.MessageEntity[] entities =
                new generated.com.tylertech.xsdbindings.linkdocument.MessageEntity[1];
            entities[0] = entity;
            message.Entities = entities;

            generated.com.tylertech.xsdbindings.linkdocumentresult.Result reply =
                (generated.com.tylertech.xsdbindings.linkdocumentresult.Result)
                    odyInvoker.Invoker(message, "LinkDocumentResult", typeof(generated.com.tylertech.xsdbindings.linkdocumentresult.Result));

            Console.WriteLine("Document linked to case event: " + reply.Success);
        }

        private void EditCaseEvent(OdysseyWebServiceInvoker odyInvoker, String eventID, String comment,
                String nodeID, String userID, String referenceNumber, String source)
        {

            generated.com.tylertech.xsdbindings.editcaseevent.Message message =
                new generated.com.tylertech.xsdbindings.editcaseevent.Message
                {
                    CaseEventID = eventID,
                    NodeID = nodeID,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    MessageType = generated.com.tylertech.xsdbindings.editcaseevent.LOCALEDITCASEEVENT.EditCaseEvent
                };

            generated.com.tylertech.xsdbindings.editcaseevent.EditCaseEventEdit editCaseEventEdit =
                new generated.com.tylertech.xsdbindings.editcaseevent.EditCaseEventEdit
                {
                    Comment = comment
                };

            message.Edit = editCaseEventEdit;

            generated.com.tylertech.xsdbindings.editcaseeventresult.Result reply =
                (generated.com.tylertech.xsdbindings.editcaseeventresult.Result)
                    odyInvoker.Invoker(message, "EditCaseEventResult", 
                        typeof(generated.com.tylertech.xsdbindings.editcaseeventresult.Result));

            Console.WriteLine("Event comment was changed: " + reply.Success);
        }

        private void AddCaseCrossReferenceNumber(OdysseyWebServiceInvoker odyInvoker, String caseId, String crossRefNumber,
            String crossRefNumberType, String nodeID, String userID, String referenceNumber, String source)
        {

            generated.com.tylertech.xsdbindings.addcasecrossreferencenumber.Message message =
                new generated.com.tylertech.xsdbindings.addcasecrossreferencenumber.Message
                {
                    CaseID = caseId,
                    CrossReferenceNumber = crossRefNumber,
                    CrossReferenceNumberType = crossRefNumberType,
                    NodeID = nodeID,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    MessageType = generated.com.tylertech.xsdbindings.addcasecrossreferencenumber.LOCALADDCASECROSSREFERENCENUMBER.AddCaseCrossReferenceNumber
                };

            generated.com.tylertech.xsdbindings.addcasecrossreferencenumberresult.Result reply =
                (generated.com.tylertech.xsdbindings.addcasecrossreferencenumberresult.Result)
                    odyInvoker.Invoker(message, "AddCaseCrossReferenceNumberResult", 
                        typeof(generated.com.tylertech.xsdbindings.addcasecrossreferencenumberresult.Result));

            Console.WriteLine("CCR was added to case: " + reply.CaseCrossReferenceNumberID);
        }

        private void GetOdysseyReleaseLevel(OdysseyWebServiceInvoker odyInvoker, 
			String userID, String referenceNumber, String source) {
		
		    generated.com.tylertech.xsdbindings.getodysseyreleaselevel.Message message = 
				new generated.com.tylertech.xsdbindings.getodysseyreleaselevel.Message
		        {
                    NodeID = generated.com.tylertech.xsdbindings.getodysseyreleaselevel.BASEREQUIREDZERO.Item0,
                    UserID = userID,
                    ReferenceNumber = referenceNumber,
                    Source = source,
                    MessageType = generated.com.tylertech.xsdbindings.getodysseyreleaselevel.GETODYSSEYRELEASELEVELMESSAGETYPENAME.GetOdysseyReleaseLevel
                };
					
		        generated.com.tylertech.xsdbindings.getodysseyreleaselevelresult.Result reply = 
				    (generated.com.tylertech.xsdbindings.getodysseyreleaselevelresult.Result) 
				        odyInvoker.Invoker(message, "GetOdysseyReleaseLevelResult", 
                            typeof(generated.com.tylertech.xsdbindings.getodysseyreleaselevelresult.Result));
		
		        Console.WriteLine("GetOdysseyReleaseLevel release: " + reply.Release);
		        Console.WriteLine("GetOdysseyReleaseLevel path: " + reply.Patch);
	    }
    }
    
    class GetResults
    {
        private String nodeID;
        private String caseID;

        public string NodeID { get => nodeID; set => nodeID = value; }
        public string CaseID { get => caseID; set => caseID = value; }

        override
        public String ToString()
        {
            return "[Node:" + NodeID + ", caseId:" + CaseID + "]";
        }
    }

    class GetDocumentResults
    {
        private String documentID;
        private String documentVersionID;

        public string DocumentID { get => documentID; set => documentID = value; }
        public string DocumentVersionID { get => documentVersionID; set => documentVersionID = value; }

        override
        public String ToString()
        {
            return "[DocumentID:" + DocumentID + ", documentVersionID:" + DocumentVersionID + "]";
        }
    }
}
