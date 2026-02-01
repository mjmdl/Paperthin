using Microsoft.EntityFrameworkCore;
using Paperthin;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AccessTokenSettings>(builder.Configuration.GetSection("AccessTokenSettings"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.UseApiValidation();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<AccessTokenEncoder>();

builder.Services.AddScoped<AccountService>();

builder.Services.AddScoped<AccountRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Database.CanConnect())
        throw new InvalidOperationException("Could not connect to the database.");
}

app.UseApiExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
