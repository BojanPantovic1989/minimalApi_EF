
using minimalApi.Configuration;
using minimalApi.Database;
using minimalApi.Endpoints;
using MySqlConnector;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration);

// Add services to the container.
builder.Services.AddDbContext<EfDbContext>(options =>
{
    var appSettings = builder.Configuration.Get<AppSettings>();
    //var connectionString = "server=localhost;port=3306;database=investorcondb;uid=root;password=8h48t984ht";
    options.UseMySql(appSettings.ConnectionString, ServerVersion.AutoDetect(appSettings.ConnectionString));
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (BadHttpRequestException ex)
    {
        ctx.Response.StatusCode = ex.StatusCode;
        await ctx.Response.WriteAsync(ex.Message);
    }
});

app.UseRouting();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.AddLoginEndpoint(builder);
app.AddGetFxRateEndpoint(builder);
app.AddGetAllFxRatesEndpoint();

app.Run();