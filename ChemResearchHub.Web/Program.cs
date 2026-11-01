using ChemResearchHub.Application.AI;
using ChemResearchHub.Application.AI.Experiment;
using ChemResearchHub.Application.AI.Ollama;
using ChemResearchHub.Application.AI.Prompting;
using ChemResearchHub.Application.AI.Routing;
using ChemResearchHub.Application.AI.Sample;
using ChemResearchHub.Application.AI.Tools;
using ChemResearchHub.Application.AI.Tools.Experiments;
using ChemResearchHub.Application.AI.Tools.Projects;
using ChemResearchHub.Application.AI.Tools.Samples;
using ChemResearchHub.Application.Attachments.Interfaces;
using ChemResearchHub.Application.Attachments.Repositories;
using ChemResearchHub.Application.Attachments.Services;
using ChemResearchHub.Application.Attachments.Storage;
using ChemResearchHub.Application.AuditLogs.Interfaces;
using ChemResearchHub.Application.AuditLogs.Repositories;
using ChemResearchHub.Application.AuditLogs.Services;
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
using ChemResearchHub.Application.ResearchReviews.Interfaces;
using ChemResearchHub.Application.ResearchReviews.Repositories;
using ChemResearchHub.Application.ResearchReviews.Services;
using ChemResearchHub.Application.Results.Interfaces;
using ChemResearchHub.Application.Results.Repositories;
using ChemResearchHub.Application.Results.Services;
using ChemResearchHub.Application.Samples.Interfaces;
using ChemResearchHub.Application.Samples.Repositories;
using ChemResearchHub.Application.Samples.Services;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Users.Repositories;
using ChemResearchHub.Application.Users.Services;
using ChemResearchHub.Application.WorkItemAssignments.Repositories;
using ChemResearchHub.Application.WorkItemBlocks.Interfaces;
using ChemResearchHub.Application.WorkItemBlocks.Repositories;
using ChemResearchHub.Application.WorkItemBlocks.Services;
using ChemResearchHub.Application.WorkItems.Interfaces;
using ChemResearchHub.Application.WorkItems.Repositories;
using ChemResearchHub.Application.WorkItems.Services;
using ChemResearchHub.Application.WorkItemTransitions.Interfaces;
using ChemResearchHub.Application.WorkItemTransitions.Repositories;
using ChemResearchHub.Application.WorkItemTransitions.Services;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Infrastructure.Persistence;
using ChemResearchHub.Infrastructure.Persistence.Repositories;
using ChemResearchHub.Infrastructure.Repositories;
using ChemResearchHub.Infrastructure.Storage;
using ChemResearchHub.Web.Authorization.Permissions;
using ChemResearchHub.Web.Authorization.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ChemResearchHub.Application.WorkItemUserAlerts.Interfaces;
using ChemResearchHub.Application.WorkItemUserAlerts.Repositories;
using ChemResearchHub.Application.WorkItemUserAlerts.Services;
using ChemResearchHub.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Infrastructure.Bale.Clients;
using ChemResearchHub.Infrastructure.Bale.Configuration;
using ChemResearchHub.Infrastructure.Bale.Services;
using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Services;
using ChemResearchHub.Infrastructure.Notifications.Channels;
using ChemResearchHub.Infrastructure.Notifications.Services;
using ChemResearchHub.Web.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .LogTo(
            Console.WriteLine,
            Microsoft.Extensions.Logging.LogLevel.Information));

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

// Tags
builder.Services.AddScoped<ChemResearchHub.Application.Tags.Repositories.ITagRepository, ChemResearchHub.Infrastructure.Persistence.Repositories.TagRepository>();
builder.Services.AddScoped<ChemResearchHub.Application.Tags.Repositories.IWorkItemTagRepository, ChemResearchHub.Infrastructure.Persistence.Repositories.WorkItemTagRepository>();
builder.Services.AddScoped<ChemResearchHub.Application.Tags.Interfaces.ITagService, ChemResearchHub.Application.Tags.Services.TagService>();

// Work Item Transitions
builder.Services.AddScoped<IWorkItemTransitionRepository, WorkItemTransitionRepository>();
builder.Services.AddScoped<IWorkItemTransitionService, WorkItemTransitionService>();
builder.Services.AddScoped<IWorkItemFlowMetricsService, WorkItemFlowMetricsService>();
builder.Services.AddScoped<IWorkItemFlowSummaryService, WorkItemFlowSummaryService>();
builder.Services.AddScoped<IWorkItemBlockRepository, WorkItemBlockRepository>();
builder.Services.AddScoped<IWorkItemBlockService, WorkItemBlockService>();
builder.Services.AddScoped<IAiTool, SearchExperimentsAiTool>();
builder.Services.AddScoped<IAiTool, GetExperimentStatisticsAiTool>();
builder.Services.AddScoped<IAiTool, GetProjectExperimentSummaryAiTool>();
builder.Services.AddScoped<IAIChatService, OllamaChatService>();

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

builder.Services.AddScoped<IResearchReviewRepository, ResearchReviewRepository>();
builder.Services.AddScoped<IResearchReviewService, ResearchReviewService>();

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IWorkItemAssignmentRepository, WorkItemAssignmentRepository>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();

// Bale Account Linking
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IBaleAccountLinkingService, BaleAccountLinkingService>();

// Notifications
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationChannel, InAppNotificationChannel>();
builder.Services.AddScoped<INotificationChannel, BaleNotificationChannel>();
builder.Services.AddScoped<NotificationDispatcher>();

// SignalR notification publisher
builder.Services.AddScoped<
    INotificationRealtimePublisher,
    SignalRNotificationRealtimePublisher>();


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

builder.Services.AddScoped<ProjectAiContext>();
builder.Services.AddScoped<IAiTool, GetProjectsAiTool>();
builder.Services.AddScoped<IAiTool, GetLatestProjectAiTool>();
builder.Services.AddScoped<AiToolRegistry>();
builder.Services.AddScoped<IAiTool, GetProjectAiTool>();
builder.Services.AddScoped<IAiTool, SearchProjectsAiTool>();
builder.Services.AddScoped<IAiTool, GetProjectStatisticsAiTool>();
builder.Services.AddScoped<IAiTool, GetProjectOverviewAiTool>();
builder.Services.AddScoped<IAiTool, GetExperimentsAiTool>();
builder.Services.AddScoped<IAiTool, GetLatestExperimentAiTool>();
builder.Services.AddScoped<IAiTool, GetExperimentAiTool>();
builder.Services.AddScoped<IAiTool, GetSamplesByExperimentAiTool>();
builder.Services.AddScoped<AiIntentRouter>();
builder.Services.AddScoped<SampleRequestHandler>();
builder.Services.AddScoped<ExperimentRequestHandler>();
builder.Services.AddScoped<IAiRequestRouter, AiRequestRouter>();
builder.Services.AddScoped<AiToolExecutor>();
builder.Services.AddScoped<IAiPromptProvider, AiSystemPrompt>();
builder.Services.AddScoped<AiToolSelector>();

// Work Item Alerts
builder.Services.AddScoped<IWorkItemUserAlertRepository, WorkItemUserAlertRepository>();
builder.Services.AddScoped<IWorkItemUserAlertService, WorkItemUserAlertService>();

// SignalR
builder.Services.AddSignalR();


builder.Services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
{
    client.BaseAddress =
        new Uri("http://127.0.0.1:11434");

    client.Timeout =
        TimeSpan.FromMinutes(5);
});

// Bale Bot
builder.Services.Configure<BaleBotOptions>(
    builder.Configuration.GetSection(BaleBotOptions.SectionName));

builder.Services.AddHttpClient<IBaleBotClient, BaleBotClient>();

builder.Services.AddHostedService<BalePollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

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

// Area routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Default routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages();

app.MapHub<WorkItemAlertHub>(WorkItemAlertHub.HubUrl);

using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("Starting Identity role seeding...");

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider);

    Console.WriteLine("Identity role seeding finished.");
}

app.Run();
