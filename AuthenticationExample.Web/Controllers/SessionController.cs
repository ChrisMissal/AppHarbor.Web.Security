﻿using System.Linq;
using System.Web.Mvc;
using AppHarbor.Web.Security;
using AuthenticationExample.Web.Model;
using AuthenticationExample.Web.PersistenceSupport;
using AuthenticationExample.Web.ViewModels;

namespace AuthenticationExample.Web.Controllers
{
	public class SessionController : Controller
	{
		private readonly IAuthenticator _authenticator;
		private readonly IRepository _repository;

		public SessionController(IAuthenticator authenticator, IRepository repository)
		{
			_authenticator = authenticator;
			_repository = repository;
		}

		[HttpGet]
		public ActionResult New(string returnUrl)
		{
			return View(new SessionViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		public ActionResult Create(SessionViewModel sessionViewModel)
		{
			User user = null;
			if (ModelState.IsValid)
			{
				user = _repository.GetAll<User>().SingleOrDefault(x => x.Username == sessionViewModel.Username);
				if (user == null)
				{
					ModelState.AddModelError("Username", "User not found");
				}
			}

			if (ModelState.IsValid)
			{
				if (!BCrypt.Net.BCrypt.Verify(sessionViewModel.Password, user.Password))
				{
					ModelState.AddModelError("Password", "Wrong password");
				}
			}

			if (ModelState.IsValid)
			{
				_authenticator.SetCookie(user.Username);
				if (!string.IsNullOrEmpty(sessionViewModel.ReturnUrl))
				{
					return Redirect(sessionViewModel.ReturnUrl);
				}

				return RedirectToAction("Index", "Home");
			}

			return View("New", sessionViewModel);
		}

		[HttpPost]
		public ActionResult Destroy()
		{
			_authenticator.SignOut();
			Session.Abandon();
			return RedirectToAction("Index", "Home");
		}
	}
}
