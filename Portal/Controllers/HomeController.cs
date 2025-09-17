using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Portal.Data;
using Portal.Models;

namespace Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortalSQL sql;

        public HomeController(ILogger<HomeController> logger, DbContextOptions<PortalContext> dbContextOptions)
        {
            _logger = logger;
            sql = new PortalSQL(dbContextOptions);
        }




        public IActionResult Index()
        {
            return View(sql.GetAllWheelSections());
        }

        [HttpGet]
        public IActionResult GetWheelSections()
        {
            try
            {

                var sections = sql.GetAllWheelSections();



                var dropDownData = sql.GetAllDropDownOfSegment();

                // ✅ Serialize with Newtonsoft.Json inside controller
                var json = JsonConvert.SerializeObject(
                    new { success = true, data = dropDownData, wheeldata = sections },
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 👈 breaks the cycle
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                        //ContractResolver = new CasePropertyNamesContractResolver() // 👈 lowercase
                    }
                );

                // Return as raw JSON (not as string)
                return Content(json, "application/json");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public bool AddWheelSection(string Name, string Colour, int orderID, int fkParentID)
        {
            try
            {
                WheelSection ws = new WheelSection();
                ws.Name = Name;
                ws.Colour = Colour;
                ws.OrderId = orderID;
                if (fkParentID != 0)
                {
                    ws.FkParentId = fkParentID;
                }
                sql.AddWheelSection(ws);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public WheelSection? EditWheelSection(int id)
        {
            return sql.GetWheelSection(id);
        }

        public bool EditWheelSectionFinal(int id, string Name, string Colour, int orderID, int fkParentID)
        {
            try
            {
                WheelSection ws = sql.GetWheelSection(id);
                ws.Name = Name;
                ws.Colour = Colour;
                ws.OrderId = orderID;
                ws.FkParentId = fkParentID;
                sql.Save();

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        public bool DeleteWheelSection(int id)
        {
            try
            {
                sql.DeleteWheelSection(sql.GetWheelSection(id));

            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
    }
}
