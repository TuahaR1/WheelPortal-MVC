using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
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
        public async Task<IActionResult> Index()
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

            // Map to DTO to avoid self-referencing loop
            var sectionDtos = sections.Select(s => new WheelSectionDto
            {
                PkWheelId = s.PkWheelId,
                Name = s.Name,
                Colour = s.Colour,
                Order = s.Order,
                FkParentWheelId = s.FkParentWheelId,
                Children = s.Children.Select(c => new WheelSectionDto
                {
                    PkWheelId = c.PkWheelId,
                    Name = c.Name,
                    Colour = c.Colour,
                    Order = c.Order,
                    FkParentWheelId = c.FkParentWheelId,
                    Children = c.Children.Select(gc => new WheelSectionDto
                    {
                        PkWheelId = gc.PkWheelId,
                        Name = gc.Name,
                        Colour = gc.Colour,
                        Order = gc.Order,
                        FkParentWheelId = gc.FkParentWheelId
                    }).ToList()
                }).ToList()
            }).ToList();

            return View(sectionDtos); // Pass DTOs to the view
        }
    }
}
