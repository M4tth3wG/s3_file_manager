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
using Microsoft.AspNetCore.Http.HttpResults;

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
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b =>
    {
        b
            .WithOrigins(builder.Configuration["App:FrontendDomain"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
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
app.UseCors();

app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    var xsrfToken = tokens.RequestToken!;
    return TypedResults.Content(xsrfToken, "text/plain");
});

app.MapPost("/file/upload", async (IFormFile file, string fileName, FileDbContext dbContext, S3Service s3Service) =>
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

app.MapGet("/file/delete", async (Guid fileId, FileDbContext dbContext, S3Service s3Service) =>
{
    await s3Service.DeleteFileAsync(fileId.ToString());

    var file = await dbContext.Files.FindAsync(fileId);
    if (file != null)
    {
        dbContext.Files.Remove(file);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();
}
)
.WithOpenApi();

app.MapGet("/file/list", async (FileDbContext dbContext) =>
{
    var files = await dbContext.Files.ToListAsync();
    return files;
}
)
.WithOpenApi();

app.MapGet("/file/edit", async (Guid fileId, string newName, FileDbContext dbContext) =>
{
    var file = await dbContext.Files.FindAsync(fileId);
    if (file != null)
    {
        file.Name = newName;
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
}
)
.WithOpenApi();

app.MapGet("/file/get", async (Guid fileId, FileDbContext dbContext) =>
{
    var file = await dbContext.Files.FindAsync(fileId);

    if (file == null)
    {
        return Results.NotFound();
    }

    return Results.Json(file);
})
.WithOpenApi();

app.Run();
