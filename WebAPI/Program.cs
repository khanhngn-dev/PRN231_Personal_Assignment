using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositorty;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Authentication
var jwtSecret = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
builder.Services.AddJWTAuthentication(jwtIssuer, jwtSecret);

// Add services to the container
builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddDatabase();
builder.Services.AddUnitOfWork();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearerAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
