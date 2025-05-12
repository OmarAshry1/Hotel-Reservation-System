using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace Hotel_Reservation_System
{
    public partial class RoomForm : Form
    {
        private DataGridView dgv;
        private Panel buttonPanel;
        private Panel gridPanel;

        public RoomForm()
        {
            InitializeComponent();
            this.Load += RoomForm_Load;
            this.Size = new System.Drawing.Size(1000, 600);
        }

        private void RoomForm_Load(object sender, EventArgs e)
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

            // Setup Room Operation Buttons
            var btnAddRoom = CreateButton("Add New Room", 20, 20);
            btnAddRoom.Click += BtnAddRoom_Click;

            var btnViewRooms = CreateButton("View All Rooms", 190, 20);
            btnViewRooms.Click += (s, args) => {
                LoadRooms();
                gridPanel.Visible = true;
            };

            var btnUpdateRoom = CreateButton("Update Room", 360, 20);
            btnUpdateRoom.Click += BtnUpdateRoom_Click;

            var btnDeleteRoom = CreateButton("Delete Room", 530, 20);
            btnDeleteRoom.Click += BtnDeleteRoom_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                btnAddRoom,
                btnViewRooms,
                btnUpdateRoom,
                btnDeleteRoom
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

        private void LoadRooms()
        {
            using (SqlConnection con = new SqlConnection(Form1.connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT RoomID, RoomNumber, Price_PerNight, Status, Capacity, RoomType, ReservationID 
                                   FROM Room";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading rooms: {ex.Message}");
                }
            }
        }

        private void BtnAddRoom_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add New Room";
                form.Size = new System.Drawing.Size(400, 500);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateRoomFormControls();
                form.Controls.AddRange(controls);

                var btnSubmit = new Button { Text = "Add Room", Top = 400, Left = 150, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateRoomInput(controls, out var roomData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"INSERT INTO Room (RoomNumber, Price_PerNight, Status, Capacity, RoomType) 
                                      VALUES (@RoomNumber, @Price_PerNight, @Status, @Capacity, @RoomType)", con))
                                {
                                    cmd.Parameters.AddWithValue("@RoomNumber", int.Parse(roomData.RoomNumber));
                                    cmd.Parameters.AddWithValue("@Price_PerNight", decimal.Parse(roomData.Price_PerNight));
                                    cmd.Parameters.AddWithValue("@Status", roomData.Status);
                                    cmd.Parameters.AddWithValue("@Capacity", int.Parse(roomData.Capacity));
                                    cmd.Parameters.AddWithValue("@RoomType", roomData.RoomType);

                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Room added successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error adding room: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRooms();
                }
            }
        }

        private void BtnUpdateRoom_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a room to update.");
                return;
            }

            var selectedRow = dgv.SelectedRows[0];
            int roomId = Convert.ToInt32(selectedRow.Cells["RoomID"].Value);

            using (var form = new Form())
            {
                form.Text = "Update Room";
                form.Size = new System.Drawing.Size(400, 500);
                form.StartPosition = FormStartPosition.CenterParent;

                var controls = CreateRoomFormControls();
                form.Controls.AddRange(controls);

                // Fill in existing data
                ((TextBox)controls[1]).Text = selectedRow.Cells["RoomNumber"].Value.ToString();
                ((TextBox)controls[3]).Text = selectedRow.Cells["Price_PerNight"].Value.ToString();
                ((TextBox)controls[5]).Text = selectedRow.Cells["Status"].Value.ToString();
                ((TextBox)controls[7]).Text = selectedRow.Cells["Capacity"].Value.ToString();
                ((TextBox)controls[9]).Text = selectedRow.Cells["RoomType"].Value.ToString();

                var btnSubmit = new Button { Text = "Update Room", Top = 400, Left = 150, Width = 100 };
                btnSubmit.Click += (s, args) =>
                {
                    if (ValidateRoomInput(controls, out var roomData))
                    {
                        using (SqlConnection con = new SqlConnection(Form1.connectionString))
                        {
                            try
                            {
                                con.Open();
                                using (SqlCommand cmd = new SqlCommand(
                                    @"UPDATE Room 
                                      SET RoomNumber = @RoomNumber,
                                          Price_PerNight = @Price_PerNight,
                                          Status = @Status,
                                          Capacity = @Capacity,
                                          RoomType = @RoomType
                                      WHERE RoomID = @RoomID", con))
                                {
                                    cmd.Parameters.AddWithValue("@RoomID", roomId);
                                    cmd.Parameters.AddWithValue("@RoomNumber", int.Parse(roomData.RoomNumber));
                                    cmd.Parameters.AddWithValue("@Price_PerNight", decimal.Parse(roomData.Price_PerNight));
                                    cmd.Parameters.AddWithValue("@Status", roomData.Status);
                                    cmd.Parameters.AddWithValue("@Capacity", int.Parse(roomData.Capacity));
                                    cmd.Parameters.AddWithValue("@RoomType", roomData.RoomType);

                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Room updated successfully!");
                                    form.DialogResult = DialogResult.OK;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error updating room: {ex.Message}");
                            }
                        }
                    }
                };

                form.Controls.Add(btnSubmit);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRooms();
                }
            }
        }

        private void BtnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a room to delete.");
                return;
            }

            int roomId = Convert.ToInt32(dgv.SelectedRows[0].Cells["RoomID"].Value);

            if (MessageBox.Show("Are you sure you want to delete this room?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(Form1.connectionString))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Room WHERE RoomID = @RoomID", con))
                        {
                            cmd.Parameters.AddWithValue("@RoomID", roomId);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Room deleted successfully!");
                            LoadRooms();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting room: {ex.Message}");
                    }
                }
            }
        }

        private Control[] CreateRoomFormControls()
        {
            var lblRoomNumber = new Label { Text = "Room Number:", Top = 20, Left = 20 };
            var txtRoomNumber = new TextBox { Top = 20, Left = 150, Width = 200 };

            var lblPricePer = new Label { Text = "Price Per Night:", Top = 70, Left = 20 };
            var txtPricePer = new TextBox { Top = 70, Left = 150, Width = 200 };

            var lblStatus = new Label { Text = "Status:", Top = 120, Left = 20 };
            var txtStatus = new TextBox { Top = 120, Left = 150, Width = 200, Text = "Free", Enabled = false };

            var lblCapacity = new Label { Text = "Capacity:", Top = 170, Left = 20 };
            var txtCapacity = new TextBox { Top = 170, Left = 150, Width = 200 };

            var lblType = new Label { Text = "Room Type:", Top = 220, Left = 20 };
            var txtType = new TextBox { Top = 220, Left = 150, Width = 200 };

            return new Control[] {
                lblRoomNumber, txtRoomNumber,
                lblPricePer, txtPricePer,
                lblStatus, txtStatus,
                lblCapacity, txtCapacity,
                lblType, txtType
            };
        }

        private bool ValidateRoomInput(Control[] controls, out (string RoomNumber, string Price_PerNight, 
            string Status, string Capacity, string RoomType) roomData)
        {
            var txtRoomNumber = (TextBox)controls[1];
            var txtPricePer = (TextBox)controls[3];
            var txtStatus = (TextBox)controls[5];
            var txtCapacity = (TextBox)controls[7];
            var txtType = (TextBox)controls[9];

            if (!int.TryParse(txtRoomNumber.Text, out _) ||
                !decimal.TryParse(txtPricePer.Text, out decimal price) ||
                price <= 0 ||
                string.IsNullOrWhiteSpace(txtStatus.Text) ||
                !int.TryParse(txtCapacity.Text, out int capacity) ||
                capacity <= 0 ||
                string.IsNullOrWhiteSpace(txtType.Text))
            {
                MessageBox.Show("Please enter valid values:\n" +
                              "- Room Number must be a number\n" +
                              "- Price must be greater than 0\n" +
                              "- Status cannot be empty\n" +
                              "- Capacity must be greater than 0\n" +
                              "- Room Type cannot be empty");
                roomData = default;
                return false;
            }

            roomData = (
                txtRoomNumber.Text.Trim(),
                txtPricePer.Text.Trim(),
                txtStatus.Text.Trim(),
                txtCapacity.Text.Trim(),
                txtType.Text.Trim()
            );
            return true;
        }
    }
} 