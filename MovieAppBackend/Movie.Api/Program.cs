using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000") // <-- frontend URL
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});



// Add DbContext with snake_case naming convention
builder.Services.AddDbContext<ImdbDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention()
);
// Repositories 
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

// Swagger, Controllers, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");  //  use CORS policy
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
