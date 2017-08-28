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