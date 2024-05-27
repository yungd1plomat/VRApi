using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonUtility;
using System.Threading.Tasks;

namespace VRClient.Models
{
	[Serializable]
    public class UserInfo
    {
        public string id { get; set; }

        public string username { get; set; }

        public string email { get; set; }

        public bool emailConfirmed { get; set; }

        public IEnumerable<string> roles { get; set; }
    }
}
