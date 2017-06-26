using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Threading;

namespace Abp.GeneralTree
{
    public static class GeneralTreeSyncExtensions
    {
        public static void Create<TTree>(this GeneralTreeManager<TTree> manager, TTree tree) 
            where TTree : class, IEntity<long>, IGeneralTree<TTree>
        {
            AsyncHelper.RunSync(() => manager.CreateAsync(tree));
        }

        public static void Update<TTree>(this GeneralTreeManager<TTree> manager, TTree tree) 
            where TTree : class, IEntity<long>, IGeneralTree<TTree>
        {
            AsyncHelper.RunSync(() => manager.UpdateAsync(tree));
        }

        public static void Delete<TTree>(this GeneralTreeManager<TTree> manager, long id) 
            where TTree : class, IEntity<long>, IGeneralTree<TTree>
        {
            AsyncHelper.RunSync(() => manager.DeleteAsync(id));
        }

        public static void Move<TTree>(this GeneralTreeManager<TTree> manager, long id, long? parentId) 
            where TTree : class, IEntity<long>, IGeneralTree<TTree>
        {
            AsyncHelper.RunSync(() => manager.MoveAsync(id, parentId));
        }
    }
}
