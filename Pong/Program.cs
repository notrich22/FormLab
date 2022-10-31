using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

List<string> files = new List<string>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/ping", async() => "pong!");

async Task UploadFile(HttpContext context)
{
    try
    {
        IFormFile file = context.Request.Form.Files[0];
        var originalFileName = Path.GetFileName(file.FileName);
        var uniqueFileName = Path.GetRandomFileName();
        var uniqueFilePath = Path.Combine(@$".\Repository\", uniqueFileName);
        using (var stream = System.IO.File.Create(uniqueFilePath))
        {
            await file.CopyToAsync(stream);
        }
    }catch(Exception e)
    {
        Console.WriteLine(e.Message + ", Something went wrong while uploading file");
    }
    //files.Add(filename);
}

app.MapGet("/download", async(context) =>
{
    string name = context.Request.Query["FileName"];
    if (!System.IO.File.Exists(@$".\Repository\{name}"))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync($"File \"{name}\" not found");
    }
    else
    {
        context.Response.Headers.ContentDisposition = $"attachment; filename={name}";
        await context.Response.SendFileAsync(@$".\Repository\{name}");
    }
});

app.MapGet("/files", async(context) =>
{
    string[] allfiles = Directory.GetFiles(@".\Repository");
    foreach (string filename in allfiles)
    {
        await context.Response.WriteAsync(Path.GetFileName(filename) + "\n");
    }
});

app.MapPost("/rename", async (context) =>
{
    string oldName = context.Request.Form["oldName"];
    Console.WriteLine("!!!FILE RENAMED!!!");
    Console.WriteLine("old name:" + oldName);
    string newName = context.Request.Form["newName"];
    Console.WriteLine("new name:" + newName);
    try
    {
        System.IO.File.Move(@$".\Repository\" + oldName, @$".\Repository\" + newName);
    }catch(Exception e)
    {
        await context.Response.WriteAsync(e.Message);
    }
});
app.MapPost("/upload", UploadFile);
app.Run();
