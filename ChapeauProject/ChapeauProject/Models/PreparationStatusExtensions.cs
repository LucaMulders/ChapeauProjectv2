namespace ChapeauProject.Models
{
    public static class PreparationStatusExtensions
    {
        public static string ToStatusLabel(this PreparationStatus status) => status switch
        {
            PreparationStatus.Pending   => "Ordered",
            PreparationStatus.Preparing => "Being Prepared",
            PreparationStatus.Ready     => "Ready to Serve",
            PreparationStatus.Served    => "Served",
            _                           => status.ToString()
        };

        public static string ToStatusPillClass(this PreparationStatus status) => status switch
        {
            PreparationStatus.Pending   => "status-pill status-ordered",
            PreparationStatus.Preparing => "status-pill status-preparing",
            PreparationStatus.Ready     => "status-pill status-ready",
            PreparationStatus.Served    => "status-pill status-served",
            _                           => "status-pill"
        };
    }
}
