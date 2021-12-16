using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OCR_Cheque.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tesseract;
using OCR;

namespace OCR_Cheque.Controllers
{
    public class HomeController : Controller
    {
        public String[] _cuenta = { "Nombre:", "Cuenta:", "Cheque: ", "GT: ", "Código Barras:" };

        public Dictionary<string, string> datos_cuenta = new Dictionary<string, string>();

        private readonly ILogger<HomeController> _logger;
        private IHostingEnvironment Environment;

        public HomeController(ILogger<HomeController> logger, IHostingEnvironment _environment)
        {
            _logger = logger;
            Environment = _environment;
        }




        public IActionResult Index()
        {
            var CurrentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(CurrentDirectory);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult UploadImage()
        {
            ViewBag.Result = false;
            ViewBag.Title = "OCR ASP-NET CORE PRUEBA";
            return View();
        }

        [HttpPost]
        public IActionResult UploadImage(List<IFormFile> postedFiles)
        {
            string wwwPath = this.Environment.WebRootPath;
            string contentPath = this.Environment.ContentRootPath;

            string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            List<string> uploadedFiles = new List<string>();
            if (postedFiles.Count == null || postedFiles.Count == 0)
            {
                ViewBag.Result = false;
            }
            else
            {
                ViewBag.Result = true;
            }
            foreach (IFormFile postedFile in postedFiles)
            {
                string fileName = Path.GetFileName(postedFile.FileName);
                using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                    uploadedFiles.Add(fileName);
                    ViewBag.Message += string.Format("<b>{0}</b> subido.<br />", fileName);
                    Console.WriteLine("TEXTOOOOOOOO");
                    Console.WriteLine(path + fileName);
                    var nombre_imagen = path + "\\" + fileName;
                    Console.WriteLine("IMAGEN:"+ nombre_imagen);
                    OCR_Image(nombre_imagen);

                    // TESSERACT OCR
                    //   OCR.Class1 hola = new OCR.Class1();
                    //var name = hola.ObtenerTexto(nombre_imagen, Directory.GetCurrentDirectory()+"\\tessdata\\");
                    //Console.WriteLine(name);
                    

                    // IRON OCR
                  //  OCR_IRON.IRON_OCR nuevo = new OCR_IRON.IRON_OCR();
                   // Console.WriteLine(nuevo.LeerImagen(nombre_imagen));
                }
            }


            return View();
        }

        public void OCR_Image(string NombreImagen)
        {
            try
            {
                using (var engine = new TesseractEngine(@"tessdata", "spa", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(NombreImagen))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            //Console.WriteLine("Tasa de precisión: " + page.GetMeanConfidence());
                            // Console.WriteLine("Texto: " + text);
                            Cuenta_Cheque(text);
                            //ViewBag.res = text;
                            ViewBag.mean = String.Format("{0:p}", page.GetMeanConfidence());
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Cuenta_Cheque(string texto)
        {
            datos_cuenta.Add("Nombre_Cuenta", "");
            datos_cuenta.Add("Numero_Cuenta", "");
            datos_cuenta.Add("Cheque_Cuenta", "");
            datos_cuenta.Add("GT", "");
            datos_cuenta.Add("Código Barras", "");
            var palabras = texto.Split("\n");
            var texto_requerido = "";
            foreach (var item in palabras)
            {
                if (item.StartsWith("Banco"))
                {
                    int found = item.IndexOf("Banco");
                    var aux = item.Substring(found + 6);
                    datos_cuenta["Nombre_Cuenta"] = aux.Trim();
                }
                if (item.Contains("CUENTA"))
                {
                    int found = item.IndexOf("CUENTA");
                    var aux = item.Substring(found + 10, 15);
                    datos_cuenta["Numero_Cuenta"] = aux.Trim();
                }
                if (item.Contains("CHEQUE"))
                {
                    int found = item.IndexOf("CHEQUE");
                    var aux = item.Substring(found + 13, 6);
                    datos_cuenta["Cheque_Cuenta"] = aux.Trim();
                }
                if (item.StartsWith("GT"))
                {
                    int found = item.IndexOf("GT");
                    var aux = item.Substring(found);
                    datos_cuenta["GT"] = aux.Trim();
                }
            }
            datos_cuenta["Código Barras"] = palabras[palabras.Length - 2].Substring(2);
            foreach (KeyValuePair<string, string> kvp in datos_cuenta)
            {
                texto_requerido += kvp.Key + ":" + kvp.Value + "\n";
            }
            ViewBag.res = texto_requerido;
        }


    }
}
