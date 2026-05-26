using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260526001)]
public class AddSuggestedTagsToLinks : Migration
{
    public override void Up()
    {
        Alter.Table("links")
            .AddColumn("suggested_tags").AsCustom("text[]").NotNullable().WithDefaultValue("{}");
    }

    public override void Down()
    {
        Delete.Column("suggested_tags").FromTable("links");
    }
}
