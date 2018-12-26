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
using OdyAPIGenBuilder.gov.nmcourts.api.builder.model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OdyAPIGenBuilder.gov.nmcourts.api.builder.main
{
    class Builder : DiGraph
    {
        private List<XjcEpisode> buildEpisodes = new List<XjcEpisode>();
        private List<XmlDependency> xmlDependencies = new List<XmlDependency>();
        private List<String> completedEpisodes = new List<String>();

        static void Main(string[] args)
        {
            var applicationSettings = ConfigurationManager.GetSection("ApplicationSettings") as NameValueCollection;
            String dirOutput = applicationSettings["dirOutput"];
            String xsdDirInit = applicationSettings["xsdDirInit"];
            String xsdDirResource = applicationSettings["xsdDirResource"];
            String xsdDirInput = applicationSettings["xsdDirInput"];
            String xsdbindingsPackageName = applicationSettings["xsdbindingsPackageName"];
            String apiWsdl = applicationSettings["apiWsdl"];
            String apiPackageName = applicationSettings["apiPackageName"];
            String ixmlWsdl = applicationSettings["ixmlWsdl"];
            String ixmlPackageName = applicationSettings["ixmlPackageName"];
            String xsdExePath = applicationSettings["xsdExePath"];
            String wsdlExePath = applicationSettings["wsdlExePath"];
            TestAndCreateDirectory(dirOutput);
            TestAndCreateDirectory(xsdDirInput);
                    
            String startupPath = Environment.CurrentDirectory;
            Console.WriteLine("Builder is running ..." + startupPath);
            
            Builder builder = new Builder();
            
            builder.PreProcessing(xsdDirInit, xsdDirInput, "bt", "*.xsd", "xs", "include", "schemaLocation");
            builder.Processing(xsdExePath, xsdbindingsPackageName, "http://common.namespace/", xsdDirInput, dirOutput, ".xsd");

            builder.GenerateClassesFromWsdl(wsdlExePath, apiWsdl, apiPackageName, dirOutput);
            builder.GenerateClassesFromWsdl(wsdlExePath, ixmlWsdl, ixmlPackageName, dirOutput);
            
        }

        public void PreProcessing(String initXsdFolder, String dirInput, String prefix, String fileExtension, 
                                  String tylerPrefix, String tagName, String tagAttributeName)
        {   
            List<GraphDependency> dependencies = FindIncludesInXsdFiles(ListFiles(initXsdFolder, fileExtension),
                                                                            tylerPrefix, tagName, tagAttributeName);

            foreach (GraphDependency dependency in dependencies)
            {
                base.AddDiGraph(dependency.Node, dependency.Predecesor);
            }

            List<String> orderPackages = new List<String>();
            base.ProcessGraph(orderPackages);
            
            int i = 1;
            foreach (String orderPackage in orderPackages)
            {
                XjcEpisode episode = new XjcEpisode(Path.GetFileName(orderPackage), prefix + (i++), GetDependencies(orderPackage, dependencies));
                buildEpisodes.Add(episode);
            }

            foreach (String filePath in ListFiles(initXsdFolder, fileExtension))
            {
                if (filePath.Contains("errorstream.xsd")) continue;
                String filename = Path.GetFileName(filePath);
                CopyFile(filePath, dirInput + filename);
                HandleCollisions(dirInput + filename);
            }            
        }

        private List<GraphDependency> FindIncludesInXsdFiles(List<String> filePaths, String tylerPrefix, String tagName, 
                                                             String tagAttributeName)
        {
            List<GraphDependency> findings = new List<GraphDependency>();

            foreach (String filePath in filePaths)
            {
                String filename = Path.GetFileName(filePath);
                List<String> includes = null;
                includes = FindInclude(filePath, tylerPrefix, tagName, tagAttributeName);
                if (includes.Count == 0 || includes == null)
                {
                    findings.Add(new GraphDependency(filename, ""));
                }
                else
                {
                    foreach (String include in includes)
                    {
                        findings.Add(new GraphDependency(filename, include));
                    }
                }
            }
            return findings;
        }
        
        private List<String> FindInclude(String filePath, String prefix, String tagName, String tagAttributeName)
        {
            List<String> includeFiles = new List<String>();
            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;            
            XNamespace xmlns = root.GetNamespaceOfPrefix(prefix);
            IEnumerable<XElement> elements = (from c in doc.Descendants(xmlns + tagName) select c);

            foreach(XElement element in elements)
            {
                IEnumerable<XAttribute> nms = element.Attributes();
                foreach(XAttribute nm in nms)
                {
                    if (nm.Name.ToString().ToLower().Equals(tagAttributeName.ToLower()))
                    {
                        includeFiles.Add(nm.Value);
                    }
                }
            }
		    return includeFiles;
	    }

        private List<String> ListFiles(String folder, String fileExtension)
        {
            string[] fileArray = Directory.GetFiles(folder, fileExtension);
            List<String> filePaths = new List<String>();
            foreach (String file in fileArray)
            {
                    filePaths.Add(file);
            }

            // For debugging
            /*
            foreach (String filePath in filePaths)
                Console.WriteLine(filePath);
            */
            return filePaths;
        }

        private ArrayList GetDependencies(String orderPackage, List<GraphDependency> dependencies)
        {
            ArrayList listDependecies = new ArrayList();

            foreach (GraphDependency dependency in dependencies)
            {
                if (orderPackage.ToLower().Equals(dependency.Node.ToLower()))
                {
                    listDependecies.Add(dependency.Predecesor);
                }
            }
            return listDependecies;
        }
        
        private static void CopyFile(String source, String destination)
        {
            // Always overwrite files
            System.IO.File.Copy(source, destination, true);
        }

        private static void TestAndCreateDirectory(String subPath)
        {
            bool exists = System.IO.Directory.Exists(subPath);
            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);
        }

        private static void DeleteDirectory(String subPath)
        {
            bool exists = System.IO.Directory.Exists(subPath);
            if (exists)
                System.IO.Directory.Delete(subPath);
        }

        public void HandleCollisions(String filename)
        {
            // For debugging
            //Console.WriteLine("Processing XSD file :" + filename);
            List<XsdFileNode> xsdFileNodes = new List<XsdFileNode>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filename);
            do
            {
                xsdFileNodes.Add(new XsdFileNode(-1, -1, "DOC ROOT NODE", "")); // Adding root node for searchings
                XmlNodeList nodeList = xDoc.ChildNodes;
                ReadNode(nodeList, xsdFileNodes, 0);   // In Java version last value is 1 because it calls a method loadXsdFile not included in C# implemetation
                XsdFileNode collision = FindPaths(xsdFileNodes);

                if (collision != null)
                {
                    Console.WriteLine("Solving collision on:" + filename);                    
                    UpdateDoc(xDoc, collision);
                    xsdFileNodes.Clear();
                }
                else
                {
                    break;
                }
            } while (true);

            try
            {
                SaveToFile(xDoc, filename);
            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace.ToString());
            }

        }

        private void SaveToFile(XmlDocument doc, String filename)
        {
            doc.Save(filename);   
	    }

        private void UpdateDoc(XmlDocument doc, XsdFileNode collision)
        {
            XmlNodeList nodes = doc.GetElementsByTagName("xs:element");
            int counter = 1;
            String newName = null;
            bool isAvailable = true;
            do
            {
                newName = collision.Name + counter;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlElement element = (XmlElement)nodes.Item(i);
                    if (element.GetAttribute("name").Equals(newName))
                    {
                        isAvailable = false;
                    }
                }
                if (isAvailable) break;
                else
                {
                    counter++;
                    isAvailable = true;
                }
            } while (true);
            Console.WriteLine("Changing element name to: " + newName);
            XmlElement elements = doc.DocumentElement;
            XmlNodeList nodeList = elements.ChildNodes;
            UpdateNode(nodeList, 1, collision, newName);
        }

        private void UpdateNode(XmlNodeList nodeList, int level, XsdFileNode collision, String newName)
        {
            level++;
            if (nodeList != null && nodeList.Count > 0)
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode node = nodeList.Item(i);
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlElement en = (XmlElement)node;
                        String name = en.GetAttribute("name");                        
                        if (name != null && name.Length > 0 && collision.Level == level &&
                                 collision.Name == name && collision.Position == i)
                        {
                            en.SetAttribute("name", newName);
                            return;
                        }
                        UpdateNode(node.ChildNodes, level, collision, newName);
                    }
                }
            }
        }

        private void ReadNode(XmlNodeList nodeList, List<XsdFileNode> xsdFileNodes, int level)
        {
            level++;
            if (nodeList != null && nodeList.Count > 0)
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode node = nodeList.Item(i);
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlElement en = (XmlElement)node;
                        String name = en.GetAttribute("name");
                        String type = en.GetAttribute("type");
                        if (name != null && name.Length > 0)
                        {
                            if (type != null && type.Length > 0)
                            {
                                xsdFileNodes.Add(new XsdFileNode(level, i, name, type));
                            }
                            else
                            {
                                xsdFileNodes.Add(new XsdFileNode(level, i, name, ""));
                            }
                        }
                        ReadNode(node.ChildNodes, xsdFileNodes, level);
                    }                    
                }
            }
        }

        private void PrintPath(List<int> path)
        {
            Console.Write("[");
            for (int i = 0; i < path.Count; i++)
            {
                Console.Write(path[i]);
                if (i == path.Count - 1)
                    Console.WriteLine("]");
                else
                    Console.Write(",");
            }            
        }

        private XsdFileNode FindPaths(List<XsdFileNode> xsdFileNodes)
        {

            for (int cursor = 0; cursor < xsdFileNodes.Count; cursor++)
            {
                // For debugging
                //Console.WriteLine("---------------------------------------------------------------"); 

                List<int> path = new List<int>();
                int index = cursor;
                while (index >= 0)
                {
                    // For debugging
                    /*
                    Console.Write(" " + xsdFileNodes[index].Level + " >> " +
                                       xsdFileNodes[index].Name  + " : " +
                                       xsdFileNodes[index].Type );  
                    */

                    path.Add(index);
                    if (index == 0) break;
                    index = FindParent(index, xsdFileNodes);
                }
                // For debugging
                //printPath(path);

                XsdFileNode collision = FindCollisions(path, xsdFileNodes);
                if (collision != null)
                {
                    return collision;
                }
            }
            return null;
        }

        private int FindParent(int index, List<XsdFileNode> xsdFileNodes)
        {

            if (index == 0) return 0;
            int target = xsdFileNodes[index].Level;
            int previous = xsdFileNodes[index - 1].Level;

            if (previous < target)
            {   // Parent - child
                return index - 1;
            }
            else if (previous == target)
            {   // Find first sibling
                return FindParent(FindFirstSibling(index, xsdFileNodes), xsdFileNodes);
            }
            else
            {   // Rollback to first sibling
                int rollback = index - 1;
                do
                {
                    rollback--;
                    if (rollback < 0) return 0;
                } while (xsdFileNodes[rollback].Level != target);
                return FindParent(FindFirstSibling(index, xsdFileNodes), xsdFileNodes);
            }
        }

        private int FindFirstSibling(int index, List<XsdFileNode> xsdFileNodes)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (xsdFileNodes[i].Level == xsdFileNodes[index].Level)
                {
                    return i;
                }
            }
            return 0;
        }

        private XsdFileNode FindCollisions(List<int> path, List<XsdFileNode> xsdFileNodes)
        {

            // For debugging
            /*
            for(int i = path.Count-1; i>=0; i--){
            	Console.WriteLine("->" + xsdFileNodes[path[i]].Name);
            }
            Console.WriteLine();
            */

            for (int i = 0; i < path.Count; i++)
            {
                for (int j = 0; j < path.Count; j++)
                {
                    if (i == j) continue;
                    if (xsdFileNodes[path[i]].Name.Equals(xsdFileNodes[path[j]].Name) &&
                            xsdFileNodes[path[i]].Type.Equals(xsdFileNodes[path[j]].Type))
                    {
                        return xsdFileNodes[path[i]];
                    }
                }
            }
            return null;
        }

        private bool IsEpisodes(ArrayList predecesors)
        {
            if(predecesors == null || predecesors.Count <= 0)
            {                
                return true;
            }
            if (predecesors[0].Equals(""))
            {
                return true;
            }
            foreach (String predecesor in predecesors)
            {
                bool isEpisode = false;
                foreach (String completeEpisode in completedEpisodes)
                {
                    if (predecesor.ToLower().Equals(completeEpisode.ToLower()))
                    {
                        isEpisode = true;
                    }
                }
                if (!isEpisode) return false;
            }
            return true;
        }

        private XjcEpisode FindBuildEpisodes(String predecesor)
        {
            foreach (XjcEpisode episode in buildEpisodes)
            {
                if (episode.XsdFile.ToLower().Equals(predecesor.ToLower()))
                {
                    return episode;
                }
            }
            return null;
        }

        private List<XmlOneAttribute> FilterXmlAttributes(List<XmlOneAttribute> xmlListAttributes, Constants.XmlAddReplace criteria)
        {
            List<XmlOneAttribute> buffer = new List<XmlOneAttribute>();
            foreach (XmlOneAttribute xmlOneAttribute in xmlListAttributes)
            {
                if (xmlOneAttribute.AddReplace == criteria)
                {
                    buffer.Add(xmlOneAttribute);
                }
            }
            return buffer;
        }

        private XmlDocument UpdateSchema(String filename, List<XmlOneAttribute> xmlAttributes)
        {
            List<XmlOneAttribute> filter = FilterXmlAttributes(xmlAttributes, Constants.XmlAddReplace.SCHEMA);
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filename);

            try
            {
                int addSchema = 0;
                XmlNodeList nodes = xDoc.GetElementsByTagName("xs:schema");
                XmlElement element = (XmlElement)nodes.Item(0);
                int initNumSchema = xDoc.GetElementsByTagName("xs:schema").Item(0).Attributes.Count;

                foreach (XmlOneAttribute xmlOneAttribute in filter)
                {
                    addSchema++;
                    element.SetAttribute(xmlOneAttribute.Name, xmlOneAttribute.Value);
                }

                XmlNamedNodeMap nm = nodes.Item(0).Attributes;

                if (addSchema + initNumSchema != nm.Count)
                {
                    Console.WriteLine("Schema definition was not updated correctly in file :" + filename);
                    Environment.Exit(1);
                }
            }            
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);                
            }
            return xDoc;
        }

        private void UpdateInclude(XmlDocument doc, List<XmlOneAttribute> xmlListAttributes)
        {
            List<XmlOneAttribute> filter = FilterXmlAttributes(xmlListAttributes, Constants.XmlAddReplace.INCLUDE);

            XmlNodeList elements = doc.GetElementsByTagName("xs:include");
            if (elements.Count != filter.Count)
            {
                Console.WriteLine("Number of include elements do not match number of imports elements");
                Environment.Exit(1);
            }

            int index = 0;
            while (elements.Count > 0)
            {
                XmlNode parent = elements.Item(0).ParentNode;
                XmlElement importElement = doc.CreateElement("xs","import", "http://www.w3.org/2001/XMLSchema"); // Different from Java version
                importElement.SetAttribute("namespace", filter[index].Name);
                importElement.SetAttribute("schemaLocation", filter[index].Value);
                index++;
                parent.ReplaceChild(importElement, elements.Item(0));
                elements = doc.GetElementsByTagName("xs:include");
            }
        }

        private ArrayList LoadEnumerations(String node, XmlNode childNode)
        {
            ArrayList buffer = new ArrayList();
            if (node.Equals("xs:restriction"))
            {
                XmlNodeList candidateEnumNodes = childNode.ChildNodes;
                for (int enumIndex = 0; enumIndex < candidateEnumNodes.Count; enumIndex++)
                {
                    XmlNode candidate = (XmlNode)candidateEnumNodes.Item(enumIndex);
                    if (candidate != null && candidate.Name.Equals("xs:enumeration"))
                    {
                        XmlElement en = (XmlElement)candidate;
                        String value = en.GetAttribute("value");
                        if (value != null && value.Length > 0)
                        {
                            buffer.Add(value);
                        }
                    }
                }
            }
            return buffer;
        }

        private void AddXmlDependencies(Constants.XmlType type, String prefix, String name, String restriction, ArrayList listEnum)
        {
            foreach (XmlDependency xmlDependency in xmlDependencies)
            {
                if (xmlDependency.Type == type &&
                    xmlDependency.Prefix.ToLower().Equals(prefix.ToLower()) &&
                        xmlDependency.Name.ToLower().Equals(name.ToLower()) &&
                            xmlDependency.Restriction.ToLower().Equals(restriction.ToLower()))
                {
                    return;
                }
            }
            xmlDependencies.Add(new XmlDependency(type, prefix, name, restriction, listEnum));
        }

        private void LoadDependencyType(XmlDocument doc, String tagName, String tagAttribute, String node,
            String nodeAttribute, XmlOneAttribute includeFile, Constants.XmlType typeOfXml)
        {
            XmlNodeList nodes = doc.GetElementsByTagName(tagName);
            for (int parent = 0; parent < nodes.Count; parent++)
            {
                bool addedType = false;
                XmlElement complexType = (XmlElement)nodes.Item(parent);
                String dependencyName = complexType.GetAttribute(tagAttribute);
                XmlNodeList childNodes = complexType.ChildNodes;

                ArrayList listEnum = new ArrayList();
                for (int child = 0; child < childNodes.Count; child++)
                {
                    XmlNode childNode = (XmlNode)childNodes.Item(child);
                    if (childNode != null && childNode.Name.Equals(node))
                    {
                        XmlElement e = (XmlElement)childNode;
                        String restriction = e.GetAttribute(nodeAttribute);
                        addedType = true;
                        listEnum = LoadEnumerations(node, childNode);
                        AddXmlDependencies(typeOfXml, includeFile.Prefix, dependencyName, restriction, listEnum);
                    }
                }
                if (!addedType && dependencyName != null && dependencyName.Length > 0)
                {
                    AddXmlDependencies(typeOfXml, includeFile.Prefix, dependencyName, "", listEnum);
                }
            }
        }

        private void LoadDependencyTypes(String pathXsd, List<XmlOneAttribute> xmlListAttributes)
        {
            List<XmlOneAttribute> filter = FilterXmlAttributes(xmlListAttributes, Constants.XmlAddReplace.INCLUDE);

            foreach (XmlOneAttribute includeFile in filter)
            {
                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(pathXsd + includeFile.Value);
                    XmlNodeList nodes = xDoc.GetElementsByTagName(includeFile.Value);                    
                    LoadDependencyType(xDoc, "xs:simpleType", "name", "xs:restriction", "base", includeFile, Constants.XmlType.SIMPLETYPE);
                    LoadDependencyType(xDoc, "xs:complexType", "name", "xs:restriction", "base", includeFile, Constants.XmlType.COMPLEXTYPE);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private String FindSimpleComplexDependency(String candidate)
        {
            if (candidate == null || candidate.Length == 0)
            {
                return null;
            }
            foreach (XmlDependency dependencyType in xmlDependencies)
            {
                if (candidate.Trim().ToLower().Equals(dependencyType.Name.ToLower().Trim()))
                {
                    return dependencyType.Prefix + ":" + dependencyType.Name.Trim();
                }
            }
            return null;
        }

        private int FindOneDependency(String candidate)
        {
            if (candidate == null || candidate.Length == 0)
            {
                return -1;
            }
            int count = 0;
            foreach (XmlDependency dependencyType in xmlDependencies)
            {
                if (candidate.Trim().ToLower().Equals(dependencyType.Name.ToLower().Trim()))
                {
                    return count;
                }
                count++;
            }
            return -1;
        }

        private bool IsEnum(ArrayList source, ArrayList destination)
        {
            foreach (String src in source)
            {
                foreach (String dst in destination)
                {
                    if (src.Equals(dst))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateType(XmlDocument doc, String tagName, String attributte)
        {
            XmlNodeList nodes = doc.GetElementsByTagName(tagName);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlElement element = (XmlElement)nodes.Item(i);
                if (tagName.ToLower().Equals("xs:restriction".ToLower()))
                {
                    String newValue = FindSimpleComplexDependency(element.GetAttribute(attributte));
                    if (newValue != null)
                    {
                        ArrayList listEnum = LoadEnumerations("xs:restriction", element);
                        if (listEnum.Count > 0)
                        {
                            int index = FindOneDependency(element.GetAttribute(attributte));
                            if (index >= 0)
                            {
                                bool find = IsEnum(listEnum, xmlDependencies[index].Enums);
                                if (find && xmlDependencies[index].Restriction != null &&
                                        xmlDependencies[index].Restriction.Length > 0)
                                {
                                    element.SetAttribute(attributte, xmlDependencies[index].Restriction);
                                }
                                else
                                {
                                    element.SetAttribute(attributte, newValue);
                                }
                            }
                        }
                        else
                        {
                            element.SetAttribute(attributte, newValue);
                        }
                    }
                }
                else
                {
                    String newValue = FindSimpleComplexDependency(element.GetAttribute(attributte));
                    if (newValue != null)
                    {
                        element.SetAttribute(attributte, newValue);
                    }
                }
            }
        }

        private void UpdateTypes(XmlDocument doc)
        {
            UpdateType(doc, "xs:element", "type");
            UpdateType(doc, "xs:attribute", "type");
            UpdateType(doc, "xs:simpleType", "name");
            UpdateType(doc, "xs:restriction", "base");
            UpdateType(doc, "xs:extension", "base");
        }

        private int GeneratePocoFromXsd(String xsdExePath, String xsdStringParameters, String dirOutputClass){
            // Creates all dirs (one per xsd file) but base type xsd files will not generate C# classes
            TestAndCreateDirectory(dirOutputClass);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = xsdExePath,
                Arguments = xsdStringParameters
            };
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    return 0;
                }
            }
            catch
            {   
                return 1;
            }
        }

        private bool IsRunXsd(String xsdExePath, XjcEpisode episode, String rootPackage,
                                String dirInput, String dirOutput, bool verbose)
        {
            String packageClass = rootPackage + episode.XsdFile.Replace(".xsd", "").ToLower();
            String dirOutputClass = dirOutput + rootPackage.Replace(".", "/") + episode.XsdFile.Replace(".xsd", "").ToLower();
            Boolean result = false;

            ArrayList binFiles = GetAllPredecessors(episode.XsdFile);
            for (int index = 0; index < binFiles.Count; index++)
            {
                binFiles[index] = dirInput + binFiles[index];
            }
            
            ArrayList xsdArrayListParameters = new ArrayList
            {
                " /outputdir:" + dirOutputClass + " /namespace:" + packageClass + " /classes"
            };

            foreach (String binFile in binFiles)
            {
                xsdArrayListParameters.Add(" " + binFile);
            }
             xsdArrayListParameters.Add(" " + dirInput + episode.XsdFile);

            String xsdStringParameters = string.Join("", xsdArrayListParameters.ToArray());

            if (verbose)
            {
                Console.Write("xsd " + xsdStringParameters.Normalize());
            }

            try
            {
                GeneratePocoFromXsd(xsdExePath, xsdStringParameters, dirOutputClass);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            String [] filePaths = Directory.GetFiles(dirOutputClass);
            if (filePaths == null || filePaths.Length <= 0)
            {
                DeleteDirectory(dirOutputClass);
            }
            return result;
        }

        private void Processing(String xsdExePath, String rootPackage, String commonSpace, String dirInput, String dirOutput, String fileExtension)
        {
            Console.WriteLine("Wait, generating files ...");

            foreach (XjcEpisode episode in buildEpisodes)
            {
                List<XjcParameters> xjcParameters = new List<XjcParameters>();
                List<XmlOneAttribute> xmlListAttributes = new List<XmlOneAttribute>();

                if (IsEpisodes(episode.Predecessors))
                {
                    foreach (String predecesor in episode.Predecessors)
                    {
                        if (predecesor != null && predecesor.Length > 0)
                        {
                            XjcEpisode predecesorData = FindBuildEpisodes(predecesor);
                            String xsdPredecesor = predecesorData.XsdFile.Replace(fileExtension, "");
                            xmlListAttributes.Add(new XmlOneAttribute(Constants.XmlAddReplace.SCHEMA, "xmlns:" + predecesorData.Prefix,
                                    commonSpace + xsdPredecesor));
                            xmlListAttributes.Add(new XmlOneAttribute(Constants.XmlAddReplace.INCLUDE, commonSpace + xsdPredecesor,
                                    predecesorData.XsdFile, predecesorData.Prefix));
                            xjcParameters.Add(new XjcParameters(xsdPredecesor, predecesorData.Prefix));
                        }
                    }
                    xmlListAttributes.Add(new XmlOneAttribute(Constants.XmlAddReplace.SCHEMA, "xmlns", commonSpace +
                            episode.XsdFile.Replace(fileExtension, "")));
                    xmlListAttributes.Add(new XmlOneAttribute(Constants.XmlAddReplace.SCHEMA, "targetNamespace", commonSpace +
                            episode.XsdFile.Replace(fileExtension, "")));

                    XmlDocument document = UpdateSchema((dirInput + episode.XsdFile), xmlListAttributes);
                    UpdateInclude(document, xmlListAttributes);
                    LoadDependencyTypes(dirInput, xmlListAttributes);

                    UpdateTypes(document);
                    xmlDependencies.Clear();

                    try
                    {
                        SaveToFile(document, dirInput + episode.XsdFile);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        continue;
                    }

                    if (IsRunXsd(xsdExePath, episode, rootPackage, dirInput, dirOutput, false))
                    {
                        completedEpisodes.Add(episode.XsdFile);
                    }
                    else
                    {
                        Console.WriteLine("Error in xsd file: " + episode.XsdFile);
                    }

                }
                else
                {
                    Console.WriteLine(episode.XsdFile + " was not generated due to the lack of dependencies");
                }
            }
            Console.WriteLine("Customized API XSD files were generated");
        }

        private int GeneratePocoFromWsdl(String wsdlExePath, String wsdlStringParameters)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = wsdlExePath,
                Arguments = wsdlStringParameters
            };
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    return 0;
                }
            }
            catch
            {
                return 1;
            }
        }

        private void GenerateClassesFromWsdl(String wsdlExePath, String apiWsdl, String rootClass, String dirOutput)
        {
            String dirOutputClass = dirOutput + rootClass.Replace(".", "/");

            bool exists = System.IO.Directory.Exists(dirOutputClass);

            if (!exists)
                System.IO.Directory.CreateDirectory(dirOutputClass);

            String wsdlParameters = " /out:" + dirOutputClass + " /namespace:" + rootClass + " " + apiWsdl;
            GeneratePocoFromWsdl(wsdlExePath, wsdlParameters);

           
        }
    }
}
