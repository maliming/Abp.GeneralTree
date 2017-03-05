using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.EntityFramework;
using TreeApplication;

namespace TreeTests
{
    public class TreeTestDbContext : AbpDbContext
    {
        public virtual IDbSet<Region> Region { get; set; }

        public TreeTestDbContext()
        : base("Default")
        {

        }

        public TreeTestDbContext(string nameOrConnectionString)
        : base(nameOrConnectionString)
        {

        }

        //This constructor is used in tests
        public TreeTestDbContext(DbConnection connection)
        : base(connection, true)
        {

        }
    }
}
