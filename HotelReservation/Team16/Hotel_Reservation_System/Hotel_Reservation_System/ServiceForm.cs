using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hotel_Reservation_System
{
    public partial class ServiceForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;

        public ServiceForm()
        {
            InitializeComponent();
            this.Load += ServiceForm_Load;
            this.Size = new System.Drawing.Size(1000, 600);
        }

        private void ServiceForm_Load(object sender, EventArgs e)
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

            // Setup Service Operation Buttons
            var btnAddService = CreateButton("Add New Service", 20, 20);
            btnAddService.Click += BtnAddService_Click;

            var btnViewServices = CreateButton("View All Services", 190, 20);
            btnViewServices.Click += (s, args) => {
                LoadServices();
                gridPanel.Visible = true;
            };

            var btnUpdateService = CreateButton("Update Service", 360, 20);
            btnUpdateService.Click += BtnUpdateService_Click;

            var btnDeleteService = CreateButton("Delete Service", 530, 20);
            btnDeleteService.Click += BtnDeleteService_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddService,
                btnViewServices,
                btnUpdateService,
                btnDeleteService
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

        private void LoadServices()
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT s.ServiceID, s.ServiceName, s.Description, s.Price, s.Process,
                                   s.RoomID, r.RoomNumber
                                   FROM Service s
                                   LEFT JOIN Room r ON r.RoomID = s.RoomID";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading services: {ex.Message}");
                }
            }
        }

        private void BtnAddService_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add New Service";
                form.Size = new System.Drawing.Size(500, 600);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateServiceFormControls();
                form.Controls.AddRange(controls);

                var btnSubmit = new Button { Text = "Add Service", Top = 500, Left = 200, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateServiceInput(controls, out var serviceData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"INSERT INTO Service (ServiceName, Description, Price, Process, RoomID) 
                                      VALUES (@ServiceName, @Description, @Price, @Process, @RoomID)", con))
                                {
                                    cmd.Parameters.AddWithValue("@ServiceName", serviceData.ServiceName);
                                    cmd.Parameters.AddWithValue("@Description", serviceData.Description);
                                    cmd.Parameters.AddWithValue("@Price", serviceData.Price);
                                    cmd.Parameters.AddWithValue("@Process", serviceData.Process);
                                    cmd.Parameters.AddWithValue("@RoomID", 
                                        string.IsNullOrEmpty(serviceData.RoomID) ? DBNull.Value : (object)int.Parse(serviceData.RoomID));

                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Service added successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error adding service: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadServices();
                }
            }
        }

        private void BtnUpdateService_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service to update.");
                return;
            }

            var selectedRow = dgv.SelectedRows[0];
            int serviceId = Convert.ToInt32(selectedRow.Cells["ServiceID"].Value);

            using (var form = new Form())
            {
                form.Text = "Update Service";
                form.Size = new System.Drawing.Size(500, 600);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateServiceFormControls();
                form.Controls.AddRange(controls);

                // Fill in existing data
                ((TextBox)controls[1]).Text = selectedRow.Cells["ServiceName"].Value.ToString();
                ((TextBox)controls[3]).Text = selectedRow.Cells["Description"].Value?.ToString() ?? "";
                ((TextBox)controls[5]).Text = selectedRow.Cells["Price"].Value.ToString();
                ((TextBox)controls[7]).Text = selectedRow.Cells["Process"].Value?.ToString() ?? "";
                ((TextBox)controls[9]).Text = selectedRow.Cells["RoomID"].Value?.ToString() ?? "";

                var btnSubmit = new Button { Text = "Update Service", Top = 500, Left = 200, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateServiceInput(controls, out var serviceData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"UPDATE Service 
                                      SET ServiceName = @ServiceName,
                                          Description = @Description,
                                          Price = @Price,
                                          Process = @Process,
                                          RoomID = @RoomID
                                      WHERE ServiceID = @ServiceID", con))
                                {
                                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                                    cmd.Parameters.AddWithValue("@ServiceName", serviceData.ServiceName);
                                    cmd.Parameters.AddWithValue("@Description", serviceData.Description);
                                    cmd.Parameters.AddWithValue("@Price", serviceData.Price);
                                    cmd.Parameters.AddWithValue("@Process", serviceData.Process);
                                    cmd.Parameters.AddWithValue("@RoomID", 
                                        string.IsNullOrEmpty(serviceData.RoomID) ? DBNull.Value : (object)int.Parse(serviceData.RoomID));

                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Service updated successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error updating service: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadServices();
                }
            }
        }

        private void BtnDeleteService_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service to delete.");
                return;
            }

            int serviceId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ServiceID"].Value);

            if (MessageBox.Show("Are you sure you want to delete this service?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(Form1.connectionString))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Service WHERE ServiceID = @ServiceID", con))
                        {
                            cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Service deleted successfully!");
                            LoadServices();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting service: {ex.Message}");
                    }
                }
            }
        }

        private Control[] CreateServiceFormControls()
        {
            var lblName = new Label { Text = "Service Name:", Top = 20, Left = 20 };
            var txtName = new TextBox { Top = 20, Left = 150, Width = 300 };

            var lblDescription = new Label { Text = "Description:", Top = 70, Left = 20 };
            var txtDescription = new TextBox 
            { 
                Top = 70, 
                Left = 150, 
                Width = 300,
                Height = 100,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var lblPrice = new Label { Text = "Price:", Top = 190, Left = 20 };
            var txtPrice = new TextBox { Top = 190, Left = 150, Width = 300 };

            var lblProcess = new Label { Text = "Process:", Top = 240, Left = 20 };
            var txtProcess = new TextBox 
            { 
                Top = 240, 
                Left = 150, 
                Width = 300,
                Height = 100,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var lblRoom = new Label { Text = "Room ID:", Top = 360, Left = 20 };
            var txtRoom = new TextBox { Top = 360, Left = 150, Width = 300 };

            return new Control[] {
                lblName, txtName,
                lblDescription, txtDescription,
                lblPrice, txtPrice,
                lblProcess, txtProcess,
                lblRoom, txtRoom
            };
        }

        private bool ValidateServiceInput(Control[] controls, out (string ServiceName, string Description, 
            decimal Price, string Process, string RoomID) serviceData)
        {
            var txtName = (TextBox)controls[1];
            var txtDescription = (TextBox)controls[3];
            var txtPrice = (TextBox)controls[5];
            var txtProcess = (TextBox)controls[7];
            var txtRoom = (TextBox)controls[9];

            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                !decimal.TryParse(txtPrice.Text, out decimal price) ||
                price < 0)
            {
                MessageBox.Show("Please enter a valid service name and price (must be >= 0).");
                serviceData = default;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtRoom.Text) && !int.TryParse(txtRoom.Text, out _))
            {
                MessageBox.Show("Room ID must be a valid number.");
                serviceData = default;
                return false;
            }

            serviceData = (
                txtName.Text.Trim(),
                txtDescription.Text.Trim(),
                price,
                txtProcess.Text.Trim(),
                txtRoom.Text.Trim()
            );
            return true;
        }
    }
} 