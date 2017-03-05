using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeApplication;

namespace TreeTests
{
    public class TreeTestInitialDataBuilder
    {
        private readonly TreeAppDbContext _context;

        public TreeTestInitialDataBuilder(TreeAppDbContext context)
        {
            _context = context;
        }

        public void Build(TreeAppDbContext context)
        {
           //init db
        }
    }
}
