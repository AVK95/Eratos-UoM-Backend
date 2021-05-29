using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections;
using EratosUoMBackend.Models;

/*DatabaseController
This is the controller which is used to connect to the database. It fetches data from database and update the database. */

namespace EratosUoMBackend.Controllers
{
    #region Database Exceptions
    public sealed class EratosDBException : Exception
    {
        public EratosDBException() { }
        public EratosDBException(string msg) : base(msg) { }
        public EratosDBException(string msg, Exception inner) : base(msg, inner) { }
    }
    #endregion

    public sealed class DatabaseController
    {
        private SqlConnectionStringBuilder builder;
        private SqlConnection connection;
        private bool isConnected;
        private static int MAX_ROWS = 100;

        //Sets up connection strings to the Database as per default EratosDB settings (doesnt connect)
        public DatabaseController()
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "eratos-uom-backend.database.windows.net";
            builder.UserID = "unimelb";
            builder.Password = "Eratos2021";
            builder.InitialCatalog = "UserDB";

            isConnected = false;
        }

        #region Connection
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
            if (!isConnected)
                return "Closed";

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

        #endregion

        #region User
        public bool CreateUser(UserTable userDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateUser(userDetails))
                return false;

            string command = null;
            if (userDetails.Info != null)
            {
                command = "INSERT INTO [dbo].[User]([EratosUserID], [Email], [Name], [Auth0ID], [CreatedAt], [Info], [isAdmin]) ";
                command += "VALUES(@EratosUserID, @Email, @Name, @Auth0ID, @CreatedAt, @Info, @isAdmin)";
            }
            else
            {
                command = "INSERT INTO [dbo].[User]([EratosUserID], [Email], [Name], [Auth0ID], [CreatedAt], [isAdmin]) ";
                command += "VALUES(@EratosUserID, @Email, @Name, @Auth0ID, @CreatedAt, @isAdmin)";
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
                sql.Parameters["@CreatedAt"].Value = userDetails.CreatedAt;

                if (userDetails.Info != null)
                {
                    sql.Parameters.Add("@Info", SqlDbType.NVarChar);
                    sql.Parameters["@Info"].Value = userDetails.Info;
                }

                sql.Parameters.Add("@isAdmin", SqlDbType.TinyInt);
                sql.Parameters["@isAdmin"].Value = userDetails.isAdmin;

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
            else if (IdentifierType.ToLower().CompareTo("isadmin") == 0)
            {
                int tempBool;
                if (bool.Parse(IdentifierValue))
                    tempBool = 1;
                else
                    tempBool = 0;
                command += "[isAdmin] = " + "\'" + tempBool + "\'";
            }
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
                    entry.CreatedAt = result.IsDBNull(5) ? null : result.GetDateTime(5).ToString();
                    entry.Info = result.IsDBNull(6) ? null : result.GetString(6);
                    if (result.GetByte(7) == 0)
                        entry.isAdmin = false;
                    else
                        entry.isAdmin = true;

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

        //Returns max 100 rows at once, first row is row 1
        public ArrayList FindUser(int startRow, int endRow = 0)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            int start = (startRow <= 0) ? 1 : startRow;
            int end = (endRow <= 0) ? start + MAX_ROWS - 1 : endRow;
            if (end < start)
                throw new EratosDBException("Invalid Parameters!");

            if (end >= start + MAX_ROWS)
                end = start + MAX_ROWS - 1;

            string command = "WITH OrderedResults AS (SELECT *, ROW_NUMBER() OVER (ORDER BY UserID) AS RowNum FROM [dbo].[User]) ";
            command += "SELECT * FROM OrderedResults WHERE RowNum BETWEEN @StartRow AND @EndRow";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@StartRow", SqlDbType.Int);
                sql.Parameters["@StartRow"].Value = start;

                sql.Parameters.Add("@EndRow", SqlDbType.Int);
                sql.Parameters["@EndRow"].Value = end;

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
                    entry.CreatedAt = result.IsDBNull(5) ? null : result.GetDateTime(5).ToString();
                    entry.Info = result.IsDBNull(6) ? null : result.GetString(6);
                    if (result.GetByte(7) == 0)
                        entry.isAdmin = false;
                    else
                        entry.isAdmin = true;

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

        public void UpdateUser(int userID, string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "UPDATE [dbo].[User] SET ";
            if (IdentifierType.ToLower().CompareTo("eratosuserid") == 0)
                command += "[EratosUserID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("email") == 0)
                command += "[Email] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("auth0id") == 0)
                command += "[Auth0ID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("createdat") == 0)
                command += "[CreatedAt] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("info") == 0)
                command += "[Info] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("isadmin") == 0)
            {
                int tempBool;
                if (bool.Parse(IdentifierValue))
                    tempBool = 1;
                else
                    tempBool = 0;
                command += "[isAdmin] = " + "\'" + tempBool + "\'";
            }
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");

            command += " WHERE UserID = " + "\'" + userID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public void DeleteUser(int userID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[User] WHERE UserID = " + "\'" + userID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
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
        #endregion

        #region Order
        //CreateOrder
        public bool CreateOrder(OrderTable orderDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "INSERT INTO [dbo].[Order]([Price], [Status], [OrderTime], [UserID], [PaymentID]) ";
            command += "VALUES(@Price, @Status, @OrderTime, @UserID, @PaymentID)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@Price", SqlDbType.Float);
                sql.Parameters["@Price"].Value = float.Parse(orderDetails.Price.ToString("0.00"));

                sql.Parameters.Add("@Status", SqlDbType.NVarChar);
                if (orderDetails.Status != null)
                    sql.Parameters["@Status"].Value = orderDetails.Status;
                else
                    sql.Parameters["@Status"].Value = DBNull.Value;

                sql.Parameters.Add("@OrderTime", SqlDbType.DateTime);
                sql.Parameters["@OrderTime"].Value = orderDetails.OrderTime;

                sql.Parameters.Add("@UserID", SqlDbType.Int);
                sql.Parameters["@UserID"].Value = orderDetails.UserID;

                sql.Parameters.Add("@PaymentID", SqlDbType.NVarChar);
                if (orderDetails.PaymentID != null)
                    sql.Parameters["@PaymentID"].Value = orderDetails.PaymentID;
                else
                    sql.Parameters["@PaymentID"].Value = DBNull.Value;

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
            else if (IdentifierType.ToLower().CompareTo("paymentid") == 0)
                command += "[PaymentID] = " + "\'" + IdentifierValue + "\'";
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
                    entry.Status = result.IsDBNull(2) ? null : result.GetString(2);
                    entry.OrderTime = result.GetDateTime(3).ToString();
                    entry.UserID = result.GetInt32(4);
                    entry.PaymentID = result.IsDBNull(5) ? null : result.GetString(5);

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

        public ArrayList FindOrder(int startRow, int endRow = 0)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            int start = (startRow <= 0) ? 1 : startRow;
            int end = (endRow <= 0) ? start + MAX_ROWS - 1 : endRow;
            if (end < start)
                throw new EratosDBException("Invalid Parameters!");

            if (end >= start + MAX_ROWS)
                end = start + MAX_ROWS - 1;

            string command = "WITH OrderedResults AS (SELECT *, ROW_NUMBER() OVER (ORDER BY OrderID) AS RowNum FROM [dbo].[Order]) ";
            command += "SELECT * FROM OrderedResults WHERE RowNum BETWEEN @StartRow AND @EndRow";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@StartRow", SqlDbType.Int);
                sql.Parameters["@StartRow"].Value = start;

                sql.Parameters.Add("@EndRow", SqlDbType.Int);
                sql.Parameters["@EndRow"].Value = end;

                SqlDataReader result = sql.ExecuteReader();
                OrderTable entry = new OrderTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.OrderID = result.GetInt32(0);
                    entry.Price = (float)result.GetDouble(1);
                    entry.Status = result.IsDBNull(2) ? null : result.GetString(2);
                    entry.OrderTime = result.GetDateTime(3).ToString();
                    entry.UserID = result.GetInt32(4);
                    entry.PaymentID = result.IsDBNull(5) ? null : result.GetString(5);

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

        public void UpdateOrder(int orderID, string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "UPDATE [dbo].[Order] SET ";
            if (IdentifierType.ToLower().CompareTo("userid") == 0)
                command += "[UserID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("paymentid") == 0)
                command += "[PaymentID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("price") == 0)
                command += "[Price] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("status") == 0)
                command += "[Status] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("ordertime") == 0)
                command += "[OrderTime] = " + "\'" + IdentifierValue + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");
            command += " WHERE OrderID = " + "\'" + orderID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public void DeleteOrder(int orderID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[Order] WHERE OrderID = " + "\'" + orderID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }
        #endregion

        #region Task
        public bool CreateTask(TaskTable taskDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateTask(taskDetails))
                return false;

            string command = "INSERT INTO [dbo].[Task]([CreatedAt], [LastUpdatedAt], [StartedAt], [EndedAt], [Priority], [State], [Type], [Meta], [Error], [UserID], [OrderID], [EratosTaskID], [Name]) ";
            command += "VALUES(@CreatedAt, @LastUpdatedAt, @StartedAt, @EndedAt, @Priority, @State, @Type, @Meta, @Error, @UserID, @OrderID, @EratosTaskID, @Name)";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@CreatedAt", SqlDbType.DateTime);
                sql.Parameters["@CreatedAt"].Value = taskDetails.CreatedAt;

                sql.Parameters.Add("@LastUpdatedAt", SqlDbType.DateTime);
                if (taskDetails.LastUpdatedAt == null)
                    sql.Parameters["@LastUpdatedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@LastUpdatedAt"].Value = taskDetails.LastUpdatedAt;

                sql.Parameters.Add("@StartedAt", SqlDbType.DateTime);
                if (taskDetails.StartedAt == null)
                    sql.Parameters["@StartedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@StartedAt"].Value = taskDetails.StartedAt;

                sql.Parameters.Add("@EndedAt", SqlDbType.DateTime);
                if (taskDetails.EndedAt == null)
                    sql.Parameters["@EndedAt"].Value = DBNull.Value;
                else
                    sql.Parameters["@EndedAt"].Value = taskDetails.EndedAt;

                sql.Parameters.Add("@Priority", SqlDbType.NVarChar);
                if (taskDetails.Priority == null)
                    sql.Parameters["@Priority"].Value = DBNull.Value;
                else
                    sql.Parameters["@Priority"].Value = taskDetails.Priority;

                sql.Parameters.Add("@State", SqlDbType.NVarChar);
                if (taskDetails.State == null)
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

                sql.Parameters.Add("@EratosTaskID", SqlDbType.NVarChar);
                if (taskDetails.EratosTaskID == null)
                    sql.Parameters["@EratosTaskID"].Value = DBNull.Value;
                else
                    sql.Parameters["@EratosTaskID"].Value = taskDetails.EratosTaskID;

                sql.Parameters.Add("@Name", SqlDbType.NVarChar);
                if (taskDetails.Name == null)
                    sql.Parameters["@Name"].Value = DBNull.Value;
                else
                    sql.Parameters["@Name"].Value = taskDetails.Name;

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
            else if (IdentifierType.ToLower().CompareTo("eratostaskid") == 0)
                command += "[EratosTaskID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("priority") == 0)
                command += "[Priority] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("state") == 0)
                command += "[State] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("type") == 0)
                command += "[Type] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("meta") == 0)
                command += "[Meta] = " + "\'" + IdentifierValue + "\'";
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
                    entry.CreatedAt = result.IsDBNull(1) ? null : result.GetDateTime(1).ToString();
                    entry.LastUpdatedAt = result.IsDBNull(2) ? null : result.GetDateTime(2).ToString();
                    entry.StartedAt = result.IsDBNull(3) ? null : result.GetDateTime(3).ToString();
                    entry.EndedAt = result.IsDBNull(4) ? null : result.GetDateTime(4).ToString();
                    entry.Priority = result.IsDBNull(5) ? null : result.GetString(5);
                    entry.State = result.IsDBNull(6) ? null : result.GetString(6);
                    entry.Type = result.IsDBNull(7) ? null : result.GetString(7);
                    entry.Meta = result.IsDBNull(8) ? null : result.GetString(8);
                    entry.Error = result.IsDBNull(9) ? null : result.GetString(9);
                    entry.UserID = result.GetInt32(10);
                    entry.OrderID = result.GetInt32(11);
                    entry.EratosTaskID = result.IsDBNull(12) ? null : result.GetString(12);
                    entry.Name = result.IsDBNull(13) ? null : result.GetString(13);

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

        public ArrayList FindTask(int startRow, int endRow = 0)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            int start = (startRow <= 0) ? 1 : startRow;
            int end = (endRow <= 0) ? start + MAX_ROWS - 1 : endRow;
            if (end < start)
                throw new EratosDBException("Invalid Parameters!");

            if (end >= start + MAX_ROWS)
                end = start + MAX_ROWS - 1;

            string command = "WITH OrderedResults AS (SELECT *, ROW_NUMBER() OVER (ORDER BY TaskID) AS RowNum FROM [dbo].[Task]) ";
            command += "SELECT * FROM OrderedResults WHERE RowNum BETWEEN @StartRow AND @EndRow";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@StartRow", SqlDbType.Int);
                sql.Parameters["@StartRow"].Value = start;

                sql.Parameters.Add("@EndRow", SqlDbType.Int);
                sql.Parameters["@EndRow"].Value = end;

                SqlDataReader result = sql.ExecuteReader();
                TaskTable entry = new TaskTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.TaskID = result.GetInt32(0);
                    entry.CreatedAt = result.IsDBNull(1) ? null : result.GetDateTime(1).ToString();
                    entry.LastUpdatedAt = result.IsDBNull(2) ? null : result.GetDateTime(2).ToString();
                    entry.StartedAt = result.IsDBNull(3) ? null : result.GetDateTime(3).ToString();
                    entry.EndedAt = result.IsDBNull(4) ? null : result.GetDateTime(4).ToString();
                    entry.Priority = result.IsDBNull(5) ? null : result.GetString(5);
                    entry.State = result.IsDBNull(6) ? null : result.GetString(6);
                    entry.Type = result.IsDBNull(7) ? null : result.GetString(7);
                    entry.Meta = result.IsDBNull(8) ? null : result.GetString(8);
                    entry.Error = result.IsDBNull(9) ? null : result.GetString(9);
                    entry.UserID = result.GetInt32(10);
                    entry.OrderID = result.GetInt32(11);
                    entry.EratosTaskID = result.IsDBNull(12) ? null : result.GetString(12);
                    entry.Name = result.IsDBNull(13) ? null : result.GetString(13);

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

        public void UpdateTask(int taskID, string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "UPDATE [dbo].[Task] SET ";
            if (IdentifierType.ToLower().CompareTo("userid") == 0)
                command += "[UserID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("orderid") == 0)
                command += "[OrderID] = " + "\'" + int.Parse(IdentifierValue) + "\'";
            else if (IdentifierType.ToLower().CompareTo("eratostaskid") == 0)
                command += "[EratosTaskID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("createdat") == 0)
                command += "[CreatedAt] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("lastupdatedat") == 0)
                command += "[LastUpdatedAt] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("startedat") == 0)
                command += "[StartedAt] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("endedat") == 0)
                command += "[EndedAt] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("priority") == 0)
                command += "[Priority] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("state") == 0)
                command += "[State] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("type") == 0)
                command += "[Type] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("meta") == 0)
                command += "[Meta] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("error") == 0)
                command += "[Error] = " + "\'" + IdentifierValue + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");
            command += " WHERE TaskID = " + "\'" + taskID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public void DeleteTask(int taskID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[Task] WHERE TaskID = " + "\'" + taskID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public bool isDuplicateTask(TaskTable taskDetails)
        {
            if (FindTask("eratostaskid", taskDetails.EratosTaskID).Count > 0)
                return true;
            return false;
        }

        #endregion

        #region Module
        public bool CreateModule(ModuleTable moduleDetails)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "INSERT INTO [dbo].[Module]([ModuleName], [ModuleSchema], [isActive]) ";
            command += "VALUES(@ModuleName, @ModuleSchema, @isActive)";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
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
            else if (IdentifierType.ToLower().CompareTo("modulename") == 0)
                command += "[ModuleName] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("moduleschema") == 0)
                command += "[ModuleSchema] = " + "\'" + IdentifierValue + "\'";
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
                    entry.ModuleName = result.GetString(1);
                    entry.ModuleSchema = result.IsDBNull(2) ? null : result.GetString(2);

                    if (result.GetByte(3) == 0)
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

        public ArrayList FindModule(int startRow, int endRow = 0)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            int start = (startRow <= 0) ? 1 : startRow;
            int end = (endRow <= 0) ? start + MAX_ROWS - 1 : endRow;
            if (end < start)
                throw new EratosDBException("Invalid Parameters!");

            if (end >= start + MAX_ROWS)
                end = start + MAX_ROWS - 1;

            string command = "WITH OrderedResults AS (SELECT *, ROW_NUMBER() OVER (ORDER BY ModuleID) AS RowNum FROM [dbo].[Module]) ";
            command += "SELECT * FROM OrderedResults WHERE RowNum BETWEEN @StartRow AND @EndRow";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@StartRow", SqlDbType.Int);
                sql.Parameters["@StartRow"].Value = start;

                sql.Parameters.Add("@EndRow", SqlDbType.Int);
                sql.Parameters["@EndRow"].Value = end;

                SqlDataReader result = sql.ExecuteReader();
                ModuleTable entry = new ModuleTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.ModuleID = result.GetInt32(0);
                    entry.ModuleName = result.GetString(1);
                    entry.ModuleSchema = result.IsDBNull(2) ? null : result.GetString(2);

                    if (result.GetByte(3) == 0)
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

        public void UpdateModule(int moduleID, string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "UPDATE [dbo].[Module] SET ";
            if (IdentifierType.ToLower().CompareTo("modulename") == 0)
                command += "[ModuleName] = " + "\'" + IdentifierValue + "\'";
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
            command += " WHERE ModuleID = " + "\'" + moduleID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public void DeleteModule(int moduleID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[Module] WHERE ModuleID = " + "\'" + moduleID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }
        #endregion

        #region Resource
        public bool CreateResource(ResourceTable resource)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");
            if (isDuplicateResource(resource))
                return false;

            string command = "INSERT INTO [dbo].[Resource]([EratosResourceID], [Date], [Policy], [Geo], [Meta], [Type], [Name]) ";
            command += "VALUES(@EratosResourceID, @Date, @Policy, @Geo, @Meta, @Type, @Name)";
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

                sql.Parameters.Add("@Type", SqlDbType.NVarChar);
                if (resource.Type != null)
                    sql.Parameters["@Type"].Value = resource.Type;
                else
                    sql.Parameters["@Type"].Value = DBNull.Value;

                sql.Parameters.Add("@Name", SqlDbType.NVarChar);
                if (resource.Name != null)
                    sql.Parameters["@Name"].Value = resource.Name;
                else
                    sql.Parameters["@Name"].Value = DBNull.Value;

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
            else if (IdentifierType.ToLower().CompareTo("type") == 0)
                command += "[Type] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
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
                    entry.Date = result.IsDBNull(2) ? null : result.GetDateTime(2).ToString();
                    entry.Policy = result.IsDBNull(3) ? null : result.GetString(3);
                    entry.Geo = result.IsDBNull(4) ? null : result.GetString(4);
                    entry.Meta = result.IsDBNull(5) ? null : result.GetString(5);
                    entry.Type = result.IsDBNull(6) ? null : result.GetString(6);
                    entry.Name = result.IsDBNull(7) ? null : result.GetString(7);

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

        public ArrayList FindResource(int startRow, int endRow = 0)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            int start = (startRow <= 0) ? 1 : startRow;
            int end = (endRow <= 0) ? start + MAX_ROWS - 1 : endRow;
            if (end < start)
                throw new EratosDBException("Invalid Parameters!");

            if (end >= start + MAX_ROWS)
                end = start + MAX_ROWS - 1;

            string command = "WITH OrderedResults AS (SELECT *, ROW_NUMBER() OVER (ORDER BY ResourceID) AS RowNum FROM [dbo].[Resource]) ";
            command += "SELECT * FROM OrderedResults WHERE RowNum BETWEEN @StartRow AND @EndRow";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                sql.Parameters.Add("@StartRow", SqlDbType.Int);
                sql.Parameters["@StartRow"].Value = start;

                sql.Parameters.Add("@EndRow", SqlDbType.Int);
                sql.Parameters["@EndRow"].Value = end;

                SqlDataReader result = sql.ExecuteReader();
                ResourceTable entry = new ResourceTable();
                ArrayList results = new ArrayList();

                while (result.Read())
                {
                    entry.ResourceID = result.GetInt32(0);
                    entry.EratosResourceID = result.GetString(1);
                    entry.Date = result.IsDBNull(2) ? null : result.GetDateTime(2).ToString();
                    entry.Policy = result.IsDBNull(3) ? null : result.GetString(3);
                    entry.Geo = result.IsDBNull(4) ? null : result.GetString(4);
                    entry.Meta = result.IsDBNull(5) ? null : result.GetString(5);
                    entry.Type = result.IsDBNull(6) ? null : result.GetString(6);
                    entry.Name = result.IsDBNull(7) ? null : result.GetString(7);

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

        public void UpdateResource(int resourceID, string IdentifierType, string IdentifierValue)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "UPDATE [dbo].[Resource] SET ";
            if (IdentifierType.ToLower().CompareTo("eratosresourceid") == 0)
                command += "[EratosResourceID] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("policy") == 0)
                command += "[Policy] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("geo") == 0)
                command += "[Geo] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("meta") == 0)
                command += "[Meta] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("type") == 0)
                command += "[Type] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("name") == 0)
                command += "[Name] = " + "\'" + IdentifierValue + "\'";
            else if (IdentifierType.ToLower().CompareTo("date") == 0)
                command += "[Date] = " + "\'" + IdentifierValue + "\'";
            else
                throw new EratosDBException("Invalid Parameter \'Identifier Type or Value\'");
            command += " WHERE ResourceID = " + "\'" + resourceID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        public void DeleteResource(int resourceID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[Resource] WHERE ResourceID = " + "\'" + resourceID.ToString() + "\'";
            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
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
        #endregion

        #region Associations
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

        public void DeleteResourceTaskAssociation(int resourceID, int taskID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[ResourceTask] ";
            command += "WHERE ResourceID = " + "\'" + resourceID.ToString() + "\' ";
            command += "AND TaskID = " + "\'" + taskID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        private bool isDuplicateRTAssociation(int ResourceID, int TaskID)
        {
            string command = "SELECT * FROM [ResourceTask] WHERE [ResourceID] = " + ResourceID.ToString() + " AND [TaskID] = " + TaskID.ToString();
            SqlCommand sql = new SqlCommand(command, connection);
            SqlDataReader result = sql.ExecuteReader();
            bool hasRows = result.HasRows;
            result.Close();
            return hasRows;
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

        public void DeleteResourceModuleAssociation(int resourceID, int moduleID)
        {
            if (!isConnected)
                throw new EratosDBException("Disconnected from Database!");

            string command = "DELETE FROM [dbo].[ResourceModule] ";
            command += "WHERE ResourceID = " + "\'" + resourceID.ToString() + "\' ";
            command += "AND ModuleID = " + "\'" + moduleID.ToString() + "\'";

            try
            {
                SqlCommand sql = new SqlCommand(command, connection);
                SqlDataReader result = sql.ExecuteReader();
                result.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new EratosDBException("Unknown Error (possibly invalid command)");
            }
        }

        private bool isDuplicateRMAssociation(int ResourceID, int ModuleID)
        {
            string command = "SELECT * FROM [ResourceModule] WHERE [ResourceID] = " + ResourceID.ToString() + " AND [ModuleID] = " + ModuleID.ToString();
            SqlCommand sql = new SqlCommand(command, connection);
            SqlDataReader result = sql.ExecuteReader();
            bool hasRows = result.HasRows;
            result.Close();
            return hasRows;
        }
        #endregion
    }


    class Program
    {
        static void Main(string[] args)
        {
            //DatabaseController dh = new DatabaseController();
            //Console.WriteLine(dh.Connect());
            //bool response = dh.CreateUser(new UserTable(0, "demouser3", "demoemail3", "demoname3", "demoauth3", DateTime.Now, null));
            //ArrayList entry = dh.FindUser("email", "demoemail3");
            /*ArrayList users = dh.FindUser(2, 2);
            foreach (UserTable element in users)
                Console.WriteLine(element.UserID + " " + element.CreatedAt + " " + element.isAdmin);*/


            //dh.UpdateUser(2, "isadmin", "true");
            //dh.CreateUser(new UserTable(0, "demouser4", "demoemail4", "demoname4", "demoauth4", DateTime.Now.ToString(), null, false));
            //dh.DeleteUser(4);

            /*
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
                Console.WriteLine(element.ModuleID + " " + element.ModuleName);
            //bool response = dh.CreateResource(new ResourceTable(0, "resource1_id", default, "policy1", "geo1", null));
            ArrayList ress = dh.FindResource("policy", "policy1");
            foreach (ResourceTable element in ress)
                Console.WriteLine(element.ResourceID + " " + element.EratosResourceID + " " + element.Geo);
            */
            //bool response = dh.CreateResourceTaskAssociation(1, 1);
            //bool response = dh.CreateResourceModuleAssociation(1, 1);
            //Console.WriteLine(response);

            //Console.ReadLine();
            //dh.Disconnect();
        }
    }

}
