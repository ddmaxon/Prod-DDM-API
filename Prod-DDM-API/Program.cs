using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting;
using System.Reflection.PortableExecutable;
using SharpCompress.Crypto;
using System.Xml.Linq;

using Prod_DDM_API.Classes;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prod DDM API", Version = "v1" });
});

var app = builder.Build();

// enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prod DDM API");
});


HttpResponseController hRC = new HttpResponseController();


app.MapGet("/", () =>
{
    var path = "./data/templates/htmlpage.html";
    var html = System.IO.File.ReadAllText(path);

    // Setze den MIME-Typ auf "text/html"
    return Results.Content(html, "text/html");
});


app.MapGet("/csv/latest/datetime", () =>
{
    return hRC.HandleErrors(() =>
    {
        CsvLoader loader = new CsvLoader("C:/vsc/_BLJ/Prod-DDM-API/Prod-DDM-API/data/csv/testdata.csv");

        return loader.GetCreationTime();
    });
});


app.Run();