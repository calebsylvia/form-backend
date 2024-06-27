using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using form_backend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy("FormPolicy", 
builder => {
    builder.WithOrigins("http://localhost:3000", "https://creating-a-form.vercel.app")
    .AllowAnyHeader()
    .AllowAnyMethod();
}));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

UserService _userService = new UserService();


app.MapGet("/GetUsers", () => _userService.GetAllUsers());

app.MapPost("/AddUser", (User user) => {
    _userService.AddUser(user);
});

app.UseCors("FormPolicy");

app.Run();

