using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Antlr.Runtime.Misc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Odontograma.Models.Odo;
namespace Odontograma.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        ODataBase db = new ODataBase();
        // GET: Estudiante
        public ActionResult Index()
        {
            string existe = getIdEstudiante(User.Identity.GetUserId());
            if (String.IsNullOrEmpty(existe))
            {
                ViewBag.Ex = "0";
            }
            else
            {
                var id = getIdEstudiante(User.Identity.GetUserId());
                var citas = (from d in db.Cita
                             where d.Estado == "Pendiente"
                             select d
                             ).ToList();
                ViewBag.Id = getIdEstudiante(User.Identity.GetUserId());
                ViewBag.Ex = "1";
                return View(citas);
            }
            ViewBag.Id = getIdEstudiante(User.Identity.GetUserId());
            return View();

        }
        [HttpGet]
        public ActionResult PerfilPaciente( String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            ViewBag.Paciente = (from p in db.Paciente
                                where p.PacienteId == id
                                select p
                                ).ToList();
            ViewBag.Detalle = (from d in db.DetallePaciente
                               where d.PacienteId == id
                               select d
                             ).ToList();
            ViewBag.AntPer = (from a in db.AntPer
                              where a.PacienteId == id
                              select a
                            ).ToList();
            ViewBag.AntFam = (from a in db.AntFam
                              where a.PacienteId == id
                              select a
                            ).ToList();
            return View();
        }

        public ActionResult Atendidas()
        {
            var idE = getIdEstudiante(User.Identity.GetUserId());
            var citas = (from c in db.Cita
                         join d in db.DetalleCita
                         on c.CitaId equals d.CitaId
                         where d.EstudianteId == idE && c.Estado == "Atendido"
                         select new AtendidasViewModel
                         {
                             cita = c,
                             detalle = d
                         }

                ).ToList();
            return View(citas);
        }
        public ActionResult Confirmadas()
        {
            var idE = getIdEstudiante(User.Identity.GetUserId());
            var citas = (from c in db.Cita
                         join d in db.DetalleCita
                         on c.CitaId equals d.CitaId
                         where d.EstudianteId == idE && c.Estado == "Confirmado"
                         select new ConfirmadasViewModel
                         {
                             cita = c, detalle = d
                         }

                ).ToList();
            return View(citas);
        }
        [HttpGet]
        public ActionResult EliminarDiag( String idD ,String idC)
        {
            if (String.IsNullOrEmpty(idD))
            {
                return RedirectToAction("Index");
            }
            var aDiagnostico = db.Diagnostico.Find(idD);
            db.Diagnostico.Remove(aDiagnostico);
            db.SaveChanges();

            return RedirectToAction("Diagnostico", new { id = idC });
        }       


        [HttpGet]
        public ActionResult VerDiagnostico(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            var exp = ( from d in db.Diagnostico
                        join c in db.Cita
                        on d.CitaId equals c.CitaId
                        join a in db.DetalleCita
                        on c.CitaId equals a.CitaId
                        where c.CitaId == id
                        select new Expediente
                        {
                            diagnostico=d,
                            cita=c,
                            detalleCita=a
                        }
                ).ToList();

            var cita = (from c in db.Cita
                        where c.CitaId == id
                        select c
                   ).ToList();
            var diag = (from d in db.Diagnostico
                        where d.CitaId == id
                        select d
                   ).ToList();
            var det = (from d in db.DetalleCita
                        where d.CitaId == id
                        select d
                ).ToList();
            var idE = getIdPacienteBy(id);
            var est = (from e in db.Paciente
                       where e.PacienteId == idE
                       select e
               ).ToList();

            ViewBag.Paciente = est;
            ViewBag.Cita = cita;
            ViewBag.Diagnostico = diag;
            ViewBag.Detalle = det;


            return View(exp);
        }


        //public ActionResult VerDiagnostico(string id)
        //{
        //    var procedimientos = (from d in db.Diagnostico
        //                          join c in db.Cita
        //                          on d.CitaId equals c.CitaId
        //                          join a in db.DetalleCita
        //                          on d.CitaId equals a.CitaId
        //                          where c.CitaId == id
        //                          select new Expediente
        //                          {
        //                              diagnostico = d,
        //                              cita = c,
        //                              detalleCita = a
        //                          }
        //        ).ToList();
        //    return View(procedimientos);
        //}

        [HttpGet]
        public ActionResult DetalleCita(String id)
        {
            var idD = getDetalleId(id);
            ObsDesViewModel model = new ObsDesViewModel();
            model.DetalleCitaId = idD;
            ViewBag.Id = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetalleCita(ObsDesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var aDetalle = db.DetalleCita.Find(model.DetalleCitaId);
                aDetalle.Descripcion = model.Descripcion;
                aDetalle.Observacion = model.Observacion;
                db.Entry(aDetalle).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var aCita = db.Cita.Find(getIdCita(model.DetalleCitaId));
                aCita.Estado = "Atendido";
                db.Entry(aCita).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Diagnostico(string id)
        {
            if (id==null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.SintomaId = new SelectList(db.Sintoma, "SintomaId", "Nombre");
            DiagnosticoViewModel modelo = new DiagnosticoViewModel();
            modelo.CitaId = id;
            var diag = (from d in db.Diagnostico
                        where d.CitaId ==id
                        select d
                          ).ToList();
            ViewBag.Diag = diag;
            ViewBag.CitaId = id;
            return View(modelo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Diagnostico (DiagnosticoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var aDiagnostico = new Diagnostico();
                aDiagnostico.DiagnosticoId = getGUID();
                aDiagnostico.Diente = model.Diente;
                aDiagnostico.SintomaId = model.SintomaId;
                aDiagnostico.Sintoma = model.Sintoma;
                aDiagnostico.CitaId = model.CitaId;
                db.Diagnostico.Add(aDiagnostico);
                db.SaveChanges();
                DetalleCita modelo = new DetalleCita();
                modelo.CitaId = model.CitaId;
                var diag = (from d in db.Diagnostico
                            where d.CitaId == model.CitaId
                            select d
                            ).ToList();
                ViewBag.CitaId = model.CitaId;
                ViewBag.Diag = diag;
                return RedirectToAction("Diagnostico", new { id = model.CitaId });
                //return RedirectToAction("DetalleCita", new { id = model.CitaId });
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Confirmar(string id)
        {
            if (id==null)
            {
               return RedirectToAction("Index","Home");
            }
            var aCita = db.Cita.Find(id);
            if (aCita == null)
            {
                return HttpNotFound();
            }
            aCita.Estado = "Confirmado";
            db.Entry(aCita).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            var aDetalle = new DetalleCita();
            aDetalle.DetalleCitaId= getGUID();
            aDetalle.Descripcion = "";
            aDetalle.Observacion = "";
            aDetalle.CitaId = id;
            aDetalle.EstudianteId=getIdEstudiante(User.Identity.GetUserId());
            db.DetalleCita.Add(aDetalle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Detalle(String id)
        {
            var idp = getIdEstudiante(User.Identity.GetUserId());
            var cita = (
                from p in db.Paciente
                join c in db.Cita
                on p.PacienteId equals c.PacienteId
                where c.CitaId == id
                select new DetalleCitaModel
                {
                    paciente=p,
                    cita=c
                }
                ).ToList();


            ViewBag.Citas = cita;
            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Nuevo(EstudianteViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var aEstudiante = new Estudiante();
                aEstudiante.EstudianteId = getGUID();
                aEstudiante.Nombre = modelo.Nombre;
                aEstudiante.Apellido = modelo.Apellido;
                aEstudiante.Telefono = modelo.Telefono;
                aEstudiante.FechaNacimiento = modelo.FechaNacimiento;
                aEstudiante.Sexo = modelo.Sexo;
                aEstudiante.EscuelaId = modelo.EscuelaId;
                aEstudiante.UserId= User.Identity.GetUserId();
                db.Estudiante.Add(aEstudiante);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            return RedirectToAction("Nuevo");
        }

        public ActionResult Nuevo()
        {
            ViewBag.ListadoSexos = Sexos();
            ViewBag.EscuelaId = new SelectList(db.Escuela, "EscuelaId", "Nombre");
            return View();
        }

        public string getGUID()
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }
        public string getDetalleId(string condicion)
        {
            var id = (from d in db.DetalleCita
                      where d.CitaId == condicion
                      select d.DetalleCitaId).FirstOrDefault();
            return id;
        }

        public string getIdCita(string condicion)
        {
            var id = (from d in db.DetalleCita
                      where d.DetalleCitaId == condicion
                      select d.CitaId).FirstOrDefault();
            return id;
        }
  
        public string getIdEstudiante(string condicion)
        {
            var id = (from d in db.Estudiante
                      where d.UserId == condicion
                      select d.EstudianteId).FirstOrDefault();
            return id;
        }


        public List<SelectListItem> Sexos()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Masculino",
                    Value = "M"

                },
                new SelectListItem()
                {
                    Text = "Femenino",
                    Value = "F"
                }
            };
        }
        public string getIdPacienteBy(string condicion)
        {
            var id = (from d in db.Cita
                      where d.CitaId == condicion
                      select d.PacienteId).FirstOrDefault();
            return id;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}