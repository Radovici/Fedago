using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public class ApplicationConfiguration
    {
        private IDictionary<string, string> _keyValuePairs = new Dictionary<string, string>();
        private IList<DatabaseConfiguration> _databaseConfigurations = new List<DatabaseConfiguration>();
        private IList<DirectoryConfiguration> _directoryConfigurations = new List<DirectoryConfiguration>();
        private IList<RestfulConfiguration> _restfulConfigurations = new List<RestfulConfiguration>();

        public ApplicationConfiguration()
        {

        }

        public IDictionary<string, string> KeyValuePairs
        { 
            get { return _keyValuePairs; }
            set { _keyValuePairs = value; }
        }
        public IList<DatabaseConfiguration> DatabaseConfigurations
        {
            get { return _databaseConfigurations; }
            set { _databaseConfigurations = value; }
        }
        public IList<DirectoryConfiguration> DirectoryConfigurations
        {
            get { return _directoryConfigurations; }
            set { _directoryConfigurations = value; }
        }
        public IList<RestfulConfiguration> RestfulConfigurations
        {
            get { return _restfulConfigurations; }
            set { _restfulConfigurations = value; }
        }

        [JsonIgnore]
        public IEnumerable<IDataSource> DataSources
        {
            get
            {
                List<IDataSource> dataSources = new List<IDataSource>();
                dataSources.AddRange(DatabaseConfigurations);
                dataSources.AddRange(DirectoryConfigurations);
                dataSources.AddRange(RestfulConfigurations);
                return dataSources;
            }
        }
    }
}
