using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.GeneralTree;
using Abp.GeneralTree.GeneralTree;
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

                //Act
                var beijing = context.Region2.First(x => x.Name == "beijing");
                beijing.Name = "newbeijing";
                await _generalRegion2TreeManager.UpdateAsync(beijing);
                await context.SaveChangesAsync();
            });

            UsingDbContext(context =>
            {
                //Assert
                var xicheng = context.Region2.First(x => x.Name == "xicheng");
                xicheng.FullName.ShouldBe("newbeijing-xicheng");
            });
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
                await context.SaveChangesAsync();
            });

            UsingDbContext(context =>
            {
                //Assert
                var xicheng = context.Region2.First(x => x.Name == "newxicheng");
                xicheng.FullName.ShouldBe("beijing-newxicheng");
            });
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
                        Name = "shuangqiaoqu",
                    },
                    new Region2
                    {
                        Name = "shuangluanqu",
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
    }
}
