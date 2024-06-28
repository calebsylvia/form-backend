using Microsoft.EntityFrameworkCore;
using form_backend;
using form_backend.Services.Context;
using form_backend.Models.DTOs;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FormBackend"); 

builder.Services.AddCors(options => options.AddPolicy("FormPolicy", 
builder => {
    builder.WithOrigins("http://localhost:3000", "https://creating-a-form.vercel.app")
    .AllowAnyHeader()
    .AllowAnyMethod();
}));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<UserDb>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.MapGet("/GetUsers", (UserDb user) => user.Users.ToList());

app.MapPost("/AddUser", (User user, UserDb db) => {
    db.Users.Add(user);
    return db.SaveChanges() != 0;
});

app.MapPost("/Create", (UserDb db, CreateAccountDTO user) => {
    if(!CheckExists(user.Email, db)){
        
        User newUser = new User();

        var hash = SaltAndHash(user.Password);

        newUser.Salt = hash.Salt;
        newUser.Hash = hash.Hash;
        newUser.Email = user.Email;
        newUser.IsAdmin = user.IsAdmin;

        db.Users.Add(newUser);

        return db.SaveChanges() != 0;
    }else{
        return false;
    }
});
app.MapPost("/Login", (UserDb db, LoginDTO login) => 
{

    if(CheckExists(login.Email, db))
    {
        User user = GetUser(login.Email, db);

        if(CheckPass(login.Password, user.Salt, user.Hash))
        {
            var secret = new byte[32];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secret);
            }

            var ssKey = new SymmetricSecurityKey(secret);
            var ssC = new SigningCredentials(ssKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                        issuer: "http://localhost:5000",
                        audience: "http://localhost:5000",
                        claims: new List<Claim>{
                            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                            new Claim(ClaimTypes.Email, user.Email), 
                            new Claim("IsManager", user.IsAdmin.ToString(), ClaimValueTypes.Boolean)
                        },
                        expires: DateTime.Now.AddMinutes(60), 
                        signingCredentials: ssC 
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

                    return Results.Ok(new { Token = tokenString });
        }
    }
    return Results.Unauthorized();
});

app.MapPut("/UpdateStudent", (UserDb db, FormModel student) => {

});

app.UseCors("FormPolicy");

app.Run();


static PasswordDTO SaltAndHash (string password) 
{
    PasswordDTO newHash = new PasswordDTO();

    byte[] SaltByte = new byte[64];
    var rng = RandomNumberGenerator.Create();

    rng.GetNonZeroBytes(SaltByte);
    string salt = Convert.ToBase64String(SaltByte);

    Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, SaltByte, 10000, HashAlgorithmName.SHA256);
    string hash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256));
    
    newHash.Hash = hash;
    newHash.Salt = salt;

    return newHash;
}

static bool CheckExists(string username, UserDb db)
{
    return db.Users.SingleOrDefault(u => u.Email == username) != null;
}

static bool CheckStudentExists(string first, UserDb db)
{
    return db.FormModels.SingleOrDefault(u => u.First == first) != null;
}

static bool CheckPass(string password, string storedHash, string storedSalt)
{

            byte[] SaltBytes = Convert.FromBase64String(storedSalt);

            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, SaltBytes, 10000, HashAlgorithmName.SHA256);

            string newHash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256));

            return newHash == storedHash;
}

static User GetUser(string username, UserDb db)
{
    return db.Users.FirstOrDefault(u => u.Email == username);
}

static FormModel GetStudent(string First, UserDb db)
{
    return db.FormModels.FirstOrDefault(u => u.First == First);
}
