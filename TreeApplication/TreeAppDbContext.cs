using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using Abp.EntityFramework;

namespace TreeApplication
{
    public class TreeAppDbContext : AbpDbContext
    {
        public virtual IDbSet<Region> Region { get; set; }

        public virtual IDbSet<Region2> Region2 { get; set; }

        public TreeAppDbContext()
            : base("Default")
        {
        }

        public TreeAppDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        //This constructor is used in tests
        public TreeAppDbContext(DbConnection connection)
        : base(connection, true)
        {

        }
    }
}
