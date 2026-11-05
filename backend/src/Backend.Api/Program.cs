using Backend.Repositories;
using Backend.Repositories.Initialization;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Backend.Api.Security.RestaurantAccess>();
builder.Services.AddControllers(options => options.Filters.AddService<Backend.Api.Security.RestaurantAccess>());
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Backend.Api.ServiceBridge>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter a valid JWT bearer token."
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document, null)] = []
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(context.ModelState);
});

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddBusinessServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Use(async (context,next) =>
{
    try { await next(context); }
    catch (InvalidOperationException error) when (!context.Response.HasStarted)
    { context.Response.StatusCode=409; await context.Response.WriteAsJsonAsync(new { message=error.Message }); }
    catch (Microsoft.Data.SqlClient.SqlException error) when (!context.Response.HasStarted && error.Number is 547 or 2601 or 2627 or 1205 or 51000)
    { context.Response.StatusCode=409; await context.Response.WriteAsJsonAsync(new { message="The operation conflicts with current data. Refresh and check references, duplicates, or quantities." }); }
});
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () =>
{
    return app.Environment.IsDevelopment()
        ? Results.Redirect("/swagger")
        : Results.Ok(new { status = "ok", service = "Backend.Api" });
});

app.MapControllers();
app.MapHub<Backend.Api.Hubs.RestaurantHub>("/hubs/restaurant");

app.Run();
