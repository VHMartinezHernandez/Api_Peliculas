using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio.CategoriaRepositorio;
using ApiPeliculas.Repositorio.PeliculaRepositorio;
using ApiPeliculas.Repositorio.UsuarioRepositorio;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


//Sporte para la autenticacion con .net identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>(); 


//Agregar la Autentication(Bearer)
builder.Services.AddSwaggerGen(options=>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = 
        "Autenticacion JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la parabra Bearer seguido de un [espacio] y despues su token en el campo de abajo.\r\n\r\n " +
        "Ejemplo: \"Bearer tkljkl125jhhk\"",
        Name ="Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


//Agregar Repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");


////Soporte para verionamineto
//var apiVerioningBuilder = builder.Services.AddApiVersioning(opcion =>
//{
//    opcion.AssumeDefaultVersionWhenUnspecified = true;
//    opcion.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
//    opcion.ReportApiVersions = true;
//opcion.ApiVersionReader = ApiVersionReader.Combine(
//    new QueryStringApiVersionReader("api-version")//?api-version=1.0
//    //new HeaderApiVersionReader("X-Version"),
//    //new MediaTypeApiVersionReader("ver"));
//        );

//});


//apiVerioningBuilder.AddApiExplorer(
//        opciones =>
//        {
//            opciones.GroupNameFormat = "'v'VVV";
//        }
//    );


//Agregar el AutoMapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));


// Registrar RespuestaAPI
builder.Services.AddScoped<RespuestaAPI>();


//Aqui se configura la Autenticacion
builder.Services.AddAuthentication
    (
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x => 
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        }
   );


//Soporte para CORS
//Se pueden habilitar: 1-Un dominio, 2-multiples dominios,
//3-cualquier dominio (Tener en cuenta seguridad)
//Usamos de ejemplo el dominio: http://localhost:3223, se debe cambiar por el correcto
//Se usa (*) para todos los dominios
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("http://localhost:3223").AllowAnyMethod().AllowAnyHeader();
}));






var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//Soporte para archivos estaticos como imagenes
app.UseStaticFiles();

app.UseHttpsRedirection();

//Soporte para CORS
app.UseCors("PoliticaCors");

//Soporte para Autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
