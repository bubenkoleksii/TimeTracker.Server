﻿using FluentMigrator;
using Microsoft.Extensions.Configuration;
using TimeTracker.Server.Data.Models.User;

namespace TimeTracker.Server.Data.Migrations;

[Migration(20230627112400)]
public class Migration_20230627112400 : Migration
{
    private readonly string _rootUserId;

    private readonly string _rootUserEmail;

    private readonly string _rootUserHashPassword;

    public Migration_20230627112400(IConfiguration configuration)
    {
        _rootUserId = configuration["RootUser:Id"];
        _rootUserEmail = configuration["RootUser:Email"];
        _rootUserHashPassword = configuration["RootUser:HashPassword"];
    }

    public override void Up()
    {
        Create.Table("User")
            .WithColumn(nameof(UserDataResponse.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(UserDataResponse.Email)).AsString(255).Unique().NotNullable()
            .WithColumn(nameof(UserDataResponse.HashPassword)).AsString(255).Nullable()
            .WithColumn(nameof(UserDataResponse.RefreshToken)).AsCustom("TEXT").Nullable()
            .WithColumn(nameof(UserDataResponse.FullName)).AsString(255).NotNullable()
            .WithColumn(nameof(UserDataResponse.Status)).AsString(255).NotNullable()
            .WithColumn(nameof(UserDataResponse.Permissions)).AsCustom("TEXT").Nullable()
            .WithColumn(nameof(UserDataResponse.EmploymentRate)).AsInt16().NotNullable()
            .WithColumn(nameof(UserDataResponse.EmploymentDate)).AsDateTime().NotNullable()
            .WithColumn(nameof(UserDataResponse.HasPassword)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(UserDataResponse.SetPasswordLink)).AsGuid().Nullable()
            .WithColumn(nameof(UserDataResponse.SetPasswordLinkExpired)).AsDateTime().Nullable();

        Insert.IntoTable("User").Row(new 
            {
                Id = Guid.Parse(_rootUserId),
                Email = _rootUserEmail,
                HashPassword = _rootUserHashPassword,
                HasPassword = true,
                FullName = "Admin Admin",
                EmploymentRate = 100,
                EmploymentDate = new DateTime(2023, 8, 7),
                Status = $"working",
                Permissions = "ALL"
            }
        );
    }

    public override void Down()
    {
        Delete.Table("User");
    }
}