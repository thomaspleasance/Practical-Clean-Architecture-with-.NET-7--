using CaWorkshop.Application;
using CaWorkshop.Application.Common.Interfaces;
using CaWorkshop.Infrastructure;
using CaWorkshop.Infrastructure.Data;
using CaWorkshop.WebUI.Filters;
using CaWorkshop.WebUI.Services;

using NSwag.Generation.Processors.Security;
using NSwag;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();


builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new ApiExceptionFilterAttribute()));

builder.Services.AddRazorPages();


builder.Services.AddScoped<ApplicationDbContextInitialiser>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApiDocument(configure =>
{
    configure.Title = "CaWorkshop API";
    configure.AddSecurity("JWT", Enumerable.Empty<string>(),
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Type into the textbox: Bearer {your JWT token}."
        });

    configure.OperationProcessors.Add(
        new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

WebApplication app = builder.Build();


// Initialise and seed the database on start-up
using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        ApplicationDbContextInitialiser initialiser =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        initialiser.Initialise();
        initialiser.Seed();
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialisation.");

        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseOpenApi();
app.UseSwaggerUi3();
app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.Run();