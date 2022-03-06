using minimalApi.Configuration;
using minimalApi.Database;
using System.Data;
using System.Data.Common;

namespace minimalApi.Endpoints;

public static class GetFxRateEndpoint
{
    private static readonly Func<EfDbContext, IAsyncEnumerable<FxRate>> getCurrencyQuery = EF.CompileAsyncQuery
        ( (EfDbContext context) => context.Rates );
    public static void AddGetAllFxRatesEndpoint(this WebApplication? app)
    {
        if (app == null) return;

        app.MapGet("/fxRate", [Authorize] async (EfDbContext dbContext, HttpResponse response) =>
        {            
            var result = getCurrencyQuery(dbContext);
            await response.WriteAsJsonAsync(result);
            return;
        })
        .Produces(StatusCodes.Status200OK);
    }

    public static void AddGetFxRateEndpoint(this WebApplication? app, WebApplicationBuilder builder)
    {
        if (app == null) return;

        app.MapGet("/fxRate/{target}", async ([FromRoute] string target, EfDbContext dbContext, HttpResponse response) =>
        {
            var result = await dbContext.Rates.Where(x => x.CurrencyCode == target).ToListAsync();
            await response.WriteAsJsonAsync(result);
            return;
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK);
    }
}

