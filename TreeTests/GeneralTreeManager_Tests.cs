﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.GeneralTree;
using Abp.GeneralTree.GeneralTree;
using Abp.UI;
using Shouldly;
using TreeApplication;
using Xunit;

namespace TreeTests
{
    public class GeneralTreeManager_Tests : TreeTestBase
    {
        public GeneralTreeManager_Tests()
        {
            _generalRegionTreeManager = LocalIocManager.Resolve<IGeneralTreeManager<Region, long>>();
            _generalTreeCodeGenerate = LocalIocManager.Resolve<IGeneralTreeCodeGenerate>();
        }

        private readonly IGeneralTreeManager<Region, long> _generalRegionTreeManager;

        private readonly IGeneralTreeCodeGenerate _generalTreeCodeGenerate;

        private Region GetRegion(string name)
        {
            return UsingDbContext(context =>
            {
                var region = context.Region.FirstOrDefault(x => x.Name == name);
                //region.ShouldNotBeNull();
                return region;
            });
        }

        private async Task<Region> CreateRegion(string name, long? parentId = null)
        {
            var region = new Region
            {
                Name = name,
                ParentId = parentId
            };
            await _generalRegionTreeManager.CreateAsync(region);
            return region;
        }

        [Fact]
        public async Task BulkCreate_ExistTree_Test()
        {
            await UsingDbContextAsync(async context =>
            {
                //Arrange
                context.Region.Add(new Region
                {
                    Name = "hebei",
                    FullName = "hebei",
                    Code = "00001",
                    Level = 1
                });
                await context.SaveChangesAsync();

                context.Region.Add(new Region
                {
                    Name = "shijiazhuang",
                    FullName = "hebei-shijiazhuang",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = 1
                });
                await context.SaveChangesAsync();
            });

            var chengde = new Region
            {
                Name = "chengde",
                ParentId = 1,
                Children = new List<Region>
                {
                    new Region
                    {
                        Name = "shuangqiaoqu"
                    },
                    new Region
                    {
                        Name = "shuangluanqu"
                    }
                }
            };

            await _generalRegionTreeManager.BulkCreateAsync(chengde);

            //Assert
            chengde = GetRegion("chengde");
            chengde.ShouldNotBeNull();
            chengde.Name.ShouldBe("chengde");
            chengde.FullName.ShouldBe("hebei-chengde");
            chengde.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2));
            chengde.Level.ShouldBe(2);
            chengde.ParentId.ShouldBe(1);

            var shuangqiaoqu = GetRegion("shuangqiaoqu");
            shuangqiaoqu.ShouldNotBeNull();
            shuangqiaoqu.Name.ShouldBe("shuangqiaoqu");
            shuangqiaoqu.FullName.ShouldBe("hebei-chengde-shuangqiaoqu");
            shuangqiaoqu.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2, 1));
            shuangqiaoqu.Level.ShouldBe(chengde.Level + 1);
            shuangqiaoqu.ParentId.ShouldBe(chengde.Id);

            var shuangluanqu = GetRegion("shuangluanqu");
            shuangluanqu.ShouldNotBeNull();
            shuangluanqu.Name.ShouldBe("shuangluanqu");
            shuangluanqu.FullName.ShouldBe("hebei-chengde-shuangluanqu");
            shuangluanqu.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2, 2));
            shuangluanqu.Level.ShouldBe(chengde.Level + 1);
            shuangluanqu.ParentId.ShouldBe(chengde.Id);
        }


        [Fact]
        public async Task BulkCreate_Test()
        {
            //Act
            var beijing = new Region
            {
                Name = "beijing"
            };

            var xicheng = new Region
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            beijing.Children = new List<Region>
            {
                xicheng,
                dongcheng
            };

            await _generalRegionTreeManager.BulkCreateAsync(beijing);

            //Assert
            var bj = GetRegion("beijing");
            bj.ShouldNotBeNull();
            bj.Name.ShouldBe("beijing");
            bj.FullName.ShouldBe("beijing");
            bj.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1));
            bj.Level.ShouldBe(1);
            bj.ParentId.ShouldBeNull();

            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task CheckSameNameExpression_Test()
        {
            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin())
            {
                var repository = LocalIocManager.Resolve<IRepository<Region, long>>();
                var config = new GeneralTreeConfiguration<Region, long>
                {
                    CheckSameNameExpression = (regionThis, regionCheck) =>
                        regionThis.SomeForeignKey == regionCheck.SomeForeignKey
                };

                var codeGenerate = new GeneralTreeCodeGenerate(new GeneralTreeCodeGenerateConfiguration());

                var manager =
                    new GeneralTreeManager<Region, long>(codeGenerate, repository, config);

                //Act
                await manager.CreateAsync(new Region
                {
                    Name = "beijing",
                    SomeForeignKey = 1
                });
                uowManager.Current.SaveChanges();

                //Act
                await manager.CreateAsync(new Region
                {
                    Name = "beijing",
                    SomeForeignKey = 2
                });
                uowManager.Current.SaveChanges();

                //Assert
                var beijing1 = repository.FirstOrDefault(x => x.Name == "beijing" && x.SomeForeignKey == 1);
                beijing1.ShouldNotBeNull();

                var beijing2 = repository.FirstOrDefault(x => x.Name == "beijing" && x.SomeForeignKey == 2);
                beijing2.ShouldNotBeNull();

                uow.Complete();
            }
        }

        [Fact]
        public async Task Create_Children_Memory_Test()
        {
            //Act
            var beijing = new Region
            {
                Name = "beijing"
            };
            await _generalRegionTreeManager.CreateAsync(beijing);

            var xicheng = new Region
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            await _generalRegionTreeManager.CreateChildrenAsync(beijing, new List<Region>
            {
                xicheng,
                dongcheng
            });

            //Assert
            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task Create_Children_Test()
        {
            //Act
            var beijing = new Region
            {
                Name = "beijing"
            };
            await _generalRegionTreeManager.CreateAsync(beijing);

            var xicheng = new Region
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };
            await _generalRegionTreeManager.CreateAsync(xicheng);

            var dongcheng = new Region
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };
            await _generalRegionTreeManager.CreateAsync(dongcheng);

            //Assert
            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task Create_Should_Not_With_Same_Name_Test()
        {
            //Act
            await _generalRegionTreeManager.CreateAsync(new Region
            {
                Name = "beijing"
            });

            //Assert
            var exception = await Record.ExceptionAsync(async () => await _generalRegionTreeManager.CreateAsync(
                new Region
                {
                    Name = "beijing"
                }
            ));

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<UserFriendlyException>();
            exception.Message.ShouldBe(
                "There is already an tree with name beijing. Two tree with same name can not be created in same level.");
        }

        [Fact]
        public async Task Create_Test()
        {
            //Act
            var beijing = new Region
            {
                Name = "beijing"
            };
            await _generalRegionTreeManager.CreateAsync(beijing);

            //Assert
            var xc = GetRegion("beijing");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("beijing");
            xc.FullName.ShouldBe("beijing");
            xc.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1));
            xc.Level.ShouldBe(1);
            xc.ParentId.ShouldBeNull();
        }

        [Fact]
        public async Task Delete_Test()
        {
            //Act
            var hebei = await CreateRegion("hebei");

            await CreateRegion("shijiazhuang", hebei.Id);

            var chengde = await CreateRegion("chengde", hebei.Id);
            await CreateRegion("shaungqiao", chengde.Id);
            await CreateRegion("shaungluan", chengde.Id);

            await _generalRegionTreeManager.DeleteAsync(hebei.Id);

            //Assert
            var hb = GetRegion("hebei");
            hb.ShouldBeNull();

            var sjz = GetRegion("shijiazhuang");
            sjz.ShouldBeNull();

            var cd = GetRegion("chengde");
            cd.ShouldBeNull();

            var cdsq = GetRegion("shaungqiao");
            cdsq.ShouldBeNull();
        }

        [Fact]
        public async Task ExceptionMessageFactory_Test()
        {
            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin())
            {
                var repository = LocalIocManager.Resolve<IRepository<Region, long>>();
                var config = new GeneralTreeConfiguration<Region, long>
                {
                    ExceptionMessageFactory =
                        tree => $"{tree.Name}已经存在"
                };

                var codeGenerate = new GeneralTreeCodeGenerate(new GeneralTreeCodeGenerateConfiguration());

                var manager =
                    new GeneralTreeManager<Region, long>(codeGenerate, repository, config);

                //Act
                await manager.CreateAsync(new Region
                {
                    Name = "beijing"
                });
                uowManager.Current.SaveChanges();

                //Assert
                var exception = await Record.ExceptionAsync(async () => await manager.CreateAsync(
                    new Region
                    {
                        Name = "beijing"
                    }
                ));

                exception.ShouldNotBeNull();
                exception.ShouldBeOfType<UserFriendlyException>();
                exception.Message.ShouldBe("beijing已经存在");

                uow.Complete();
            }
        }

        [Fact]
        public async Task FillUp_Test()
        {
            //Act
            var beijing = new Region
            {
                Name = "beijing"
            };

            var xicheng = new Region
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            var balizhuang = new Region
            {
                Name = "balizhuang",
                ParentId = dongcheng.Id
            };
            dongcheng.Children = new List<Region>
            {
                balizhuang
            };

            beijing.Children = new List<Region>
            {
                xicheng,
                dongcheng
            };

            await _generalRegionTreeManager.FillUpAsync(beijing);

            //Assert
            beijing.FullName.ShouldBe("beijing");
            beijing.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1));
            beijing.Level.ShouldBe(1);
            beijing.ParentId.ShouldBeNull();
            beijing.Children.Count.ShouldBe(2);

            xicheng.FullName.ShouldBe("beijing-xicheng");
            xicheng.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 1));
            xicheng.Level.ShouldBe(beijing.Level + 1);
            xicheng.ParentId.ShouldBe(beijing.Id);

            dongcheng.FullName.ShouldBe("beijing-dongcheng");
            dongcheng.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2));
            dongcheng.Level.ShouldBe(beijing.Level + 1);
            dongcheng.ParentId.ShouldBe(beijing.Id);

            balizhuang.FullName.ShouldBe("beijing-dongcheng-balizhuang");
            balizhuang.Code.ShouldBe(_generalTreeCodeGenerate.CreateCode(1, 2, 1));
            balizhuang.Level.ShouldBe(dongcheng.Level + 1);
            balizhuang.ParentId.ShouldBe(dongcheng.Id);
        }

        [Fact]
        public async Task FullName_Hyphen_Test()
        {
            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin())
            {
                var repository = LocalIocManager.Resolve<IRepository<Region, long>>();
                var config = new GeneralTreeConfiguration<Region, long>
                {
                    Hyphen = "->"
                };

                var codeGenerate = new GeneralTreeCodeGenerate(new GeneralTreeCodeGenerateConfiguration());

                var manager =
                    new GeneralTreeManager<Region, long>(codeGenerate, repository, config);

                //Act
                var beijing = new Region
                {
                    Name = "beijing"
                };
                await manager.CreateAsync(beijing);
                uowManager.Current.SaveChanges();

                var xicheng = new Region
                {
                    Name = "xicheng",
                    ParentId = beijing.Id
                };
                await manager.CreateAsync(xicheng);
                uowManager.Current.SaveChanges();

                //Assert
                var xc = GetRegion("xicheng");
                xc.ShouldNotBeNull();
                xc.Name.ShouldBe("xicheng");
                xc.FullName.ShouldBe("beijing->xicheng");

                uow.Complete();
            }
        }

        [Fact]
        public async Task Move_ChildrenAction_Test()
        {
            //Act
            var beijing = await CreateRegion("beijing");
            await CreateRegion("dongcheng", beijing.Id);
            await CreateRegion("xicheng", beijing.Id);

            var hebei = await CreateRegion("hebei");
            await CreateRegion("shijiazhuang", hebei.Id);
            var chengde = await CreateRegion("chengde", hebei.Id);

            await CreateRegion("shaungqiao", chengde.Id);
            await CreateRegion("shaungluan", chengde.Id);

            var beijingLastChild = GetRegion("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id, x => { x.MyCustomData = x.Code; });

            //Assert
            var shaungqiao = GetRegion("shaungqiao");
            shaungqiao.ShouldNotBeNull();
            shaungqiao.MyCustomData.ShouldBe(shaungqiao.Code);
        }

        [Fact]
        public async Task Move_Root_Parent_Test()
        {
            //Act
            var beijing = await CreateRegion("beijing");
            await CreateRegion("dongcheng", beijing.Id);
            await CreateRegion("xicheng", beijing.Id);

            var hebei = await CreateRegion("hebei");
            await CreateRegion("shijiazhuang", hebei.Id);
            var chengde = await CreateRegion("chengde", hebei.Id);

            var shuangqiao = await CreateRegion("shaungqiao", chengde.Id);
            await CreateRegion("shaungluan", chengde.Id);

            var beijingLastChild = GetRegion("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegionTreeManager.MoveAsync(shuangqiao.Id, null);

            //Assert
            var shaungqiao = GetRegion("shaungqiao");
            shaungqiao.ShouldNotBeNull();
            shaungqiao.FullName.ShouldBe("shaungqiao");
            shaungqiao.ParentId.ShouldBe(null);
            shaungqiao.Level.ShouldBe(1);
            shaungqiao.Code.ShouldBe(_generalTreeCodeGenerate.GetNextCode(hebei.Code));
        }

        [Fact]
        public async Task Move_Test()
        {
            //Act
            var beijing = await CreateRegion("beijing");
            await CreateRegion("dongcheng", beijing.Id);
            await CreateRegion("xicheng", beijing.Id);

            var hebei = await CreateRegion("hebei");
            await CreateRegion("shijiazhuang", hebei.Id);
            var chengde = await CreateRegion("chengde", hebei.Id);
            chengde.Code.ShouldBe("00002.00002");

            var shaungqiao = await CreateRegion("va", chengde.Id);
            shaungqiao.Code.ShouldBe("00002.00002.00001");
            var shaungluan = await CreateRegion("shaungluan", chengde.Id);
            shaungluan.Code.ShouldBe("00002.00002.00002");

            var bazhong = await CreateRegion("bazhong", shaungluan.Id);
            bazhong.Code.ShouldBe("00002.00002.00002.00001");

            var beijingLastChild = GetRegion("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id);

            //Assert
            var cd = GetRegion(chengde.Name);
            cd.ShouldNotBeNull();
            cd.FullName.ShouldBe(beijing.FullName + "-" + chengde.Name);
            cd.ParentId.ShouldBe(beijing.Id);
            cd.Level.ShouldBe(beijing.Level + 1);
            var cdCode = _generalTreeCodeGenerate.GetNextCode(beijingLastChild.Code);//00001.00003
            cd.Code.ShouldBe(cdCode);

            shaungqiao = GetRegion(shaungqiao.Name);
            shaungqiao.Code.ShouldBe("00001.00003.00001");

            shaungluan = GetRegion(shaungluan.Name);
            shaungluan.Code.ShouldBe("00001.00003.00002");

            bazhong = GetRegion(bazhong.Name);
            bazhong.Code.ShouldBe("00001.00003.00002.00001");
        }

        [Fact]
        public async Task Move_Index_Test()
        {
            //Act
            var beijing = await CreateRegion("beijing");
            var dongcheng = await CreateRegion("dongcheng", beijing.Id);
            dongcheng.Code.ShouldBe("00001.00001");
            var xicheng = await CreateRegion("xicheng", beijing.Id);
            xicheng.Code.ShouldBe("00001.00002");

            var hebei = await CreateRegion("hebei");
            await CreateRegion("shijiazhuang", hebei.Id);
            var chengde = await CreateRegion("chengde", hebei.Id);
            chengde.Code.ShouldBe("00002.00002");

            var shaungqiao = await CreateRegion("va", chengde.Id);
            shaungqiao.Code.ShouldBe("00002.00002.00001");
            var shaungluan = await CreateRegion("shaungluan", chengde.Id);
            shaungluan.Code.ShouldBe("00002.00002.00002");

            var bazhong = await CreateRegion("bazhong", shaungluan.Id);
            bazhong.Code.ShouldBe("00002.00002.00002.00001");

            var beijingLastChild = GetRegion("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id, index: 0);

            //Assert
            var cd = GetRegion(chengde.Name);
            cd.ShouldNotBeNull();
            cd.FullName.ShouldBe(beijing.FullName + "-" + chengde.Name);
            cd.ParentId.ShouldBe(beijing.Id);
            cd.Level.ShouldBe(beijing.Level + 1);
            cd.Code.ShouldBe("00001.00001");

            shaungqiao = GetRegion(shaungqiao.Name);
            shaungqiao.Code.ShouldBe("00001.00001.00001");

            var dc = GetRegion(dongcheng.Name);
            dc.Code.ShouldBe("00001.00002");

            var xc = GetRegion(xicheng.Name);
            xc.Code.ShouldBe("00001.00003");

            //Move and check children
            await _generalRegionTreeManager.MoveAsync(shaungqiao.Id, beijing.Id, index: 0);
            cd = GetRegion(chengde.Name);
            cd.Code.ShouldBe("00001.00002");
            shaungqiao = GetRegion(shaungqiao.Name);
            shaungqiao.Code.ShouldBe("00001.00001");

            //Only change index
            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id, index: 2);// Move cd from index 0 to index 2
            cd = GetRegion(chengde.Name);
            cd.Code.ShouldBe("00001.00003");

            shaungqiao = GetRegion(shaungqiao.Name);
            shaungqiao.Code.ShouldBe("00001.00001");

            dc = GetRegion(dongcheng.Name);
            dc.Code.ShouldBe("00001.00002");

            xc = GetRegion(xicheng.Name);
            xc.Code.ShouldBe("00001.00004");

            shaungluan = GetRegion(shaungluan.Name);
            shaungluan.Code.ShouldBe("00001.00003.00002");

            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id, index: 20);// Move cd to last
            cd = GetRegion(chengde.Name);
            cd.Code.ShouldBe("00001.00004");
        }

        [Fact]
        public async Task Update_ChildrenAction_Test()
        {
            await UsingDbContext(async context =>
            {
                context.Region.Add(new Region
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1,
                    MyCustomData = "beijing"
                });
                await context.SaveChangesAsync();

                context.Region.Add(new Region
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = 1
                });
                await context.SaveChangesAsync();
            });

            //Act
            var beijing = GetRegion("beijing");
            beijing.Name = "newbeijing";
            await _generalRegionTreeManager.UpdateAsync(beijing, x => { x.MyCustomData = x.Code; });

            //Assert
            var xicheng = GetRegion("xicheng");
            xicheng.FullName.ShouldBe("newbeijing-xicheng");
            xicheng.MyCustomData.ShouldBe("00001.00001");
        }

        [Fact]
        public async Task Update_Name_Child_FullName_ShouldBe_Update_Test()
        {
            await UsingDbContextAsync(async context =>
            {
                //Arrange
                context.Region.Add(new Region
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                });
                await context.SaveChangesAsync();

                context.Region.Add(new Region
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = 1
                });
                await context.SaveChangesAsync();
            });

            //Act
            var beijing = GetRegion("beijing");
            beijing.Name = "newbeijing";
            await _generalRegionTreeManager.UpdateAsync(beijing);

            //Assert
            var xicheng = GetRegion("xicheng");
            xicheng.FullName.ShouldBe("newbeijing-xicheng");
        }

        [Fact]
        public async Task Update_Name_FullName_ShouldBe_Update_With_Parent_FullName_Test()
        {
            await UsingDbContextAsync(async context =>
            {
                //Arrange
                context.Region.Add(new Region
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                });
                await context.SaveChangesAsync();

                context.Region.Add(new Region
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = 1
                });
                await context.SaveChangesAsync();

                //Act
                var xicheng = context.Region.First(x => x.Name == "xicheng");
                xicheng.Name = "newxicheng";
                await _generalRegionTreeManager.UpdateAsync(xicheng);
            });

            //Assert
            var newxicheng = GetRegion("newxicheng");
            newxicheng.FullName.ShouldBe("beijing-newxicheng");
        }

        [Fact]
        public async Task Update_Test()
        {
            //Arrange
            await UsingDbContext(async context =>
            {
                context.Region.Add(new Region
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                });
                await context.SaveChangesAsync();

                //Act
                var beijing = context.Region.First(x => x.Name == "beijing");
                beijing.Name = "newbeijing";
                await _generalRegionTreeManager.UpdateAsync(beijing);
                await context.SaveChangesAsync();

                //Assert
                var newbeijing = context.Region.First(x => x.Name == "newbeijing");
                newbeijing.ShouldNotBeNull();
                newbeijing.Name.ShouldBe("newbeijing");
                newbeijing.FullName.ShouldBe("newbeijing");
                newbeijing.Code.ShouldBe("00001");
            });
        }
    }
}