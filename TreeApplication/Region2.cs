using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.GeneralTree;

namespace TreeApplication
{
    public class Region2 : Entity<string>, IGeneralTreeWithReferenceType<Region2, string>
    {
        public Region2()
        {
            Id = Guid.NewGuid().ToString();
        }

        public virtual string MyCustomData { get; set; }

        public virtual string Name { get; set; }

        public virtual string FullName { get; set; }

        public virtual string Code { get; set; }

        public virtual int Level { get; set; }

        public virtual Region2 Parent { get; set; }

        public virtual string ParentId { get; set; }

        public virtual ICollection<Region2> Children { get; set; }
    }
}