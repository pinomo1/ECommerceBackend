using Azure.Storage.Blobs;
using ECommerce1.Extensions;
using ECommerce1.Models;
using ECommerce1.Models.Validators;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

#region Services
services.AddCors();
services.AddDbContextPool<ResourceDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("ResourcesHost")));
services.AddDbContextPool<AccountDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("AccountHost")));
services.AddIdentity<AuthUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;

    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AccountDbContext>();

services.AddControllers().AddJsonOptions(x =>
   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
services.AddEndpointsApiExplorer();
services.AddSwagger();
// services.AddJwtAuthentication(config["Secret"], new List<string>() { "User", "Seller", "Admin" });
services.AddJwtAuthentication("3c66ae61-d405-4d24-8622-096087df7d22", new List<string>() { "User", "Seller", "Admin" });
services.AddScoped<IValidator<AddProductViewModel>, ProductValidator>();
services.AddScoped<IValidator<UserCredentials>, UserRegistrationValidator>();
services.AddScoped<IValidator<StaffCredentials>, StaffRegistrationValidator>();
services.AddScoped<IValidator<SellerCredentials>, SellerRegistrationValidator>();
services.AddScoped<IValidator<LoginCredentials>, LoginValidator>();
services.AddTransient<BlobServiceClient>(x =>
{
    return new BlobServiceClient(config.GetConnectionString("BlobStorage"));
});
services.AddTransient<BlobWorker>();

services.AddAzureClients(builder =>
{
    builder.AddBlobServiceClient(config["ConnectionStrings:BlobStorage:blob"], preferMsi: true);
    builder.AddQueueServiceClient(config["ConnectionStrings:BlobStorage:queue"], preferMsi: true);
});
#endregion

#region Configure
var app = builder.Build();

app.UseCors((options) =>
{
    options.WithOrigins()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed((x) => true)
    .AllowCredentials();
});

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Site API");
});

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion