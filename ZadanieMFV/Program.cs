using System;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http.Metadata;
using System.Net.Mime;
using System.Reflection;
using ZadanieMFV.Database;
using ZadanieMFV.Logic;
using ZadanieMFV.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();
    builder.Services.AddScoped<ProcessTools>();
    builder.Services.AddScoped<SendMail>();
    builder.Services.AddCors();
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication();


    Log.Information("Starting web application");

    /*Dodanie contextu do bazy ale ze jako inmemory 
     * services.AddDbContext<ProcessContext>(o =>
              {
                  o.UseSqlServer(Configuration.GetConnectionString("ConnectionString"), sqlServerOptions => sqlServerOptions.CommandTimeout(120));
              }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
     */

    var app = builder.Build();


    if (app.Environment.IsDevelopment())
    {

        app.UseDeveloperExceptionPage();
    }
    app.UseCors(builder => builder
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowAnyOrigin()
           .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
        );
    app.UseAuthentication();
       
    app.UseRouting();

    builder.Services.AddAuthorization();
    //Trzeba cerytyfiakt zeby uruchomic  dlatego zakomentowane 
    //builder.Services.AddAuthorization(options =>
    //{
    //    options.AddPolicy("Over18", policy =>
    //    {
    //        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
    //        policy.RequireAuthenticatedUser();
           
    //    });
    //});

    using var context = new ProcessContext();
    context.Users.Add(
        new UserModel
        {
            Name = "mfvpolska",
            Email = "mfvpolska@gmail.com"
        }
        );


    app.MapGet("/", () => "Przyklad");

    app.MapPost("SendMail", async (SendMail sendmail, MailToUserModel message) =>
        await sendmail.SendMailToUser(message)

        is MailStatusModel mailStatus
            ? Results.Ok(mailStatus)
            : Results.NotFound()
        );
    //Nie implementuje pozostalych funkcji jako ze to jest przyklad 

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}