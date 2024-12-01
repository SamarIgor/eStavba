using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eStavba.Data.Migrations
{
    public partial class AddElectionIdForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElectionId",
                table: "Candidate",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectionId",
                table: "Candidate");
        }
    }
}
