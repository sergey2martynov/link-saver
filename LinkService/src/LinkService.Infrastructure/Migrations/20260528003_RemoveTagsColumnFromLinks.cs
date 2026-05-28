using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260528003)]
public class RemoveTagsColumnFromLinks : Migration
{
    public override void Up()
    {
        Delete.Column("tags").FromTable("links");
    }

    public override void Down()
    {
        Alter.Table("links")
            .AddColumn("tags").AsCustom("text[]").NotNullable().WithDefaultValue("{}");
    }
}
