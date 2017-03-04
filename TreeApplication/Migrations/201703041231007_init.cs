namespace TreeApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        FullName = c.String(),
                        Code = c.String(),
                        Level = c.Int(nullable: false),
                        ParentId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Regions", t => t.ParentId)
                .Index(t => t.ParentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Regions", "ParentId", "dbo.Regions");
            DropIndex("dbo.Regions", new[] { "ParentId" });
            DropTable("dbo.Regions");
        }
    }
}
