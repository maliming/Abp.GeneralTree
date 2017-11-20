using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TreeApplication
{
    public class TreeAppDbContext : AbpDbContext
    {
        public TreeAppDbContext(DbContextOptions<TreeAppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Region> Region { get; set; }

        public virtual DbSet<Region2> Region2 { get; set; }
    }
}
