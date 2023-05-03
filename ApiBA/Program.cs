using System.Text;
using ApiBA;
using ApiBA.Authorize;
using ApiBA.Contexts;
using ApiBA.Data;
using ApiBA.Options;
using ApiBA.Repositories;
using ApiBA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var configServices = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("RequestManager");

builder.Services.Configure<JWT>(configServices.GetSection("JWT"));
builder.Services.Configure<ApiUrlOption>(configServices.GetSection("ApiUrlOption"));
builder.Services.Configure<ApiLoginOption>(configServices.GetSection("ApiLoginOption"));
//User Manager Service
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<BaseDBContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoginConfigService, LoginConfigService>();
builder.Services.AddScoped<IRequestLogsService, RequestLogsService>();

builder.Services.AddScoped<IAuthorizationHandler, IpCheckHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MaxRequestHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameIpPolicy",
        policy => policy.Requirements.Add(new IpCheckRequirement { IpClaimRequired = true }));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MaxRequest",
        policy => policy.Requirements.Add(new MaxRequestRequirement()));
});
//Adding DB Context with MSSQL
builder.Services.AddDbContext<BaseDBContext>(options =>
    options.UseSqlServer(connectionString));
//Adding Athentication - JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = configServices["JWT:Issuer"],
            ValidAudience = configServices["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configServices["JWT:Key"]))
        };
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        //Seed Default Users
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

