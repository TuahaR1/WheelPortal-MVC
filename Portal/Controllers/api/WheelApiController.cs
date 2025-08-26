using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Portal.Data;
using Portal.Models;
using Portal.ViewModel;
using System.Text.Json.Serialization;

namespace Portal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class WheelApiController : ControllerBase
    {
        private readonly PortalContext _dbContext;
        public WheelApiController(PortalContext context)
        {
            _dbContext = context;
        }
        // GET: api/WheelApi
        [HttpGet("get-all")]
        public async Task<IActionResult> GetWheelSections()
        {
            try
            {

                var sections = await _dbContext.WheelSections
                 .Where(x => x.FkParentWheelId == null)
                 .Include(x => x.Children)
                     .ThenInclude(c => c.Children) // grandchildren
                 .ToListAsync();

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

                var dropDownData = await _dbContext.WheelSections
            .OrderBy(s => s.FkParentWheelId)
            .ThenBy(s => s.Order)
            .Select(x => new DropDownOfSegment
            {
                PkWheelId = x.PkWheelId,
                Name = x.Name
            })
            .ToListAsync();
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

        [HttpPost("save")]
        public async Task<IActionResult> SaveWheelSection([FromBody] UpdateAddWheelParentDto req)
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
                    var existing = await _dbContext.WheelSections.FindAsync(req.PkWheelId);
                    if (existing != null)
                    {
                        existing.Name = req.Name;
                        existing.Colour = req.Colour;
                        existing.Order = req.Order;
                        existing.FkParentWheelId = req.FkParentWheelId;
                        existing.UpdatedAt = DateTime.Now; // Update timestamp  
                    }
                }


                await _dbContext.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                // Log to console or log file
                Console.WriteLine("Error: " + ex.Message);
                return new JsonResult(new { success = false, message = ex.Message });
            }

        }

        [HttpPost("delete")]

        public async Task<IActionResult> DeleteWheelSection([FromBody] DeleteWheelParentDto req)
        {
            try
            {
                if (req.PkWheelId <= 0)
                {
                    return new JsonResult(new { success = false, message = "Invalid ID." });
                }

                var sections = await _dbContext.WheelSections
                    .Where(x => x.PkWheelId == req.PkWheelId)
                    .Include(x => x.Children)
                        .ThenInclude(c => c.Children) // grandchildren
                    .ToListAsync();

                if (!sections.Any())
                {
                    return new JsonResult(new { success = false, message = "No record found!" });
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

                await _dbContext.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
