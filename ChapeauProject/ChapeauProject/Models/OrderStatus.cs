namespace ChapeauProject.Models
{
    //NOTE: OrderStatus is missing Served — it exists as a magic string 'Served' in SQL queries but not in this enum, breaking consistency
    public enum OrderStatus
    {
        Pending,
        Complete
    }
}