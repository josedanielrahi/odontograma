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
using Microsoft.SqlServer.Server;
using Odontograma.Models.Odo;

namespace Odontograma.Controllers
{
    //[Authorize(Roles = "Paciente")]

    public class PacienteController : Controller
    {
        ODataBase db = new ODataBase();
        // GET: Paciente
        public ActionResult Index()
        {

            string existe = getIdPaciente(User.Identity.GetUserId());
            if (String.IsNullOrEmpty(existe))
            {
                ViewBag.Ex = "0";
            }
            else
            {
                var id = getIdPaciente(User.Identity.GetUserId());
                var citas = (from d in db.Cita
                             where d.PacienteId == id && d.Estado == "Pendiente"
                             select d
                             ).ToList();
                ViewBag.Id = getIdPaciente(User.Identity.GetUserId());
                ViewBag.Ex = "1";
                return View(citas);
            }
            ViewBag.Id = getIdPaciente(User.Identity.GetUserId());
            return View();
        }
        [HttpGet]
        public ActionResult Expedinte(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return View("Index");

            }
            var exp = (from d in db.Diagnostico
                join c in db.Cita
                on d.CitaId equals  c.CitaId
                join a in db.DetalleCita
                on d.CitaId equals a.CitaId
                join e in db.Estudiante
                on a.EstudianteId equals e.EstudianteId
                where c.CitaId==id
                       select new VerExpediente
                       {
                           diagnostico=d,
                           cita=c,
                           dCita=a,
                           estudiante=e
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
            var idE = getIdEstudianteBy(id);
            var est = (from e in db.Estudiante
                       where e.EstudianteId == idE
                       select e
               ).ToList();

            ViewBag.Estudiante = est;
            ViewBag.Cita = cita;
            ViewBag.Diagnostico = diag;
            ViewBag.Detalle = det;



            return View(exp);
        }

        [HttpGet]
        public ActionResult DetCita(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            var cita=(from c in db.Cita 
                      join d in db.DetalleCita
                      on c.CitaId equals d.CitaId
                      join e in db.Estudiante
                      on d.EstudianteId equals e.EstudianteId
                      where c.CitaId==id
                      select new VerEstudiante{
                        cita=c,
                        dCita=d,
                        estudiante=e
                      } 
                      ).ToList();

            return View(cita);
        } 
        public ActionResult Detalle()
        {
            ViewBag.DerechohabienciaId = new SelectList(db.Derechohabiencia, "DerechohabienciaId", "Alias");
            ViewBag.EstadoId = new SelectList(db.Estado, "EstadoId", "Nombre");
            ViewBag.OcupacionId = new SelectList(db.Ocupacion, "OcupacionId", "Nombre");
            ViewBag.EscolaridadId = new SelectList(db.Escolaridad, "EscolaridadId", "Nombre");
            
            return View();
        }

        [HttpGet]
        public ActionResult EliminarPer (String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            var aAntPer = db.AntPer.Find(id);
            db.AntPer.Remove(aAntPer);
            db.SaveChanges();
            return RedirectToAction("AntPer");
        }
        [HttpGet]
        public ActionResult EliminarFam(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            var aAntFam = db.AntFam.Find(id);
            db.AntFam.Remove(aAntFam);
            db.SaveChanges();
            return RedirectToAction("AntFam");
        }

        public ActionResult AntPer()
        {
            ViewBag.EnfermedadId = new SelectList(db.Enfermedad, "EnfermedadId", "Nombre");
            var valor= getIdPaciente(User.Identity.GetUserId());
            var getAntPer = (from a in db.AntPer
                             where a.PacienteId == valor
                             select a
                         ).ToList();


            if (getAntPer.Count > 0)
            {
                ViewBag.Existe = 1;
            }
            ViewBag.AntPer = getAntPer;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AntPer(AntPerViewModel model)
        {
            var aAntPer = new AntPer();
            if (ModelState.IsValid)
            {
                aAntPer.AntPerId = getGUID();
                aAntPer.EnfermedadId = model.EnfermedadId;
                aAntPer.PacienteId = getIdPaciente(User.Identity.GetUserId());
                db.AntPer.Add(aAntPer);
                db.SaveChanges();

                return RedirectToAction("AntPer", "Paciente");
            }

            return RedirectToAction("Index");
        }
        public ActionResult AntFam()
        {
            ViewBag.EnfermedadId = new SelectList(db.Enfermedad, "EnfermedadId", "Nombre");
            ViewBag.ParentescoId = new SelectList(db.Parentesco, "ParentescoId", "Nombre");
            var valor = getIdPaciente(User.Identity.GetUserId());
            var getAntFam = (from a in db.AntFam
                             where a.PacienteId == valor
                             select a
                         ).ToList();
            if (getAntFam.Count > 0)
            {
                ViewBag.Existe = 1;
            }
            ViewBag.AntFam = getAntFam;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AntFam(AntFamViewModel model)
        {
            var aAntFam = new AntFam();
            if (ModelState.IsValid)
            {
                aAntFam.AntFamId = getGUID();
                aAntFam.NombreCompleto = model.NombreCompleto;
                aAntFam.ParentescoId = model.ParentescoId;
                aAntFam.EnfermedadId = model.EnfermedadId;
                aAntFam.PacienteId = getIdPaciente(User.Identity.GetUserId());
                db.AntFam.Add(aAntFam);
                db.SaveChanges();

                return RedirectToAction("AntFam", "Paciente");
            }

            return RedirectToAction("Index");
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AntPer(AntPerForm modelo)
        //{
        //    return View();
        //}

        public ActionResult Editar(string id)
        {
            if(id == null)
            {
                return RedirectToAction("Index");
            }
            Cita cita = db.Cita.Find(id);
            if (cita == null)
            {
                return HttpNotFound();
            }
            CitaEditarModel modelo = new CitaEditarModel();
            modelo.CitaId = cita.CitaId;
            modelo.Fecha = cita.Fecha;
            modelo.Hora = cita.Hora;
            ViewBag.ClinicaId = new SelectList(db.Clinica, "ClinicaId", "Nombre", cita.ClinicaId);
            return View(modelo);
        }
        public ActionResult Eliminar(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var aCita = db.Cita.Find(id);
            db.Cita.Remove(aCita);
            db.SaveChanges();
            return RedirectToAction("Index");


        }
        public ActionResult Historial()
        {
            var id = getIdPaciente(User.Identity.GetUserId());
            var citas = (from c in db.Cita
                         join d in db.DetalleCita
                         on c.CitaId equals d.CitaId
                         where c.PacienteId == id && c.Estado == "Atendido"
                         select new HistorialViewModel{
                            detalle=d,
                            cita =c
                         }
                         ).ToList();
            return View(citas);
        }


        public ActionResult Confirmado()
        {
            var id = getIdPaciente(User.Identity.GetUserId());
            var citas = (from d in db.Cita
                         where d.PacienteId == id && d.Estado == "Confirmado"
                         select d
                         ).ToList();
            return View(citas);
        }
        public ActionResult Nuevo()
        {
            ViewBag.ListadoSexos = Sexos();
            return View();
        }

        public ActionResult Cita()
        {
            ViewBag.ClinicaId = new SelectList(db.Clinica, "ClinicaId", "Nombre");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cita(CitaViewModel modelo)
        {
            var aCita = new Cita();
            if (ModelState.IsValid)
            {
                aCita.CitaId = getGUID();
                aCita.Fecha= modelo.Fecha;
                aCita.Hora= modelo.Hora;
                aCita.ClinicaId = modelo.ClinicaId;
                aCita.PacienteId = getIdPaciente(User.Identity.GetUserId());
                aCita.Estado = "Pendiente";
                db.Cita.Add(aCita);
                db.SaveChanges();

                return RedirectToAction("Index", "Paciente");
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(CitaEditarModel modelo)
        {
            if (ModelState.IsValid)
            {
                var aCita = db.Cita.Find(modelo.CitaId);
                aCita.Fecha = modelo.Fecha;
                aCita.Hora = modelo.Hora;
                aCita.ClinicaId = modelo.ClinicaId;
                db.Entry(aCita).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            return RedirectToAction ("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalle(DetalleViewModel modelo)
        {
            var aDetalle = new DetallePaciente();
            if (ModelState.IsValid)
            {
                aDetalle.DetPacId = getGUID();
                aDetalle.Calle = modelo.Calle;
                aDetalle.Colonia = modelo.Colonia;
                aDetalle.Cp = modelo.Cp;
                aDetalle.DerechohabienciaId = modelo.DerechohabienciaId;
                aDetalle.EstadoId = modelo.EstadoId;
                aDetalle.OcupacionId = modelo.OcupacionId;
                aDetalle.EscolaridadId = modelo.EscolaridadId;
                aDetalle.PacienteId = getIdPaciente(User.Identity.GetUserId());
                db.DetallePaciente.Add(aDetalle);
                db.SaveChanges();

                return RedirectToAction("AntPer", "Paciente");
            }
            return RedirectToAction("Index", "Home");
        }
            

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Nuevo(PacienteViewModel modelo)
        {
            var aPaciente = new Paciente();
            aPaciente.PacienteId = getGUID();
            aPaciente.Nombre = modelo.Nombre;
            aPaciente.Apellido = modelo.Apellido;
            aPaciente.Telefono = modelo.Telefono;
            aPaciente.FechaNacimiento = modelo.FechaNacimiento;
            aPaciente.Sexo = modelo.Sexo;
            aPaciente.UserId = User.Identity.GetUserId();
            db.Paciente.Add(aPaciente);
            db.SaveChanges();
            return RedirectToAction("Detalle","Paciente");
        }

        public ActionResult Perfil()
        {
            var id = getIdPaciente(User.Identity.GetUserId());
            ViewBag.Paciente = (from p in db.Paciente
                                where p.PacienteId == id
                                select p
                                ).ToList();
            ViewBag.Detalle = ( from d in db.DetallePaciente
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
        

        public string getGUID()
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }
        public string getIdCita()
        {

            return "";
        }
        public string getIdPaciente(String condicion)
        {
            var id = (from d in db.Paciente
                      where d.UserId == condicion
                      select d.PacienteId).FirstOrDefault();
            return id;
        }

        public string getIdEstudianteBy(string condicion)
        {
            var id = (from d in db.DetalleCita
                      where d.CitaId == condicion
                      select d.EstudianteId).FirstOrDefault();
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