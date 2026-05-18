using FluentMigrator;

namespace UserService.Infrastructure.Migrations;

[Migration(20260518001)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString().NotNullable().Unique()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}
