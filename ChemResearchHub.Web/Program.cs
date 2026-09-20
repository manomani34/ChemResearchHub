using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.Attachments.Repositories;
using ChemResearchHub.Application.Attachments.Services;
using ChemResearchHub.Application.Attachments.Storage;
using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Application.Boards.Services;
using ChemResearchHub.Application.DecisionLogs.Interfaces;
using ChemResearchHub.Application.DecisionLogs.Repositories;
using ChemResearchHub.Application.DecisionLogs.Services;
using ChemResearchHub.Application.Experiments.Interfaces;
using ChemResearchHub.Application.Experiments.Repositories;
using ChemResearchHub.Application.Experiments.Services;
using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Application.Projects.Repositories;
using ChemResearchHub.Application.Projects.Services;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Results.Repositories;
using ChemResearchHub.Application.Results.Services;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.Samples.Repositories;
using ChemResearchHub.Application.Samples.Services;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Users.Repositories;
using ChemResearchHub.Application.Users.Services;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Application.WorkItems.Repositories;
using ChemResearchHub.Application.WorkItems.Services;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Infrastructure.Persistence;
using ChemResearchHub.Infrastructure.Persistence.Repositories;
using ChemResearchHub.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages for ASP.NET Core Identity UI
builder.Services.AddControllersWithViews(options =>
{
    var policy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

    options.Filters.Add(
        new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Validate Identity security stamps on every authenticated request.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// Validate custom Active/Inactive status on every authenticated request.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnValidatePrincipal = async context =>
    {
        var userManager =
            context.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.GetUserAsync(
                context.Principal!);

        if (user is null || !user.IsActive)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                IdentityConstants.ApplicationScheme);
        }
    };
});

// Projects
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Boards
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IBoardService, BoardService>();

// Users
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Work Items
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();

// Experiments
builder.Services.AddScoped<IExperimentRepository, ExperimentRepository>();
builder.Services.AddScoped<IExperimentService, ExperimentService>();

// Samples
builder.Services.AddScoped<ISampleRepository, SampleRepository>();
builder.Services.AddScoped<ISampleService, SampleService>();

// Results
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IResultService, ResultService>();

// Attachments
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IAttachmentStorage>(sp =>
{
    var rootPath =
        Path.Combine(
            builder.Environment.ContentRootPath,
            "App_Data",
            "Attachments");

    return new LocalAttachmentStorage(
        rootPath);
});

// Decision Logs
builder.Services.AddScoped<IDecisionLogRepository, DecisionLogRepository>();
builder.Services.AddScoped<IDecisionLogService, DecisionLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Public registration is disabled.
// Users can only be created by an Administrator
// through /Users/Create.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments(
            "/Identity/Account/Register"))
    {
        context.Response.StatusCode =
            StatusCodes.Status404NotFound;

        return;
    }

    await next();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("Starting Identity role seeding...");

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider);

    Console.WriteLine("Identity role seeding finished.");
}

app.Run();