using CollegeWebsite.Components;
using CollegeWebsite.Models;
using CollegeWebsite.Services;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);

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

// Add memory cache (required for session)
builder.Services.AddDistributedMemoryCache();

// Add session with proper configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

// Enable session
app.UseSession();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();