using Microsoft.OpenApi.Models;

using Prod_DDM_API.Classes;
using Prod_DDM_API.Data;



var builder = WebApplication.CreateBuilder(args);

// Docs (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prod DDM API", Version = "v1" });
});


// Cors (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();

// Add services to the container.
var app = builder.Build();

// Use Cors for origin
app.UseCors();

// enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prod DDM API");
});

//
// Add routes to the app
//

// HttpResponseController = C# Error to HTTP-Error-Code and standard response
HttpResponseController hRC = new HttpResponseController();

app.MapGet("/", () =>
{
    var path = "./Data/Templates/htmlpage.html";
    var html = System.IO.File.ReadAllText(path);

    // Setze den MIME-Typ auf "text/html"
    return Results.Content(html, "text/html");
});
app.MapGet("/csv/latest/count", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetCsvLines().Count;
    });
});
app.MapGet("/csv/latest/datetime", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetCreationTime();
    });
});
app.MapGet("/csv/recent/timeline", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

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
app.MapGet("/csv/tests", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetAllTests();
    });
});
app.MapGet("/csv/search/{substring}", (string substring) =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.SearchSubstringInCsv(substring);
    });
});
app.MapGet("/csv/tests/proofedornot", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetFilteredTests(loader.SearchSubstringInCsv("_TS_Execution"));
    });
});
app.MapGet("/csv/tests/proofedornot2", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

        dynamic tests = loader.GetAllTests();

        return loader.GetFilteredTests2(tests.data);
    });
});
app.MapGet("/csv/tests/dbinsert", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.testInsert();
    });
});
app.MapGet("/csv/tests/dbselect", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

        return loader.testSelect();
    });
});




app.Run();