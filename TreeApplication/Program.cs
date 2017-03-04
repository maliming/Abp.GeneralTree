using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Castle.Facilities.Logging;

namespace TreeApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //Bootstrapping ABP system
            using (var bootstrapper = AbpBootstrapper.Create<TreeModule>())
            {

                bootstrapper.Initialize();

                //Getting a treeApp object from DI and running it
                using (var treeApp = bootstrapper.IocManager.ResolveAsDisposable<TreeAppService>())
                {
                    using (var uow = bootstrapper.IocManager.Resolve<IUnitOfWorkManager>().Begin())
                    {
                        treeApp.Object.Create();

                        var all = treeApp.Object.GetAll();

                        //not commit
                        //uow.Complete();
                    }
                    
                } //Disposes treeApp and all it's dependencies

                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
