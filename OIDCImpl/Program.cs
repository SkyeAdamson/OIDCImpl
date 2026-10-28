using OIDCImpl;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
RegistrationUtil.RegisterServices(builder.Services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();