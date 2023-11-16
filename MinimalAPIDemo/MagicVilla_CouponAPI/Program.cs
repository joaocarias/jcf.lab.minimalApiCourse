using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    _logger.Log(LogLevel.Information, "Getting all Coupons");

    APIResponse response = new()
    {
        Result = CouponStore.Coupons,
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };

    return Results.Ok(response);
}).WithName("GetCoupons").Produces<APIResponse>(200);

app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
{
    APIResponse response = new()
    {
        Result = CouponStore.Coupons.FirstOrDefault(u => u.Id == id),
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };
    return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);

app.MapPost("/api/coupon", async (IMapper _mapper, 
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

    if(CouponStore.Coupons.FirstOrDefault(u => u.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    var coupon = _mapper.Map<Coupon>(coupon_C_DTO);

    coupon.Id = CouponStore.Coupons.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
    CouponStore.Coupons.Add(coupon);
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response = new()
    {
        Result = couponDTO,
        IsSuccess = true,
        StatusCode = System.Net.HttpStatusCode.OK
    };

    return Results.Ok(response);
    //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id}, couponDTO); 
}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

app.MapPut("/api/coupon/{id:int}", async (IMapper _mapper,
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

    if (CouponStore.Coupons.FirstOrDefault(u => u.Name.ToLower() == coupon_U_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    var coupon = CouponStore.Coupons.FirstOrDefault(u => u.Id == coupon_U_DTO.Id);
    coupon.IsActive = coupon_U_DTO.IsActive;
    coupon.Name = coupon_U_DTO.Name;
    coupon.Percent = coupon_U_DTO.Percent;
    coupon.LastUpdated = DateTime.Now;

    response.Result = _mapper.Map<CouponDTO>(coupon);
    response.IsSuccess = true;
    response.StatusCode = System.Net.HttpStatusCode.OK;
    return Results.Ok(response);
})
    .WithName("UpdateCoupon")
    .Accepts<CouponUpdateDTO>("application/json")
    .Produces<APIResponse>(201).Produces(400);

app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

    var coupon = CouponStore.Coupons.FirstOrDefault(u => u.Id == id);
    if(coupon != null)
    {
        CouponStore.Coupons.Remove(coupon);
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
