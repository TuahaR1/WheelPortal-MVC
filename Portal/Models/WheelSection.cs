using System;
using System.Collections.Generic;

namespace Portal.Models
{
    public partial class WheelSection
    {
        public WheelSection()
        {
            InverseFkParent = new HashSet<WheelSection>();
        }

        public int PkWheelSectionId { get; set; }
        public int? FkParentId { get; set; }
        public string Name { get; set; } = null!;
        public string Colour { get; set; } = null!;
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual WheelSection? FkParent { get; set; }
        public virtual ICollection<WheelSection> InverseFkParent { get; set; }
    }
}
