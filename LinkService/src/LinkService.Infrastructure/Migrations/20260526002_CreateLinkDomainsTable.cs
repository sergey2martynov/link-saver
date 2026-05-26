using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260526002)]
public class CreateLinkDomainsTable : Migration
{
    public override void Up()
    {
        Create.Table("link_domains")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("domain").AsString().NotNullable().Unique()
            .WithColumn("tags").AsCustom("text[]").NotNullable().WithDefaultValue("{}");
    }

    public override void Down()
    {
        Delete.Table("link_domains");
    }
}
