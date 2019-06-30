using DataBazrPeer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public class DatabaseConfiguration : DataSource
    {
        public string ConnectionString { get; set; }
        public string Table { get; set; }
        public bool IsFunction { get; set; }

        public override string Display { get { return string.Format("{0} ({1})", Name, ConnectionString.ToShortName()); } }
        public override string Type { get { return "Database"; } }
    }
}
