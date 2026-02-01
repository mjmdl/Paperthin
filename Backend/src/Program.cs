using Microsoft.EntityFrameworkCore;

namespace Paperthin;

public class Program
{
    static void Main(string[] args)
    {
        var app = CreateWebApplication(args);
        ConfigureApplication(app);
        app.Run();
    }

    static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<AccessTokenSettings>(builder.Configuration.GetSection("AccessTokenSettings"));
        builder.Services.AddSingleton<AccessTokenEncoder>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddScoped<AccountRepository>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers();
        builder.Services.UseApiValidation();

        builder.Services.AddScoped<AccountService>();

        return builder.Build();
    }

    static void ConfigureApplication(WebApplication app)
    {
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
    }
}
