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
        public IActionResult Index()
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

            
            return View(sections); // Pass DTOs to the view
        }
    }
}
