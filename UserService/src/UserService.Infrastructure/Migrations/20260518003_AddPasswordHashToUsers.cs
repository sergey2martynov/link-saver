using FluentMigrator;

namespace UserService.Infrastructure.Migrations;

[Migration(20260518003)]
public class AddPasswordHashToUsers : Migration
{
    public override void Up() =>
        Alter.Table("users").AddColumn("password_hash").AsString(500).Nullable();

    public override void Down() =>
        Delete.Column("password_hash").FromTable("users");
}
