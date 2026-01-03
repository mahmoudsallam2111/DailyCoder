using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyCoder.Api.Migrations;

/// <inheritdoc />
public partial class Update : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_user",
            schema: "dev",
            table: "user");

        migrationBuilder.RenameTable(
            name: "user",
            schema: "dev",
            newName: "users",
            newSchema: "dev");

        migrationBuilder.RenameIndex(
            name: "ix_user_identity_id",
            schema: "dev",
            table: "users",
            newName: "ix_users_identity_id");

        migrationBuilder.RenameIndex(
            name: "ix_user_email",
            schema: "dev",
            table: "users",
            newName: "ix_users_email");

        migrationBuilder.AddPrimaryKey(
            name: "pk_users",
            schema: "dev",
            table: "users",
            column: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_users",
            schema: "dev",
            table: "users");

        migrationBuilder.RenameTable(
            name: "users",
            schema: "dev",
            newName: "user",
            newSchema: "dev");

        migrationBuilder.RenameIndex(
            name: "ix_users_identity_id",
            schema: "dev",
            table: "user",
            newName: "ix_user_identity_id");

        migrationBuilder.RenameIndex(
            name: "ix_users_email",
            schema: "dev",
            table: "user",
            newName: "ix_user_email");

        migrationBuilder.AddPrimaryKey(
            name: "pk_user",
            schema: "dev",
            table: "user",
            column: "id");
    }
}
