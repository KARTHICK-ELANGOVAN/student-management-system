using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StudentManagement.API.Data;
using System.Linq;
using System.Text.Json;
using StudentManagement.API.Models;
using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Configuration.Memory;


namespace StudentManagement.IntegrationTests;

public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_GetStudents_ReturnsOk()
    {
        // Use in-memory SQLite for tests
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            // Set environment to 'Testing' so Program.cs will configure SQLite
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                // Ensure JWT key is long enough for HS256 in tests
                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = new string('x', 64)
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace the DbContext with one that uses the opened SQLite connection
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) || d.ServiceType == typeof(ApplicationDbContext)).ToList();
                foreach (var d in descriptors) services.Remove(d);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(connection);
                });

                // Build the service provider and create DB
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                // Seed sample students for this test
                db.Students.AddRange(
                    new Student { Name = "Alice", RollNumber = "R001", Address = "111 Main St", Grade = "A" },
                    new Student { Name = "Bob", RollNumber = "R002", Address = "222 Oak Ave", Grade = "B" }
                );
                db.SaveChanges();
            });
        }).CreateClient();

        // Register
        var registerResp = await client.PostAsJsonAsync("/api/auth/register", new { username = "itest", passwordHash = "pwd" });
        registerResp.EnsureSuccessStatusCode();

        // Login
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { username = "itest", passwordHash = "pwd" });
        loginResp.EnsureSuccessStatusCode();
        var loginBody = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginBody.GetProperty("token").GetString();
        Assert.False(string.IsNullOrEmpty(token));

        // GET students (authorized)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var studentsResp = await client.GetAsync("/api/students");
        studentsResp.EnsureSuccessStatusCode();

        var studentsArr = await studentsResp.Content.ReadFromJsonAsync<JsonElement[]>();
        Assert.NotNull(studentsArr);
        Assert.True(studentsArr.Length >= 2);
        var names = studentsArr.Select(s => s.GetProperty("name").GetString()).ToList();
        Assert.Contains("Alice", names);
        Assert.Contains("Bob", names);
    }
}
