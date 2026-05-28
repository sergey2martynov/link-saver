using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260528002)]
public class CreateLinkTagsTable : Migration
{
    public override void Up()
    {
        Create.Table("link_tags")
            .WithColumn("link_id").AsGuid().NotNullable().ForeignKey("links", "id").OnDelete(System.Data.Rule.Cascade)
            .WithColumn("tag_id").AsGuid().NotNullable().ForeignKey("tags", "id").OnDelete(System.Data.Rule.Cascade);

        Create.PrimaryKey("pk_link_tags").OnTable("link_tags").Columns("link_id", "tag_id");
    }

    public override void Down()
    {
        Delete.Table("link_tags");
    }
}
