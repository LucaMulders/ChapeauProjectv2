using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class CourseGroupViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public List<RunningOrderItemViewModel> Items { get; set; } = new();

        public bool AllReady     => Items.All(i => i.PreparationStatus == PreparationStatus.Ready);
        public bool AllPreparing => !AllReady && Items.All(i => i.PreparationStatus != PreparationStatus.Pending);
        public bool AllServed    => Items.All(i => i.PreparationStatus == PreparationStatus.Served);

        public string CourseAdvanceLabel => AllReady      ? "↺ Reset"
                                         : AllPreparing  ? "▶ All Ready"
                                                         : "▶ All Preparing";
    }

    public class RunningOrderViewModel
    {
        private static readonly string[] CourseOrder = { "Starters", "Main", "Desserts", "Other" };

        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public string? Status { get; internal set; }

        public List<RunningOrderItemViewModel> Items { get; set; } = new();

        public TimeSpan WaitingTime => DateTime.Now - OrderTime;
        public bool AllItemsReady   => Items.All(i => i.PreparationStatus == PreparationStatus.Served);

        public List<RunningOrderItemViewModel> FoodItems  =>
            Items.Where(i => i.MenuCard == "Lunch" || i.MenuCard == "Dinner").ToList();

        public List<RunningOrderItemViewModel> DrinkItems =>
            Items.Where(i => i.MenuCard == "Drinks").ToList();

        public List<CourseGroupViewModel> FoodItemsByCourse =>
            CourseOrder
                .Select(course => new CourseGroupViewModel
                {
                    CourseName = course,
                    Items      = FoodItems.Where(i => i.CourseName == course).ToList()
                })
                .Where(g => g.Items.Any())
                .ToList();
    }

    public class TableOrderGroupViewModel
    {
        public int TableNumber { get; set; }
        public List<RunningOrderViewModel> Orders { get; set; } = new();
    }

    public class RunningOrderItemViewModel
    {
        public int OrderItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public string MenuCard { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Comment { get; set; }

        public string StatusPillClass => PreparationStatus switch
        {
            PreparationStatus.Pending   => "status-pill status-ordered",
            PreparationStatus.Preparing => "status-pill status-preparing",
            PreparationStatus.Ready     => "status-pill status-ready",
            PreparationStatus.Served    => "status-pill status-served",
            _                           => "status-pill"
        };

        public string StatusLabel => PreparationStatus switch
        {
            PreparationStatus.Pending   => "Ordered",
            PreparationStatus.Preparing => "Being Prepared",
            PreparationStatus.Ready     => "Ready to Serve",
            PreparationStatus.Served => "Served",
            _                           => PreparationStatus.ToString()
        };

        public string NameCssClass =>
            PreparationStatus == PreparationStatus.Ready || PreparationStatus == PreparationStatus.Served
                ? "order-item-name done"
                : "order-item-name";
    }
}
