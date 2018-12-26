Using the Prod shared library (C# version) 
------------------------------------------   
Examples for demonstrating how to use the library are included in this project. 

Pre-requisites
--------------
Visual Studio 2017 and .NET Framework Version 4.7.2 must be installed before you execute the next procedure.

From the command line 
---------------------
   
 Change the current working directory to /OdyApiGenExample folder and run:
   
      msbuild OdyApiGenExampleCSharp.csproj -t:run
   
From Visual Studio
------------------

   Open the "Debug" menu and select "Start without debugging" or press <Ctrl+F5>

Note
----
   - To setup the OdyApiGenExample parameters open OdyAPIGenerator\OdyAPIGenLibrary\ExampleAPIs.cs file and edit lines
     41-47. You must provide the Odyssey API wsdl URL, the Odyssey site id, the user id -- a number associated to the 
	 user account that is running the program (user ids can be found on Operations.dbo.AppUser table of your Odyssey 
	 DB), and a valid Odyssey case number that has at least one case event linked to a pdf document. 
   

# End     