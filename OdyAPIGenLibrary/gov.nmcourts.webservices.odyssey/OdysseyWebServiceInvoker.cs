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
using generated.com.tylertech.api;
using System;
using System.IO;
using System.Xml;
using gov.nmcourts.webservices.exception;
using gov.nmcourts.webservices.util;

namespace gov.nmcourts.webservices.odyssey
{
    public class OdysseyWebServiceInvoker
    {
        public static String ERROR_COULD_NOT_MARSHAL_XML       = "Unable to marshall request object to XML";
        public static String ERROR_COULD_NOT_UNMARSHAL_XML     = "Unable to unmarshall reply XML to object";	    
	    public static String ERROR_COULD_NOT_GET_API_WEB_STUB  = "Could not get api web service stub";        

        private static APIWebService apiWebServiceStub;
        private String endpointAPIWebService;
        private String siteId;

        public OdysseyWebServiceInvoker(String endpointAPIWebService, String siteId)
        {
            this.endpointAPIWebService = endpointAPIWebService;
            this.siteId = siteId;
            Console.WriteLine("API URI is: " + endpointAPIWebService);
            Console.WriteLine("Tyler siteId is: " + siteId);
        }

        private void InitApiWebService() 
        {
            try
            {
                if (apiWebServiceStub == null)
                {
                    Uri wsdlLocation = new Uri(endpointAPIWebService);
                    apiWebServiceStub = new APIWebService(wsdlLocation);
                }
            } catch (OdysseyWebServiceException e)
            {
                Console.WriteLine(e.StackTrace);
            }
	    }

        private APIWebService GetApiWebServiceStub()
        {
            try
            {
                if (null == apiWebServiceStub)
                {
                    InitApiWebService();
                }
            } catch (OdysseyWebServiceException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return apiWebServiceStub;
        }

        public static string AsString(XmlDocument xmlDoc)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    xmlDoc.WriteTo(tx);
                    string strXmlText = sw.ToString();
                    return strXmlText;
                }
            }
        }

        public object Invoker(Object message, String expectedReplyName, Type expectedReplyClass)
        { 
            String requestXML = null;
		    try
            {
                requestXML = XmlMarshallingUtil.MarshallRequest(message);
                // For debugging
                //Console.WriteLine("XML for request is: \n" + requestXML);
            } catch (Exception e) {
			    throw new OdysseyWebServiceException(ERROR_COULD_NOT_MARSHAL_XML, e);
            }

            String rawResultXml = GetApiWebServiceStub().OdysseyMsgExecution(requestXML, siteId);
            // For debugging
            //Console.WriteLine("Odyssey response was: " + rawResultXml);

            XmlDocument addNameSpaceXmlDocument = new XmlDocument();
            addNameSpaceXmlDocument.LoadXml(rawResultXml);
            XmlElement root = addNameSpaceXmlDocument.DocumentElement;
            root.SetAttribute("xmlns", "http://common.namespace/" + expectedReplyName);
            String replyXML = AsString(addNameSpaceXmlDocument);

            // For debugging
            //Console.WriteLine(replyXML);

            object unmarshal = null;
            try {
			    unmarshal = XmlMarshallingUtil.UnmarshallResponse(replyXML, expectedReplyClass);
		    } catch (Exception e) {
			    throw new OdysseyWebServiceException(ERROR_COULD_NOT_UNMARSHAL_XML, e);
		    } 
		    return unmarshal;
	    }
    }
}
