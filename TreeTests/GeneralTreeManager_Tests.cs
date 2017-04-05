using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.GeneralTree;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Shouldly;
using TreeApplication;
using Xunit;

namespace TreeTests
{
    public class GeneralTreeManager_Tests : TreeTestBase
    {
        private readonly GeneralTreeManager<Region> _generalRegionTreeManager;

        public GeneralTreeManager_Tests()
        {
            _generalRegionTreeManager = LocalIocManager.Resolve<GeneralTreeManager<Region>>();
        }

        [Fact]
        public async Task Create_Children_Test()
        {
            //Act
            var beijing = new Region()
            {
                Name = "beijing"
            };
            await _generalRegionTreeManager.CreateAsync(beijing);

            var xicheng = new Region()
            {
                Name = "xicheng",
                ParentId = beijing.Id
            };
            await _generalRegionTreeManager.CreateAsync(xicheng);

            var dongcheng = new Region()
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
            await _generalRegionTreeManager.CreateAsync(new Region()
            {
                Name = "beijing"
            });

            //Assert
            await Assert.ThrowsAsync<UserFriendlyException>(
                () => _generalRegionTreeManager.CreateAsync(
                    new Region()
                    {
                        Name = "beijing"
                    }
                )
            );
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
            });
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

                //Act
                var beijing = context.Region.First(x => x.Name == "beijing");
                beijing.Name = "newbeijing";
                await _generalRegionTreeManager.UpdateAsync(beijing);
                await context.SaveChangesAsync();
            });

            UsingDbContext(context =>
            {
                //Assert
                var xicheng = context.Region.First(x => x.Name == "xicheng");
                xicheng.FullName.ShouldBe("newbeijing-xicheng");
            });
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
                await context.SaveChangesAsync();
            });

            UsingDbContext(context =>
            {
                //Assert
                var xicheng = context.Region.First(x => x.Name == "newxicheng");
                xicheng.FullName.ShouldBe("beijing-newxicheng");
            });
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
            await _generalRegionTreeManager.MoveAsync(chengde.Id, beijing.Id);

            //Assert
            var cd = GetRegion(chengde.Name);
            cd.ShouldNotBeNull();
            cd.FullName.ShouldBe(beijing.FullName + "-" + chengde.Name);
            cd.ParentId.ShouldBe(beijing.Id);
            cd.Level.ShouldBe(beijing.Level + 1);
            cd.Code.ShouldBe(GeneralTreeCodeGenerate.GetNextCode(beijingLastChild.Code));
        }

        private Region GetRegion(string name)
        {
            return UsingDbContext(context =>
            {
                var region = context.Region.FirstOrDefault(x => x.Name == name);
                region.ShouldNotBeNull();
                return region;
            });
        }

        private async Task<Region> CreateRegion(string name, long? parentId = null)
        {
            var region = new Region()
            {
                Name = name,
                ParentId = parentId
            };
            await _generalRegionTreeManager.CreateAsync(region);
            return region;
        }
    }
}
