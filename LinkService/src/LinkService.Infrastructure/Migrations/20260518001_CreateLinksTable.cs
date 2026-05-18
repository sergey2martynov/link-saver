using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260518001)]
public class CreateLinksTable : Migration
{
    public override void Up()
    {
        Create.Table("links")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("url").AsString().NotNullable()
            .WithColumn("title").AsString().NotNullable()
            .WithColumn("description").AsString().Nullable()
            .WithColumn("tags").AsCustom("text[]").NotNullable().WithDefaultValue("{}")
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("links");
    }
}
