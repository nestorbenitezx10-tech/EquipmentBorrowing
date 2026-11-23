using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignActivity4Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowedDate",
                table: "Borrowings");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Students",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "AssetTag",
                table: "Equipment",
                newName: "Identifier");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Students",
                newName: "IsEligible");

            migrationBuilder.RenameColumn(
                name: "ReturnDate",
                table: "Borrowings",
                newName: "ActualReturnDate");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "Borrowings",
                newName: "CheckoutDate");

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Students",
                type: "TEXT",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentNumber",
                table: "Students",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Equipment",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierNormalized",
                table: "Equipment",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentNumberNormalized",
                table: "Students",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE Students SET Contact = Email, StudentNumber = TRIM(Id), " +
                "StudentNumberNormalized = UPPER(TRIM(Id));");
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Students");

            migrationBuilder.Sql(
                "UPDATE Equipment SET IdentifierNormalized = UPPER(TRIM(Identifier));");
            migrationBuilder.Sql(
                "UPDATE Borrowings SET " +
                "CheckoutDate = date(CheckoutDate), " +
                "ExpectedReturnDate = date(ExpectedReturnDate), " +
                "ActualReturnDate = date(ActualReturnDate), " +
                "Status = CASE WHEN Status = 3 THEN 1 ELSE 0 END;");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumberNormalized",
                table: "Students",
                column: "StudentNumberNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_IdentifierNormalized",
                table: "Equipment",
                column: "IdentifierNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_EquipmentId_Active",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId",
                table: "Borrowings",
                column: "StudentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Borrowings_ExpectedReturnDate",
                table: "Borrowings",
                sql: "\"ExpectedReturnDate\" > \"CheckoutDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Borrowings_Status",
                table: "Borrowings",
                sql: "\"Status\" IN (0, 1, 2, 3, 4, 5)");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Equipment_EquipmentId",
                table: "Borrowings",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Students_StudentId",
                table: "Borrowings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Equipment_EquipmentId",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Students_StudentId",
                table: "Borrowings");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentNumberNormalized",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_IdentifierNormalized",
                table: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Borrowings_EquipmentId_Active",
                table: "Borrowings");

            migrationBuilder.DropIndex(
                name: "IX_Borrowings_StudentId",
                table: "Borrowings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Borrowings_ExpectedReturnDate",
                table: "Borrowings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Borrowings_Status",
                table: "Borrowings");

            migrationBuilder.DropColumn(
                name: "StudentNumberNormalized",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IdentifierNormalized",
                table: "Equipment");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
            migrationBuilder.Sql("UPDATE Students SET Email = COALESCE(Contact, '');");
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Students",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Identifier",
                table: "Equipment",
                newName: "AssetTag");

            migrationBuilder.Sql(
                "UPDATE Borrowings SET " +
                "CheckoutDate = datetime(CheckoutDate), " +
                "ExpectedReturnDate = datetime(ExpectedReturnDate), " +
                "ActualReturnDate = datetime(ActualReturnDate), " +
                "Status = CASE Status " +
                "WHEN 1 THEN 3 WHEN 2 THEN 0 WHEN 3 THEN 1 " +
                "WHEN 4 THEN 2 WHEN 5 THEN 4 ELSE 0 END;");

            migrationBuilder.RenameColumn(
                name: "IsEligible",
                table: "Students",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CheckoutDate",
                table: "Borrowings",
                newName: "RequestDate");

            migrationBuilder.RenameColumn(
                name: "ActualReturnDate",
                table: "Borrowings",
                newName: "ReturnDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "BorrowedDate",
                table: "Borrowings",
                type: "TEXT",
                nullable: true);
        }
    }
}
