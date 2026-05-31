using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.Helpers;

public static class AuthTestHelper
{
    public static async Task<string> LoginAsAdminAndGetTokenAsync(HttpClient client)
    {
        return await LoginAndGetTokenAsync(
            client,
            usuarioOCorreo: "admin",
            password: "admin123"
        );
    }

    public static async Task<string> LoginAndGetTokenAsync(
        HttpClient client,
        string usuarioOCorreo,
        string password,
        string loginUrl = "/api/auth/login")
    {
        var loginRequest = new
        {
            UsuarioOCorreo = usuarioOCorreo,
            Password = password
        };

        var response = await client.PostAsJsonAsync(loginUrl, loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();

        var token = ExtractToken(json);

        token.Should().NotBeNullOrWhiteSpace("el login debe devolver un token JWT");

        return token!;
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearBearerToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task AuthenticateAsAdminAsync(HttpClient client)
    {
        var token = await LoginAsAdminAndGetTokenAsync(client);
        SetBearerToken(client, token);
    }

    private static string? ExtractToken(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (TryGetString(root, "token", out var token))
            return token;

        if (root.TryGetProperty("data", out var data))
        {
            if (TryGetString(data, "token", out var dataToken))
                return dataToken;

            if (TryGetString(data, "Token", out var dataTokenUpper))
                return dataTokenUpper;
        }

        return null;
    }

    private static bool TryGetString(
        JsonElement element,
        string propertyName,
        out string? value)
    {
        value = null;

        if (!element.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind != JsonValueKind.String)
            return false;

        value = property.GetString();

        return !string.IsNullOrWhiteSpace(value);
    }
}