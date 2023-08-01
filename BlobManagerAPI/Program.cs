using FileUpload;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<BlobUtilityService, BlobUtilityService>();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient("connection");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/upload", async (IFormFile file, BlobUtilityService blobUtilityService) =>
{
    await blobUtilityService.UploadAsBlob(file);
})
.WithName("Upload");

app.MapGet("/get-untagged", async (BlobUtilityService blobUtilityService) =>
{
    return await blobUtilityService.GetUntaggedBlobsAsync();
});

app.Run();