using System;
using System.Data;
using System.Data.SqlClient;

namespace Hotel_Reservation_System
{
    public static class DatabaseManager
    {
        // Create a new reservation and update room status
        public static int CreateReservation(int guestId, DateTime checkinDate, DateTime checkoutDate, int numberOfGuests, int roomId)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Get next reservation ID
                        int nextReservationId;
                        using (SqlCommand getMaxId = new SqlCommand(
                            "SELECT ISNULL(MAX(ReservationID), 0) + 1 FROM Reservation", con))
                        {
                            getMaxId.Transaction = transaction;
                            nextReservationId = Convert.ToInt32(getMaxId.ExecuteScalar());
                        }

                        // Create the reservation
                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO Reservation 
                              (ReservationID, GuestID, CheckinDate, CheckoutDate, Number_of_Guests, Reservation_Status) 
                              VALUES 
                              (@ReservationID, @GuestID, @CheckinDate, @CheckoutDate, @Number_of_Guests, 'Active')", con))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@ReservationID", nextReservationId);
                            cmd.Parameters.AddWithValue("@GuestID", guestId);
                            cmd.Parameters.AddWithValue("@CheckinDate", checkinDate);
                            cmd.Parameters.AddWithValue("@CheckoutDate", checkoutDate);
                            cmd.Parameters.AddWithValue("@Number_of_Guests", numberOfGuests);
                            cmd.ExecuteNonQuery();
                        }

                        // Update the room's status and link it to the reservation
                        using (SqlCommand updateRoom = new SqlCommand(
                            @"UPDATE Room 
                              SET Status = 'booked', 
                                  ReservationID = @ReservationID 
                              WHERE RoomID = @RoomID", con))
                        {
                            updateRoom.Transaction = transaction;
                            updateRoom.Parameters.AddWithValue("@ReservationID", nextReservationId);
                            updateRoom.Parameters.AddWithValue("@RoomID", roomId);
                            updateRoom.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return nextReservationId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error in transaction: {ex.Message}");
                    }
                }
            }
        }

        // Cancel a reservation
        public static bool CancelReservation(int reservationId)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("usp_CancelReservation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                    
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqlException)
                    {
                        return false;
                    }
                }
            }
        }

        // Get available rooms
        public static DataTable GetAvailableRooms(int numberOfGuests)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT r.RoomID, r.RoomNumber, r.RoomType, r.Capacity, r.Status
                    FROM Room r
                    WHERE r.Status = 'Free'
                    AND r.Capacity = @NumberOfGuests
                    ORDER BY r.RoomNumber", con))
                {
                    cmd.Parameters.AddWithValue("@NumberOfGuests", numberOfGuests);
                    
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // Calculate reservation total
        public static decimal CalculateReservationTotal(int reservationId)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT dbo.fn_CalculateReservationTotal(@ReservationID)", con))
                {
                    cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                    return (decimal)cmd.ExecuteScalar();
                }
            }
        }

        // Record payment
        public static bool RecordPayment(int reservationId, string paymentMethod, decimal amountPaid)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("usp_RecordPayment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                    cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                    cmd.Parameters.AddWithValue("@AmountPaid", amountPaid);
                    
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqlException)
                    {
                        return false;
                    }
                }
            }
        }

        // Assign staff to reservation
        public static bool AssignStaffToReservation(int staffId, int reservationId)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("usp_AssignStaffToReservation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StaffID", staffId);
                    cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                    
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqlException)
                    {
                        return false;
                    }
                }
            }
        }
    }
} 