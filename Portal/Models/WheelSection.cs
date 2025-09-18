using System;
using System.Collections.Generic;

namespace Portal.Models;

public partial class WheelSection
{
    public int PkWheelSectionId { get; set; }

    public string? Name { get; set; }

    public string? Colour { get; set; }

    public int? FkParentId { get; set; }

    public int? OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual WheelSection? FkParent { get; set; }

    public virtual ICollection<WheelSection> InverseFkParent { get; set; } = new List<WheelSection>();
}
