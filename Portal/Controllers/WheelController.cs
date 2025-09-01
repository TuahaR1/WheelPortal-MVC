using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Portal.Data;
using Portal.Models;
using Portal.ViewModel;

namespace Portal.Controllers
{
    public class WheelController : Controller
    {
        private readonly PortalContext _dbContext;

        public WheelController(PortalContext context)
        {
            _dbContext = context;
        }
        public IActionResult Index()
        {
            var sections =  _dbContext.WheelSections
                .Where(x => x.FkParentWheelId == null)
                .Include(x => x.Children) // subchildren
                    .ThenInclude(c => c.Children) // grandchildren
                .ToList();

            // Order top-level sections
            sections = sections.OrderBy(s => s.Order).ToList();

            // Order children and grandchildren
            foreach (var section in sections)
            {
                section.Children = section.Children.OrderBy(s => s.Order).ThenByDescending(s => s.CreatedAt).ToList();
                foreach (var child in section.Children)
                {
                    child.Children = child.Children.OrderBy(s => s.Order).ThenByDescending(s => s.CreatedAt).ToList();
                }
            }

            
            return View(sections); // Pass DTOs to the view
        }
        [HttpPost]
        public bool DeleteWheelSection([FromBody] DeleteWheelParentDto req)
        {
            try
            {
                if (req.PkWheelId <= 0)
                {
                    return false;
                }

                var sections =  _dbContext.WheelSections
                    .Where(x => x.PkWheelId == req.PkWheelId)
                    .Include(x => x.Children)
                        .ThenInclude(c => c.Children) // grandchildren
                    .ToList();

                if (!sections.Any())
                {
                    return false;
                }

                foreach (var parent in sections)
                {
                    // Delete grandchildren
                    foreach (var child in parent.Children)
                    {
                        _dbContext.WheelSections.RemoveRange(child.Children); // Grandchildren
                    }

                    // Delete children
                    _dbContext.WheelSections.RemoveRange(parent.Children);

                    // Delete parent
                    _dbContext.WheelSections.Remove(parent);
                }

                 _dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public  bool SaveWheelSection([FromBody] UpdateAddWheelParentDto req)
        {
            try
            {

                if (req.PkWheelId <= 0)
                {
                    _dbContext.WheelSections.Add(new WheelSection
                    {
                        Name = req.Name,
                        Colour = req.Colour,
                        Order = req.Order,
                        FkParentWheelId = req.FkParentWheelId,
                        CreatedAt = DateTime.Now
                    });
                }
                else
                {
                    var existing =  _dbContext.WheelSections.Find(req.PkWheelId);
                    if (existing != null)
                    {
                        existing.Name = req.Name;
                        existing.Colour = req.Colour;
                        existing.Order = req.Order;
                        existing.FkParentWheelId = req.FkParentWheelId;
                        existing.UpdatedAt = DateTime.Now; // Update timestamp  
                    }
                }


                 _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log to console or log file
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }

        }

        [HttpGet]
        public  IActionResult GetWheelSections()
        {
            try
            {

                var sections =  _dbContext.WheelSections
                 .Where(x => x.FkParentWheelId == null)
                 .Include(x => x.Children)
                     .ThenInclude(c => c.Children) // grandchildren
                 .ToList();

                // Order top-level sections
                sections = sections.OrderBy(s => s.Order).ToList();

                // Order children and grandchildren
                foreach (var section in sections)
                {
                    section.Children = section.Children.OrderBy(s => s.Order).ThenByDescending(s => s.CreatedAt).ToList();
                    foreach (var child in section.Children)
                    {
                        child.Children = child.Children.OrderBy(s => s.Order).ThenByDescending(s => s.CreatedAt).ToList();
                    }
                }

                var dropDownData =  _dbContext.WheelSections
            .OrderBy(s => s.FkParentWheelId)
            .ThenBy(s => s.Order)
            .Select(x => new DropDownOfSegment
            {
                PkWheelId = x.PkWheelId,
                Name = x.Name
            })
            .ToList();
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


    }
}
