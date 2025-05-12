using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hotel_Reservation_System
{
    public partial class PaymentForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;

        public PaymentForm()
        {
            InitializeComponent();
            this.Load += PaymentForm_Load;
            this.Size = new System.Drawing.Size(1000, 600);
        }

        private void PaymentForm_Load(object sender, EventArgs e)
        {
            // Create main layout panels
            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100
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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            gridPanel.Controls.Add(dgv);

            // Setup Payment Operation Buttons
            var btnAddPayment = CreateButton("Add Payment", 20, 20);
            btnAddPayment.Click += BtnAddPayment_Click;

            var btnViewPayments = CreateButton("View Payments", 190, 20);
            btnViewPayments.Click += BtnViewPayments_Click;

            var btnProcessPayment = CreateButton("Process Payment", 360, 20);
            btnProcessPayment.Click += BtnProcessPayment_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddPayment,
                btnViewPayments,
                btnProcessPayment
            });
        }

        private Button CreateButton(string text, int left, int top)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 150,
                Height = 40
            };
        }

        private decimal CalculateReservationAmount(int reservationId)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT 
                            (DATEDIFF(day, o.CheckinDate, o.CheckoutDate) * r.Price_PerNight) + ISNULL(SUM(s.Price), 0) as TotalAmount
                        FROM Reservation o
                        JOIN Room r ON r.ReservationID = o.ReservationID
                        LEFT JOIN Service s ON s.RoomID = r.RoomID
                        WHERE o.ReservationID = @ReservationID
                        GROUP BY o.CheckinDate, o.CheckoutDate, r.Price_PerNight", con))
                    {
                        cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                        object result = cmd.ExecuteScalar();
                        return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error calculating amount: {ex.Message}");
                    return 0;
                }
            }
        }

        private void BtnAddPayment_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add Payment";
                form.Size = new System.Drawing.Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;

                var lblReservationId = new Label { Text = "Reservation ID:", Top = 20, Left = 20 };
                var txtReservationId = new TextBox { Top = 20, Left = 150, Width = 200 };

                var lblAmount = new Label { Text = "Total Amount:", Top = 60, Left = 20 };
                var lblCalculatedAmount = new Label { Top = 60, Left = 150, Width = 200 };

                txtReservationId.TextChanged += (s, args) =>
                {
                    if (int.TryParse(txtReservationId.Text, out int resId))
                    {
                        decimal amount = CalculateReservationAmount(resId);
                        lblCalculatedAmount.Text = amount.ToString("C");
                    }
                    else
                    {
                        lblCalculatedAmount.Text = "$0.00";
                    }
                };

                var btnSubmit = new Button { Text = "Create Payment Record", Top = 100, Left = 150, Width = 150 };
                btnSubmit.Click += (s, args) =>
                {
                    if (!int.TryParse(txtReservationId.Text, out int reservationId))
                    {
                        MessageBox.Show("Please enter a valid Reservation ID.");
                        return;
                    }

                    decimal amount = CalculateReservationAmount(reservationId);
                    
                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            
                            // Check if a payment record already exists for this reservation
                            using (SqlCommand checkCmd = new SqlCommand(
                                @"SELECT COUNT(*) FROM Payment 
                                  WHERE ReservationID = @ReservationID", con))
                            {
                                checkCmd.Parameters.AddWithValue("@ReservationID", reservationId);
                                int existingPayments = (int)checkCmd.ExecuteScalar();
                                
                                if (existingPayments > 0)
                                {
                                    MessageBox.Show("A payment record already exists for this reservation. Please use the 'Process Payment' option instead.");
                                    return;
                                }
                            }
                            
                            // Create new payment record if none exists
                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO Payment (ReservationID, AmountPaid, Payment_Status) 
                                  VALUES (@ReservationID, @AmountPaid, 'Not Paid')", con))
                            {
                                cmd.Parameters.AddWithValue("@ReservationID", reservationId);
                                cmd.Parameters.AddWithValue("@AmountPaid", amount);

                                cmd.ExecuteNonQuery();
                                MessageBox.Show($"Payment record created successfully!\nTotal Amount: {amount:C}");
                                form.DialogResult = DialogResult.OK;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error creating payment: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblReservationId, txtReservationId,
                    lblAmount, lblCalculatedAmount,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadPayments();
                }
            }
        }

        private void BtnViewPayments_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "View Payments";
                form.Size = new System.Drawing.Size(400, 150);
                form.StartPosition = FormStartPosition.CenterParent;

                var lblReservationId = new Label { Text = "Reservation ID:", Top = 20, Left = 20 };
                var txtReservationId = new TextBox { Top = 20, Left = 150, Width = 200 };

                var btnView = new Button { Text = "View", Top = 60, Left = 150, Width = 100 };
                btnView.Click += (s, args) =>
                {
                    if (int.TryParse(txtReservationId.Text, out int reservationId))
                    {
                        LoadPayments(reservationId);
                        gridPanel.Visible = true;
                        form.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid Reservation ID.");
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblReservationId, txtReservationId,
                    btnView
                });

                form.ShowDialog();
            }
        }

        private void BtnProcessPayment_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Process Payment";
                form.Size = new System.Drawing.Size(400, 300);
                form.StartPosition = FormStartPosition.CenterParent;

                var lblReservationId = new Label { Text = "Reservation ID:", Top = 20, Left = 20 };
                var txtReservationId = new TextBox { Top = 20, Left = 150, Width = 200 };

                var lblAmount = new Label { Text = "Total Amount:", Top = 60, Left = 20 };
                var lblCalculatedAmount = new Label { Top = 60, Left = 150, Width = 200 };

                var lblMethod = new Label { Text = "Payment Method:", Top = 100, Left = 20 };
                var txtMethod = new TextBox { Top = 100, Left = 150, Width = 200 };

                txtReservationId.TextChanged += (s, args) =>
                {
                    if (int.TryParse(txtReservationId.Text, out int resId))
                    {
                        decimal amount = CalculateReservationAmount(resId);
                        lblCalculatedAmount.Text = amount.ToString("C");
                    }
                    else
                    {
                        lblCalculatedAmount.Text = "$0.00";
                    }
                };

                var btnProcess = new Button { Text = "Process Payment", Top = 200, Left = 150, Width = 100 };
                btnProcess.Click += (s, args) =>
                {
                    if (!int.TryParse(txtReservationId.Text, out int reservationId))
                    {
                        MessageBox.Show("Please enter a valid Reservation ID.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtMethod.Text))
                    {
                        MessageBox.Show("Please enter a payment method.");
                        return;
                    }

                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE Payment 
                                  SET Payment_Method = @Method,
                                      Payment_Status = 'Paid',
                                      PaymentDate = GETDATE()
                                  WHERE ReservationID = @ReservationID", con))
                            {
                                cmd.Parameters.AddWithValue("@Method", txtMethod.Text);
                                cmd.Parameters.AddWithValue("@ReservationID", reservationId);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Payment processed successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                                else
                                {
                                    MessageBox.Show("No payment record found for this reservation. Please create a payment record first.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error processing payment: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblReservationId, txtReservationId,
                    lblAmount, lblCalculatedAmount,
                    lblMethod, txtMethod,
                    btnProcess
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadPayments();
                }
            }
        }

        private void LoadPayments(int? reservationId = null)
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT o.ReservationID, p.PaymentID, p.PaymentDate, 
                                   p.Payment_Method, p.Payment_Status, p.AmountPaid
                                   FROM Reservation o 
                                   JOIN Payment p ON o.ReservationID = p.ReservationID";

                    if (reservationId.HasValue)
                    {
                        query += " WHERE o.ReservationID = @ReservationID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (reservationId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ReservationID", reservationId.Value);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading payments: {ex.Message}");
                }
            }
        }
    }
} 