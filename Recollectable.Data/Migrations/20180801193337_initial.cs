﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Recollectable.Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectorValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    G4Value = table.Column<double>(nullable: true),
                    VG8Value = table.Column<double>(nullable: true),
                    F12Value = table.Column<double>(nullable: true),
                    VF20Value = table.Column<double>(nullable: true),
                    XF40Value = table.Column<double>(nullable: true),
                    AU50Value = table.Column<double>(nullable: true),
                    MS60Value = table.Column<double>(nullable: true),
                    MS63Value = table.Column<double>(nullable: true),
                    PF60Value = table.Column<double>(nullable: true),
                    PF63Value = table.Column<double>(nullable: true),
                    PF65Value = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectorValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Grade = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Collectables",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CountryId = table.Column<Guid>(nullable: false),
                    CollectorValueId = table.Column<Guid>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    FaceValue = table.Column<int>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    ReleaseDate = table.Column<int>(nullable: true),
                    Size = table.Column<string>(nullable: true),
                    ObverseDescription = table.Column<string>(nullable: true),
                    ReverseDescription = table.Column<string>(nullable: true),
                    FrontImagePath = table.Column<string>(nullable: true),
                    BackImagePath = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Watermark = table.Column<string>(nullable: true),
                    Signature = table.Column<string>(nullable: true),
                    Coin_FaceValue = table.Column<int>(nullable: true),
                    Coin_Type = table.Column<string>(nullable: true),
                    Coin_ReleaseDate = table.Column<int>(nullable: true),
                    Coin_Size = table.Column<string>(nullable: true),
                    Coin_ObverseDescription = table.Column<string>(nullable: true),
                    Coin_ReverseDescription = table.Column<string>(nullable: true),
                    Coin_FrontImagePath = table.Column<string>(nullable: true),
                    Coin_BackImagePath = table.Column<string>(nullable: true),
                    Mintage = table.Column<int>(nullable: true),
                    Weight = table.Column<string>(nullable: true),
                    Metal = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    ObverseLegend = table.Column<string>(nullable: true),
                    ReverseLegend = table.Column<string>(nullable: true),
                    EdgeType = table.Column<string>(nullable: true),
                    EdgeLegend = table.Column<string>(nullable: true),
                    Designer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collectables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collectables_CollectorValues_CollectorValueId",
                        column: x => x.CollectorValueId,
                        principalTable: "CollectorValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Collectables_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCollectable",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(nullable: false),
                    CollectableId = table.Column<Guid>(nullable: false),
                    ConditionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCollectable", x => new { x.CollectionId, x.CollectableId, x.ConditionId });
                    table.ForeignKey(
                        name: "FK_CollectionCollectable_Collectables_CollectableId",
                        column: x => x.CollectableId,
                        principalTable: "Collectables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionCollectable_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionCollectable_Conditions_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Conditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collectables_CollectorValueId",
                table: "Collectables",
                column: "CollectorValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectables_CountryId",
                table: "Collectables",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCollectable_CollectableId",
                table: "CollectionCollectable",
                column: "CollectableId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCollectable_ConditionId",
                table: "CollectionCollectable",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId",
                table: "Collections",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionCollectable");

            migrationBuilder.DropTable(
                name: "Collectables");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "CollectorValues");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
