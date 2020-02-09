using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BackgroundQueue.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageQueue",
                columns: table => new
                {
                    MessageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageType = table.Column<string>(nullable: true),
                    ErrorCount = table.Column<int>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    NextRunTime = table.Column<DateTime>(nullable: false),
                    LastRunTime = table.Column<DateTime>(nullable: false),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageQueue", x => x.MessageId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageQueue");
        }
    }
}
