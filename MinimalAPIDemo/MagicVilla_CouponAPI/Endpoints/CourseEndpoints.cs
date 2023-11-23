using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repositories.IRepsitories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MagicVilla_CouponAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CourseEndpoints
    {
        public static async void ConfigureCouponEndpoints(this WebApplication app)
        {
            app.MapGet("/api/coupon", GetAllCoupon)
                .WithName("GetCoupons")
                .Produces<APIResponse>(200)
                .RequireAuthorization("AdminOnly");

            app.MapGet("/api/coupon/{id:int}", GetCounpon).WithName("GetCoupon").Produces<APIResponse>(200);

            app.MapPost("/api/coupon", PostCoupon).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

            app.MapPut("/api/coupon/{id:int}", PutCoupon).WithName("UpdateCoupon").Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400);

            app.MapDelete("/api/coupon/{id:int}", DeleteCoupon);

            app.MapGet("/api/coupon/special", ([AsParameters] CouponRequest req, AppDbContext _db) =>
            {
                if (req.CouponName != null)
                {
                    return _db.Coupons.Where(x => x.Name.Contains(req.CouponName)).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);
                }

                return _db.Coupons.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);
            });
        }

        #region Privates

        [Authorize]
        private static async Task<IResult> GetCounpon(ICouponRepository _couponRepository, ILogger<Program> _logger, int id) 
        {
            APIResponse response = new()
            {
                Result = await _couponRepository.GetAsync(id),
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
            return Results.Ok(response);
        }

        [Authorize]
        private static async Task<IResult> PostCoupon(ICouponRepository _couponRepository, IMapper _mapper,
                IValidator<CouponCreateDTO> _validation,
                [FromBody] CouponCreateDTO coupon_C_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

            var validationResult = _validation.ValidateAsync(coupon_C_DTO).Result;
            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }

            if (await _couponRepository.GetAsync(coupon_C_DTO.Name.ToLower()) != null)
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

            return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
        }

        [Authorize]
        private static async Task<IResult> PutCoupon(ICouponRepository _couponRepository, IMapper _mapper,
                IValidator<CouponUpdateDTO> _validation,
                int id,
                [FromBody] CouponUpdateDTO coupon_U_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

            var validationResult = _validation.ValidateAsync(coupon_U_DTO).Result;
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
        }

        [Authorize]
        private static async Task<IResult> DeleteCoupon(ICouponRepository _couponRepository, int id)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.BadRequest };

            var coupon =  await _couponRepository.GetAsync(id);
            if (coupon != null)
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
        }

        [Authorize]
        private static async Task<IResult> GetAllCoupon(ICouponRepository _couponRepository, ILogger<Program> _logger)
        {
            APIResponse response = new();
            _logger.Log(LogLevel.Information, "Getting all Coupons");

            response.Result = await _couponRepository.GetAllAsync();
            response.IsSuccess = true;
            response.StatusCode = System.Net.HttpStatusCode.OK;            

            return Results.Ok(response);
        }

        #endregion
    }

    class CouponRequest
    {
        public string CouponName { get; set; }
        [FromHeader(Name = "PageSize")]
        public int PageSize { get; set; }
        [FromHeader(Name = "Page")]
        public int Page { get; set; }
        public ILogger<CouponRequest> Logger { get; set; }
    }
}


