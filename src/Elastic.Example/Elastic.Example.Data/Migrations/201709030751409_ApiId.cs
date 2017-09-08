namespace Elastic.Example.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApiId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actors", "ApiId", c => c.Int(nullable: false));
            AddColumn("dbo.Movies", "ApiId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "ApiId");
            DropColumn("dbo.Actors", "ApiId");
        }
    }
}
