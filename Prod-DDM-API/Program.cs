using Microsoft.OpenApi.Models;
using Prod_DDM_API;
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
KardinalSystem kardinal = new KardinalSystem();


app.MapGet("/", () =>
{
    var path = "./Data/Templates/htmlpage.html";
    var html = System.IO.File.ReadAllText(path);

    // Setze den MIME-Typ auf "text/html"
    return Results.Content(html, "text/html");
});
/*
app.MapGet("/api/csv/latest/count", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetCsvLines().Count;
    });
});
app.MapGet("/api/csv/latest/datetime", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetCreationTime();
    });
});
*/
app.MapGet("/api/csv/recent/timeline", () =>
{
    return hRC.HandleErrors(() =>
    {
        return kardinal.GetFunctionExecution("Timeline");
    });
});
/*
app.MapGet("/api/csv/tests", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

        return loader.GetAllTests();
    });
});
app.MapGet("/api/csv/search/{substring}", (string substring) =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.SearchSubstringInCsv(substring);
    });
});
app.MapGet("/api/csv/tests/proofedornot", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata.csv");

        return loader.GetFilteredTests(loader.SearchSubstringInCsv("_TS_Execution"));
    });
});
app.MapGet("/api/csv/tests/proofedornot2", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

        dynamic tests = loader.GetAllTests();

        return loader.GetFilteredTests2(tests.data);
    });
});
*/
app.MapGet("/api/csv/tests/dbinsert", () =>
{
    return hRC.HandleErrors(() =>
    {
        return kardinal.GetFunctionExecution("TestInsertFile");
    });
});
/*
app.MapGet("/csv/tests/dbselect", () =>
{
    return hRC.HandleErrors(() =>
    {
        FileController loader = new FileController("./Data/Csv/testdata2.csv");

        return loader.testSelect();
    });
});
*/
app.MapGet("/api/csv/tests/getvalues", () =>
{
    return hRC.HandleErrors(() =>
    {
        return kardinal.GetFunctionExecution("GetValues");
    });
});


app.MapGet("/api/csv/recent/history/{id}", (string id) =>
{
    return hRC.HandleErrors(() =>
    {
        return kardinal.GetFunctionExecution("HistoryID", new string[]{id});
    });
});



app.Run();