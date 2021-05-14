using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoM_Server.Controllers;
using UoM_Server.Models;

namespace UoM_Server
{
    public class Test
    {
        public void testFunctions()
        {
            OutRequestController orc = new OutRequestController();
            Console.WriteLine("Testing create resource.");
            Resource rsc = orc.CreateResource("https://schemas.eratos.ai/json/person", "TestRes");
            Console.WriteLine("Done! The resource ID is " + rsc.id);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing retrieve resource.");
            Resource newRsc = orc.GetResource(rsc.id);
            Console.WriteLine("Done! The resource ID is " + newRsc.id + " , should be same as above!");
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing get resource versions.");
            string versions = orc.ShowResourceVersions(rsc.id);
            Console.WriteLine("Done! The versions are " + versions);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing get policy.");
            ResourcePolicy policy = orc.GetPolicy(rsc.id);
            Console.WriteLine("Done! The policy id is " + policy.id);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing modify the policy.");
            string response = orc.AddUserToPolicy(policy, "https://staging.e-pn.io/users/f4jg4ai6guuiy2ldq26fpieq");
            Console.WriteLine("Done! Message: " + response);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing get JWT token.");
            string token = orc.GetToken();
            Console.WriteLine("Done! Token: " + token);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing create new task.");
            GNTaskResponse taskResponse = orc.CreateNewTask(token,Priority.Low,rsc.id);
            Console.WriteLine("Done! Task ID: " + taskResponse.id);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing retrieve task.");
            GNTaskResponse newTaskResponse = orc.GetTask(token, taskResponse.id);
            Console.WriteLine("Done! Task state: " + newTaskResponse.state);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing delete task.");
            string deleteResponse = orc.RemoveTask(token, taskResponse.id);
            Console.WriteLine("Done! Message: " + deleteResponse);
            Console.WriteLine("-----------------------------------");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Testing delete resource.");
            string deleteResource = orc.DeleteResource(rsc.id);
            Console.WriteLine("Done! Message: " + deleteResource);

        }
    }
}
