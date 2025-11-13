using KurzUrl.Repository;
using KurzUrl.BusinessLayer;
using Microsoft.Extensions.Options;
using KurzUrl.Repository.Models;
using Microsoft.EntityFrameworkCore;
using KurzUrl.Repository.Interface;
using KurzUrl.BusinessLayer.Interface;
using KurzUrl.BusinessLayer.ViewModel;
using KurzUrl.Repository.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using KurzUrl.Infrastructure.Initializer;
using KurzUrl.Services;
using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using static IdentityModel.ClaimComparer;
using KurzUrl.BusinessLayer.Implementation;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services
.AddRepositoryLayer()
.AddBusinessLayer()
.AddCors(options => options.AddPolicy("cors", policy =>

    policy
    .WithOrigins("http://localhost:4201", "http://localhost:4200", "http://localhost:5057", "http://localhost:61595", "http://localhost:56873/")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
   


builder.Services.AddDbContext<ShortUrlContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("KurzUrlConnection"),
    sql => sql.MigrationsAssembly("KurzUrl.Repository")
));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "YourApp.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});


#region jwt
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    // options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
// options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JWTBearerSettings:Issuer"],
        ValidAudience = builder.Configuration["JWTBearerSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTBearerSettings:Key"])),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier
    };
})
.AddCookie(options =>
{
    options.Cookie.Name = "YourApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/";
})
.AddGoogle(Options =>
{
    Options.ClientId = builder.Configuration["Authentication:ClientId"];
    Options.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
    Options.CallbackPath = builder.Configuration["Authentication:CallbackPath"];


});
#endregion
/*builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //Require user to be authenticated for all endpoints by default
});*/

builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kurz-Urlc ", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"Enter 'Bearer' [space] and your token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    },
                    Scheme="oauth2",
                    Name="Bearer",
                    In=ParameterLocation.Header
                },
                new List<string>()
            }

        });
    });

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ShortUrlContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    {
        var dataContextsql = scope.ServiceProvider.GetRequiredService<ShortUrlContext>();
        //dataContextsql.Database.Migrate();
    }

    var f = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    f.Initialize();
}

app.UseCors("cors");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapGet("/api/{string}", async (HttpContext? context , ShortUrlContext dbContext) =>
{
    string[]? arrString = context?.Request?.Path.Value?.ToString().Split('/');
    var val = arrString[2];

    var rtn = await dbContext.TblUrlDetails
        .Where(ss => ss.ShortUrl.Contains(val))
        .FirstOrDefaultAsync();
  
    if (rtn?.MainUrl is null)
        return Results.NotFound("ShortUrl Is Not Valid");

    // Redirect to the main URL
    return Results.Redirect("https://" + rtn.MainUrl, permanent: false);
});

app.UseHttpsRedirection();



app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.Run();