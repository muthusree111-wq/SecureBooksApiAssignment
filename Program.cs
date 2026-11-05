using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// Keep the in-memory SQLite connection open for the app lifetime.
var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();

using (var command = connection.CreateCommand())
{
    command.CommandText = @"
        CREATE TABLE Books (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Author TEXT NOT NULL
        );";

    command.ExecuteNonQuery();
}

builder.Services.AddSingleton(connection);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();