using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContributionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalContributions",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Transactions",
                newName: "DebitAccountId");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Accounts",
                newName: "TotalContributions");

            migrationBuilder.AddColumn<string>(
                name: "CreditAccountBankCode",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreditAccountId",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DebitAccountBankCode",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BenefitType",
                table: "BenefitEligibilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditAccountBankCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreditAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DebitAccountBankCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BenefitType",
                table: "BenefitEligibilities");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "DebitAccountId",
                table: "Transactions",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "TotalContributions",
                table: "Accounts",
                newName: "Balance");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalContributions",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
