using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhonesFirmware.GetDevicesByName;
using System.Xml.Linq;

namespace PhonesFirmware
{
    class PhoneInfo
    {        
        public CmDevice device { get; set; }
        //public string version { get; set; }
        public List<XElement> info { get; set; }
    }
}
