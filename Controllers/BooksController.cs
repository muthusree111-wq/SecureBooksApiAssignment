using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SecureBooksApi.Models;

namespace SecureBooksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly SqliteConnection _connection;

    public BooksController(SqliteConnection connection)
    {
        _connection = connection;
    }

    [HttpGet("antiforgery-token")]
    public IActionResult GetAntiForgeryToken(
        [FromServices] Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title) ||
            string.IsNullOrWhiteSpace(book.Author))
        {
            return BadRequest("Title and Author are required.");
        }

        await using var command = _connection.CreateCommand();

        command.CommandText =
            "INSERT INTO Books (Title, Author) VALUES ($title, $author); " +
            "SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$title", book.Title);
        command.Parameters.AddWithValue("$author", book.Author);

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return Created($"/api/books/{id}", new
        {
            id,
            book.Title,
            book.Author
        });
    }
}
