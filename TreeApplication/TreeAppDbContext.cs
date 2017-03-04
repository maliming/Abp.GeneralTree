using System;
using System.Collections.Generic;
using System.Data.Entity;
using Abp.EntityFramework;

namespace TreeApplication
{
    public class TreeAppDbContext : AbpDbContext
    {
        public virtual IDbSet<Region> Region { get; set; }

        public TreeAppDbContext()
            : base("Default")
        {
        }

        public TreeAppDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }
    }
}
