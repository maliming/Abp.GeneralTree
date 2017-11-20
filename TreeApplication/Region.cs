using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;
using Abp.GeneralTree;

namespace TreeApplication
{
    public class Region : Entity<long>, IGeneralTree<Region, long>
    {
        public virtual string Name { get; set; }

        public virtual string FullName { get; set; }

        public virtual string Code { get; set; }

        public virtual int Level { get; set; }

        public virtual Region Parent { get; set; }

        public virtual long? ParentId { get; set; }

        public virtual ICollection<Region> Children { get; set; }
    }
}
