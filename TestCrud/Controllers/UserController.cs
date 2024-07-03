using CrudApp.Controllers;
using CrudApp.DB;
using CrudApp.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TestCrud.Entities;
using TestCrud.Models;
using TestCrud.Services;

namespace TestCrud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly UserServices _userServices;
        private readonly IValidator<User> _userValidator;
        private readonly IValidator<UserLogin> _userLoginValidator;
        public UserController(MongoDbService mongoDbService, IValidator<User> userValidator, IValidator<UserLogin> userLoginValidator)
        {
            _userCollection = mongoDbService.Database?.GetCollection<User>("user");
            _userServices = new UserServices(_userCollection);
            _userValidator = userValidator;
            _userLoginValidator = userLoginValidator;
        }

        // done
        [HttpPost("/api/[controller]/add")]
        public async Task<ActionResult> AddUser([FromBody] User user)
        {
            try
            {
                var validationResult = await _userValidator.ValidateAsync(user);
                if (!validationResult.IsValid)
                {
                    var errorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage;
                    return BadRequest(new ApiResponse<string> { Message = errorMessage });
                }
                var existUser = await _userServices.GetUserByEmailAsync(user.Email);
                if (existUser != null)
                {
                    return BadRequest(new ApiResponse<string> { Message = "This email already exists " + user.Email });
                }
                var response = await _userServices.CreateUserServiceAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = response.Data.Id.ToString() }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        // done
        [HttpGet("/api/[controller]/get_by_id/{id}")]
        public async Task<ActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userServices.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User Not Found.");
                }
                return Ok(user);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid ID format.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);

            }
        }

        // done
        [HttpGet("/api/[controller]/get_all")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                return await _userServices.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
        [HttpPost("/api/[controller]/login")]
        public async Task<ActionResult> Login([FromBody] UserLogin userLogin)
        {
            try
            {
                var validationResult = await _userLoginValidator.ValidateAsync(userLogin);
                if (!validationResult.IsValid)
                {
                    var errorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage;
                    return BadRequest(new ApiResponse<string> { Message = errorMessage });
                }
                var existUser = await _userServices.GetUserByEmailAsync(userLogin.Email);
                if (existUser == null)
                {
                    return BadRequest(new ApiResponse<string> { Message = "No User Found with this email"});
                }
                var matched = BCrypt.Net.BCrypt.Verify(userLogin.Password, existUser.Password);
                if (!matched)
                {
                    return Unauthorized(new ApiResponse<string> { Message = "Wrong password!" });
                }
                var jwtService = new JwtService("ajdddnb132h4vv23");
                var token = jwtService.GenerateJWTToken(existUser);
                
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}
