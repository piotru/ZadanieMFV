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


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ProductRep>();
builder.Services.AddCors();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();


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
app.UseAuthorization();

using var context = new ProcessContext();
//dla weryfikacji jeden rekord
context.ProductModels.Add(
    new ProductModel
    {
        Code = "aaa",
        Price = 120,
        Id = 1,
        Name = "aaaProduct"
    }
    );


app.MapGet("/", () => "Przyklad");


app.Run();
