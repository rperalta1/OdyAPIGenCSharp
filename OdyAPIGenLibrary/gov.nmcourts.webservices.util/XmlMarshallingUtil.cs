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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace gov.nmcourts.webservices.util
{
    public class XmlMarshallingUtil
    {
        public static String ERROR_COULD_NOT_INSTANTIATE_CLASS = "Unable to instantiate class for the reply XML object";
        
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString();
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                {
                    if(attribute.IsNamespaceDeclaration)
                    {
                        continue;
                    }  
                    xElement.Add(attribute);
                }
                return xElement;
            }
            XElement buffer = new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
            foreach (XAttribute attribute in xmlDocument.Attributes())
            {
                if(attribute.IsNamespaceDeclaration){
                    continue;
                }                
                buffer.Add(attribute);
            }
            return buffer;
        }

        public static String MarshallRequest(Object request) {

            XmlSerializer serializer1 = new XmlSerializer(request.GetType());
            String xmlDocument = "";

            using (StringWriter stm = new StringWriter())
            {
                using(XmlWriter writer = XmlWriter.Create(stm))
                {
                    serializer1.Serialize(writer, request);
                    xmlDocument = stm.ToString();
                }

            }
            String rawResultXml = RemoveAllNamespaces(xmlDocument);
            return rawResultXml;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static object UnmarshallResponse(String replyXml, Type expectedReplyClass)
        {
            XmlSerializer serializer = new XmlSerializer(expectedReplyClass);
            using (TextReader reader = new StringReader(replyXml))
            {
                return serializer.Deserialize(reader);
            }
        }
    }
}

