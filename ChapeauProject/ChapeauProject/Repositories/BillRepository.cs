using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public class BillRepository : RepositoryBase, IBillRepository
    {
        public BillRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public int CreateBill(Bill bill)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    INSERT INTO Bills (OrderID, GuestID, TotalAmount, VatAmount, SubTotalAmount, BillTimeStamp)
                    OUTPUT INSERTED.BillID
                    VALUES (@OrderID, @GuestID, @TotalAmount, @VatAmount, @SubTotalAmount, @BillTimeStamp)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID",       (object?)bill.OrderID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@GuestID",       (object?)bill.GuestID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount",    bill.TotalAmount);
                    command.Parameters.AddWithValue("@VatAmount",      bill.VatAmount);
                    command.Parameters.AddWithValue("@SubTotalAmount", bill.SubTotalAmount);
                    command.Parameters.AddWithValue("@BillTimeStamp",  bill.BillTimeStamp);

                    return (int)command.ExecuteScalar();
                }
            }
        }

        public void CreatePayment(Payment payment)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    INSERT INTO Payments (BillID, PaymentAmount, PaymentMethod, TipAmount, PaymentTimeStamp, Feedback)
                    VALUES (@BillID, @PaymentAmount, @PaymentMethod, @TipAmount, @PaymentTimeStamp, @Feedback)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BillID",           payment.BillID);
                    command.Parameters.AddWithValue("@PaymentAmount",     payment.PaymentAmount);
                    command.Parameters.AddWithValue("@PaymentMethod",     payment.PaymentMethod);
                    command.Parameters.AddWithValue("@TipAmount",         payment.TipAmount);
                    command.Parameters.AddWithValue("@PaymentTimeStamp",  payment.PaymentTimeStamp);
                    command.Parameters.AddWithValue("@Feedback",          (object?)payment.Feedback ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CompleteOrdersForTable(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    UPDATE Orders
                    SET    OrderStatus = 'Complete'
                    WHERE  GuestID IN (SELECT GuestID FROM Guests WHERE TableNumber = @TableNumber)
                      AND  OrderStatus = 'Pending'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
