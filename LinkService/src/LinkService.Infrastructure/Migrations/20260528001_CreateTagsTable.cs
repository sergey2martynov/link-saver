using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260528001)]
public class CreateTagsTable : Migration
{
    public override void Up()
    {
        Create.Table("tags")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("color").AsString(20).NotNullable()
            .WithColumn("start_date").AsDate().Nullable()
            .WithColumn("end_date").AsDate().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("ix_tags_user_id").OnTable("tags").OnColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table("tags");
    }
}
