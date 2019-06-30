using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public abstract class DataSource : IDataSource
    {
        //private static object _lock = new object();
        //private static volatile int _lastIndex = -1; //TODO: 

        public DataSource()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual string Display { get { return Name; } }
        public virtual string Type { get { return "DataSource"; } }
    }
}
