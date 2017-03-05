using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using TreeApplication;
using Xunit;

namespace TreeTests
{
    public class TreeAppService_Tests : TreeTestBase
    {
        private readonly TreeAppService _treeAppService;

        public TreeAppService_Tests()
        {
            //Creating the class which is tested (SUT - Software Under Test)
            _treeAppService = LocalIocManager.Resolve<TreeAppService>();
        }

        [Fact]
        public void Should_Create_New_Tasks()
        {
            //Prepare for test
            var initialTaskCount = UsingDbContext(context => context.Region.Count());

            //Run SUT
            _treeAppService.GetAll();

            //Check results
            UsingDbContext(context =>
            {

            });
        }

    }
}
