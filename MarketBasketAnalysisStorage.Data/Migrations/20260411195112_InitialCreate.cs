using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketBasketAnalysis.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssociationRuleSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TransactionCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationRuleSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssociationRuleChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<byte[]>(type: "smallint[]", nullable: false),
                    AssociationRuleSetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationRuleChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssociationRuleChunks_AssociationRuleSets_AssociationRuleSe~",
                        column: x => x.AssociationRuleSetId,
                        principalTable: "AssociationRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssociationRuleSetMetadatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastSavedItemChunkIndex = table.Column<int>(type: "integer", nullable: true),
                    LastItemChunkIndex = table.Column<int>(type: "integer", nullable: true),
                    LastSavedRuleChunkIndex = table.Column<int>(type: "integer", nullable: true),
                    LastRuleChunkIndex = table.Column<int>(type: "integer", nullable: true),
                    AssociationRuleSetId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationRuleSetMetadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssociationRuleSetMetadatas_AssociationRuleSets_Association~",
                        column: x => x.AssociationRuleSetId,
                        principalTable: "AssociationRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<byte[]>(type: "smallint[]", nullable: false),
                    AssociationRuleSetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemChunks_AssociationRuleSets_AssociationRuleSetId",
                        column: x => x.AssociationRuleSetId,
                        principalTable: "AssociationRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssociationRuleChunks_AssociationRuleSetId",
                table: "AssociationRuleChunks",
                column: "AssociationRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssociationRuleSetMetadatas_AssociationRuleSetId",
                table: "AssociationRuleSetMetadatas",
                column: "AssociationRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemChunks_AssociationRuleSetId",
                table: "ItemChunks",
                column: "AssociationRuleSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssociationRuleChunks");

            migrationBuilder.DropTable(
                name: "AssociationRuleSetMetadatas");

            migrationBuilder.DropTable(
                name: "ItemChunks");

            migrationBuilder.DropTable(
                name: "AssociationRuleSets");
        }
    }
}
