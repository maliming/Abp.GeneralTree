using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Services;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeManager<in TTree> 
        where TTree : class, IGeneralTree<TTree>, IEntity<long>
    {
        Task CreateAsync(TTree tree);

        Task UpdateAsync(TTree tree);

        Task MoveAsync(long id, long? parentId);

        Task DeleteAsync(long id);
    }
}
