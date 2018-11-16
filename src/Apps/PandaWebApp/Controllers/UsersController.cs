using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PandaWebApp.Enums;
using PandaWebApp.Models;
using PandaWebApp.ViewModels.Users;
using SIS.HTTP.Cookies;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using SIS.MvcFramework.Services;

namespace PandaWebApp.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IHashService hashService;

        public UsersController(IHashService hashService)
        {
            this.hashService = hashService;
        }

        [HttpGet("/users/logout")]
        public IHttpResponse Logout()
        {
            if (!this.Request.Cookies.ContainsCookie(".auth-cakes"))
            {
                return this.Redirect("/home/index");
            }

            var cookie = this.Request.Cookies.GetCookie(".auth-cakes");
            cookie.Delete();
            this.Response.Cookies.Add(cookie);
            return this.Redirect("/");
        }

        [HttpGet("/users/login")]
        public IHttpResponse Login()
        {
            if (this.User.IsLoggedIn)
            {
                return this.Redirect("/home/index");
            }

            return this.View("Users/Login");
        }

        [HttpPost("/users/login")]
        public IHttpResponse DoLogin(DoLoginInputModel model)
        {
            if (this.User.IsLoggedIn)
            {
                return this.Redirect("/home/index");
            }

            var hashedPassword = this.hashService.Hash(model.Password);

            var user = this.Db.Users.FirstOrDefault(u =>
                u.Username == model.Username.Trim()
                && u.Password == hashedPassword);

            if (user == null)
            {
                return this.BadRequestError("Invalid username or password.");
            }

            var mvcUser = new MvcUserInfo { Username = user.Username, Role = user.Role.ToString(), Info = user.Email };
            var cookieContent = this.UserCookieService.GetUserCookie(mvcUser);

            var cookie = new HttpCookie(".auth-cakes", cookieContent, 7) { HttpOnly = true };
            this.Response.Cookies.Add(cookie);
            return this.Redirect("/home/index");
        }

        [HttpGet("/users/register")]
        public IHttpResponse Register()
        {
            if (this.User.IsLoggedIn)
            {
                return this.Redirect("/home/index");
            }

            return this.View("Users/Register");
        }

        [HttpPost("/users/register")]
        public IHttpResponse DoRegister(DoRegisterInputModel model)
        {
            if (this.User.IsLoggedIn)
            {
                return this.Redirect("/home/index");
            }

            if (string.IsNullOrEmpty(model.Username) || model.Username.Trim().Length < 4)
            {
                return this.BadRequestError("Please provide valid username with length of 4 or more characters.");
            }

            if (string.IsNullOrEmpty(model.Email) || model.Email.Trim().Length < 6 || !model.Email.Contains("@"))
            {
                return this.BadRequestError("Please provide valid email address.");
            }

            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 4)
            {
                return this.BadRequestError("Please provide valid password with length of 4 or more characters.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return this.BadRequestError("Passwords does not match.");
            }

            if (this.Db.Users.Any(u => u.Username == model.Username.Trim()))
            {
                return this.BadRequestError("User with the same username already exists.");
            }

            var hashedPassword = this.hashService.Hash(model.Password);

            var user = new User();
            user.Username = model.Username.Trim();
            user.Password = hashedPassword;
            user.Email = model.Email;
            user.Role = this.Db.Users.Any() ? Role.User : Role.Admin;

            this.Db.Users.Add(user);

            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.ServerError(e.Message);
            }

            return this.Redirect("/users/login");
        }
    }
}