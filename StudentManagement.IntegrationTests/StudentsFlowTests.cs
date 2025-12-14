using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StudentManagement.API.Data;
using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace StudentManagement.IntegrationTests;

public class StudentsFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StudentsFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithDb(out SqliteConnection connection)
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        connection = conn;

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = new string('x', 64)
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) || d.ServiceType == typeof(ApplicationDbContext)).ToList();
                foreach (var d in descriptors) services.Remove(d);

                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(conn));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                // Seed a user for login
                db.Users.Add(new StudentManagement.API.Models.User { Username = "tester", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pwd") });
                db.SaveChanges();
            });
        }).CreateClient();

        return client;
    }

    [Fact]
    public async Task Unauthorized_Get_Returns401()
    {
        var client = CreateClientWithDb(out var conn);
        var resp = await client.GetAsync("/api/students");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Create_Update_Delete_Student_Works()
    {
        var client = CreateClientWithDb(out var conn);

        // Login to get token
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { username = "tester", passwordHash = "pwd" });
        loginResp.EnsureSuccessStatusCode();
        var loginBody = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginBody.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create student
        var createResp = await client.PostAsJsonAsync("/api/students", new { name = "Charlie", rollNumber = "R010", address = "10 Test Ln", grade = "C" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();

        // Update
        var updateResp = await client.PutAsJsonAsync($"/api/students/{id}", new { name = "Charlie2", rollNumber = "R010", address = "10 Test Ln", grade = "B" });
        updateResp.EnsureSuccessStatusCode();

        // Delete
        var delResp = await client.DeleteAsync($"/api/students/{id}");
        delResp.EnsureSuccessStatusCode();
    }
}
