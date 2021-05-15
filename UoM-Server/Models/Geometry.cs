using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UoM_Server.Models
{
    [DataContract]
    public class GeometryResponse
    {
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string type { get; set; }

        [DataMember(Name = "properties", EmitDefaultValue = false)]
        public List<string> properties { get; set; }

        [DataMember(Name = "geometry", EmitDefaultValue = false)]
        public Geometry geometry { get; set; }
    }

    [DataContract]
    public class Geometry
    {
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string type { get; set; }

        [DataMember(Name = "coordinates")]
        public List<float> coordinates { get; set; }
    }
}