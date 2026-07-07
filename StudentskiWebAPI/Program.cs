using Microsoft.EntityFrameworkCore;
using StudentskiWebAPI.Data;

var builder = WebApplication.CreateBuilder(args);

const string CONNECTION_STRING =
    "Data Source=pabp.viser.edu.rs;" +
    "Initial Catalog=Studentska;" +
    "User ID=student;" +
    "Password=password;" +
    "TrustServerCertificate=True;" +
    "MultipleActiveResultSets=True";

builder.Services.AddDbContext<StudentskiContext>(options =>
    options.UseSqlServer(CONNECTION_STRING)  // ← direktno, bez GetConnectionString()
);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(o =>
    o.AddPolicy("VueFrontend", p =>
        p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Studentski Web API", Version = "v1" }));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

//Swager
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Studentski API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Studentski Web API";
});

 //redirect
app.MapGet("/", () => Results.Redirect("/swagger", permanent: false))
   .ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseCors("VueFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();


