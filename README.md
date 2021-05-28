<h2>About the Client</h2>

Eratos is a platform that functions as the digital infrastructure for data-driven operations and decision-making for the natural and built worlds. Eratos has worked with CSIRO to develop a technology "stack" from streaming data from IoT sensors and edge computing to federating all of Australia's climate data, models, and frameworks from many disparate sources.

<h2>Project Goal</h2>

The project is to develop a standalone web application which enables users to perform data-related operation and management that supported by the Eratos core functions. The application is designed to separate in to the front-end and the back-end. The front-end development will include building both user-front and admin-front where the application collects the user input and requests and send them through to the back-end for further processing, whereas the back-end development will focus on building the gateway nodes to connect the Eratos API for forwarding the user requests and back forwarding the returning data.

<h2>Confluence</h2>
Detailed documentation about the project is found in our confluence directory:
<a href="https://confluence.cis.unimelb.edu.au:8443/display/COMP900822021SM1ER/Project+Background">Confluence</a>

<h2>About the final release</h2>
Most of the work is done. The program is in a MVC style. The folder Models has models of all objects being used in it. And the Controllers folder has three files in it, the InRequestController (for handling requests received by our APIs), the OutRequestController (for connecting with the Eratos) and the Database Controller (for updating the database). Some tool functions (for example functions for mapping object we get from the Eratos to objects can be stored into our database) are in the Util.cs. Domains and our userID and user secret, and the Eratos API information are in the Config.cs. APIs are in the UoM-Server folder to meet the required format of Azure.

<h3>About the Team</h3>
<ul>
  <li>Kelvin Wijaya Wijaya</li>
  <li>Yan Dai Scrum Master</li>
  <li>Aditya Vikram Khandelwal</li>
  <li>DEYOU ZOU</li>
  <li>En Wen Tsai</li>
</ul>

