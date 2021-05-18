using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections;

namespace UOM_Server
{
    public struct UserTable
    {
        public int UserID { get; set; }
        public string EratosUserID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Auth0ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Info { get; set; }

        public UserTable(int userID, string eratosUserID, string email, string name, string auth0ID, DateTime createdAt, string info)
        {
            UserID = userID;
            EratosUserID = eratosUserID;
            Email = email;
            Name = name;
            Auth0ID = auth0ID;
            CreatedAt = createdAt;
            Info = info;
        }
    }

    public struct OrderTable
    {
        public int OrderID { get; set; }
        public float Price { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public int UserID { get; set; }

        public OrderTable(int orderID, float price, string status, DateTime orderTime, int userID)
        {
            OrderID = orderID;
            Price = price;
            Status = status;
            OrderTime = orderTime;
            UserID = userID;
        }
    }

    public struct TaskTable
    {
        public int TaskID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string Meta { get; set; }
        public string Error { get; set; }
        public int UserID { get; set; }
        public int OrderID { get; set; }

        public TaskTable(int taskID, DateTime createdAt, DateTime lastUpdatedAt, DateTime startedAt, DateTime endedAt, string priority, string state, string type, string meta, string error, int userID, int orderID)
        {
            TaskID = taskID;
            CreatedAt = createdAt;
            LastUpdatedAt = lastUpdatedAt;
            StartedAt = startedAt;
            EndedAt = endedAt;
            Priority = priority;
            State = state;
            Type = type;
            Meta = meta;
            Error = error;
            UserID = userID;
            OrderID = orderID;
        }
    }

    public struct ModuleTable
    {
        public int ModuleID { get; set; }
        public string EratosModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleSchema { get; set; }
        public bool isActive { get; set; }

        public ModuleTable(int moduleID, string eratosModuleID, string moduleName, string moduleSchema, bool isActive)
        {
            ModuleID = moduleID;
            EratosModuleID = eratosModuleID;
            ModuleName = moduleName;
            ModuleSchema = moduleSchema;
            this.isActive = isActive;
        }
    }

    public struct ResourceTable
    {
        public int ResourceID { get; set; }
        public string EratosResourceID { get; set; }
        public DateTime Date { get; set; }
        public string Policy { get; set; }
        public string Geo { get; set; }
        public string Meta { get; set; }

        public ResourceTable(int resourceID, string eratosResourceID, DateTime date, string policy, string geo, string meta)
        {
            ResourceID = resourceID;
            EratosResourceID = eratosResourceID;
            Date = date;
            Policy = policy;
            Geo = geo;
            Meta = meta;
        }
    }

    public sealed class EratosDBException : Exception
    {
        public EratosDBException() { }
        public EratosDBException(string msg) : base(msg) { }
        public EratosDBException(string msg, Exception inner) : base(msg, inner) { }
    }
    
    public sealed class DatabaseHandler
    {
        private SqlConnectionStringBuilder builder;
        private SqlConnection connection;
        private bool isConnected;

        //Sets up connection strings to the Database as per default EratosDB settings (doesnt connect)
        public DatabaseHandler ()
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "eratos-uom-backend.database.windows.net";
            builder.UserID = "unimelb";
            builder.Password = "Eratos2021";
            builder.InitialCatalog = "UserDB";

            isConnected = false;
        }

        public string Connect()
        {
            try
            {
                connection = new SqlConnection(builder.ConnectionString);
                connection.Open();
                isConnected = true;
                return connection.State.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string Disconnect()
        {
            try
            {
                connection.Close();
                isConnected = false;
                return connection.State.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public bool CreateUser (UserTable userDetails)
        {
            if(!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateUser(userDetails))
                return false;

            string command = null;
            if (userDetails.Info != null)
            {
                command = "INSERT INTO [dbo].[User]([EratosUserID], [Email], [Name], [Auth0ID], [CreatedAt], [Info]) ";
                command += "VALUES(@EratosUserID, @Email, @Name, @Auth0ID, @CreatedAt, @Info)";
            }
            else
            {
                command = "INSERT INTO [dbo].[User]([EratosUserID], [Email], [Name], [Auth0ID], [CreatedAt]) ";
                command += "VALUES(@EratosUserID, @Email, @Name, @Auth0ID, @CreatedAt)";
            }

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@EratosUserID", SqlDbType.NVarChar);
                sql.Parameters["@EratosUserID"].Value = userDetails.EratosUserID;

                sql.Parameters.Add("@Email", SqlDbType.NVarChar);
                sql.Parameters["@Email"].Value = userDetails.Email;

                sql.Parameters.Add("@Name", SqlDbType.NVarChar);
                sql.Parameters["@Name"].Value = userDetails.Name;

                sql.Parameters.Add("@Auth0ID", SqlDbType.NVarChar);
                sql.Parameters["@Auth0ID"].Value = userDetails.Auth0ID;

                sql.Parameters.Add("@CreatedAt", SqlDbType.DateTime);
                sql.Parameters["@CreatedAt"].Value = userDetails.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (userDetails.Info != null)
                {
                    sql.Parameters.Add("@Info", SqlDbType.NVarChar);
                    sql.Parameters["@Info"].Value = userDetails.Info;
                }

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        //A fluid find user command that returns the user details based on any of the following inputs:
        //EratosUserID, Email, Auth0ID, UserID, name
        public ArrayList FindUser(string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "SELECT * FROM [User] WHERE ";
            if (IdentifierType.ToLower().CompareTo("eratosuserid") == 0)
                command += "[EratosUserID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("email") == 0)
                command += "[Email] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("auth0id") == 0)
                command += "[Auth0ID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("userid") == 0)
                command += "[UserID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();

                UserTable entry = new UserTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.UserID = result.GetInt32(0);
                    entry.EratosUserID = result.GetString(1);
                    entry.Email = result.GetString(2);
                    entry.Name = result.GetString(3);
                    entry.Auth0ID = result.GetString(4);
                    entry.CreatedAt = result.GetDateTime(5);
                    entry.Info = result.IsDBNull(6) ? null : result.GetString(6);

                    results.Add(entry);
                    entry = new UserTable();
                }
                
                result.Close();
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        private bool isDuplicateUser(UserTable userDetails)
        {
            if (FindUser("eratosuserid", userDetails.EratosUserID).Count > 0)
                return true;
            if (FindUser("email", userDetails.Email).Count > 0)
                return true;
            if (FindUser("auth0id", userDetails.Auth0ID).Count > 0)
                return true;
            return false;
        }


        //CreateOrder
        public bool CreateOrder(OrderTable orderDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "INSERT INTO [dbo].[Order]([Price], [Status], [OrderTime], [UserID]) ";
            command += "VALUES(@Price, @Status, @OrderTime, @UserID)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@Price", SqlDbType.Float);
                sql.Parameters["@Price"].Value = float.Parse(orderDetails.Price.ToString("0.00"));

                sql.Parameters.Add("@Status", SqlDbType.NVarChar);
                sql.Parameters["@Status"].Value = orderDetails.Status;

                sql.Parameters.Add("@OrderTime", SqlDbType.DateTime);
                sql.Parameters["@OrderTime"].Value = orderDetails.OrderTime;

                sql.Parameters.Add("@UserID", SqlDbType.Int);
                sql.Parameters["@UserID"].Value = orderDetails.UserID;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public ArrayList FindOrder(string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "SELECT * FROM [Order] WHERE ";
            if (IdentifierType.ToLower().CompareTo("orderid") == 0)
                command += "[OrderID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("userid") == 0)
                command += "[UserID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();

                OrderTable entry = new OrderTable();
                ArrayList results = new ArrayList();
                
                while (result.Read())
                {
                    entry.OrderID = result.GetInt32(0);
                    entry.Price = (float)result.GetDouble(1);
                    entry.Status = result.GetString(2);
                    entry.OrderTime = result.GetDateTime(3);
                    entry.UserID = result.GetInt32(4);
                    
                    results.Add(entry);
                    entry = new OrderTable();
                }

                result.Close();
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public bool CreateTask(TaskTable taskDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "INSERT INTO [dbo].[Task]([CreatedAt], [LastUpdatedAt], [StartedAt], [EndedAt], [Priority], [State], [Type], [Meta], [Error], [UserID], [OrderID]) ";
            command += "VALUES(@CreatedAt, @LastUpdatedAt, @StartedAt, @EndedAt, @Priority, @State, @Type, @Meta, @Error, @UserID, @OrderID)";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@CreatedAt", SqlDbType.DateTime);
                sql.Parameters["@CreatedAt"].Value = taskDetails.CreatedAt;

                sql.Parameters.Add("@LastUpdatedAt", SqlDbType.DateTime);
                if (taskDetails.LastUpdatedAt.CompareTo(default) == 0)
                    sql.Parameters["@LastUpdatedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@LastUpdatedAt"].Value = taskDetails.LastUpdatedAt;

                sql.Parameters.Add("@StartedAt", SqlDbType.DateTime);
                if (taskDetails.StartedAt.CompareTo(default) == 0)
                    sql.Parameters["@StartedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@StartedAt"].Value = taskDetails.StartedAt;

                sql.Parameters.Add("@EndedAt", SqlDbType.DateTime);
                if (taskDetails.EndedAt.CompareTo(default) == 0)
                    sql.Parameters["@EndedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@EndedAt"].Value = taskDetails.EndedAt;

                sql.Parameters.Add("@Priority", SqlDbType.NVarChar);
                if (taskDetails.Priority.CompareTo(null) == 0)
                    sql.Parameters["@Priority"].Value = DBNull.Value;
                else
                    sql.Parameters["@Priority"].Value = taskDetails.Priority;

                sql.Parameters.Add("@State", SqlDbType.NVarChar);
                if (taskDetails.State.CompareTo(null) == 0)
                    sql.Parameters["@State"].Value = DBNull.Value;
                else
                    sql.Parameters["@State"].Value = taskDetails.State;

                sql.Parameters.Add("@Type", SqlDbType.NVarChar);
                if (taskDetails.Type == null)
                    sql.Parameters["@Type"].Value = DBNull.Value;
                else
                    sql.Parameters["@Type"].Value = taskDetails.Type;

                sql.Parameters.Add("@Meta", SqlDbType.NVarChar);
                if (taskDetails.Meta == null)
                    sql.Parameters["@Meta"].Value = DBNull.Value;
                else
                    sql.Parameters["@Meta"].Value = taskDetails.Meta;

                sql.Parameters.Add("@Error", SqlDbType.NVarChar);
                if (taskDetails.Error == null)
                    sql.Parameters["@Error"].Value = DBNull.Value;
                else
                    sql.Parameters["@Error"].Value = taskDetails.Error;

                sql.Parameters.Add("@UserID", SqlDbType.Int);
                sql.Parameters["@UserID"].Value = taskDetails.UserID;

                sql.Parameters.Add("@OrderID", SqlDbType.Int);
                sql.Parameters["@OrderID"].Value = taskDetails.OrderID;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public ArrayList FindTask(string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "SELECT * FROM [Task] WHERE ";
            if (IdentifierType.ToLower().CompareTo("taskid") == 0)
                command += "[TaskID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("userid") == 0)
                command += "[UserID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("orderid") == 0)
                command += "[OrderID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();

                TaskTable entry = new TaskTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.TaskID = result.GetInt32(0);
                    entry.CreatedAt = result.GetDateTime(1);
                    if(!result.IsDBNull(2))
                        entry.LastUpdatedAt = result.GetDateTime(2);
                    if (!result.IsDBNull(3))
                        entry.StartedAt = result.GetDateTime(3);
                    if (!result.IsDBNull(4))
                        entry.EndedAt = result.GetDateTime(4);

                    if (!result.IsDBNull(5))
                        entry.Priority = result.GetString(5);
                    else
                        entry.Priority = null;

                    if (!result.IsDBNull(6))
                        entry.State = result.GetString(6);
                    else
                        entry.State = null;

                    if (!result.IsDBNull(7))
                        entry.Type = result.GetString(7);
                    else
                        entry.Type = null;

                    if (!result.IsDBNull(8))
                        entry.Meta = result.GetString(8);
                    else
                        entry.Meta = null;

                    if (!result.IsDBNull(9))
                        entry.Error = result.GetString(9);
                    else
                        entry.Error = null;

                    entry.UserID = result.GetInt32(10);
                    entry.OrderID = result.GetInt32(11);
                    
                    results.Add(entry);
                    entry = new TaskTable();
                }

                result.Close();
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public bool CreateModule(ModuleTable moduleDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateModule(moduleDetails))
                return false;

            string command = "INSERT INTO [dbo].[Module]([EratosModuleID], [ModuleName], [ModuleSchema], [isActive]) ";
            command += "VALUES(@EratosModuleID, @ModuleName, @ModuleSchema, @isActive)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@EratosModuleID", SqlDbType.NVarChar);
                sql.Parameters["@EratosModuleID"].Value = moduleDetails.EratosModuleID;

                sql.Parameters.Add("@ModuleName", SqlDbType.NVarChar);
                sql.Parameters["@ModuleName"].Value = moduleDetails.ModuleName;

                sql.Parameters.Add("@ModuleSchema", SqlDbType.NVarChar);
                if (moduleDetails.ModuleSchema != null)
                    sql.Parameters["@ModuleSchema"].Value = moduleDetails.ModuleSchema;
                else
                    sql.Parameters["@ModuleSchema"].Value = DBNull.Value;

                sql.Parameters.Add("@isActive", SqlDbType.TinyInt);
                sql.Parameters["@isActive"].Value = moduleDetails.isActive;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        
        public ArrayList FindModule(string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "SELECT * FROM [Module] WHERE ";
            if (IdentifierType.ToLower().CompareTo("moduleid") == 0)
                command += "[ModuleID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("eratosmoduleid") == 0)
                command += "[EratosModuleID] = " + IdentifierValue;
            else if (IdentifierType.ToLower().CompareTo("modulename") == 0)
                command += "[ModuleName] = " + IdentifierValue;
            else if (IdentifierType.ToLower().CompareTo("moduleschema") == 0)
                command += "[ModuleSchema] = " + IdentifierValue;
            else if (IdentifierType.ToLower().CompareTo("isactive") == 0)
            {
                int tempBool;
                if (bool.Parse(IdentifierValue))
                    tempBool = 1;
                else
                    tempBool = 0;
                command += "[isActive] = " + "\'" + tempBool + "\'";
            }
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();

                ModuleTable entry = new ModuleTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.ModuleID = result.GetInt32(0);
                    entry.EratosModuleID = result.GetString(1);
                    entry.ModuleName = result.GetString(2);

                    if (!result.IsDBNull(3))
                        entry.ModuleSchema = result.GetString(3);
                    else
                        entry.ModuleSchema = null;

                    if (result.GetByte(4) == 0)
                        entry.isActive = false;
                    else
                        entry.isActive = true;

                    results.Add(entry);
                    entry = new ModuleTable();
                }

                result.Close();
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        private bool isDuplicateModule(ModuleTable module)
        {
            if (FindModule("eratosmoduleid", module.EratosModuleID).Count > 0)
                return true;
            return false;
        }

        public bool CreateResource (ResourceTable resource)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateResource(resource))
                return false;

            string command = "INSERT INTO [dbo].[Resource]([EratosResourceID], [Date], [Policy], [Geo], [Meta]) ";
            command += "VALUES(@EratosResourceID, @Date, @Policy, @Geo, @Meta)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@EratosResourceID", SqlDbType.NVarChar);
                sql.Parameters["@EratosResourceID"].Value = resource.EratosResourceID;

                sql.Parameters.Add("@Date", SqlDbType.DateTime);
                if (resource.Date.CompareTo(default) == 0)
                    sql.Parameters["@Date"].Value = DBNull.Value;
                else
                    sql.Parameters["@Date"].Value = resource.Date;

                sql.Parameters.Add("@Policy", SqlDbType.NVarChar);
                if (resource.Policy != null)
                    sql.Parameters["@Policy"].Value = resource.Policy;
                else
                    sql.Parameters["@Policy"].Value = DBNull.Value;

                sql.Parameters.Add("@Geo", SqlDbType.NVarChar);
                if (resource.Geo != null)
                    sql.Parameters["@Geo"].Value = resource.Geo;
                else
                    sql.Parameters["@Geo"].Value = DBNull.Value;

                sql.Parameters.Add("@Meta", SqlDbType.NVarChar);
                if (resource.Meta != null)
                    sql.Parameters["@Meta"].Value = resource.Meta;
                else
                    sql.Parameters["@Meta"].Value = DBNull.Value;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public ArrayList FindResource(string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "SELECT * FROM [Resource] WHERE ";
            if (IdentifierType.ToLower().CompareTo("resourceid") == 0)
                command += "[ResourceID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("eratosresourceid") == 0)
                command += "[EratosResourceID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("policy") == 0)
                command += "[Policy] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("geo") == 0)
                command += "[Geo] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("meta") == 0)
                command += "[Meta] = " + "\'" + IdentifierValue + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();

                ResourceTable entry = new ResourceTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.ResourceID = result.GetInt32(0);
                    entry.EratosResourceID = result.GetString(1);
                    
                    if(!result.IsDBNull(2))
                        entry.Date = result.GetDateTime(2);

                    if (!result.IsDBNull(3))
                        entry.Policy = result.GetString(3);
                    else
                        entry.Policy = null;

                    if (!result.IsDBNull(4))
                        entry.Geo = result.GetString(4);
                    else
                        entry.Geo = null;

                    if (!result.IsDBNull(5))
                        entry.Meta = result.GetString(5);
                    else
                        entry.Meta = null;

                    results.Add(entry);
                    entry = new ResourceTable();
                }

                result.Close();
                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        private bool isDuplicateResource(ResourceTable resource)
        {
            if (FindResource("eratosresourceid", resource.EratosResourceID).Count > 0)
                return true;
            return false;
        }

        public bool CreateResourceTaskAssociation(int ResourceID, int TaskID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateRTAssociation(ResourceID, TaskID))
                return false;

            string command = "INSERT INTO [dbo].[ResourceTask]([ResourceID], [TaskID]) ";
            command += "VALUES(@ResourceID, @TaskID)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@ResourceID", SqlDbType.Int);
                sql.Parameters["@ResourceID"].Value = ResourceID;

                sql.Parameters.Add("@TaskID", SqlDbType.Int);
                sql.Parameters["@TaskID"].Value = TaskID;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private bool isDuplicateRTAssociation(int ResourceID, int TaskID)
        {
            string command = "SELECT * FROM [ResourceTask] WHERE [ResourceID] = " + ResourceID.ToString() + " AND [TaskID] = " + TaskID.ToString();
            SqlCommand sql = new SqlCommand(command, connection);
            SqlDataReader result = sql.ExecuteReader();
            return result.HasRows;
        }

        public bool CreateResourceModuleAssociation(int ResourceID, int ModuleID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateRMAssociation(ResourceID, ModuleID))
                return false;

            string command = "INSERT INTO [dbo].[ResourceModule]([ResourceID], [ModuleID]) ";
            command += "VALUES(@ResourceID, @ModuleID)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@ResourceID", SqlDbType.Int);
                sql.Parameters["@ResourceID"].Value = ResourceID;

                sql.Parameters.Add("@ModuleID", SqlDbType.Int);
                sql.Parameters["@ModuleID"].Value = ModuleID;

                SqlDataReader result = sql.ExecuteReader();
                result.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private bool isDuplicateRMAssociation(int ResourceID, int ModuleID)
        {
            string command = "SELECT * FROM [ResourceModule] WHERE [ResourceID] = " + ResourceID.ToString() + " AND [ModuleID] = " + ModuleID.ToString();
            SqlCommand sql = new SqlCommand(command, connection);
            SqlDataReader result = sql.ExecuteReader();
            return result.HasRows;
        }
    }   
    
    
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseHandler dh = new DatabaseHandler();
            Console.WriteLine(dh.Connect());
            //bool response = dh.CreateUser(new UserTable(0, "demouser3", "demoemail3", "demoname3", "demoauth3", DateTime.Now, null));
            ArrayList entry = dh.FindUser("email", "demoemail3");
            UserTable myUser = (UserTable)entry[0];
            Console.WriteLine(myUser.UserID + " " + myUser.CreatedAt);

            //bool response = dh.CreateOrder(new OrderTable(0, (float)19.65, "Unpaid", DateTime.Now, 3));
            ArrayList orders = dh.FindOrder("userid", "3");
            foreach (OrderTable element in orders)
                Console.WriteLine(element.OrderID + " " + element.Price);

            //bool response = dh.CreateTask(new TaskTable(0, DateTime.Now, default, default, default, "Regular", "Ended", null, null, null, 1, 2));
            ArrayList tasks = dh.FindTask("userid", "1");
            foreach (TaskTable element in tasks)
                Console.WriteLine(element.TaskID + " " + element.CreatedAt + " " + element.OrderID);

            //bool response = dh.CreateModule(new ModuleTable(0, "module2_id", "module2", null, true));
            ArrayList modules = dh.FindModule("isactive", "false");
            foreach (ModuleTable element in modules)
                Console.WriteLine(element.ModuleID + " " + element.ModuleName + " " + element.EratosModuleID);

            //bool response = dh.CreateResource(new ResourceTable(0, "resource1_id", default, "policy1", "geo1", null));
            ArrayList ress = dh.FindResource("policy", "policy1");
            foreach (ResourceTable element in ress)
                Console.WriteLine(element.ResourceID + " " + element.EratosResourceID + " " + element.Geo);

            //bool response = dh.CreateResourceTaskAssociation(1, 1);
            bool response = dh.CreateResourceModuleAssociation(1, 1);
            Console.WriteLine(response);

            Console.ReadLine();
            dh.Disconnect();
        }
    }
}
