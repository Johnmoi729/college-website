using CollegeWebsite.Components;
using CollegeWebsite.Components.Shared;
using CollegeWebsite.Models;
using CollegeWebsite.Services;
using CollegeWebsite.Utilities;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

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

// ** SIMPLIFIED AND CLEAR CONFIGURATION ** 
// Server-side Blazor configuration
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
});

// Razor components with interactive server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the error handler for Blazor components
builder.Services.AddScoped<IErrorBoundaryLogger, ErrorHandler>();

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

// Add HttpContext and Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CollegeAdmin.Session";

    // Critical settings for session persistence
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Use 'Always' in production with HTTPS
    options.Cookie.Path = "/";
    options.Cookie.Domain = null;
});

// Register our services
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
builder.Services.AddScoped<IAuthStateService, AuthStateService>();

// Enhanced logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);

    // Add more detailed logging for Blazor
    logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Debug);
    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// ** CRITICAL FIX: Separate the Blazor framework static files from other middleware **
// First, handle requests for Blazor framework files by special mapping
app.Map("/_framework", app =>
{
    app.UseStaticFiles();
});
app.Map("/_blazor", app =>
{
    app.UseStaticFiles();
});

// Main application middleware pipeline
app.UseRouting();
app.UseSession();
app.UseStaticFiles(); // This serves other static files
app.UseAntiforgery();

// Map Blazor hub
app.MapBlazorHub();

// Map Razor components with the correct render mode
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

// Diagnostic endpoint for Blazor status
app.MapGet("/blazor-status", async context =>
{
    string html = @"
    <html>
    <head>
        <title>Blazor Status</title>
        <style>
            body { font-family: Arial; margin: 20px; }
            .success { color: green; }
            .error { color: red; }
        </style>
    </head>
    <body>
        <h1>Blazor Status Check</h1>
        <div id='status'>Checking Blazor status...</div>
        
        <script>
            document.addEventListener('DOMContentLoaded', function() {
                var status = '';
                
                // Check for Blazor
                if (window.Blazor) {
                    status += '<p class=""success"">✓ Blazor is loaded</p>';
                } else {
                    status += '<p class=""error"">✗ Blazor is not loaded</p>';
                }
                
                // Check for SignalR
                if (window.signalR) {
                    status += '<p class=""success"">✓ SignalR is loaded</p>';
                } else {
                    status += '<p class=""error"">✗ SignalR is not loaded</p>';
                }
                
                // Test fetching the Blazor script directly
                fetch('/_framework/blazor.server.js')
                    .then(response => {
                        if (response.ok) {
                            status += '<p class=""success"">✓ blazor.server.js is accessible</p>';
                        } else {
                            status += '<p class=""error"">✗ blazor.server.js returned ' + response.status + '</p>';
                        }
                        document.getElementById('status').innerHTML = status;
                    })
                    .catch(error => {
                        status += '<p class=""error"">✗ blazor.server.js error: ' + error.message + '</p>';
                        document.getElementById('status').innerHTML = status;
                    });
            });
        </script>
    </body>
    </html>";

    await context.Response.WriteAsync(html);
});

app.Run();