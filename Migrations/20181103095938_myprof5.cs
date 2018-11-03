using Microsoft.EntityFrameworkCore.Migrations;

namespace myprofile.Migrations
{
    public partial class myprof5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Use",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Use", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LP",
                columns: table => new
                {
                    LearningPlanId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LP", x => x.LearningPlanId);
                    table.ForeignKey(
                        name: "FK_LP_Use_UserId",
                        column: x => x.UserId,
                        principalTable: "Use",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LP_UserId",
                table: "LP",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LP");

            migrationBuilder.DropTable(
                name: "Use");
        }
    }
}
