using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Portal.Data;
using Portal.Models;

namespace Portal.Controllers
{
    public class WheelController : Controller
    {

        private readonly PortalSQL sql;

        public WheelController(DbContextOptions<PortalContext> dbContextOptions)
        {
            sql = new PortalSQL(dbContextOptions);
        }



        public IActionResult Index()
        {
            return View(sql.GetAllWheelSections());
        }public IActionResult Index2()
        {
            return View();
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
                var ws = sql.GetWheelSection(id);
                if (ws == null)
                    return false;

                // Prevent self-parenting
                if (fkParentID == id)
                    return false;

                // Set parent if valid, otherwise null
                if (fkParentID != 0)
                {
                    var parent = sql.GetWheelSection(fkParentID);
                    if (parent == null)
                        return false;
                    ws.FkParentId = fkParentID;
                }
                else
                {
                    ws.FkParentId = null;
                }

                ws.Name = Name;
                ws.Colour = Colour;
                ws.OrderId = orderID;
                sql.Save();
            }
            catch
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



    }

}
