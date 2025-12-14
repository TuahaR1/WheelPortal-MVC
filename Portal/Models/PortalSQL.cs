
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Portal.Data;
using System.Drawing;
using System.Net.WebSockets;
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
                section.Children = section.Children.OrderBy(s => s.Order).ToList();
                foreach (var child in section.Children)
                {
                    child.Children = child.Children.OrderBy(s => s.Order).ToList();
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
        public WheelSection? GetWheelSection(int? id)
        {
            return db.WheelSections.Where(x => x.PkWheelSectionId == id)
                    .Include(x => x.InverseFkParent)
                        .ThenInclude(x => x.InverseFkParent)
                .FirstOrDefault();

        }

        public void AddWheelSection(WheelSection ws)
        {
            var query = db.WheelSections
                .Where(x => x.OrderId >= ws.OrderId);

            // If ordering is per parent
            if (ws.FkParentId != null)
            {
                query = query.Where(x => x.FkParentId == ws.FkParentId);
            }
            else
            {
                query = query.Where(x => x.FkParentId == null);

            }

            var sectionsToUpdate = query.ToList();

            foreach (var item in sectionsToUpdate)
            {
                item.OrderId += 1;
            }

            ws.CreatedAt = DateTime.Now;

            db.WheelSections.Add(ws);
            db.SaveChanges();
        }
        public void UpdateWheelSection(int id, string Name, string? Colour, int orderID, int? fkParentID)
        {
            // 1️⃣ Load existing record
            var existing = db.WheelSections
                .FirstOrDefault(x => x.PkWheelSectionId == id);

            if (existing == null)
                throw new Exception("WheelSection not found");

            int oldOrder = existing.OrderId!.Value;
            int newOrder = orderID>0?orderID:1;
           
            // 2️⃣ Only reorder if order actually changed
            if (oldOrder != newOrder)
            {
                var query = db.WheelSections.AsQueryable();

                // Same parent scope
                if (fkParentID != null)
                    query = query.Where(x => x.FkParentId == fkParentID);
                else
                    query = query.Where(x => x.FkParentId == null);

                // Exclude current item
                query = query.Where(x => x.PkWheelSectionId != id);

                // 🔼 Move UP (5 → 2)
                if (newOrder < oldOrder)
                {
                    query = query.Where(x =>
                        x.OrderId >= newOrder &&
                        x.OrderId < oldOrder);

                    foreach (var item in query.ToList())
                        item.OrderId += 1;
                }
                // 🔽 Move DOWN (2 → 5)
                else
                {
                    query = query.Where(x =>
                        x.OrderId <= newOrder &&
                        x.OrderId > oldOrder);

                    foreach (var item in query.ToList())
                        item.OrderId -= 1;
                }
            }

            // 3️⃣ Update current record
            existing.Name = Name;
            existing.Colour = Colour;
            existing.FkParentId = fkParentID;
            existing.OrderId = orderID;
            existing.UpdatedAt = DateTime.Now;

            db.SaveChanges();
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
