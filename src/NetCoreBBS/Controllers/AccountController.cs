﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NetCoreBBS.Entities;
using NetCoreBBS.ViewModels;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreBBS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _logger = logger;
        }

        public UserManager<User> UserManager { get; }

        public SignInManager<User> SignInManager { get; }

        //
        // GET: /Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("Logged in {userName}.", model.UserName);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                _logger.LogWarning("Failed to log in {userName}.", model.UserName);
                ModelState.AddModelError("", "用户名或密码错误");
                return View(model);
            }
        }

        //
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.UserName, Email = model.Email,CreateOn=DateTime.Now,LastTime=DateTime.Now };
                //将用户名存储到identity的储存器中去
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {userName} was created.", model.Email);

                    //生成用户的指定令牌
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                    //通过使用指定的动作名称、控制器名称、路由值和使用的协议，为操作方法生成一个完全限定或绝对的URL。 将用户的userid和code传递到url上
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    // 提供邮件服务，注册成功后 向用户的邮箱里发送一封邮件 让他确认注册
                    await MessageServices.SendEmailAsync(model.Email, "Confirm your account",
                        "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    if (user.UserName.ToLower().Equals("wzl"))
                    {
                        //给指定用户添加权限
                        await UserManager.AddClaimAsync(user, new Claim("Admin", "Allowed"));
                    }
                    else if(user.UserName.ToLower().Equals("wss"))
                    {
                        //给指定用户添加权限
                        await UserManager.AddClaimAsync(user, new Claim("Wss", "Allowed"));
                    }
                    return RedirectToAction("Login");
                }
                AddErrors(result);
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            var userName = HttpContext.User.Identity.Name;

            await SignInManager.SignOutAsync();

            _logger.LogInformation("{userName} logged out.", userName);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        #region 辅助方法

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogWarning("Error in creating user: {error}", error.Description);
            }
        }

        private Task<User> GetCurrentUserAsync()
        {
            return UserManager.GetUserAsync(HttpContext.User);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion
    }
}
