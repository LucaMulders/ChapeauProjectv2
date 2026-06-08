namespace ChapeauProject.Models
{
    // Same idea as CourseFilter. "finished" and "running" are used in both the controller and service, so one constant beats copy-pasting the string.
    public static class OrderFilter
    {
        public const string Running  = "running";
        public const string Finished = "finished";
    }
}
