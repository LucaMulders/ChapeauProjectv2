namespace ChapeauProject.Models
{
    public class OrderCategories
    {
        public bool HasFood  { get; set; }
        public bool HasDrink { get; set; }
        // null = no active items
        public PreparationStatus? OverallStatus { get; set; }
    }
}
