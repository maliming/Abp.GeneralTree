using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.GeneralTree;
using Abp.UI;
using Shouldly;
using TreeApplication;
using Xunit;

namespace TreeTests
{
    public class GeneralTreeManagerWithReferenceType_Tests : TreeTestBase
    {
        public GeneralTreeManagerWithReferenceType_Tests()
        {
            _generalRegion2TreeManager = Resolve<IGeneralTreeManagerWithReferenceType<Region2, string>>();
        }

        private readonly IGeneralTreeManagerWithReferenceType<Region2, string> _generalRegion2TreeManager;

        private Region2 GetRegion(string name)
        {
            return UsingDbContext(context =>
            {
                var region = context.Region2.FirstOrDefault(x => x.Name == name);
                //region.ShouldNotBeNull();
                return region;
            });
        }

        private async Task<Region2> CreateRegion(string name, string parentId = null)
        {
            var region = new Region2
            {
                Name = name,
                ParentId = parentId
            };
            await _generalRegion2TreeManager.CreateAsync(region);
            return region;
        }

        [Fact]
        public async Task BulkCreate_ExistTree_Test()
        {
            //Arrange
            var hebei = new Region2
            {
                Name = "hebei",
                FullName = "hebei",
                Code = "00001",
                Level = 1
            };

            await UsingDbContextAsync(async context =>
            {
                context.Region2.Add(hebei);
                await context.SaveChangesAsync();

                context.Region2.Add(new Region2
                {
                    Name = "shijiazhuang",
                    FullName = "hebei-shijiazhuang",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = hebei.Id
                });
                await context.SaveChangesAsync();
            });

            var chengde = new Region2
            {
                Name = "chengde",
                ParentId = hebei.Id,
                Children = new List<Region2>
                {
                    new Region2
                    {
                        Name = "shuangqiaoqu"
                    },
                    new Region2
                    {
                        Name = "shuangluanqu"
                    }
                }
            };

            await _generalRegion2TreeManager.BulkCreateAsync(chengde);

            //Assert
            chengde = GetRegion("chengde");
            chengde.ShouldNotBeNull();
            chengde.Name.ShouldBe("chengde");
            chengde.FullName.ShouldBe("hebei-chengde");
            chengde.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2));
            chengde.Level.ShouldBe(2);
            chengde.ParentId.ShouldBe(hebei.Id);

            var shuangqiaoqu = GetRegion("shuangqiaoqu");
            shuangqiaoqu.ShouldNotBeNull();
            shuangqiaoqu.Name.ShouldBe("shuangqiaoqu");
            shuangqiaoqu.FullName.ShouldBe("hebei-chengde-shuangqiaoqu");
            shuangqiaoqu.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2, 1));
            shuangqiaoqu.Level.ShouldBe(chengde.Level + 1);
            shuangqiaoqu.ParentId.ShouldBe(chengde.Id);

            var shuangluanqu = GetRegion("shuangluanqu");
            shuangluanqu.ShouldNotBeNull();
            shuangluanqu.Name.ShouldBe("shuangluanqu");
            shuangluanqu.FullName.ShouldBe("hebei-chengde-shuangluanqu");
            shuangluanqu.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2, 2));
            shuangluanqu.Level.ShouldBe(chengde.Level + 1);
            shuangluanqu.ParentId.ShouldBe(chengde.Id);
        }

        [Fact]
        public async Task BulkCreate_Test()
        {
            //Act
            var beijing = new Region2
            {
                Name = "beijing"
            };

            var xicheng = new Region2
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region2
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            beijing.Children = new List<Region2>
            {
                xicheng,
                dongcheng
            };

            await _generalRegion2TreeManager.BulkCreateAsync(beijing);

            //Assert
            var bj = GetRegion("beijing");
            bj.ShouldNotBeNull();
            bj.Name.ShouldBe("beijing");
            bj.FullName.ShouldBe("beijing");
            bj.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1));
            bj.Level.ShouldBe(1);
            bj.ParentId.ShouldBeNull();

            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task Create_Children_Test()
        {
            //Act
            var beijing = new Region2
            {
                Name = "beijing"
            };
            await _generalRegion2TreeManager.CreateAsync(beijing);

            var xicheng = new Region2
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };
            await _generalRegion2TreeManager.CreateAsync(xicheng);

            var dongcheng = new Region2
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };
            await _generalRegion2TreeManager.CreateAsync(dongcheng);

            //Assert
            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task Create_Children_Memory_Test()
        {
            //Act
            var beijing = new Region2
            {
                Name = "beijing"
            };
            await _generalRegion2TreeManager.CreateAsync(beijing);

            var bj = GetRegion("beijing");

            var xicheng = new Region2
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region2
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            await _generalRegion2TreeManager.CreateChildrenAsync(bj, new List<Region2>
            {
                xicheng,
                dongcheng
            });

            //Assert
            var xc = GetRegion("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetRegion("dongcheng");
            dc.ShouldNotBeNull();
            dc.Name.ShouldBe("dongcheng");
            dc.FullName.ShouldBe("beijing-dongcheng");
            dc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2));
            dc.Level.ShouldBe(beijing.Level + 1);
            dc.ParentId.ShouldBe(beijing.Id);
        }

        [Fact]
        public async Task Create_Should_Not_With_Same_Name_Test()
        {
            //Act
            await _generalRegion2TreeManager.CreateAsync(new Region2
            {
                Name = "beijing"
            });

            //Assert
            var exception = await Record.ExceptionAsync(async () => await _generalRegion2TreeManager.CreateAsync(
                new Region2
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
            var beijing = new Region2
            {
                Name = "beijing"
            };
            await _generalRegion2TreeManager.CreateAsync(beijing);

            //Assert
            var xc = GetRegion("beijing");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("beijing");
            xc.FullName.ShouldBe("beijing");
            xc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1));
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

            await _generalRegion2TreeManager.DeleteAsync(hebei.Id);

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
                var repository = LocalIocManager.Resolve<IRepository<Region2, string>>();
                var config = new GeneralTreeConfigurationWithReferenceType<Region2, string>
                {
                    ExceptionMessageFactory =
                        tree => $"{tree.Name}已经存在"
                };

                var manager =
                    new GeneralTreeManagerWithReferenceType<Region2, string>(repository, config);

                //Act
                await manager.CreateAsync(new Region2
                {
                    Name = "beijing"
                });
                uowManager.Current.SaveChanges();

                //Assert
                var exception = await Record.ExceptionAsync(async () => await manager.CreateAsync(
                    new Region2
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
            var beijing = new Region2
            {
                Name = "beijing"
            };

            var xicheng = new Region2
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };

            var dongcheng = new Region2
            {
                Name = "dongcheng",
                ParentId = beijing.Id
            };

            var balizhuang = new Region2
            {
                Name = "balizhuang",
                ParentId = dongcheng.Id
            };
            dongcheng.Children = new List<Region2>
            {
                balizhuang
            };

            beijing.Children = new List<Region2>
            {
                xicheng,
                dongcheng
            };

            await _generalRegion2TreeManager.FillUpAsync(beijing);

            //Assert
            beijing.FullName.ShouldBe("beijing");
            beijing.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1));
            beijing.Level.ShouldBe(1);
            beijing.ParentId.ShouldBeNull();
            beijing.Children.Count.ShouldBe(2);

            xicheng.FullName.ShouldBe("beijing-xicheng");
            xicheng.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 1));
            xicheng.Level.ShouldBe(beijing.Level + 1);
            xicheng.ParentId.ShouldBe(beijing.Id);

            dongcheng.FullName.ShouldBe("beijing-dongcheng");
            dongcheng.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2));
            dongcheng.Level.ShouldBe(beijing.Level + 1);
            dongcheng.ParentId.ShouldBe(beijing.Id);

            balizhuang.FullName.ShouldBe("beijing-dongcheng-balizhuang");
            balizhuang.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 2, 1));
            balizhuang.Level.ShouldBe(dongcheng.Level + 1);
            balizhuang.ParentId.ShouldBe(dongcheng.Id);
        }

        [Fact]
        public async Task FullName_Hyphen_Test()
        {
            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin())
            {
                var repository = LocalIocManager.Resolve<IRepository<Region2, string>>();
                var config = new GeneralTreeConfigurationWithReferenceType<Region2, string>
                {
                    Hyphen = "->"
                };

                var manager =
                    new GeneralTreeManagerWithReferenceType<Region2, string>(repository, config);

                //Act
                var beijing = new Region2
                {
                    Name = "beijing"
                };
                await manager.CreateAsync(beijing);
                uowManager.Current.SaveChanges();

                var xicheng = new Region2
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
            await _generalRegion2TreeManager.MoveAsync(chengde.Id, beijing.Id, x => { x.MyCustomData = x.Code; });

            //Assert
            var shaungqiao = GetRegion("shaungqiao");
            shaungqiao.ShouldNotBeNull();
            shaungqiao.MyCustomData.ShouldBe(shaungqiao.Code);
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

            await CreateRegion("shaungqiao", chengde.Id);
            await CreateRegion("shaungluan", chengde.Id);

            var beijingLastChild = GetRegion("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegion2TreeManager.MoveAsync(chengde.Id, beijing.Id);

            //Assert
            var cd = GetRegion(chengde.Name);
            cd.ShouldNotBeNull();
            cd.FullName.ShouldBe(beijing.FullName + "-" + chengde.Name);
            cd.ParentId.ShouldBe(beijing.Id);
            cd.Level.ShouldBe(beijing.Level + 1);
            cd.Code.ShouldBe(GeneralTreeCodeGenerate.GetNextCode(beijingLastChild.Code));
        }

        [Fact]
        public async Task Update_ChildrenAction_Test()
        {
            //Arrange
            await UsingDbContext(async context =>
            {
                context.Region2.Add(new Region2
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1,
                    MyCustomData = "beijing"
                });
                await context.SaveChangesAsync();

                context.Region2.Add(new Region2
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = "1"
                });
                await context.SaveChangesAsync();
            });

            //Act
            var beijing = GetRegion("beijing");
            beijing.Name = "newbeijing";
            await _generalRegion2TreeManager.UpdateAsync(beijing, x => { x.MyCustomData = x.Code; });

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
                var region = new Region2
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                };

                context.Region2.Add(region);
                await context.SaveChangesAsync();

                context.Region2.Add(new Region2
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = region.Id
                });
                await context.SaveChangesAsync();
            });

            //Act
            var beijing = GetRegion("beijing");
            beijing.Name = "newbeijing";
            await _generalRegion2TreeManager.UpdateAsync(beijing);

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
                var region = new Region2
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                };
                context.Region2.Add(region);
                await context.SaveChangesAsync();

                context.Region2.Add(new Region2
                {
                    Name = "xicheng",
                    FullName = "beijing-xicheng",
                    Code = "00001.00001",
                    Level = 2,
                    ParentId = region.Id
                });
                await context.SaveChangesAsync();

                //Act
                var xicheng = context.Region2.First(x => x.Name == "xicheng");
                xicheng.Name = "newxicheng";
                await _generalRegion2TreeManager.UpdateAsync(xicheng);
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
                context.Region2.Add(new Region2
                {
                    Name = "beijing",
                    FullName = "beijing",
                    Code = "00001",
                    Level = 1
                });
                await context.SaveChangesAsync();

                //Act
                var beijing = context.Region2.First(x => x.Name == "beijing");
                beijing.Name = "newbeijing";
                await _generalRegion2TreeManager.UpdateAsync(beijing);
                await context.SaveChangesAsync();

                //Assert
                var newbeijing = context.Region2.First(x => x.Name == "newbeijing");
                newbeijing.ShouldNotBeNull();
                newbeijing.Name.ShouldBe("newbeijing");
                newbeijing.FullName.ShouldBe("newbeijing");
            });
        }

        [Fact]
        public async Task CheckSameNameExpression_Test()
        {
            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin()) {
                var repository = LocalIocManager.Resolve<IRepository<Region2, string>>();
                var config = new GeneralTreeConfigurationWithReferenceType<Region2, string>
                {
                    CheckSameNameExpression = (regionThis, regionCheck) =>
                        regionThis.SomeForeignKey == regionCheck.SomeForeignKey
                };

                var manager =
                    new GeneralTreeManagerWithReferenceType<Region2, string>(repository, config);

                //Act
                await manager.CreateAsync(new Region2
                {
                    Name = "beijing",
                    SomeForeignKey = 1
                });
                uowManager.Current.SaveChanges();

                //Act
                await manager.CreateAsync(new Region2
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
    }
}