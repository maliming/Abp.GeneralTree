using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.GeneralTree;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFramework.Repositories;

namespace TreeApplication
{
    public class TreeAppService : IApplicationService
    {
        private readonly GeneralTreeManager<Region> _generalRegionTreeManager;

        private readonly IRepository<Region, long> _regionRepository;

        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TreeAppService(
            GeneralTreeManager<Region> generalRegionTreeManager,
            IRepository<Region, long> regionRepository,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _generalRegionTreeManager = generalRegionTreeManager;
            _regionRepository = regionRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public void Create()
        {
            var beijing = new Region()
            {
                Name = "北京"
            };
            _generalRegionTreeManager.Create(beijing);
            _unitOfWorkManager.Current.SaveChanges();

            var beijingChildren = new List<Region>()
            {
                new Region()
                {
                    Name = "东城区",
                    ParentId = beijing.Id
                },
                new Region()
                {
                    Name = "西城区",
                    ParentId = beijing.Id
                },
                new Region()
                {
                    Name = "朝阳区",
                    ParentId = beijing.Id
                },
                new Region()
                {
                    Name = "丰台区",
                    ParentId = beijing.Id
                }
            };
            beijingChildren.ForEach(x =>
            {
                _generalRegionTreeManager.Create(x);
                _unitOfWorkManager.Current.SaveChanges();
            });

            var hebei = new Region()
            {
                Name = "河北省"
            };
            _generalRegionTreeManager.Create(hebei);
            _unitOfWorkManager.Current.SaveChanges();

            var hebeiChildren = new List<Region>()
            {
                new Region()
                {
                    Name = "石家庄",
                    ParentId = hebei.Id
                },
                new Region()
                {
                    Name = "张家口",
                    ParentId = hebei.Id
                },
                new Region()
                {
                    Name = "承德",
                    ParentId = hebei.Id
                }
            };
            hebeiChildren.ForEach(x =>
            {
                _generalRegionTreeManager.Create(x);
                _unitOfWorkManager.Current.SaveChanges();
            });
        }
        
        public IReadOnlyList<Region> GetAll()
        {
            return _regionRepository.GetAllList();
        }
    }
}
