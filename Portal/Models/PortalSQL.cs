
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Portal.Data;
using System.Resources;
using static System.Collections.Specialized.BitVector32;

namespace Portal.Models
{
    public class PortalSQL
    {
        //PortalContext db = new PortalContext();

        private readonly PortalContext db;

        public PortalSQL(DbContextOptions<PortalContext> options)
        {
            db = new PortalContext(options);
        }


        #region WheelSections
        public IEnumerable<WheelSectionDto> GetAllWheelSections()
        {
            var sections = db.WheelSections
     .Where(x => x.FkParentId == null)
     .Select(x => new WheelSectionDto
     {
         PkWheelId = x.PkWheelSectionId,
         FkParentWheelId = x.FkParentId,
         Name = x.Name,
         Colour = x.Colour,
         Order = x.OrderId,
         CreatedAt = x.CreatedAt,
         UpdatedAt = x.UpdatedAt,

         // First level children
         Children = x.InverseFkParent.Select(c => new WheelSectionDto
         {
             PkWheelId = c.PkWheelSectionId,
             FkParentWheelId = c.FkParentId,
             Name = c.Name,
             Colour = c.Colour,
             Order = c.OrderId,
             CreatedAt = c.CreatedAt,
             UpdatedAt = c.UpdatedAt,

             // Second level children
             Children = c.InverseFkParent.Select(gc => new WheelSectionDto
             {
                 PkWheelId = gc.PkWheelSectionId,
                 FkParentWheelId = gc.FkParentId,
                 Name = gc.Name,
                 Colour = gc.Colour,
                 Order = gc.OrderId,
                 CreatedAt = gc.CreatedAt,
                 UpdatedAt = gc.UpdatedAt,

                 // you can go deeper if needed
                 Children = new List<WheelSectionDto>() // stop at this level
             })
         })
     })
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
            return sections;

        }
        public IEnumerable<DropDownOfSegment> GetAllDropDownOfSegment()
        {
            return db.WheelSections.Select(x => new DropDownOfSegment
            {
                PkWheelId = x.PkWheelSectionId,
                Name = x.Name
            })
            .ToList();
        }
        public WheelSection? GetWheelSection(int id)
        {
            return db.WheelSections.Where(x => x.PkWheelSectionId == id)
                    .Include(x => x.InverseFkParent)
                        .ThenInclude(x => x.InverseFkParent)
                .FirstOrDefault();

        }
        public void AddWheelSection(WheelSection ws)
        {
            db.WheelSections.Add(ws);
            db.SaveChanges();
            return;
        }
        public void DeleteWheelSection(WheelSection? ws)
        {


                foreach (var child in ws.InverseFkParent)
                {
                    db.WheelSections.RemoveRange(child.InverseFkParent); // Grandchildren
                }

                // Delete children
                db.WheelSections.RemoveRange(ws.InverseFkParent);
            

            // Delete parent
            db.WheelSections.Remove(ws);


            db.SaveChanges();
            return;
        }
        #endregion

        public void Save()
        {
            db.SaveChanges();
        }

    }
}
