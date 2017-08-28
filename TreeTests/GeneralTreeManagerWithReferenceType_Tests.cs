using System.Linq;
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
            _generalRegion2TreeManager = LocalIocManager
                .Resolve<IGeneralTreeManagerWithReferenceType<Region2, string>>();
        }

        private readonly IGeneralTreeManagerWithReferenceType<Region2, string> _generalRegion2TreeManager;

        private Region2 GetOrganization(string name)
        {
            return UsingDbContext(context =>
            {
                var region = context.Region2.FirstOrDefault(x => x.Name == name);
                //region.ShouldNotBeNull();
                return region;
            });
        }

        private async Task<Region2> CreateOrganization(string name, string parentId = null)
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
            var xc = GetOrganization("xicheng");
            xc.ShouldNotBeNull();
            xc.Name.ShouldBe("xicheng");
            xc.FullName.ShouldBe("beijing-xicheng");
            xc.Code.ShouldBe(GeneralTreeCodeGenerate.CreateCode(1, 1));
            xc.Level.ShouldBe(beijing.Level + 1);
            xc.ParentId.ShouldBe(beijing.Id);

            var dc = GetOrganization("dongcheng");
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
            var hebei = await CreateOrganization("hebei");

            await CreateOrganization("shijiazhuang", hebei.Id);

            var chengde = await CreateOrganization("chengde", hebei.Id);
            await CreateOrganization("shaungqiao", chengde.Id);
            await CreateOrganization("shaungluan", chengde.Id);

            await _generalRegion2TreeManager.DeleteAsync(hebei.Id);

            //Assert
            var hb = GetOrganization("hebei");
            hb.ShouldBeNull();

            var sjz = GetOrganization("shijiazhuang");
            sjz.ShouldBeNull();

            var cd = GetOrganization("chengde");
            cd.ShouldBeNull();

            var cdsq = GetOrganization("shaungqiao");
            cdsq.ShouldBeNull();
        }

        [Fact]
        public async Task Move_Test()
        {
            //Act
            var beijing = await CreateOrganization("beijing");
            await CreateOrganization("dongcheng", beijing.Id);
            await CreateOrganization("xicheng", beijing.Id);

            var hebei = await CreateOrganization("hebei");
            await CreateOrganization("shijiazhuang", hebei.Id);
            var chengde = await CreateOrganization("chengde", hebei.Id);

            await CreateOrganization("shaungqiao", chengde.Id);
            await CreateOrganization("shaungluan", chengde.Id);

            var beijingLastChild = GetOrganization("xicheng");
            beijingLastChild.ShouldNotBeNull();
            await _generalRegion2TreeManager.MoveAsync(chengde.Id, beijing.Id);

            //Assert
            var cd = GetOrganization(chengde.Name);
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
    }
}