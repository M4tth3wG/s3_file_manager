using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using s3_file_manager_backend.Data;
using s3_file_manager_backend.Models;
using System.Globalization;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using s3_file_manager_backend.Services;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<FileDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("FileDb")));
builder.Services.AddSingleton<S3Service>();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery.4-a-mNcJ_3w";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

app.UseAntiforgery();

app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    var xsrfToken = tokens.RequestToken!;
    return TypedResults.Content(xsrfToken, "text/plain");
});

app.MapPost("/uploadfile", async (IFormFile file, string fileName, FileDbContext dbContext, S3Service s3Service) =>
{
    if (file == null || string.IsNullOrEmpty(fileName))
    {
        return Results.BadRequest("Missing file or file name.");
    }

    var fileGuid = Guid.NewGuid();
    var keyName = fileGuid.ToString();
    await s3Service.UploadFileAsync(file, keyName);
    var fileLink = s3Service.GenerateFileLink(keyName);

    var storedFile = new StoredFile()
    {
        Id = fileGuid,
        Name = fileName,
        FileLink = fileLink
    };

    await dbContext.Files.AddAsync(storedFile);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/file/{storedFile.Id}", storedFile);
})
.WithOpenApi();

app.Run();
