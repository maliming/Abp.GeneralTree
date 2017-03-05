using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Modules;
using TreeApplication;

namespace TreeTests
{
    [DependsOn(typeof(TreeModule))]
    public class TreeTestModule : AbpModule
    {

    }
}
