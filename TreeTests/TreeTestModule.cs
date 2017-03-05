using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.GeneralTree;
using Abp.Modules;
using TreeApplication;

namespace TreeTests
{
    [DependsOn(typeof(TreeAppModule))]
    public class TreeTestModule : AbpModule
    {

    }
}
