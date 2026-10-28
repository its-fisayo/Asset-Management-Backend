using System.Text;
using AssetManagementBackend.Data;
using AssetManagementBackend.Middleware;
using AssetManagementBackend.Models;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers and Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //Tell Swagger that our API uses JWT Bearer authentication.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    //Apply the Bearer authentication scheme to API operations.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Application services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductImageService>();
builder.Services.AddScoped<VendorService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<IssuanceService>();
builder.Services.AddScoped<StockMovementService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BrandService>();

// Password hashing
builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
        .WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5137"
            )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Read JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

// Configure JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Get the user ID from the JWT.
                var userIdClaim = context.Principal?
                .FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                // Reject tokens without a valid user ID.
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Fail("Invalid user identity.");
                    return;
                }

                // Retrieve the database context through dependency injection.
                var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<ApplicationDbContext>();

                // Fetch the user's latest account information.
                var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == userId);

                // Reject deleted or deactivated accouts
                if (user == null || !user.IsActive)
                {
                    context.Fail("This account is inactive or unavailable.");
                    return;
                }

                // Compare the role in the JWT with the current database role.
                var tokenRole = context.Principal?
                .FindFirst(
                    System.Security.Claims.ClaimTypes.Role)?
                    .Value;

                if (!string.Equals(
                    tokenRole,
                    user.Role,
                    StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail(
                        "Your account permissions have changed. Please log in again.");
                }
            }
        };
    });

// Authorization policies and [Authorize]
builder.Services.AddAuthorization();

var app = builder.Build();

// CSwagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();