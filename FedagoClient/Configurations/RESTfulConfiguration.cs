using DataBazrPeer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public class RestfulConfiguration : DataSource
    {
        public string Url { get; set; }
        public string Body { get; set; }

        public override string Display { get { return string.Format("{0} ({1})", Name, Url.ToShortName()); } }
        public override string Type {  get { return "RESTful API"; } }
    }
}
