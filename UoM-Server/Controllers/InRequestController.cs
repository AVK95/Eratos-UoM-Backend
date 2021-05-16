using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Server.Controllers
{
    class InRequestController
    {
        public string getResourceVersion(string resourceUri)
        {
            OutRequestController orc = new OutRequestController();
            string versions = orc.ShowResourceVersions(resourceUri);
            return versions;
        }
    }
}
