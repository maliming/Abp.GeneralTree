using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp.GeneralTree;
using Abp.EntityFramework;
using Abp.Modules;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkModule), typeof(GeneralTreeModule))]
    public class TreeAppModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
