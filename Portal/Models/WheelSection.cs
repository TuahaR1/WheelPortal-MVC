using System;
using System.Collections.Generic;

namespace Portal.Models
{
    public partial class WheelSection
    {
        public WheelSection()
        {
            Children = new HashSet<WheelSection>();
        }

        public int PkWheelId { get; set; }
        public int? FkParentWheelId { get; set; }
        public string Name { get; set; } = null!;
        public string Colour { get; set; } = null!;
        public int Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

       
    public virtual WheelSection? FkParentWheel { get; set; }
    public virtual ICollection<WheelSection> Children { get; set; } = new List<WheelSection>();
    }
}
