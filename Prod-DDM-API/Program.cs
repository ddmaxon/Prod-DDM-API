using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting;
using System.Reflection.PortableExecutable;
using SharpCompress.Crypto;
using System.Xml.Linq;

using Prod_DDM_API.Classes;
using Prod_DDM_API.Data;

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


app.MapGet("/csv/latest/count", () =>
{
    return hRC.HandleErrors(() =>
    {
        CsvLoader loader = new CsvLoader("./data/csv/testdata.csv");

        return loader.GetCsvLines().Count;
    });
});


app.MapGet("/csv/latest/datetime", () =>
{
    return hRC.HandleErrors(() =>
    {
        CsvLoader loader = new CsvLoader("./data/csv/testdata.csv");

        return loader.GetCreationTime();
    });
});


app.MapGet("/csv/recent/timeline", () =>
{
    return hRC.HandleErrors(() =>
    {
        CsvLoader loader = new CsvLoader("./data/csv/testdata2.csv");

        List<object> list = new List<object>();

        object timeline = loader.GetTimeline();

        object all = loader.GetExecutionTimeWithSelectors(loader.GetExecutionTime(loader.GetCsvLines()).ToArray());

        foreach(var key in Config.CSVDataKeyConfig)
        {
            list.Add( new { key, value = loader.GetExecutionTimeWithSelectors(loader.GetExecutionTime(loader.GetCsvLines(), key).ToArray())});
        }

        return new { timeline, execTimeline = new { all, sorted = list } };
    });
});



app.Run();