using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Models
{
    public class UserAccessModel
    {
        public string Id { get; set; }

        public string Secret { get; set; }

        public string ServerUrl { get { return "https://demo.fedago.com"; } }

        public string CallbackUrl { get; set; }
    }
}
