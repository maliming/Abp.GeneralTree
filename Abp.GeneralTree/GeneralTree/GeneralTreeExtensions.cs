using System.Collections.Generic;
using System.Linq;

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
                if (!treeDic.ContainsKey(x.ParentId.Value))
                {
                    return;
                }
                var parent = treeDic[x.ParentId.Value];
                if (parent.Children == null)
                {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            if (treeDic.Values.Any(x => x.ParentId == null))
            {
                return treeDic.Values.Where(x => x.ParentId == null);
            }

            return treeDic.Values.Where(x =>
                x.ParentId != null && !treeDic.Values.Select(q => q.Id).Contains(x.ParentId.Value));
        }

        public static IEnumerable<TTree> ToTreeWithReferenceType<TTree, TPrimaryKey>(this IEnumerable<TTree> tree)
            where TPrimaryKey : class
            where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
        {
            var treeDic = tree.ToDictionary(x => x.Id);

            treeDic.Where(x => x.Value.ParentId != null).Select(x => x.Value).ForEach(x =>
            {
                if (!treeDic.ContainsKey(x.ParentId))
                {
                    return;
                }
                var parent = treeDic[x.ParentId];
                if (parent.Children == null)
                {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            if (treeDic.Values.Any(x => x.ParentId == null))
            {
                return treeDic.Values.Where(x => x.ParentId == null);
            }

            return treeDic.Values.Where(x =>
                x.ParentId != null && !treeDic.Values.Select(q => q.Id).Contains(x.ParentId));
        }

        public static IEnumerable<TTree> ToTreeDto<TTree, TPrimaryKey>(this IEnumerable<TTree> tree)
            where TPrimaryKey : struct
            where TTree : class, IGeneralTreeDto<TTree, TPrimaryKey>
        {
            var treeDic = tree.ToDictionary(x => x.Id);

            treeDic.Where(x => x.Value.ParentId.HasValue).Select(x => x.Value).ForEach(x =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                if (!treeDic.ContainsKey(x.ParentId.Value))
                {
                    return;
                }
                var parent = treeDic[x.ParentId.Value];
                if (parent.Children == null)
                {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            if (treeDic.Values.Any(x => x.ParentId == null))
            {
                return treeDic.Values.Where(x => x.ParentId == null);
            }

            return treeDic.Values.Where(x =>
                x.ParentId != null && !treeDic.Values.Select(q => q.Id).Contains(x.ParentId.Value));
        }


        public static IEnumerable<TTree> ToTreeDtoWithReferenceType<TTree, TPrimaryKey>(this IEnumerable<TTree> tree)
            where TPrimaryKey : class
            where TTree : class, IGeneralTreeDtoWithReferenceType<TTree, TPrimaryKey>
        {
            var treeDic = tree.ToDictionary(x => x.Id);

            treeDic.Where(x => x.Value.ParentId != null).Select(x => x.Value).ForEach(x =>
            {
                if (!treeDic.ContainsKey(x.ParentId))
                {
                    return;
                }
                var parent = treeDic[x.ParentId];
                if (parent.Children == null)
                {
                    parent.Children = new List<TTree>();
                }
                parent.Children.Add(x);
            });

            if (treeDic.Values.Any(x => x.ParentId == null))
            {
                return treeDic.Values.Where(x => x.ParentId == null);
            }

            return treeDic.Values.Where(x =>
                x.ParentId != null && !treeDic.Values.Select(q => q.Id).Contains(x.ParentId));
        }
    }
}