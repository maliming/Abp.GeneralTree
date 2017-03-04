using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace Abp.Domain.GeneralTree
{
    public interface IGeneralTree<TTree>
    {
        string Name { get; set; }

        string FullName { get; set; }

        string Code { get; set; }

        int Level { get; set; }

        TTree Parent { get; set; }

        long? ParentId { get; set; }

        ICollection<TTree> Children { get; set; }
    }
}
