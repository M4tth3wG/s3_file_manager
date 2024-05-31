using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using s3_file_manager_backend.Data;
using s3_file_manager_backend.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<FileDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("FileDb")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

app.MapPost("/uploadfile", async (FileDbContext dbContext, StoredFile file) =>
{
    // Add the provided file to the database
    await dbContext.Files.AddAsync(file);
    await dbContext.SaveChangesAsync();

    // Return a response indicating success
    return Results.Created($"/file/{file.Id}", file);
})
.WithName("PostFile")
.WithOpenApi();

app.Run();
