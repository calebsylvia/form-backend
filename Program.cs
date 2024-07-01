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

app.MapGet("/GetStudents", (UserDb user) => user.FormModels.ToList());

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

app.MapPut("/ResetPassword", (UserDb db, ResetPasswordDTO password) => 
{
    var user = GetUser(password.Email, db);


    if(user != null)
    {
        Console.WriteLine("User Not Found");
        User foundUser = GetUser(password.Email, db);

    if(VerifyPassword(password.OldPassword, foundUser.Salt, foundUser.Hash))
    {
        Console.WriteLine("Password Incorrect");
        var newHash = SaltAndHash(password.NewPassword);
        
        foundUser.Salt = newHash.Salt;
        foundUser.Hash = newHash.Hash;

        db.Update<User>(foundUser);
    }
    }


    return db.SaveChanges() != 0;
});


app.MapPost("/Login", Login);

app.MapPost("/SubmitForm", (UserDb user, FormModel student) => 
{
    FormModel studentInfo = new FormModel();

    studentInfo.First = student.First;
    studentInfo.Last = student.Last;
    studentInfo.Email = student.Email;
    studentInfo.DoB = student.DoB;
    studentInfo.Phone = student.Phone;
    studentInfo.Address = student.Address;

    user.FormModels.Add(studentInfo);

    return user.SaveChanges() != 0;
});

app.MapPut("/UpdateStudent/{Email}", (UserDb db, EditAccountDTO student, string Email) => {

    var existingStudent = GetStudent(Email, db);

    if(existingStudent != null)
    {
        existingStudent.First = student.First;
        existingStudent.Last = student.Last;
        existingStudent.Email = student.Email;
        existingStudent.Phone = student.Phone;
        existingStudent.Address = student.Address;
        existingStudent.DoB = student.DoB;
    }

    return db.SaveChanges() != 0;
});

app.MapDelete("/RemoveStudent/{Email}", (UserDb db, string Email) => 
{
    var userToDelete = GetStudent(Email, db);
    db.FormModels.Remove(userToDelete);
    return db.SaveChanges() != 0;
});

app.UseCors("FormPolicy");

app.Run();

static PasswordDTO SaltAndHash(string password)
{
    PasswordDTO newHash = new PasswordDTO();

    byte[] saltBytes = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetNonZeroBytes(saltBytes);
    }
    string salt = Convert.ToBase64String(saltBytes);

    using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
    {
        string hash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256)); 
        newHash.Hash = hash;
    }
    newHash.Salt = salt;

    return newHash;
}


static bool CheckExists(string username, UserDb db)
{
    return db.Users.SingleOrDefault(u => u.Email == username) != null;
}


static bool CheckStudentExists(string Email, UserDb db)
{
    return db.FormModels.SingleOrDefault(u => u.Email == Email) != null;
}

static bool VerifyPassword(string password, string storedSalt, string storedHash)
{
    byte[] saltBytes = Convert.FromBase64String(storedSalt);
    using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
    {
        string hash = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256)); 
        return hash == storedHash;
    }
}


static User GetUser(string username, UserDb db)
{
    return db.Users.FirstOrDefault(u => u.Email == username);
}

static FormModel GetStudent(string Email, UserDb db)
{
    return db.FormModels.FirstOrDefault(u => u.Email == Email);
}

static IResult Login(LoginDTO login, UserDb db)
{
    IResult result = Results.Unauthorized();

    if (CheckExists(login.Email, db))
    {
        User user = GetUser(login.Email, db);

            bool hashMatches = VerifyPassword(login.Password, user.Salt, user.Hash);
            Console.WriteLine($"Hashes match: {hashMatches}");

            Console.WriteLine($"Stored Salt: {user.Salt}");
            Console.WriteLine($"Stored Hash: {user.Hash}");
        if (user != null && VerifyPassword(login.Password, user.Salt, user.Hash))
        {
            Console.WriteLine("Password verified successfully.");

            var secret = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secret);
            }

            var ssKey = new SymmetricSecurityKey(secret);
            var ssC = new SigningCredentials(ssKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: "http://localhost:5000",
                audience: "http://localhost:5000",
                claims: new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("IsAdmin", user.IsAdmin.ToString(), ClaimValueTypes.Boolean)
                },
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: ssC
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            result = Results.Ok(new {Token = tokenString, AdminStatus = user.IsAdmin });
        }
        else
        {
            Console.WriteLine("Password verification failed.");
        }
    }

    return result;
}

static FormModel GetUserByUsername(string first, UserDb db)
{
    return db.FormModels.SingleOrDefault(x => x.First == first);
}