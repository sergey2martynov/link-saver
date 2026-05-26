using FluentMigrator;

namespace LinkService.Infrastructure.Migrations;

[Migration(20260518002)]
public class AddUserIdToLinks : Migration
{
    public override void Up()
    {
        Alter.Table("links")
            .AddColumn("user_id").AsGuid().NotNullable().WithDefaultValue(Guid.Empty);
    }

    public override void Down()
    {
        Delete.Column("user_id").FromTable("links");
    }
}
