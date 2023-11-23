using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repositories.IRepsitories;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void ConfigureAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/login", Login).WithName("Login").Accepts<LoginRequestDTO>("application/json")
                        .Produces<APIResponse>(200).Produces(400);
            app.MapPost("/api/register", Register).WithName("Register").Accepts<RegisterationRequestDTO>("application/json")
                        .Produces<APIResponse>(200).Produces(400);
        }

        private async static Task<IResult> Login(IAuthRepository authRepository, 
            [FromBody] LoginRequestDTO loginRequestDTO)
        {
            APIResponse response = new APIResponse { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            var loginResponse = await authRepository.Login(loginRequestDTO);

            if(loginResponse == null)
            {
                response.ErrorMessages.Add("Username or password is incorrect");
                return Results.BadRequest(response);
            }

            response.Result = loginResponse;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            
            return Results.Ok(response);    
        }

        private async static Task<IResult> Register(IAuthRepository authRepository,
            [FromBody] RegisterationRequestDTO model)
        {
            APIResponse response = new APIResponse { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            bool isUnique = authRepository.IsUniqueUser(model.UserName);
            if (!isUnique)
            {
                response.ErrorMessages.Add("Username already exists");
                return Results.BadRequest(response);
            }
            
            var loginResponse = await authRepository.Register(model);

            if (loginResponse == null)
            {
                response.ErrorMessages.Add("Username or password is incorrect");
                return Results.BadRequest(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;

            return Results.Ok(response);
        }
    }
}
