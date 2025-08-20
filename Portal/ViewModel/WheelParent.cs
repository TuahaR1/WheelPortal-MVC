namespace Portal.ViewModel
{
  

    public class WheelParent
    {
        public int PkWheelId { get; set; }
        public string Name { get; set; }
        public string Colour { get; set; }
        public int Order { get; set; }

        // Navigation property: One parent can have many children
        public List<SubSectionWheel> SubSections { get; set; } = new();
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public class DropDownOfSegment
    {
        public int PkWheelId { get; set; }
        public string Name { get; set; }
    }
    public class UpdateAddWheelParentDto
    {
        public int PkWheelId { get; set; }
        public string? Name { get; set; }
        public string? Colour { get; set; }
        public int Order { get; set; }
        public int? FkParentWheelId { get; set; }

    }
    public class DeleteWheelParentDto
    {
        public int PkWheelId { get; set; }
       

    }
    public class SubSectionWheel
{
    public int PkWheelId { get; set; }
    public int FkParentWheelId { get; set; }
    public string? Name { get; set; }
    public string? Colour { get; set; }
    public int Order { get; set; }

    // Optional navigation back to parent
    public WheelParent? Parent { get; set; }
}


}
