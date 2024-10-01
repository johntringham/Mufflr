using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mufflr
{
    [XmlRoot(ElementName = "Settings")]
    public class Settings
    {

        [XmlElement(ElementName = "VolumeCap")]
        public float VolumeCap { get; set; } = 0.05f;
    }
}
