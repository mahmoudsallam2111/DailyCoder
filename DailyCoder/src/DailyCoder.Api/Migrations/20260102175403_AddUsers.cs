using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyCoder.Api.Migrations;

/// <inheritdoc />
public partial class AddUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user",
            schema: "dev",
            columns: table => new
            {
                id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                identity_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_email",
            schema: "dev",
            table: "user",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_identity_id",
            schema: "dev",
            table: "user",
            column: "identity_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user",
            schema: "dev");
    }
}
