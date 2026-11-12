using FitLog.Host.Auth;
using FitLog.Host.Swagger;
using FitLog.Shared;
using FitLog.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerWithOAuth(builder.Configuration);

builder.Services.AddOidcAuthentication(builder.Configuration);
builder.Services.AddShared(builder.Configuration);
builder.Services.AddUsers(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithOAuth();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
