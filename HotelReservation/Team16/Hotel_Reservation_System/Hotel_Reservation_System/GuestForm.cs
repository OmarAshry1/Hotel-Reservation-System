using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace Hotel_Reservation_System
{
    public partial class GuestForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;


        public GuestForm()
        {
            InitializeComponent();
            this.Load += GuestForm_Load;
            this.Size = new System.Drawing.Size(800, 600);
        }

        private void GuestForm_Load(object sender, EventArgs e)
        {
            // Create main layout panels
            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150
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

            // Setup Guest Operation Buttons
            var btnAddGuest = CreateButton("Add New Guest", 20, 20);
            btnAddGuest.Click += BtnAddGuest_Click;

            var btnViewAllGuests = CreateButton("View All Guests", 190, 20);
            btnViewAllGuests.Click += BtnViewAllGuests_Click;

            var btnSearchGuest = CreateButton("Search Guest", 360, 20);
            btnSearchGuest.Click += BtnSearchGuest_Click;

            var btnUpdateGuest = CreateButton("Update Guest", 20, 70);
            btnUpdateGuest.Click += BtnUpdateGuest_Click;

            var btnDeleteGuest = CreateButton("Delete Guest", 190, 70);
            btnDeleteGuest.Click += BtnDeleteGuest_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddGuest,
                btnViewAllGuests,
                btnSearchGuest,
                btnUpdateGuest,
                btnDeleteGuest
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
                Height = 35
            };
        }

        private void BtnViewAllGuests_Click(object sender, EventArgs e)
        {
            LoadGuests();
            gridPanel.Visible = true;
        }

        private void LoadGuests()
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT g.GuestID, g.Email, g.First_Name, g.Last_Name, p.GPhone_Number 
                                   FROM Guest g, Guest_PhoneNumber p
                                   WHERE g.GuestID = p.GuestID";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading guests: {ex.Message}");
                }
            }
        }

        private void BtnAddGuest_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add New Guest";
                form.Size = new System.Drawing.Size(400, 300);

                var lblFirstName = new Label { Text = "First Name:", Top = 20, Left = 20 };
                var txtFirstName = new TextBox { Top = 20, Left = 120, Width = 200 };

                var lblLastName = new Label { Text = "Last Name:", Top = 60, Left = 20 };
                var txtLastName = new TextBox { Top = 60, Left = 120, Width = 200 };

                var lblEmail = new Label { Text = "Email:", Top = 100, Left = 20 };
                var txtEmail = new TextBox { Top = 100, Left = 120, Width = 200 };

                var lblPhone = new Label { Text = "Phone Number:", Top = 140, Left = 20 };
                var txtPhone = new TextBox { Top = 140, Left = 120, Width = 200 };

                var btnSubmit = new Button 
                { 
                    Text = "Add Guest", 
                    Top = 180, 
                    Left = 120, 
                    Width = 200,
                    Height = 40,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                btnSubmit.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                        string.IsNullOrWhiteSpace(txtLastName.Text) ||
                        string.IsNullOrWhiteSpace(txtEmail.Text) ||
                        string.IsNullOrWhiteSpace(txtPhone.Text))
                    {
                        MessageBox.Show("Please fill in all fields.");
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
                                    // First, get the next available ID
                                    int nextId;
                                    using (SqlCommand getMaxId = new SqlCommand("SELECT ISNULL(MAX(GuestID), 0) + 1 FROM Guest", con))
                                    {
                                        getMaxId.Transaction = transaction;
                                        nextId = Convert.ToInt32(getMaxId.ExecuteScalar());
                                    }

                                    // Insert into Guest table
                                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Guest (GuestID, First_Name, Last_Name, Email) VALUES (@GuestID, @First_Name, @Last_Name, @Email)", con))
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.Parameters.AddWithValue("@GuestID", nextId);
                                        cmd.Parameters.AddWithValue("@First_Name", txtFirstName.Text);
                                        cmd.Parameters.AddWithValue("@Last_Name", txtLastName.Text);
                                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Insert into Guest_PhoneNumber table with the same GuestID
                                    using (SqlCommand phoneCmd = new SqlCommand("INSERT INTO Guest_PhoneNumber (GuestID, GPhone_Number) VALUES (@GuestID, @Phone)", con))
                                    {
                                        phoneCmd.Transaction = transaction;
                                        phoneCmd.Parameters.AddWithValue("@GuestID", nextId);
                                        phoneCmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                                        phoneCmd.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    MessageBox.Show("Guest added successfully!");
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
                            MessageBox.Show($"Error adding guest: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblFirstName, txtFirstName,
                    lblLastName, txtLastName,
                    lblEmail, txtEmail,
                    lblPhone, txtPhone,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK && gridPanel.Visible)
                {
                    LoadGuests();
                }
            }
        }

        private void BtnSearchGuest_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Search Guest by ID";
                form.Size = new System.Drawing.Size(400, 200);

                var lblSearch = new Label { Text = "Guest ID:", Top = 20, Left = 20 };
                var txtSearch = new TextBox { Top = 20, Left = 120, Width = 200 };

                var btnSubmit = new Button { Text = "Search", Top = 60, Left = 120, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        MessageBox.Show("Please enter a Guest ID.");
                        return;
                    }

                    if (!int.TryParse(txtSearch.Text, out int guestId))
                    {
                        MessageBox.Show("Please enter a valid numeric Guest ID.");
                        return;
                    }

                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            string query = @"SELECT g.GuestID, g.Email, g.First_Name, g.Last_Name, p.GPhone_Number 
                                          FROM Guest g, Guest_PhoneNumber p
                                          WHERE g.GuestID = p.GuestID 
                                          AND g.GuestID = @GuestID";
                            
                            SqlDataAdapter da = new SqlDataAdapter(query, con);
                            da.SelectCommand.Parameters.AddWithValue("@GuestID", guestId);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            
                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show("No guest found with this ID.");
                                return;
                            }

                            dgv.DataSource = dt;
                            gridPanel.Visible = true;
                            form.DialogResult = DialogResult.OK;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error searching guest: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblSearch, txtSearch,
                    btnSubmit
                });

                form.ShowDialog();
            }
        }

        private void BtnUpdateGuest_Click(object sender, EventArgs e)
        {
            if (!gridPanel.Visible)
            {
                MessageBox.Show("Please view all guests first and select a guest to update.");
                return;
            }

            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a guest to update.");
                return;
            }

            var selectedRow = dgv.SelectedRows[0];
            int guestId = Convert.ToInt32(selectedRow.Cells["GuestID"].Value);

            using (var form = new Form())
            {
                form.Text = "Update Guest";
                form.Size = new System.Drawing.Size(400, 300);

                var lblFirstName = new Label { Text = "First Name:", Top = 20, Left = 20 };
                var txtFirstName = new TextBox 
                { 
                    Top = 20, 
                    Left = 120, 
                    Width = 200,
                    Text = selectedRow.Cells["First_Name"].Value.ToString()
                };

                var lblLastName = new Label { Text = "Last Name:", Top = 60, Left = 20 };
                var txtLastName = new TextBox 
                { 
                    Top = 60, 
                    Left = 120, 
                    Width = 200,
                    Text = selectedRow.Cells["Last_Name"].Value.ToString()
                };

                var lblEmail = new Label { Text = "Email:", Top = 100, Left = 20 };
                var txtEmail = new TextBox 
                { 
                    Top = 100, 
                    Left = 120, 
                    Width = 200,
                    Text = selectedRow.Cells["Email"].Value.ToString()
                };

                var lblPhone = new Label { Text = "Phone Number:", Top = 140, Left = 20 };
                var txtPhone = new TextBox 
                { 
                    Top = 140, 
                    Left = 120, 
                    Width = 200,
                    Text = selectedRow.Cells["GPhone_Number"].Value.ToString()
                };

                var btnSubmit = new Button { Text = "Update", Top = 180, Left = 120, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                        string.IsNullOrWhiteSpace(txtLastName.Text) ||
                        string.IsNullOrWhiteSpace(txtEmail.Text) ||
                        string.IsNullOrWhiteSpace(txtPhone.Text))
                    {
                        MessageBox.Show("Please fill in all fields.");
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
                                    // Update Guest table
                                    using (SqlCommand cmd = new SqlCommand(
                                        "UPDATE Guest SET First_Name = @First_Name, Last_Name = @Last_Name, Email = @Email WHERE GuestID = @GuestID", con))
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.Parameters.AddWithValue("@GuestID", guestId);
                                        cmd.Parameters.AddWithValue("@First_Name", txtFirstName.Text);
                                        cmd.Parameters.AddWithValue("@Last_Name", txtLastName.Text);
                                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Update Guest_PhoneNumber table
                                    using (SqlCommand phoneCmd = new SqlCommand(
                                        "UPDATE Guest_PhoneNumber SET GPhone_Number = @Phone WHERE GuestID = @GuestID", con))
                                    {
                                        phoneCmd.Transaction = transaction;
                                        phoneCmd.Parameters.AddWithValue("@GuestID", guestId);
                                        phoneCmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                                        phoneCmd.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    MessageBox.Show("Guest updated successfully!");
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
                            MessageBox.Show($"Error updating guest: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblFirstName, txtFirstName,
                    lblLastName, txtLastName,
                    lblEmail, txtEmail,
                    lblPhone, txtPhone,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadGuests();
                }
            }
        }

        private void BtnDeleteGuest_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Delete Guest by ID";
                form.Size = new System.Drawing.Size(400, 200);

                var lblGuestId = new Label { Text = "Guest ID:", Top = 20, Left = 20 };
                var txtGuestId = new TextBox { Top = 20, Left = 120, Width = 200 };

                var btnSubmit = new Button { Text = "Delete", Top = 60, Left = 120, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtGuestId.Text))
                    {
                        MessageBox.Show("Please enter a Guest ID.");
                        return;
                    }

                    if (!int.TryParse(txtGuestId.Text, out int guestId))
                    {
                        MessageBox.Show("Please enter a valid numeric Guest ID.");
                        return;
                    }

                    // First check if the guest exists
                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            using (SqlCommand checkCmd = new SqlCommand("SELECT First_Name, Last_Name FROM Guest WHERE GuestID = @GuestID", con))
                            {
                                checkCmd.Parameters.AddWithValue("@GuestID", guestId);
                                using (SqlDataReader reader = checkCmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        MessageBox.Show("No guest found with this ID.");
                                        return;
                                    }

                                    string guestName = $"{reader["First_Name"]} {reader["Last_Name"]}";
                                    reader.Close();

                                    if (MessageBox.Show(
                                        $"Are you sure you want to delete guest {guestName} (ID: {guestId})?\nThis will also delete all associated records.",
                                        "Confirm Delete",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning) == DialogResult.Yes)
                                    {
                                        // First delete from Guest_PhoneNumber table
                                        using (SqlCommand phoneCmd = new SqlCommand(
                                            "DELETE FROM Guest_PhoneNumber WHERE GuestID = @GuestID", con))
                                        {
                                            phoneCmd.Parameters.AddWithValue("@GuestID", guestId);
                                            phoneCmd.ExecuteNonQuery();
                                        }

                                        // Then delete from Guest table
                                        using (SqlCommand deleteCmd = new SqlCommand(
                                            "DELETE FROM Guest WHERE GuestID = @GuestID", con))
                                        {
                                            deleteCmd.Parameters.AddWithValue("@GuestID", guestId);
                                            deleteCmd.ExecuteNonQuery();
                                        }

                                        MessageBox.Show($"Guest {guestName} deleted successfully!");
                                        form.DialogResult = DialogResult.OK;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting guest: {ex.Message}");
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lblGuestId,
                    txtGuestId,
                    btnSubmit
                });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadGuests(); // Refresh the guest list
                }
            }
        }
    }
}