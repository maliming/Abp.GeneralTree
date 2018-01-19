using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abp.GeneralTree
{
    public static class GeneralTreeExtensions
    {
        public static IEnumerable<TTree> ToTree<TTree, TPrimaryKey>(this IEnumerable<TTree> tree)
            where TPrimaryKey : struct
            where TTree : class, IGeneralTree<TTree, TPrimaryKey>
        {
            var treeDic = tree.ToDictionary(x => x.Id);

            treeDic.Where(x => x.Value.ParentId.HasValue).Select(x => x.Value).ForEach(x =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                var parent = treeDic[x.ParentId.Value];
                if (parent.Children == null)
                {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            return treeDic.Values.Where(x => x.ParentId == null);
        }

        public static IEnumerable<TTree> ToTreeWithReferenceType<TTree, TPrimaryKey>(this IEnumerable<TTree> tree)
            where TPrimaryKey : class
            where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
        {
            var treeDic = tree.ToDictionary(x => x.Id);

            treeDic.Where(x => x.Value.ParentId != null).Select(x => x.Value).ForEach(x =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                var parent = treeDic[x.ParentId];
                if (parent.Children == null) {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            return treeDic.Values.Where(x => x.ParentId == null);
        }
    }
}
