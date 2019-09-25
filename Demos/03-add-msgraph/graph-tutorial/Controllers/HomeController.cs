// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace graph_tutorial.Controllers
{
	using System.Web.Mvc;

	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		public ActionResult Error(string message, string debug)
		{
			Flash(message, debug);
			return RedirectToAction("Index");
		}
	}
}