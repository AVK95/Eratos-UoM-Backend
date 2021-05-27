<h2> Backend code </h2>

This folder contains all the code that has been deployed on the Microsoft Azure server (server details in the server folder). To be able to run this server on your localhost, please follow these instructions:
<ol>
  <li> Install Microsoft Azure SDK in your Visual Studio installation </li>
  <li> Create a new C# Azure function project. Select HTTP trigger. Clone this repository </li>
  <li> Open the project by double clicking the .sln file in the local repository folder. This will start the project as a Azure class library </li>
  <li> By simply running the project, your computer should start the server on the localhost. At this point, you will be able to run GET and POST requests using any REST client, such as Postman. </li>
  <li> Check the API docs for arguments and return values (see docs folder) </li>
</ol>
