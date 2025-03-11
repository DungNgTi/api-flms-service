﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api_flms_service.Migrations
{
    /// <inheritdoc />
    public partial class LowercaseNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    mobile = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "authors",
                columns: table => new
                {
                    authorid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    biography = table.Column<string>(type: "text", nullable: true),
                    cloudinaryid = table.Column<string>(type: "text", nullable: true),
                    countryoforigin = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.authorid);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    categoryid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoryname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.categoryid);
                });

            migrationBuilder.CreateTable(
                name: "issuedbooks",
                columns: table => new
                {
                    sno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bookno = table.Column<string>(type: "text", nullable: true),
                    bookname = table.Column<string>(type: "text", nullable: true),
                    bookauthor = table.Column<string>(type: "text", nullable: true),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    issuedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issuedbooks", x => x.sno);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    phonenumber = table.Column<string>(type: "text", nullable: true),
                    registrationdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    bookid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    authorid = table.Column<int>(type: "integer", nullable: false),
                    availablecopies = table.Column<int>(type: "integer", nullable: false),
                    bookdescription = table.Column<string>(type: "text", nullable: true),
                    cloudinaryimageid = table.Column<string>(type: "text", nullable: false),
                    isbn = table.Column<string>(type: "text", nullable: true),
                    publicationyear = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.bookid);
                    table.ForeignKey(
                        name: "FK_books_authors_authorid",
                        column: x => x.authorid,
                        principalTable: "authors",
                        principalColumn: "authorid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookcategories",
                columns: table => new
                {
                    bookid = table.Column<int>(type: "integer", nullable: false),
                    categoryid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookcategories", x => new { x.bookid, x.categoryid });
                    table.ForeignKey(
                        name: "FK_bookcategories_books_bookid",
                        column: x => x.bookid,
                        principalTable: "books",
                        principalColumn: "bookid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookcategories_categories_categoryid",
                        column: x => x.categoryid,
                        principalTable: "categories",
                        principalColumn: "categoryid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookcategory",
                columns: table => new
                {
                    booksbookid = table.Column<int>(type: "integer", nullable: false),
                    categoriescategoryid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookcategory", x => new { x.booksbookid, x.categoriescategoryid });
                    table.ForeignKey(
                        name: "FK_bookcategory_books_booksbookid",
                        column: x => x.booksbookid,
                        principalTable: "books",
                        principalColumn: "bookid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookcategory_categories_categoriescategoryid",
                        column: x => x.categoriescategoryid,
                        principalTable: "categories",
                        principalColumn: "categoryid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookloans",
                columns: table => new
                {
                    bookloanid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bookid = table.Column<int>(type: "integer", nullable: false),
                    loandate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returndate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookloans", x => x.bookloanid);
                    table.ForeignKey(
                        name: "FK_bookloans_books_bookid",
                        column: x => x.bookid,
                        principalTable: "books",
                        principalColumn: "bookid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookloans_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookreviews",
                columns: table => new
                {
                    reviewid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bookid = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    reviewdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewtext = table.Column<string>(type: "text", nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    userid1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookreviews", x => x.reviewid);
                    table.ForeignKey(
                        name: "FK_bookreviews_books_bookid",
                        column: x => x.bookid,
                        principalTable: "books",
                        principalColumn: "bookid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookreviews_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookreviews_users_userid1",
                        column: x => x.userid1,
                        principalTable: "users",
                        principalColumn: "userid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookcategories_categoryid",
                table: "bookcategories",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_bookcategory_categoriescategoryid",
                table: "bookcategory",
                column: "categoriescategoryid");

            migrationBuilder.CreateIndex(
                name: "IX_bookloans_bookid",
                table: "bookloans",
                column: "bookid");

            migrationBuilder.CreateIndex(
                name: "IX_bookloans_userid",
                table: "bookloans",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_bookreviews_bookid",
                table: "bookreviews",
                column: "bookid");

            migrationBuilder.CreateIndex(
                name: "IX_bookreviews_userid",
                table: "bookreviews",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_bookreviews_userid1",
                table: "bookreviews",
                column: "userid1");

            migrationBuilder.CreateIndex(
                name: "IX_books_authorid",
                table: "books",
                column: "authorid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "bookcategories");

            migrationBuilder.DropTable(
                name: "bookcategory");

            migrationBuilder.DropTable(
                name: "bookloans");

            migrationBuilder.DropTable(
                name: "bookreviews");

            migrationBuilder.DropTable(
                name: "issuedbooks");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "authors");
        }
    }
}
