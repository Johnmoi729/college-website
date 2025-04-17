using CollegeWebsite.Components;
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

// Add memory cache (required for session)
builder.Services.AddDistributedMemoryCache();

// Add session with proper configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add HttpContextAccessor (needed for accessing session from services)
builder.Services.AddHttpContextAccessor();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    // Only run schema validation in development environment
    // The schema validation would be done here, but let's implement it first
}

// Enable session
app.UseSession();

app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();