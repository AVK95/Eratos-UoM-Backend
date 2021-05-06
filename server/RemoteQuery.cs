/***************************************************************************
 * C#.NET script to create a remote query to the Eratos-UOM Backend server *
 * @author: Aditya Vikram Khandelwal                                       *
 *                                                                         *
 * Usage: Create a function with a string argument 'query' in your program *
 * Copy the code inside the main function and put it inside your function  *
 * Pass on the query to the statement in line 34.                          *
 * Table Names must be fully qualified !!!                                 *
 **************************************************************************/

using System;
using Microsoft.Data.SqlClient;
using System.Text;

namespace sqltest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "eratos-uom-backend.database.windows.net";
                builder.UserID = "unimelb";
                builder.Password = "Please contact dev team for the password!";
                builder.InitialCatalog = "UserDB";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    String sql = "select * from [SalesLT].[ProductModelProductDescription]";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();
        }
    }
}
