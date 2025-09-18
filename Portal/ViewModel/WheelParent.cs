namespace Portal.ViewModel
{

    
    public class DropDownOfSegment
    {
        public int PkWheelId { get; set; }
        public string? Name { get; set; }
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
 


}
