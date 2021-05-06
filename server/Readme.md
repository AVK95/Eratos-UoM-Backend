All server related scripts will go in this folder.
Technical details about the server are as follows. (For the EER diagram go the documentation folder: https://github.com/AVK95/Eratos-UoM-Backend/tree/main/docs)

<ul>
  <li> <b>Server Hoster:</b> <i>Microsoft Azure</i> </li>
  <li> <b>Server URI:<b> <i>eratos-uom-backend.database.windows.net</i> </li>
    <li> <b>ADO.NET Connection String:</b> <br/><i>Server=tcp:eratos-uom-backend.database.windows.net,1433;Initial Catalog=UserDB;Persist Security Info=False;User ID=unimelb;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;</i> </li>
    <li> Database Capacity: 1GB </li>
    </ul>
    
This folder additionally contains a C#.NET script allowing you to query the server from a remote IP. Note: The passowrd for actually accessing the server is masked in this sample to prevent unauthorized CREATE / DROP statements!

Please ask the development team for the password (with reason).
