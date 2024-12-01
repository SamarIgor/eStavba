using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eStavba.Data.Migrations
{
    public partial class AddElectionIdForeignKey2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Elections_ElectionModelId",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "ElectionId",
                table: "Candidate");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionModelId",
                table: "Candidate",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Elections_ElectionModelId",
                table: "Candidate",
                column: "ElectionModelId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Elections_ElectionModelId",
                table: "Candidate");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionModelId",
                table: "Candidate",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ElectionId",
                table: "Candidate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Elections_ElectionModelId",
                table: "Candidate",
                column: "ElectionModelId",
                principalTable: "Elections",
                principalColumn: "Id");
        }
    }
}
