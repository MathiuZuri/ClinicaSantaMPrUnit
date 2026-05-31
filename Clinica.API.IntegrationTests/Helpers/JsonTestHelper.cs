using System.Text.Json;
using FluentAssertions;

namespace Clinica.API.IntegrationTests.Helpers;

public static class JsonTestHelper
{
    public static async Task<JsonDocument> ReadJsonDocumentAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        json.Should().NotBeNullOrWhiteSpace("la respuesta debería contener JSON");

        return JsonDocument.Parse(json);
    }

    public static async Task<JsonElement> ReadRootAsync(HttpResponseMessage response)
    {
        using var document = await ReadJsonDocumentAsync(response);
        return document.RootElement.Clone();
    }

    public static async Task<JsonElement> ReadDataAsync(HttpResponseMessage response)
    {
        var root = await ReadRootAsync(response);

        root.TryGetProperty("data", out var data)
            .Should()
            .BeTrue("la respuesta debería tener una propiedad 'data'");

        return data.Clone();
    }

    public static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        var root = await ReadRootAsync(response);

        if (root.TryGetProperty("success", out var success))
        {
            success.GetBoolean().Should().BeTrue();
        }

        if (root.TryGetProperty("isSuccess", out var isSuccess))
        {
            isSuccess.GetBoolean().Should().BeTrue();
        }
    }

    public static async Task AssertErrorAsync(HttpResponseMessage response)
    {
        var root = await ReadRootAsync(response);

        if (root.TryGetProperty("success", out var success))
        {
            success.GetBoolean().Should().BeFalse();
        }

        if (root.TryGetProperty("isSuccess", out var isSuccess))
        {
            isSuccess.GetBoolean().Should().BeFalse();
        }
    }

    public static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    public static Guid GetGuid(JsonElement element, string propertyName)
    {
        element.TryGetProperty(propertyName, out var property)
            .Should()
            .BeTrue($"debería existir la propiedad '{propertyName}'");

        return property.GetGuid();
    }

    public static int GetInt32(JsonElement element, string propertyName)
    {
        element.TryGetProperty(propertyName, out var property)
            .Should()
            .BeTrue($"debería existir la propiedad '{propertyName}'");

        return property.GetInt32();
    }

    public static decimal GetDecimal(JsonElement element, string propertyName)
    {
        element.TryGetProperty(propertyName, out var property)
            .Should()
            .BeTrue($"debería existir la propiedad '{propertyName}'");

        return property.GetDecimal();
    }

    public static bool HasProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out _);
    }
}