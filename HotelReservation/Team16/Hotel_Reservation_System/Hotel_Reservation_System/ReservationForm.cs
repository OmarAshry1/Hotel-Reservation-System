using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hotel_Reservation_System
{
    public partial class ReservationForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;

        public ReservationForm()
        {
            InitializeComponent();
            this.Load += ReservationForm_Load;
            this.Size = new System.Drawing.Size(800, 600);
        }

        private void ReservationForm_Load(object sender, EventArgs e)
        {
            // Create main layout panels
            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150  // Increased height to accommodate more buttons
            };

            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            this.Controls.Add(gridPanel);
            this.Controls.Add(buttonPanel);

            // Setup DataGridView
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            gridPanel.Controls.Add(dgv);

            // Setup Reservation Operation Buttons
            var btnAddReservation = CreateButton("Add New Reservation", 20, 20);
            btnAddReservation.Click += BtnCreateReservation_Click;

            var btnViewReservations = CreateButton("View All Reservations", 230, 20);
            btnViewReservations.Click += (s, args) => {
                LoadReservations();
                gridPanel.Visible = true;
            };

            var btnUpdateCheckout = CreateButton("Update Checkout Date", 20, 70);
            btnUpdateCheckout.Click += BtnUpdateCheckout_Click;

            var btnDeleteReservation = CreateButton("Delete Reservation", 230, 70);
            btnDeleteReservation.Click += BtnDeleteReservation_Click;

            var btnCheckout = CreateButton("Guest Checkout", 440, 70);
            btnCheckout.Click += BtnCheckout_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddReservation,
                btnViewReservations,
                btnUpdateCheckout,
                btnDeleteReservation,
                btnCheckout
            });
        }

        private Button CreateButton(string text, int left, int top)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 200,
                Height = 40
            };
        }

        private void LoadReservations()
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT o.ReservationID, o.Reservation_Status, o.CheckinDate, 
                                   o.Number_of_Guests, o.CheckoutDate, g.GuestID, r.RoomID,
                                   ISNULL(p.AmountPaid, 0) as AmountPaid,
                                   ISNULL(p.Payment_Status, 'Not Paid') as Payment_Status,
                                   ISNULL(p.Payment_Method, '') as Payment_Method
                                   FROM Guest g
                                   JOIN Reservation o ON o.GuestID = g.GuestID 
                                   JOIN Room r ON r.ReservationID = o.ReservationID
                                   LEFT JOIN Payment p ON p.ReservationID = o.ReservationID";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading reservations: {ex.Message}");
                }
            }
        }

        private void BtnCreateReservation_Click(object sender, EventArgs e)
        {
            // Create a form to collect reservation details
            using (var form = new Form())
            {
                form.Text = "Create New Reservation";
                form.Size = new System.Drawing.Size(500, 400);

                // Add controls for input
                var lblGuestId = new Label { Text = "Guest ID:", Top = 20, Left = 20 };
                var txtGuestId = new TextBox { Top = 20, Left = 150, Width = 250 };

                var lblCheckin = new Label { Text = "Check-in Date:", Top = 60, Left = 20 };
                var dtpCheckin = new DateTimePicker 
                { 
                    Top = 60, 
                    Left = 150, 
                    Width = 250,
                    Value = DateTime.Today,
                    Enabled = false // Disable editing since it's automatic
                };

                var lblCheckout = new Label { Text = "Check-out Date:", Top = 100, Left = 20 };
                var dtpCheckout = new DateTimePicker 
                { 
                    Top = 100, 
                    Left = 150, 
                    Width = 250,
                    MinDate = DateTime.Today.AddDays(1) // Can't check out on same day
                };

                var lblGuests = new Label { Text = "Number of Guests:", Top = 140, Left = 20 };
                var txtGuests = new TextBox { Top = 140, Left = 150, Width = 250 };

                var btnSelectRoom = new Button 
                { 
                    Text = "Select Room", 
                    Top = 180, 
                    Left = 150, 
                    Width = 200,
                    Height = 40
                };
                var lblSelectedRoom = new Label { Text = "No room selected", Top = 230, Left = 20, Width = 400 };
                int selectedRoomId = -1;
                int selectedRoomCapacity = 0;

                btnSelectRoom.Click += (s, args) =>
                {
                    if (!int.TryParse(txtGuests.Text, out int numGuests))
                    {
                        MessageBox.Show("Please enter a valid number of guests first.");
                        return;
                    }

                    // Get available rooms
                    DataTable availableRooms = DatabaseManager.GetAvailableRooms(numGuests);

                    if (availableRooms.Rows.Count == 0)
                    {
                        MessageBox.Show("No available rooms found with sufficient capacity.");
                        return;
                    }

                    // Show available rooms in a new form
                    using (var roomsForm = new Form())
                    {
                        roomsForm.Text = "Select Available Room";
                        roomsForm.Size = new System.Drawing.Size(800, 500);

                        var dgvRooms = new DataGridView();
                        dgvRooms.Dock = DockStyle.Fill;
                        dgvRooms.DataSource = availableRooms;
                        dgvRooms.ReadOnly = true;
                        dgvRooms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgvRooms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dgvRooms.AllowUserToAddRows = false;

                        var btnSelect = new Button 
                        { 
                            Text = "Select Room", 
                            Dock = DockStyle.Bottom,
                            Height = 40
                        };

                        btnSelect.Click += (s2, args2) =>
                        {
                            if (dgvRooms.SelectedRows.Count > 0)
                            {
                                var selectedRow = dgvRooms.SelectedRows[0];
                                selectedRoomId = Convert.ToInt32(selectedRow.Cells["RoomID"].Value);
                                selectedRoomCapacity = Convert.ToInt32(selectedRow.Cells["Capacity"].Value);
                                lblSelectedRoom.Text = $"Selected Room: {selectedRow.Cells["RoomNumber"].Value} " +
                                                     $"(Type: {selectedRow.Cells["RoomType"].Value}, " +
                                                     $"Capacity: {selectedRoomCapacity})";
                                roomsForm.DialogResult = DialogResult.OK;
                            }
                        };

                        roomsForm.Controls.Add(dgvRooms);
                        roomsForm.Controls.Add(btnSelect);
                        roomsForm.ShowDialog();
                    }
                };

                var btnSubmit = new Button 
                { 
                    Text = "Create Reservation", 
                    Top = 280, 
                    Left = 150, 
                    Width = 200,
                    Height = 40
                };
                btnSubmit.Click += (s, args) =>
                {
                    if (!int.TryParse(txtGuestId.Text, out int guestId) ||
                        !int.TryParse(txtGuests.Text, out int numGuests))
                    {
                        MessageBox.Show("Please enter valid numbers for Guest ID and Number of Guests.");
                        return;
                    }

                    if (selectedRoomId == -1)
                    {
                        MessageBox.Show("Please select a room first.");
                        return;
                    }

                    if (numGuests > selectedRoomCapacity)
                    {
                        MessageBox.Show($"Number of guests ({numGuests}) exceeds room capacity ({selectedRoomCapacity}).");
                        return;
                    }

                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            using (SqlTransaction transaction = con.BeginTransaction())
                            {
                                try
                                {
                                    int newReservationId;
                                    // Create reservation
                                    using (SqlCommand cmd = new SqlCommand(
                                        "INSERT INTO Reservation (GuestID, CheckinDate, CheckoutDate, Number_of_Guests, Reservation_Status) " +
                                        "VALUES (@GuestID, @CheckinDate, @CheckoutDate, @Number_of_Guests, 1); " +
                                        "SELECT SCOPE_IDENTITY();", con))
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.Parameters.AddWithValue("@GuestID", guestId);
                                        cmd.Parameters.AddWithValue("@CheckinDate", dtpCheckin.Value);
                                        cmd.Parameters.AddWithValue("@CheckoutDate", dtpCheckout.Value);
                                        cmd.Parameters.AddWithValue("@Number_of_Guests", numGuests);
                                        newReservationId = Convert.ToInt32(cmd.ExecuteScalar());
                                    }

                                    // Update room status
                                    using (SqlCommand updateRoom = new SqlCommand(
                                        "UPDATE Room SET Status = 'booked', ReservationID = @ReservationID WHERE RoomID = @RoomID", con))
                                    {
                                        updateRoom.Transaction = transaction;
                                        updateRoom.Parameters.AddWithValue("@ReservationID", newReservationId);
                                        updateRoom.Parameters.AddWithValue("@RoomID", selectedRoomId);
                                        updateRoom.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    MessageBox.Show($"Reservation created successfully! ID: {newReservationId}");
                                    form.DialogResult = DialogResult.OK;
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    throw new Exception($"Error in transaction: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error creating reservation: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblGuestId, txtGuestId,
                    lblCheckin, dtpCheckin,
                    lblCheckout, dtpCheckout,
                    lblGuests, txtGuests,
                    btnSelectRoom,
                    lblSelectedRoom,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }

        private void BtnCancelReservation_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to cancel.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
            
            if (MessageBox.Show($"Are you sure you want to cancel reservation {reservationId}?", 
                "Confirm Cancellation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (DatabaseManager.CancelReservation(reservationId))
                {
                    MessageBox.Show("Reservation cancelled successfully.");
                    LoadReservations();
                }
                else
                {
                    MessageBox.Show("Failed to cancel reservation.");
                }
            }
        }

        private void BtnCalculateTotal_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to calculate total.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
            decimal total = DatabaseManager.CalculateReservationTotal(reservationId);
            MessageBox.Show($"Total amount for reservation {reservationId}: {total:C}");
        }

        private void BtnRecordPayment_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to record payment.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
            
            using (var form = new Form())
            {
                form.Text = "Record Payment";
                form.Size = new System.Drawing.Size(400, 200);

                var lblMethod = new Label { Text = "Payment Method:", Top = 20, Left = 20 };
                var txtMethod = new TextBox { Top = 20, Left = 120, Width = 200 };

                var lblAmount = new Label { Text = "Amount:", Top = 60, Left = 20 };
                var txtAmount = new TextBox { Top = 60, Left = 120, Width = 200 };

                var btnSubmit = new Button { Text = "Record", Top = 100, Left = 120, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (decimal.TryParse(txtAmount.Text, out decimal amount))
                    {
                        if (DatabaseManager.RecordPayment(reservationId, txtMethod.Text, amount))
                        {
                            MessageBox.Show("Payment recorded successfully.");
                            form.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show("Failed to record payment.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid amount.");
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblMethod, txtMethod,
                    lblAmount, txtAmount,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }

        private void BtnAssignStaff_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to assign staff.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
            
            using (var form = new Form())
            {
                form.Text = "Assign Staff";
                form.Size = new System.Drawing.Size(400, 150);

                var lblStaffId = new Label { Text = "Staff ID:", Top = 20, Left = 20 };
                var txtStaffId = new TextBox { Top = 20, Left = 120, Width = 200 };

                var btnSubmit = new Button { Text = "Assign", Top = 60, Left = 120, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (int.TryParse(txtStaffId.Text, out int staffId))
                    {
                        if (DatabaseManager.AssignStaffToReservation(staffId, reservationId))
                        {
                            MessageBox.Show("Staff assigned successfully.");
                            form.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show("Failed to assign staff.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid staff ID.");
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblStaffId, txtStaffId,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }

        private void BtnGetAvailableRooms_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Get Available Rooms";
                form.Size = new System.Drawing.Size(400, 150);  // Made form smaller since we need less inputs

                var lblGuests = new Label { Text = "Number of Guests:", Top = 20, Left = 20 };
                var txtGuests = new TextBox { Top = 20, Left = 150, Width = 200 };

                var btnSubmit = new Button { Text = "Search", Top = 60, Left = 150, Width = 200, Height = 40 };
                btnSubmit.Click += (s, args) =>
                {
                    if (int.TryParse(txtGuests.Text, out int numGuests))
                    {
                        DataTable availableRooms = DatabaseManager.GetAvailableRooms(numGuests);

                        if (availableRooms.Rows.Count > 0)
                        {
                            using (var roomsForm = new Form())
                            {
                                roomsForm.Text = "Available Rooms";
                                roomsForm.Size = new System.Drawing.Size(800, 500);

                                var dgvRooms = new DataGridView();
                                dgvRooms.Dock = DockStyle.Fill;
                                dgvRooms.DataSource = availableRooms;
                                dgvRooms.ReadOnly = true;
                                dgvRooms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                dgvRooms.AllowUserToAddRows = false;

                                roomsForm.Controls.Add(dgvRooms);
                                roomsForm.ShowDialog();
                            }
                        }
                        else
                        {
                            MessageBox.Show("No available rooms found with sufficient capacity.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number of guests.");
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblGuests, txtGuests,
                    btnSubmit
                });

                form.ShowDialog();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReservations();
        }

        private void BtnUpdateCheckout_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to update.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);
            DateTime currentCheckout = Convert.ToDateTime(dgv.SelectedRows[0].Cells["CheckoutDate"].Value);

            using (var form = new Form())
            {
                form.Text = "Update Checkout Date";
                form.Size = new System.Drawing.Size(400, 200);

                var lblNewCheckout = new Label { Text = "New Checkout Date:", Top = 20, Left = 20 };
                var dtpNewCheckout = new DateTimePicker
                {
                    Top = 20,
                    Left = 150,
                    Width = 200,
                    Value = currentCheckout,
                    MinDate = DateTime.Today
                };

                var btnSubmit = new Button { Text = "Update", Top = 70, Left = 150, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE Reservation SET CheckoutDate = @NewCheckoutDate WHERE ReservationID = @ReservationID", con))
                            {
                                cmd.Parameters.AddWithValue("@NewCheckoutDate", dtpNewCheckout.Value);
                                cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                                cmd.ExecuteNonQuery();
                            }
                            MessageBox.Show("Checkout date updated successfully!");
                            form.DialogResult = DialogResult.OK;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating checkout date: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] { lblNewCheckout, dtpNewCheckout, btnSubmit });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }

        private void BtnDeleteReservation_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to delete.");
                return;
            }

            int reservationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ReservationID"].Value);

            if (MessageBox.Show("Are you sure you want to delete this reservation?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(Form1.connectionString))
                {
                    try
                    {
                        con.Open();
                        using (SqlTransaction transaction = con.BeginTransaction())
                        {
                            try
                            {
                                // Delete related payment records first
                                using (SqlCommand deletePayments = new SqlCommand(
                                    "DELETE FROM Payment WHERE ReservationID = @ReservationID", con))
                                {
                                    deletePayments.Transaction = transaction;
                                    deletePayments.Parameters.AddWithValue("@ReservationID", reservationId);
                                    deletePayments.ExecuteNonQuery();
                                }

                                // Update room status first
                                using (SqlCommand updateRoom = new SqlCommand(
                                    @"UPDATE Room 
                                      SET Status = 'Free', 
                                          ReservationID = NULL 
                                      WHERE ReservationID = @ReservationID", con))
                                {
                                    updateRoom.Transaction = transaction;
                                    updateRoom.Parameters.AddWithValue("@ReservationID", reservationId);
                                    updateRoom.ExecuteNonQuery();
                                }

                                // Delete the reservation
                                using (SqlCommand deleteReservation = new SqlCommand(
                                    "DELETE FROM Reservation WHERE ReservationID = @ReservationID", con))
                                {
                                    deleteReservation.Transaction = transaction;
                                    deleteReservation.Parameters.AddWithValue("@ReservationID", reservationId);
                                    deleteReservation.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Reservation deleted successfully!");
                                LoadReservations();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Error in transaction: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting reservation: {ex.Message}");
                    }
                }
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Guest Checkout";
                form.Size = new System.Drawing.Size(400, 150);

                var lblGuestId = new Label { Text = "Guest ID:", Top = 20, Left = 20 };
                var txtGuestId = new TextBox { Top = 20, Left = 120, Width = 200 };

                var btnSubmit = new Button { Text = "Find Reservations", Top = 60, Left = 120, Width = 150 };
                btnSubmit.Click += (s, args) =>
                {
                    if (!int.TryParse(txtGuestId.Text, out int guestId))
                    {
                        MessageBox.Show("Please enter a valid Guest ID.");
                        return;
                    }

                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            // Find all active reservations for the guest
                            using (SqlCommand findReservations = new SqlCommand(
                                @"SELECT r.ReservationID, r.CheckinDate, r.CheckoutDate, r.Number_of_Guests,
                                         rm.RoomNumber, rm.RoomType
                                  FROM Reservation r
                                  JOIN Room rm ON rm.ReservationID = r.ReservationID
                                  WHERE r.GuestID = @GuestID 
                                  AND r.Reservation_Status = 1", con))
                            {
                                findReservations.Parameters.AddWithValue("@GuestID", guestId);
                                
                                using (SqlDataAdapter da = new SqlDataAdapter(findReservations))
                                {
                                    DataTable dt = new DataTable();
                                    da.Fill(dt);

                                    if (dt.Rows.Count == 0)
                                    {
                                        MessageBox.Show("No active reservations found for this guest.");
                                        return;
                                    }

                                    // If there are reservations, show them in a new form
                                    using (var selectForm = new Form())
                                    {
                                        selectForm.Text = "Select Reservation to Checkout";
                                        selectForm.Size = new System.Drawing.Size(800, 400);
                                        selectForm.StartPosition = FormStartPosition.CenterParent;

                                        var dgvReservations = new DataGridView
                                        {
                                            Dock = DockStyle.Fill,
                                            ReadOnly = true,
                                            AllowUserToAddRows = false,
                                            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                                            DataSource = dt
                                        };

                                        var btnCheckoutSelected = new Button
                                        {
                                            Text = "Checkout Selected Reservation",
                                            Dock = DockStyle.Bottom,
                                            Height = 40
                                        };

                                        btnCheckoutSelected.Click += (s2, args2) =>
                                        {
                                            if (dgvReservations.SelectedRows.Count == 0)
                                            {
                                                MessageBox.Show("Please select a reservation to checkout.");
                                                return;
                                            }

                                            int selectedReservationId = Convert.ToInt32(dgvReservations.SelectedRows[0].Cells["ReservationID"].Value);

                                            if (MessageBox.Show("Are you sure you want to checkout this reservation?",
                                                "Confirm Checkout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                            {
                                                using (SqlTransaction transaction = con.BeginTransaction())
                                                {
                                                    try
                                                    {
                                                        // Delete related payment records first
                                                        using (SqlCommand deletePayments = new SqlCommand(
                                                            "DELETE FROM Payment WHERE ReservationID = @ReservationID", con))
                                                        {
                                                            deletePayments.Transaction = transaction;
                                                            deletePayments.Parameters.AddWithValue("@ReservationID", selectedReservationId);
                                                            deletePayments.ExecuteNonQuery();
                                                        }

                                                        // Update room status
                                                        using (SqlCommand updateRoom = new SqlCommand(
                                                            @"UPDATE Room 
                                                              SET Status = 'Free', 
                                                                  ReservationID = NULL 
                                                              WHERE ReservationID = @ReservationID", con))
                                                        {
                                                            updateRoom.Transaction = transaction;
                                                            updateRoom.Parameters.AddWithValue("@ReservationID", selectedReservationId);
                                                            updateRoom.ExecuteNonQuery();
                                                        }

                                                        // Delete the reservation
                                                        using (SqlCommand deleteReservation = new SqlCommand(
                                                            "DELETE FROM Reservation WHERE ReservationID = @ReservationID", con))
                                                        {
                                                            deleteReservation.Transaction = transaction;
                                                            deleteReservation.Parameters.AddWithValue("@ReservationID", selectedReservationId);
                                                            deleteReservation.ExecuteNonQuery();
                                                        }

                                                        transaction.Commit();
                                                        MessageBox.Show("Checkout completed successfully!");
                                                        selectForm.DialogResult = DialogResult.OK;
                                                        form.DialogResult = DialogResult.OK;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        transaction.Rollback();
                                                        throw new Exception($"Error in transaction: {ex.Message}");
                                                    }
                                                }
                                            }
                                        };

                                        selectForm.Controls.Add(dgvReservations);
                                        selectForm.Controls.Add(btnCheckoutSelected);
                                        selectForm.ShowDialog();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error during checkout: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblGuestId, txtGuestId,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }
    }
} 