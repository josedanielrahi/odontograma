using Odontograma.Models.Odo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Odontograma.Controllers
{
    [Authorize(Roles = "Solicitud")]
    public class AspirantesController : Controller
    {
        ODataBase db = new ODataBase(); 
        // GET: Aspirantes
        public ActionResult Index()
        {
            return View();
        }
    }
}