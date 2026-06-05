namespace ChapeauProject.Models
{
    //NOTE Bill uses raw int? OrderID and int? GuestID, needs to change to object
    public class Bill
    {
        public int BillID { get; set; }
        public int? OrderID { get; set; }
        public int? GuestID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal SubTotalAmount { get; set; }
        public DateTime BillTimeStamp { get; set; }
    }
}
