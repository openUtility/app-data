using app_data_switch.config;
using app_data_switch.Service;
using app_data_switch.Data;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add memory cache dependencies 
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add dependencies
builder.Services.AddTransient<MySqlConnection>(_ => new MySqlConnection(builder.Configuration.GetConnectionString("switch")));
builder.Services.AddSingleton(builder.Configuration.GetSection("SwitchEndpointConfiguration").Get<SwitchEndpointConfiguration>());
builder.Services.AddScoped<ISwitchService, SwitchService>();
builder.Services.AddScoped<ISwitchSource, SwitchSourceTSql>();
builder.Services.AddHealthChecks();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler(errorApp => {
        errorApp.Run(async context => {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/plain";
            
            await context.Response.WriteAsync("false");
            var exceptionHandlerPathFeature = 
                context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        });
    });
}

// disabled for now
// app.UseHttpsRedirection();

app.MapHealthChecks("/healthz").RequireHost("*:5001");;

app.MapControllers();

app.Run();
