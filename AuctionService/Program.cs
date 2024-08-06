using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MassTransit.EntityFrameworkCoreIntegration;
using AuctionService.Consumers;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(u =>
{
     u.AddEntityFrameworkOutbox<AuctionDbContext>(c =>
     {
         c.QueryDelay = TimeSpan.FromSeconds(10);
          
         c.UsePostgres();
         c.UseBusOutbox();

     });
    u.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    u.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    u.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();
