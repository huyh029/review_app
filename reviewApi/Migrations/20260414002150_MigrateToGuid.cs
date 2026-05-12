using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ApplicableYears = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ApplicableMonths = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    DepartmentCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    DepartmentName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ParentId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FlowCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    FlowName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    Code = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationObjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    Code = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ApplicableYears = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ApplicableMonths = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Criteria = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    RoleCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    RoleName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    VirtualId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Abbreviation = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MinScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    MaxScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classifications_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    DisplayCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Content = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ScoreType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VirtualParentCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Criterias_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowCriterias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FlowId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowCriterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowCriterias_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowCriterias_EvaluationFlows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FlowId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowDepartments_EvaluationFlows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriaSetObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EvaluationObjectId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSetObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_EvaluationObjects_EvaluationObjectId",
                        column: x => x.EvaluationObjectId,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FlowId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    EvaluationObjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualParentCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowObjects_EvaluationFlows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowObjects_EvaluationObjects_EvaluationObjectId",
                        column: x => x.EvaluationObjectId,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportTypeCriterias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    ReportTypeId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTypeCriterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTypeCriterias_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportTypeCriterias_ReportTypes_ReportTypeId",
                        column: x => x.ReportTypeId,
                        principalTable: "ReportTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FlowId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    RoleId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualParentCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowRoles_EvaluationFlows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    FullName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RoleId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationObjectRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    EvaluationObjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationObjectRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationObjectRoles_EvaluationObjects_EvaluationObjectId",
                        column: x => x.EvaluationObjectId,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationObjectRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    Month = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Year = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ManagerId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CriteriaSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SelfScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ManagerScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    EvaluationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Content = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ReplyToCommentId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ReplyToCommentId",
                        column: x => x.ReplyToCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    EvaluationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VirtualCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    SelfScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ManagerScore = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationScores_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentAudios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CommentId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    AudioPath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AudioType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentAudios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentAudios_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CommentId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FileName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FileSize = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    FileType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FilePath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentFiles_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false, defaultValueSql: "SYS_GUID()"),
                    CommentId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Emoji = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReactions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_CriteriaSetId_Code",
                table: "Classifications",
                columns: new[] { "CriteriaSetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentAudios_CommentId",
                table: "CommentAudios",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentFiles_CommentId",
                table: "CommentFiles",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentId_UserId_Emoji",
                table: "CommentReactions",
                columns: new[] { "CommentId", "UserId", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_UserId",
                table: "CommentReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_EvaluationId_CreatedAt",
                table: "Comments",
                columns: new[] { "EvaluationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyToCommentId",
                table: "Comments",
                column: "ReplyToCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_CriteriaSetId_VirtualCode",
                table: "Criterias",
                columns: new[] { "CriteriaSetId", "VirtualCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_CriteriaSetId_EvaluationObjectId",
                table: "CriteriaSetObjects",
                columns: new[] { "CriteriaSetId", "EvaluationObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_EvaluationObjectId",
                table: "CriteriaSetObjects",
                column: "EvaluationObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentId",
                table: "Departments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowCriterias_CriteriaSetId",
                table: "EvaluationFlowCriterias",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowCriterias_FlowId_CriteriaSetId",
                table: "EvaluationFlowCriterias",
                columns: new[] { "FlowId", "CriteriaSetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowDepartments_DepartmentId",
                table: "EvaluationFlowDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowDepartments_FlowId_DepartmentId",
                table: "EvaluationFlowDepartments",
                columns: new[] { "FlowId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowObjects_EvaluationObjectId",
                table: "EvaluationFlowObjects",
                column: "EvaluationObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowObjects_FlowId_VirtualCode",
                table: "EvaluationFlowObjects",
                columns: new[] { "FlowId", "VirtualCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowRoles_FlowId_VirtualCode",
                table: "EvaluationFlowRoles",
                columns: new[] { "FlowId", "VirtualCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowRoles_RoleId",
                table: "EvaluationFlowRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlows_FlowCode",
                table: "EvaluationFlows",
                column: "FlowCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_EvaluationObjectId",
                table: "EvaluationObjectRoles",
                column: "EvaluationObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_UserId",
                table: "EvaluationObjectRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjects_Code",
                table: "EvaluationObjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CriteriaSetId",
                table: "Evaluations",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ManagerId",
                table: "Evaluations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_UserId_Year_Month",
                table: "Evaluations",
                columns: new[] { "UserId", "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationScores_EvaluationId_VirtualCode",
                table: "EvaluationScores",
                columns: new[] { "EvaluationId", "VirtualCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTypeCriterias_CriteriaSetId",
                table: "ReportTypeCriterias",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTypeCriterias_ReportTypeId_CriteriaSetId",
                table: "ReportTypeCriterias",
                columns: new[] { "ReportTypeId", "CriteriaSetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTypes_Code",
                table: "ReportTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleCode",
                table: "Roles",
                column: "RoleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Classifications");

            migrationBuilder.DropTable(
                name: "CommentAudios");

            migrationBuilder.DropTable(
                name: "CommentFiles");

            migrationBuilder.DropTable(
                name: "CommentReactions");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropTable(
                name: "CriteriaSetObjects");

            migrationBuilder.DropTable(
                name: "EvaluationFlowCriterias");

            migrationBuilder.DropTable(
                name: "EvaluationFlowDepartments");

            migrationBuilder.DropTable(
                name: "EvaluationFlowObjects");

            migrationBuilder.DropTable(
                name: "EvaluationFlowRoles");

            migrationBuilder.DropTable(
                name: "EvaluationObjectRoles");

            migrationBuilder.DropTable(
                name: "EvaluationScores");

            migrationBuilder.DropTable(
                name: "ReportTypeCriterias");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "EvaluationFlows");

            migrationBuilder.DropTable(
                name: "EvaluationObjects");

            migrationBuilder.DropTable(
                name: "ReportTypes");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "CriteriaSets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
