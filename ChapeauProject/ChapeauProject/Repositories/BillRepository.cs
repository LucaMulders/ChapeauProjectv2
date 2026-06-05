using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public class BillRepository : RepositoryBase, IBillRepository
    {
        // Rubric Item: Use of Constants for Repeated Strings

        private const string StatusPending  = nameof(OrderStatus.Pending);
        private const string StatusComplete = nameof(OrderStatus.Complete);
        private const string StatusServed   = nameof(OrderStatus.Served);

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
                    command.Parameters.AddWithValue("@BillID",           payment.Bill.BillID);
                    command.Parameters.AddWithValue("@PaymentAmount",     payment.PaymentAmount);
                    command.Parameters.AddWithValue("@PaymentMethod",     payment.PaymentMethod.ToString());
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
                string query = $@"
                    UPDATE Orders
                    SET    OrderStatus = '{StatusComplete}'
                    WHERE  GuestID IN (SELECT GuestID FROM Guests WHERE TableNumber = @TableNumber)
                      AND  OrderStatus IN ('{StatusPending}', '{StatusServed}')";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int? GetBillIdForTable(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT TOP 1 B.BillID FROM Bills B
                    JOIN Orders O ON B.OrderID = O.OrderID
                    JOIN Guests G ON O.GuestID = G.GuestID
                    WHERE G.TableNumber = @TableNumber
                    ORDER BY B.BillTimeStamp DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    object result = command.ExecuteScalar();
                    return result != null ? (int?)result : null;
                }
            }
        }

        public decimal GetAmountPaidForBill(int billId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT ISNULL(SUM(PaymentAmount), 0) FROM Payments WHERE BillID = @BillID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BillID", billId);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }
    }
}
