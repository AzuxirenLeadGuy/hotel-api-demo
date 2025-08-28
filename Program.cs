using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

using HotelManagement.Data;
using Microsoft.Extensions.DependencyInjection;
namespace HotelManagement;

/// <summary>The main class of the program</summary>
public static class Program
{
    static SqlHotelDataContext PrepareContext()
    {
        const string SECRET_MSSQL_FILE_NAME = "secret-mssql.txt";
        const string SECRET_PSQL_FILE_NAME = "secret-psql.txt";

        if (Path.Exists(SECRET_MSSQL_FILE_NAME))
            return new SqlHotelDataContext(File.ReadAllText(SECRET_MSSQL_FILE_NAME), true);

        if (Path.Exists(SECRET_PSQL_FILE_NAME))
            return new SqlHotelDataContext(File.ReadAllText(SECRET_PSQL_FILE_NAME), false);

        throw new Exception("Error: Cannot find the secret input file");
    }
    /// <summary>The entry point of the program</summary>
    public static void Main(string[] args)
    {

        Api running_app = new(PrepareContext());
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(
            c => c.IncludeXmlComments(
                Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")
            )
        );

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/", running_app.Greet);

        app.MapGet("/find-hotel/{keyword}", running_app.FindHotel);

        app.MapPost("/search-room", running_app.SearchRoom);

        app.MapPost("/book-room", running_app.BookRoom);

        app.MapPost("/check-booking", running_app.CheckBooking);

        app.MapGet("/seed", running_app.SeedDatabase);

        app.MapGet("/reset", running_app.ResetDatabase);

        app.Run();
    }
}

