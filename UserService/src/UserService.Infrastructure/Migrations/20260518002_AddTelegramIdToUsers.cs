using FluentMigrator;

namespace UserService.Infrastructure.Migrations;

[Migration(20260518002)]
public class AddTelegramIdToUsers : Migration
{
    public override void Up()
    {
        Alter.Table("users").AlterColumn("email").AsString().Nullable();

        Alter.Table("users").AddColumn("telegram_id").AsInt64().Nullable();

        Execute.Sql("CREATE UNIQUE INDEX idx_users_telegram_id ON users(telegram_id) WHERE telegram_id IS NOT NULL");
    }

    public override void Down()
    {
        Execute.Sql("DROP INDEX IF EXISTS idx_users_telegram_id");
        Delete.Column("telegram_id").FromTable("users");
        Alter.Table("users").AlterColumn("email").AsString().NotNullable();
    }
}
