using Gympt.Application.Facades;
using Gympt.Common.Middleware;
using Gympt.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<GymFacade>(); // scoped porque depende de UserApiClient

// Add Authentication and Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;

        // Para desarrollo HTTP
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Add Session storage
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Lax o None según tu setup
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

// Add HttpClient for various microservices
builder.Services.AddHttpClient<ClientApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5058");
});
builder.Services.AddHttpClient<MembershipApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5250");
});
builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5076/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<UserValidationMiddleware>(); // debe ir aquí
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();