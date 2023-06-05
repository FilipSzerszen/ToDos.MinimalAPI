using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using ToDos.MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// rejestracja serwisu - pod interfejsem IToDoService chcemy dostaæ instancjê ToDoService
builder.Services.AddSingleton<IToDoService, ToDoService>();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(ToDoValidator));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(cfg =>
    {
        cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JwtIssuer"],
            ValidAudience = builder.Configuration["JwtIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.MapGet("/todos", (IToDoService service) => service.GetAll());
//// jawny sposób podawania œcie¿ki do parametrów:
////app.MapGet("/todos/{id}", ([FromServices]IToDoService service, [FromQuery]Guid id) => service.GetById(id));
//app.MapGet("/todos/{id}", (IToDoService service, Guid id) => service.GetById(id));
//app.MapPost("/todos", (IToDoService service, ToDo toDo) => service.Create(toDo));
//app.MapPut("/todos/{id}", (IToDoService service, ToDo toDo, Guid id) => service.Update(toDo));
//app.MapDelete("/todos/{id}", (IToDoService service, Guid id) => service.Delete(id));

// minimal api z mo¿liwoœci¹ napisania testów jednostkowych (bez obiektów anonimowych w lambdzie)
//app.MapGet("/todos", ToDoRequests.GetAll);
//app.MapGet("/todos/{id}", ToDoRequests.GetById);
//app.MapPost("/todos", ToDoRequests.Create);
//app.MapPut("/todos/{id}", ToDoRequests.Update);
//app.MapDelete("/todos/{id}", ToDoRequests.Delete);

app.RegisterEndpoints();

app.MapGet("/token", () =>
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "user-id"),
        new Claim(ClaimTypes.Name, "Test Name"),
        new Claim(ClaimTypes.Role, "Admin"),
    };

    var token = new JwtSecurityToken
    (
        issuer: builder.Configuration["JwtIssuer"],
        audience: builder.Configuration["JwtIssuer"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(60),
        notBefore: DateTime.UtcNow,
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"])),
            SecurityAlgorithms.HmacSha256)
    );

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
    return jwtToken;
});

app.MapGet("/hello", (ClaimsPrincipal user) =>
{
    var userName = user.Identity.Name;
    return $"Hello {userName}";
});


app.Run();
