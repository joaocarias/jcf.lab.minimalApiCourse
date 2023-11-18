using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repositories;
using MagicVilla_CouponAPI.Repositories.IRepsitories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICouponRepository, CouponRepository>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", async (ICouponRepository _couponRepository, ILogger<Program> _logger) =>
{
    _logger.Log(LogLevel.Information, "Getting all Coupons");

    APIResponse response = new()
    {
        Result = await _couponRepository.GetAllAsync(),
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };

    return Results.Ok(response);
}).WithName("GetCoupons").Produces<APIResponse>(200);

app.MapGet("/api/coupon/{id:int}", async (ICouponRepository _couponRepository, ILogger<Program> _logger, int id) =>
{
    APIResponse response = new()
    {
        Result =  await _couponRepository.GetAsync(id),
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };
    return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);

app.MapPost("/api/coupon", async (ICouponRepository _couponRepository, IMapper _mapper, 
    IValidator<CouponCreateDTO> _validation,
    [FromBody] CouponCreateDTO coupon_C_DTO) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    if(await _couponRepository.GetAsync(coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    var coupon = _mapper.Map<Coupon>(coupon_C_DTO);
    
    await _couponRepository.CreateAsync(coupon);
    await _couponRepository.SaveAsync();
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response = new()
    {
        Result = couponDTO,
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };

    //return Results.Ok(response);
    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id}, couponDTO); 
}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

app.MapPut("/api/coupon/{id:int}", async (ICouponRepository _couponRepository, IMapper _mapper,
    IValidator<CouponUpdateDTO> _validation,
    int id,
    [FromBody] CouponUpdateDTO coupon_U_DTO) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    if (await _couponRepository.GetAsync(coupon_U_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    await _couponRepository.UpdateAsync(_mapper.Map<Coupon>(coupon_U_DTO));
    await _couponRepository.SaveAsync();

    response.Result = _mapper.Map<CouponDTO>(await _couponRepository.GetAsync(coupon_U_DTO.Id));
    response.IsSuccess = true;
    response.StatusCode = System.Net.HttpStatusCode.OK;
    return Results.Ok(response);
})
    .WithName("UpdateCoupon")
    .Accepts<CouponUpdateDTO>("application/json")
    .Produces<APIResponse>(200).Produces(400);

app.MapDelete("/api/coupon/{id:int}", async (ICouponRepository _couponRepository, int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

    var coupon = await _couponRepository.GetAsync(id);
    if(coupon != null)
    {
        await _couponRepository.RemoveAsysc(coupon);
        await _couponRepository.SaveAsync();
        response.IsSuccess = true;
        response.StatusCode = System.Net.HttpStatusCode.NoContent;
        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }
});

app.UseHttpsRedirection();

app.Run();
