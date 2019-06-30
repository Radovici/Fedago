using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Configurations
{
    public interface IDataSource
    {
        /// <summary>
        /// Id: string GUID locally, int GUID after published
        /// </summary>
        string Id { get; set; }
        string Name { get; set; }
        string Display { get; }
        string Type { get; }
    }
}
