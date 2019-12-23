﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.GeneralTree.GeneralTree;
using Abp.Linq.Extensions;
using Abp.UI;

namespace Abp.GeneralTree
{
    public class
        GeneralTreeManagerWithReferenceType<TTree, TPrimaryKey> : IGeneralTreeManagerWithReferenceType<TTree,
            TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
    {
        private readonly IGeneralTreeCodeGenerate _generalTreeCodeGenerate;
        private readonly IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> _generalTreeConfiguration;
        private readonly IRepository<TTree, TPrimaryKey> _generalTreeRepository;

        public IUnitOfWorkManager UnitOfWorkManager { get; set; }

        public GeneralTreeManagerWithReferenceType(IGeneralTreeCodeGenerate generalTreeCodeGenerate,
            IRepository<TTree, TPrimaryKey> generalTreeRepository,
            IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> generalTreeConfiguration)
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

            var children = await GetChildrenAsync(tree.Id, true);
            var oldFullName = tree.FullName;

            if (tree.ParentId != null)
            {
                var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId);
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + _generalTreeConfiguration.Hyphen + tree.Name;
            }
            else
            {
                tree.FullName = tree.Name;
            }

            foreach (var child in children)
            {
                child.FullName = _generalTreeCodeGenerate.MergeFullName(tree.FullName,
                    _generalTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));

                childrenAction?.Invoke(child);
            }
        }

        [UnitOfWork]
        public virtual async Task MoveAsync(TPrimaryKey id, TPrimaryKey parentId, Action<TTree> childrenAction = null, int? index = null)
        {
            var tree = await _generalTreeRepository.GetAsync(id);

            if (!tree.ParentId.Equals(parentId))//Move level
            {
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
                        _generalTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));//RemoveParentCode just sub string
                    child.Level = child.Code.Split('.').Length;

                    childrenAction?.Invoke(child);
                }
            }

            await UnitOfWorkManager.Current.SaveChangesAsync();//Must commit...

            //Move index
            if (index.HasValue)
            {
                if (index.Value < 0)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(index)} cannot be less than 0.");
                }

                //Store the code update info
                Dictionary<string, TTree> updateDic = new Dictionary<string, TTree>();

                //Update all the same level
                var sameLevelNodes = await GetChildrenAsync(parentId, false);// this incule 'tree'
                sameLevelNodes = sameLevelNodes.OrderBy(m => m.Code).ToList();

                if (index.Value > sameLevelNodes.Count - 1)
                {
                    //Move to last
                    index = sameLevelNodes.Count - 1;
                }

                sameLevelNodes.RemoveAll(m => m.Id.Equals(tree.Id));
                sameLevelNodes.Insert(index.Value, tree);

                var parentCode = _generalTreeCodeGenerate.GetParentCode(tree.Code);
                var startCode = _generalTreeCodeGenerate.MergeCode(parentCode,
                    _generalTreeCodeGenerate.CreateCode(1));
                foreach (var sameLevel in sameLevelNodes)
                {
                    if (sameLevel.Code != startCode)//Ignore index has not changed
                    {
                        updateDic[startCode] = sameLevel;
                        var levelChildren = await GetChildrenAsync(sameLevel.Id, true);
                        foreach (var levelChild in levelChildren)
                        {
                            var childCode = _generalTreeCodeGenerate.MergeCode(startCode,
                                _generalTreeCodeGenerate.RemoveParentCode(levelChild.Code, sameLevel.Code));
                            updateDic[childCode] = levelChild;
                        }
                    }

                    startCode = _generalTreeCodeGenerate.GetNextCode(startCode);
                }

                //Do update
                foreach (var up in updateDic)
                {
                    up.Value.Code = up.Key;
                }

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

            if (tree.ParentId != null)
            {
                var parent =
                    await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId);
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
        private async Task<string> GetNextChildCodeAsync(TPrimaryKey parentId)
        {
            var lastChild =
                _generalTreeRepository.GetAll()
                    .Where(x => x.ParentId == parentId)
                    .OrderByDescending(x => x.Code)
                    .FirstOrDefault();
            if (lastChild != null)
            {
                //Get the next code
                return _generalTreeCodeGenerate.GetNextCode(lastChild.Code);
            }

            //Generate a code
            var parentCode = parentId != null ? await GetCodeAsync(parentId) : null;
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
        private async Task<List<TTree>> GetChildrenAsync(TPrimaryKey parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return await _generalTreeRepository.GetAllListAsync(x => x.ParentId == parentId);
            }

            if (parentId == null)
            {
                return await _generalTreeRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId);

            return _generalTreeRepository.GetAll().Where(x => x.Code.StartsWith(code) && x.Id != parentId)
                .ToList();
        }

        /// <summary>
        /// Check if there are same names at the same tree level
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private void CheckSameName(TTree tree)
        {
            if (_generalTreeConfiguration.CheckSameNameExpression == null)
            {
                if (!_generalTreeRepository.GetAll().Where(x => x.ParentId == tree.ParentId && x.Id != tree.Id)
                    .Any(x => x.Name == tree.Name))
                {
                    return;
                }
            }
            else
            {
                var trees = _generalTreeRepository.GetAll().Where(x => x.ParentId == tree.ParentId && x.Id != tree.Id)
                    .Where(x => x.Name == tree.Name).ToList();
                if (!trees.Any() || !trees.Any(x => _generalTreeConfiguration.CheckSameNameExpression(x, tree)))
                {
                    return;
                }
            }

            throw new UserFriendlyException(_generalTreeConfiguration.ExceptionMessageFactory.Invoke(tree));
        }

        /// <summary>
        /// Get Child FullName Async
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="childFullName"></param>
        /// <returns></returns>
        private async Task<string> GetChildFullNameAsync(TPrimaryKey parentId, string childFullName)
        {
            var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == parentId);
            return parent != null ? parent.FullName + _generalTreeConfiguration.Hyphen + childFullName : childFullName;
        }
    }
}