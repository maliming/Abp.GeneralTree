using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.GeneralTree.GeneralTree;
using Abp.Linq.Extensions;
using Abp.UI;

namespace Abp.GeneralTree
{
    public class GeneralTreeManager<TTree, TPrimaryKey> : IGeneralTreeManager<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        private readonly IGeneralTreeCodeGenerate _generalTreeCodeGenerate;
        private readonly IGeneralTreeConfiguration<TTree, TPrimaryKey> _generalTreeConfiguration;
        private readonly IRepository<TTree, TPrimaryKey> _generalTreeRepository;

        public GeneralTreeManager(IGeneralTreeCodeGenerate generalTreeCodeGenerate,
            IRepository<TTree, TPrimaryKey> generalTreeRepository,
            IGeneralTreeConfiguration<TTree, TPrimaryKey> generalTreeConfiguration)
        {
            _generalTreeCodeGenerate = generalTreeCodeGenerate;
            _generalTreeRepository = generalTreeRepository;
            _generalTreeConfiguration = generalTreeConfiguration;
        }

        [UnitOfWork]
        public virtual async Task CreateAsync(TTree tree)
        {
            await _generalTreeRepository.InsertAsync(await GenerateTree(tree));
        }

        [UnitOfWork]
        public virtual async Task BulkCreateAsync(TTree tree, Action<TTree> childrenAction = null)
        {
            //Traverse Tree
            TraverseTree(await GenerateTree(tree), tree.Children, childrenAction);

            await _generalTreeRepository.InsertAsync(tree);
        }

        [UnitOfWork]
        public virtual async Task CreateChildrenAsync(TTree parent, ICollection<TTree> children,
            Action<TTree> childrenAction = null)
        {
            if (parent.Children.IsNullOrEmpty())
            {
                parent.Children = children;
            }
            else
            {
                children.ForEach(x => parent.Children.Add(x));
            }

            //Traverse Tree
            TraverseTree(parent, children, childrenAction);

            await _generalTreeRepository.UpdateAsync(parent);
        }

        [UnitOfWork]
        public virtual async Task FillUpAsync(TTree tree, Action<TTree> childrenAction = null)
        {
            //Traverse Tree
            TraverseTree(await GenerateTree(tree), tree.Children, childrenAction);
        }

        [UnitOfWork]
        public virtual async Task UpdateAsync(TTree tree, Action<TTree> childrenAction = null)
        {
            CheckSameName(tree);

            var oldFullName = tree.FullName;

            if (tree.ParentId.HasValue)
            {
                var parent = await _generalTreeRepository.FirstOrDefaultAsync(EqualId(tree.ParentId.Value));
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + _generalTreeConfiguration.Hyphen + tree.Name;
            }
            else
            {
                tree.FullName = tree.Name;
            }

            var children = await GetChildrenAsync(tree.Id, true);
            foreach (var child in children)
            {
                child.FullName = _generalTreeCodeGenerate.MergeFullName(tree.FullName,
                    _generalTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));

                childrenAction?.Invoke(child);
            }
        }

        [UnitOfWork]
        public virtual async Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId, Action<TTree> childrenAction = null)
        {
            var tree = await _generalTreeRepository.GetAsync(id);
            if (tree.ParentId.Equals(parentId))
            {
                return;
            }

            //Should find children before Code change
            var children = await GetChildrenAsync(id, true);

            //Store old code and full name of Tree
            var oldCode = tree.Code;
            var oldFullName = tree.FullName;

            //Move Tree
            tree.Code = await GetNextChildCodeAsync(parentId);
            tree.Level = tree.Code.Split('.').Length;
            tree.ParentId = parentId;
            tree.FullName = await GetChildFullNameAsync(parentId, tree.Name);

            CheckSameName(tree);

            //Update Children Codes and FullName
            foreach (var child in children)
            {
                child.Code = _generalTreeCodeGenerate.MergeCode(tree.Code,
                    _generalTreeCodeGenerate.RemoveParentCode(child.Code, oldCode));
                child.FullName = _generalTreeCodeGenerate.MergeFullName(tree.FullName,
                    _generalTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));
                child.Level = child.Code.Split('.').Length;

                childrenAction?.Invoke(child);
            }
        }

        [UnitOfWork]
        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            var tree = await _generalTreeRepository.FirstOrDefaultAsync(id);
            if (tree != null)
            {
                await _generalTreeRepository.DeleteAsync(x => x.Code.StartsWith(tree.Code));
            }
        }

        private async Task<TTree> GenerateTree(TTree tree)
        {
            tree.Code = await GetNextChildCodeAsync(tree.ParentId);
            tree.Level = tree.Code.Split('.').Length;

            if (tree.ParentId.HasValue)
            {
                var parent =
                    await _generalTreeRepository.FirstOrDefaultAsync(EqualId(tree.ParentId.Value));
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + _generalTreeConfiguration.Hyphen + tree.Name;
            }
            else
            {
                //root
                tree.FullName = tree.Name;
            }

            CheckSameName(tree);

            return tree;
        }

        private void TraverseTree(TTree parent, ICollection<TTree> children, Action<TTree> childrenAction = null)
        {
            if (children == null || !children.Any())
            {
                return;
            }

            children.ForEach((tree, index) =>
            {
                //CheckSameName
                if (children.Count(x => x.Name == tree.Name) > 1)
                {
                    throw new UserFriendlyException(_generalTreeConfiguration.ExceptionMessageFactory.Invoke(tree));
                }

                tree.Code = index == 0
                    ? _generalTreeCodeGenerate.MergeCode(parent.Code, _generalTreeCodeGenerate.CreateCode(1))
                    : _generalTreeCodeGenerate.GetNextCode(children.ElementAt(index - 1).Code);

                tree.Level = tree.Code.Split('.').Length;
                tree.FullName = parent.FullName + _generalTreeConfiguration.Hyphen + tree.Name;

                childrenAction?.Invoke(tree);
            });

            children.ForEach(tree => { TraverseTree(tree, tree.Children, childrenAction); });
        }

        /// <summary>
        /// Get next child code
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task<string> GetNextChildCodeAsync(TPrimaryKey? parentId)
        {
            var lastChild =
                _generalTreeRepository.GetAll()
                    .Where(EqualParentId(parentId))
                    .OrderByDescending(x => x.Code)
                    .FirstOrDefault();
            if (lastChild != null)
            {
                //Get the next code
                return _generalTreeCodeGenerate.GetNextCode(lastChild.Code);
            }

            //Generate a code
            var parentCode = parentId != null ? await GetCodeAsync(parentId.Value) : null;
            return _generalTreeCodeGenerate.MergeCode(parentCode, _generalTreeCodeGenerate.CreateCode(1));
        }

        /// <summary>
        /// Get Code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<string> GetCodeAsync(TPrimaryKey id)
        {
            return (await _generalTreeRepository.GetAsync(id)).Code;
        }

        /// <summary>
        /// Get all children, can be recursively
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        private async Task<List<TTree>> GetChildrenAsync(TPrimaryKey? parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return await _generalTreeRepository.GetAllListAsync(EqualParentId(parentId));
            }

            if (!parentId.HasValue)
            {
                return await _generalTreeRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId.Value);

            return _generalTreeRepository.GetAll().Where(x => x.Code.StartsWith(code))
                .Where(NotEqualId(parentId.Value)).ToList();
        }

        /// <summary>
        /// Check if there are same names at the same tree level
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private void CheckSameName(TTree tree)
        {
            if (_generalTreeRepository.GetAll().Where(EqualParentId(tree.ParentId))
                .WhereIf(_generalTreeConfiguration.CheckSameNameExpression != null,
                    x => _generalTreeConfiguration.CheckSameNameExpression(x, tree))
                .Where(NotEqualId(tree.Id))
                .Any(x => x.Name == tree.Name))
            {
                throw new UserFriendlyException(_generalTreeConfiguration.ExceptionMessageFactory.Invoke(tree));
            }
        }

        /// <summary>
        /// Get Child FullName Async
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="childFullName"></param>
        /// <returns></returns>
        private async Task<string> GetChildFullNameAsync(TPrimaryKey? parentId, string childFullName)
        {
            if (!parentId.HasValue)
            {
                return childFullName;
            }

            var parent = await _generalTreeRepository.FirstOrDefaultAsync(EqualId(parentId.Value));
            return parent != null ? parent.FullName + _generalTreeConfiguration.Hyphen + childFullName : childFullName;
        }

        #region EqualExpression

        private static Expression<Func<TTree, bool>> EqualId(TPrimaryKey id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));
            var leftExpression = Expression.PropertyOrField(lambdaParam, "Id");
            var rightExpression = Expression.Constant(id, typeof(TPrimaryKey));
            var lambdaBody = Expression.Equal(leftExpression, rightExpression);
            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqualId(TPrimaryKey id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));
            var leftExpression = Expression.PropertyOrField(lambdaParam, "Id");
            var rightExpression = Expression.Constant(id, typeof(TPrimaryKey));
            var lambdaBody = Expression.NotEqual(leftExpression, rightExpression);
            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> EqualParentId(TPrimaryKey? parentId)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));
            var leftExpression = Expression.PropertyOrField(lambdaParam, "ParentId");
            var rightExpression = Expression.Constant(parentId, typeof(TPrimaryKey?));
            var lambdaBody = Expression.Equal(leftExpression, rightExpression);
            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqualParentId(TPrimaryKey? parentId)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));
            var leftExpression = Expression.PropertyOrField(lambdaParam, "ParentId");
            var rightExpression = Expression.Constant(parentId, typeof(TPrimaryKey?));
            var lambdaBody = Expression.NotEqual(leftExpression, rightExpression);
            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        #endregion
    }
}