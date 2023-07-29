﻿namespace App.Controllers
{
    using App.Authentication;
    using Core;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using Services.DTO.User;

    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IUserManager userManager;
        private readonly IJwtProvider jwtProvider;

        public UserController(
            IUserService userService, 
            IUserManager userManager,
            IJwtProvider jwtProvider)
        {
            this.userService = userService;
            this.userManager = userManager;
            this.jwtProvider = jwtProvider;
        }

        [HttpGet("~/api/Profile/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Profile(int id)
        {

            if (id <= 0) return BadRequest(id);

            if (!this.userManager.IsAuhenticated) return StatusCode(StatusCodes.Status401Unauthorized);

            UserDTO user = await this.userService.FindIdAsync(id);

            return Ok(user);
        }

        [HttpPost("~/api/Register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] UserCreateDTO userDTO)
        {
            if (userDTO == null) return BadRequest(userDTO); // TODO: Add prop validations.


            if (await this.userService.ContainsEmailAsync(userDTO.Email))
            {
                ModelState.AddModelError("CustomError", $"User with email {userDTO.Email} already exist!");
                return BadRequest(ModelState);
            }

            UserDTO user = await this.userService.CreateAsync(userDTO.Email, userDTO.Password);

            string token = this.jwtProvider.Generate(user);

            return Ok(token);
        }
    }
}
