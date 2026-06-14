using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class TablesViewModel
    {
        public Table Table { get; set; }
        public int OrderCount { get; set; }
        public int GuestCount { get; set; }
        public OrderCategories? Categories { get; set; }

        public bool HasFoodOrder         => Categories?.HasFood  ?? false;
        public bool HasDrinkOrder        => Categories?.HasDrink ?? false;
        public PreparationStatus? RunningOrderStatus => Categories?.OverallStatus;

        public string? StatusLabel    => RunningOrderStatus?.ToStatusLabel();
        public string? StatusCssClass => RunningOrderStatus?.ToStatusPillClass();
    }
}
