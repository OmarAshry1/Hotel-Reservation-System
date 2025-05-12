using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hotel_Reservation_System
{
    public partial class StaffForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;

        public StaffForm()
        {
            InitializeComponent();
            this.Load += StaffForm_Load;
            this.Size = new System.Drawing.Size(800, 600);
        }

        private void StaffForm_Load(object sender, EventArgs e)
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

            // Setup Staff Operation Buttons
            var btnAddStaff = CreateButton("Add New Staff", 20, 20);
            btnAddStaff.Click += BtnAddStaff_Click;

            var btnViewStaff = CreateButton("View All Staff", 190, 20);
            btnViewStaff.Click += (s, args) => {
                LoadStaff();
                gridPanel.Visible = true;
            };

            var btnUpdateStaff = CreateButton("Update Staff", 360, 20);
            btnUpdateStaff.Click += BtnUpdateStaff_Click;

            var btnDeleteStaff = CreateButton("Delete Staff", 530, 20);
            btnDeleteStaff.Click += BtnDeleteStaff_Click;

            var btnAssignService = CreateButton("Assign Service", 700, 20);
            btnAssignService.Click += BtnAssignService_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddStaff,
                btnViewStaff,
                btnUpdateStaff,
                btnDeleteStaff,
                btnAssignService
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

        private void LoadStaff()
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT s.StaffID, s.First_Name, s.Last_Name, s.Email, 
                                   s.Role, s.Phone_Number, s.ServiceID, srv.ServiceName
                                   FROM Staff s
                                   LEFT JOIN Service srv ON s.ServiceID = srv.ServiceID";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading staff: {ex.Message}");
                }
            }
        }

        private void BtnAddStaff_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add New Staff";
                form.Size = new System.Drawing.Size(400, 500);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateStaffFormControls();
                form.Controls.AddRange(controls);

                // Hide ServiceID control for new staff
                controls[10].Visible = false;  // Label
                controls[11].Visible = false;  // TextBox

                var btnSubmit = new Button { Text = "Add Staff", Top = 400, Left = 150, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateStaffInput(controls, out var staffData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"INSERT INTO Staff (First_Name, Last_Name, Email, Role, Phone_Number) 
                                      VALUES (@FirstName, @LastName, @Email, @Role, @PhoneNumber);
                                      SELECT SCOPE_IDENTITY();", con))
                                {
                                    cmd.Parameters.AddWithValue("@FirstName", staffData.FirstName);
                                    cmd.Parameters.AddWithValue("@LastName", staffData.LastName);
                                    cmd.Parameters.AddWithValue("@Email", staffData.Email);
                                    cmd.Parameters.AddWithValue("@Role", staffData.Role);
                                    cmd.Parameters.AddWithValue("@PhoneNumber", staffData.PhoneNumber);

                                    int newStaffId = Convert.ToInt32(cmd.ExecuteScalar());
                                    MessageBox.Show($"Staff added successfully! Staff ID: {newStaffId}");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error adding staff: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadStaff();
                }
            }
        }

        private void BtnUpdateStaff_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a staff member to update.");
                return;
            }

            var selectedRow = dgv.SelectedRows[0];
            int staffId = Convert.ToInt32(selectedRow.Cells["StaffID"].Value);

            using (var form = new Form())
            {
                form.Text = "Update Staff";
                form.Size = new System.Drawing.Size(400, 500);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateStaffFormControls();
                form.Controls.AddRange(controls);

                // Fill in existing data
                ((TextBox)controls[1]).Text = selectedRow.Cells["First_Name"].Value.ToString();
                ((TextBox)controls[3]).Text = selectedRow.Cells["Last_Name"].Value.ToString();
                ((TextBox)controls[5]).Text = selectedRow.Cells["Email"].Value.ToString();
                ((TextBox)controls[7]).Text = selectedRow.Cells["Role"].Value.ToString();
                ((TextBox)controls[9]).Text = selectedRow.Cells["Phone_Number"].Value.ToString();
                ((TextBox)controls[11]).Text = selectedRow.Cells["ServiceID"].Value?.ToString() ?? "";

                var btnSubmit = new Button { Text = "Update Staff", Top = 400, Left = 150, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateStaffInput(controls, out var staffData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"UPDATE Staff 
                                      SET First_Name = @FirstName,
                                          Last_Name = @LastName,
                                          Email = @Email,
                                          Role = @Role,
                                          Phone_Number = @PhoneNumber,
                                          ServiceID = @ServiceID
                                      WHERE StaffID = @StaffID", con))
                                {
                                    cmd.Parameters.AddWithValue("@StaffID", staffId);
                                    cmd.Parameters.AddWithValue("@FirstName", staffData.FirstName);
                                    cmd.Parameters.AddWithValue("@LastName", staffData.LastName);
                                    cmd.Parameters.AddWithValue("@Email", staffData.Email);
                                    cmd.Parameters.AddWithValue("@Role", staffData.Role);
                                    cmd.Parameters.AddWithValue("@PhoneNumber", staffData.PhoneNumber);
                                    cmd.Parameters.AddWithValue("@ServiceID", 
                                        string.IsNullOrEmpty(staffData.ServiceID) ? DBNull.Value : (object)int.Parse(staffData.ServiceID));

                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Staff updated successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error updating staff: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadStaff();
                }
            }
        }

        private void BtnDeleteStaff_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a staff member to delete.");
                return;
            }

            int staffId = Convert.ToInt32(dgv.SelectedRows[0].Cells["StaffID"].Value);

            if (MessageBox.Show("Are you sure you want to delete this staff member?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(Form1.connectionString))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Staff WHERE StaffID = @StaffID", con))
                        {
                            cmd.Parameters.AddWithValue("@StaffID", staffId);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Staff deleted successfully!");
                            LoadStaff();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting staff: {ex.Message}");
                    }
                }
            }
        }

        private void BtnAssignService_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a staff member to assign a service.");
                return;
            }

            int staffId = Convert.ToInt32(dgv.SelectedRows[0].Cells["StaffID"].Value);

            using (var form = new Form())
            {
                form.Text = "Assign Service to Staff";
                form.Size = new System.Drawing.Size(600, 400);
                form.StartPosition = FormStartPosition.CenterParent;

                var dgvServices = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    MultiSelect = false
                };

                // Load available services
                using (SqlConnection con = new SqlConnection(Form1.connectionString))
                {
                    try
                    {
                        con.Open();
                        string query = "SELECT ServiceID, ServiceName, Description, Price FROM Service";
                        SqlDataAdapter da = new SqlDataAdapter(query, con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvServices.DataSource = dt;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading services: {ex.Message}");
                        return;
                    }
                }

                var btnAssign = new Button
                {
                    Text = "Assign Selected Service",
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                btnAssign.Click += (s, args) =>
                {
                    if (dgvServices.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("Please select a service to assign.");
                        return;
                    }

                    int serviceId = Convert.ToInt32(dgvServices.SelectedRows[0].Cells["ServiceID"].Value);

                    using (SqlConnection con = new SqlConnection(Form1.connectionString))
                    {
                        try
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE Staff SET ServiceID = @ServiceID WHERE StaffID = @StaffID", con))
                            {
                                cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                                cmd.Parameters.AddWithValue("@StaffID", staffId);
                                cmd.ExecuteNonQuery();
                                MessageBox.Show("Service assigned successfully!");
                                form.DialogResult = DialogResult.OK;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error assigning service: {ex.Message}");
                        }
                    }
                };

                form.Controls.Add(dgvServices);
                form.Controls.Add(btnAssign);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadStaff();
                }
            }
        }

        private Control[] CreateStaffFormControls()
        {
            var lblFirstName = new Label { Text = "First Name:", Top = 20, Left = 20 };
            var txtFirstName = new TextBox { Top = 20, Left = 150, Width = 200 };

            var lblLastName = new Label { Text = "Last Name:", Top = 70, Left = 20 };
            var txtLastName = new TextBox { Top = 70, Left = 150, Width = 200 };

            var lblEmail = new Label { Text = "Email:", Top = 120, Left = 20 };
            var txtEmail = new TextBox { Top = 120, Left = 150, Width = 200 };

            var lblRole = new Label { Text = "Role:", Top = 170, Left = 20 };
            var txtRole = new TextBox { Top = 170, Left = 150, Width = 200 };

            var lblPhone = new Label { Text = "Phone Number:", Top = 220, Left = 20 };
            var txtPhone = new TextBox { Top = 220, Left = 150, Width = 200 };

            var lblService = new Label { Text = "Service ID:", Top = 270, Left = 20 };
            var txtService = new TextBox { Top = 270, Left = 150, Width = 200 };

            return new Control[] {
                lblFirstName, txtFirstName,
                lblLastName, txtLastName,
                lblEmail, txtEmail,
                lblRole, txtRole,
                lblPhone, txtPhone,
                lblService, txtService
            };
        }

        private bool ValidateStaffInput(Control[] controls, out (string FirstName, string LastName, string Email, 
            string Role, string PhoneNumber, string ServiceID) staffData)
        {
            var txtFirstName = (TextBox)controls[1];
            var txtLastName = (TextBox)controls[3];
            var txtEmail = (TextBox)controls[5];
            var txtRole = (TextBox)controls[7];
            var txtPhone = (TextBox)controls[9];
            var txtService = (TextBox)controls[11];

            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtRole.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                staffData = default;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtService.Text) && !int.TryParse(txtService.Text, out _))
            {
                MessageBox.Show("Service ID must be a valid number.");
                staffData = default;
                return false;
            }

            staffData = (
                txtFirstName.Text.Trim(),
                txtLastName.Text.Trim(),
                txtEmail.Text.Trim(),
                txtRole.Text.Trim(),
                txtPhone.Text.Trim(),
                txtService.Text.Trim()
            );
            return true;
        }
    }
} 