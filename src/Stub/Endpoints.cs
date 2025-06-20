using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Defra.TradeImportsDataApiStub.Stub;

[ExcludeFromCodeCoverage]
public static class Endpoints
{
    private const string NotificationsRoot = "import-pre-notifications/{ched}";

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("import-pre-notifications/{ched}", (string ched) => GetNotification(ched));

        app.MapGet(
            $"{NotificationsRoot}/customs-declarations",
            (string ched) => GetNotification(ched, "_customs-declarations")
        );

        app.MapGet($"{NotificationsRoot}/gmrs", (string ched) => GetNotification(ched, "_gmrs"));

        app.MapGet("/import-pre-notification-updates", GetUpdates);
    }

    private static readonly Regex IncludeInUpdatesEndpoint = new(
        @"^.*\d{7}\.json$",
        RegexOptions.Compiled
    );

    private static IResult GetNotification(string ched, string? suffix = "")
    {
        try
        {
            return Results.Content(
                EmbeddedStubData.GetBody($"_import-pre-notifications_{ched}{suffix}.json"),
                MediaTypeNames.Application.Json
            );
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static Updates GetUpdates(
        [FromQuery] string[]? type,
        [FromQuery] string[]? pointOfEntry,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100
    )
    {
        var updates = EmbeddedStubData
            .GetAllScenariosStubs()
            .Where(s => IncludeInUpdatesEndpoint.IsMatch(s))
            .Select(s => JsonNode.Parse(EmbeddedStubData.GetBody(s))!)
            .Where(j =>
                IncludeNotification(
                    type,
                    j["importPreNotification"]!["importNotificationType"]!.GetValue<string>()
                )
                && IncludeNotification(
                    pointOfEntry,
                    j["importPreNotification"]!["partOne"]!["pointOfEntry"]!.GetValue<string>()
                )
            )
            .Select(j => new Update(
                j["importPreNotification"]!["referenceNumber"]!.GetValue<string>(),
                j["updated"]!.GetValue<DateTime>()
            ))
            .ToArray();

        return new Updates(
            updates.Page(page, pageSize).ToArray(),
            Total: updates.Length,
            PageSize: pageSize,
            Page: page
        );

        bool IncludeNotification(string[]? queryValues, string value) =>
            queryValues is null || queryValues.Length == 0 || queryValues.Contains(value);
    }

    private record Updates(
        [property: JsonPropertyName("importPreNotificationUpdates")]
            Update[] ImportPreNotificationUpdates,
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("pageSize")] int PageSize,
        [property: JsonPropertyName("page")] int Page
    );

    private record Update(
        [property: JsonPropertyName("referenceNumber")] string ReferenceNumber,
        [property: JsonPropertyName("updated")] DateTime Updated
    );

    private static IEnumerable<TSource> Page<TSource>(
        this IEnumerable<TSource> source,
        int page,
        int pageSize
    ) => source.Skip((page - 1) * pageSize).Take(pageSize);
}
