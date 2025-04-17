using CollegeWebsite.Components;
using CollegeWebsite.Components.Shared;
using CollegeWebsite.Models;
using CollegeWebsite.Services;
using CollegeWebsite.Utilities;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB serialization
var pack = new ConventionPack
{
    new CamelCaseElementNameConvention(),
    new IgnoreExtraElementsConvention(true)
};
ConventionRegistry.Register("CamelCase", pack, t => true);

// Register class maps to explicitly control serialization
BsonClassMap.RegisterClassMap<Department>(cm =>
{
    cm.AutoMap();
    cm.SetIgnoreExtraElements(true);
    cm.MapMember(c => c.FacultyIds).SetElementName("faculty");
    cm.MapMember(c => c.CourseIds).SetElementName("courses");
    cm.MapMember(c => c.HeadOfDepartmentId).SetElementName("headOfDepartmentId");
});

BsonClassMap.RegisterClassMap<Student>(cm =>
{
    cm.AutoMap();
    cm.SetIgnoreExtraElements(true);
    cm.MapMember(c => c.EnrolledCourseIds).SetElementName("enrolledCourseIds");
    // If your database actually uses "courses" instead:
    // cm.MapMember(c => c.EnrolledCourseIds).SetElementName("courses");
});

BsonClassMap.RegisterClassMap<Course>(cm =>
{
    cm.AutoMap();
    cm.SetIgnoreExtraElements(true);
    cm.MapMember(c => c.EnrolledStudentIds).SetElementName("enrolledStudents");
});

BsonClassMap.RegisterClassMap<Faculty>(cm =>
{
    cm.AutoMap();
    cm.SetIgnoreExtraElements(true);
    cm.MapMember(c => c.CourseIds).SetElementName("courses");
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add explicit SignalR configuration
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.EnableDetailedErrors = true;
});


// Configure MongoDB
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("MongoDB"));

// Register MongoDB services
builder.Services.AddSingleton<DatabaseSettings>(sp =>
{
    var settings = new DatabaseSettings
    {
        ConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017",
        DatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "CollegeDB",
        StudentsCollectionName = builder.Configuration["MongoDB:StudentsCollectionName"] ?? "Students",
        CoursesCollectionName = builder.Configuration["MongoDB:CoursesCollectionName"] ?? "Courses",
        DepartmentsCollectionName = builder.Configuration["MongoDB:DepartmentsCollectionName"] ?? "Departments",
        FacultyCollectionName = builder.Configuration["MongoDB:FacultyCollectionName"] ?? "Faculty",
        AdminsCollectionName = builder.Configuration["MongoDB:AdminsCollectionName"] ?? "Admins",
        FeedbackCollectionName = builder.Configuration["MongoDB:FeedbackCollectionName"] ?? "Feedback"
    };
    return settings;
});

// Add MongoDB client for diagnostic purposes
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<DatabaseSettings>();
    return new MongoClient(settings.ConnectionString);
});

// Important: Add specific Blazor Server configuration
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
});

// Add these service registrations
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Longer timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CollegeAdmin.Session"; // Custom name helps with debugging

    // These are the critical settings for session persistence
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Use 'Always' in production with HTTPS

    // Make cookies work in development
    options.Cookie.Path = "/";
    options.Cookie.Domain = null;
});

// Register services
builder.Services.AddScoped<IMongoDBService<Student>, StudentService>();
builder.Services.AddScoped<IMongoDBService<Course>, CourseService>();
builder.Services.AddScoped<IMongoDBService<Department>, DepartmentService>();
builder.Services.AddScoped<IMongoDBService<Faculty>, FacultyService>();
builder.Services.AddScoped<IMongoDBService<Admin>, AdminService>();
builder.Services.AddScoped<IMongoDBService<Feedback>, FeedbackService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<FacultyService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<AdminAuthComponent>();
// In your service registrations:
builder.Services.AddServerSideBlazor(options =>
{
    // Increase timeout for Blazor circuit
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.DetailedErrors = true; // Show detailed errors in development
});
// Add this after other service registrations
builder.Services.AddScoped<IAuthStateService, AuthStateService>();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug); // Set to Debug for more verbose logs
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // In development, you could disable HTTPS redirection
    // Don't use HTTPS redirection here if you don't have SSL set up
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection(); // Only use in production with proper SSL
}

// And in your middleware pipeline:
app.UseStaticFiles();
app.UseRouting();

// Session must come before endpoints but after routing
app.UseSession();

// Must add Blazor Hub AFTER UseSession but BEFORE MapRazorComponents
app.MapBlazorHub();
app.UseAntiforgery();

// Map endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Create a default admin account if none exists
using (var scope = app.Services.CreateScope())
{
    try
    {
        var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
        await adminService.EnsureAdminExistsAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during startup: {ex.Message}");
    }
}

// Add this to test session and confirm it works
app.MapGet("/session-test", async context =>
{
    // Set a test value in session
    context.Session.SetString("TestValue", "Session is working! " + DateTime.Now);
    await context.Session.CommitAsync();

    // Read it back
    var value = context.Session.GetString("TestValue");

    // Return a response
    await context.Response.WriteAsync($"Session test:<br>Value: {value}<br>Session ID: {context.Session.Id}");
});

// Add this directly in Program.cs after app is built:
app.MapGet("/direct-login", async context =>
{
    // Set admin session directly
    var adminJson = @"{""Id"":""admin123"",""Username"":""admin"",""FullName"":""API Admin"",""Role"":""Admin""}";
    context.Session.SetString("CurrentAdmin", adminJson);
    await context.Session.CommitAsync();

    // Return HTML with links
    await context.Response.WriteAsync(@"
        <html>
        <head>
            <title>Direct Login</title>
            <style>
                body { font-family: Arial; margin: 20px; }
                a { display: block; margin: 10px 0; padding: 10px; background: #007bff; color: white; text-decoration: none; }
            </style>
        </head>
        <body>
            <h1>Direct Login Complete</h1>
            <p>Admin session created. Session ID: " + context.Session.Id + @"</p>
            <a href='/admin/dashboard'>Go to Dashboard</a>
            <a href='/direct-session-test'>Go to Session Test</a>
            <a href='/button-test'>Go to Button Test</a>
        </body>
        </html>
    ");
});

// Simple static test endpoint that doesn't use Blazor
app.MapGet("/static-test", (HttpContext context) =>
{
    string html = "<html>" +
                  "<head>" +
                  "    <title>Static Test</title>" +
                  "</head>" +
                  "<body>" +
                  "    <h1>Static Page Test</h1>" +
                  "    <p>This is a non-Blazor static page to test basic server functionality.</p>" +
                  "    <p>Current time: " + DateTime.Now.ToString() + "</p>" +
                  "    <p><a href='/'>Return to home</a></p>" +
                  "</body>" +
                  "</html>";

    return context.Response.WriteAsync(html);
});

// Test if the Blazor framework file is accessible
app.MapGet("/framework-test", (HttpContext context) =>
{
    string html = "<html>" +
                  "<head>" +
                  "    <title>Framework Test</title>" +
                  "</head>" +
                  "<body>" +
                  "    <h1>Testing Blazor JavaScript Framework</h1>" +
                  "    <p>This page checks if the Blazor framework script is accessible:</p>" +
                  "    <div id='result'>Checking...</div>" +
                  "    " +
                  "    <script>" +
                  "        document.addEventListener('DOMContentLoaded', function() {" +
                  "            try {" +
                  "                fetch('_framework/blazor.web.js')" +
                  "                    .then(response => {" +
                  "                        if (response.ok) {" +
                  "                            document.getElementById('result').innerHTML = " +
                  "                                '<span style=\"color:green\">SUCCESS: blazor.web.js is accessible!</span>';" +
                  "                        } else {" +
                  "                            document.getElementById('result').innerHTML = " +
                  "                                '<span style=\"color:red\">ERROR: blazor.web.js returned ' + " +
                  "                                response.status + ' ' + response.statusText + '</span>';" +
                  "                        }" +
                  "                    })" +
                  "                    .catch(error => {" +
                  "                        document.getElementById('result').innerHTML = " +
                  "                            '<span style=\"color:red\">ERROR: ' + error.message + '</span>';" +
                  "                    });" +
                  "            } catch (error) {" +
                  "                document.getElementById('result').innerHTML = " +
                  "                    '<span style=\"color:red\">ERROR: ' + error.message + '</span>';" +
                  "            }" +
                  "        });" +
                  "    </script>" +
                  "</body>" +
                  "</html>";

    return context.Response.WriteAsync(html);
});

app.Run();