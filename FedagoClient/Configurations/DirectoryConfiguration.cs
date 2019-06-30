using DataBazrPeer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public class DirectoryConfiguration : DataSource
    {
        public string Path { get; set; }
        public string Extension { get; set; }

        public override string Display { get { return string.Format("{0} ({1})", Name, Path.ToShortName()); } }
        public override string Type { get { return "File System"; } }
    }
}
