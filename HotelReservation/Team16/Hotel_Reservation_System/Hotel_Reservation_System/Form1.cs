using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hotel_Reservation_System
{
    public partial class Form1 : Form
    {
        // You can use this connection string in any form!
        public static readonly string connectionString =
            "Data Source=.;Initial Catalog=Hotel_Reservation_System;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        public Form1()
        {
            InitializeComponent();

            // Add buttons to open different forms
            Button btnGuests = new Button();
            btnGuests.Text = "Manage Guests";
            btnGuests.Width = 200;
            btnGuests.Height = 40;
            btnGuests.Top = 30;
            btnGuests.Left = 30;
            btnGuests.Click += BtnGuests_Click;
            this.Controls.Add(btnGuests);

            Button btnRooms = new Button();
            btnRooms.Text = "Manage Rooms";
            btnRooms.Width = 200;
            btnRooms.Height = 40;
            btnRooms.Top = 80;
            btnRooms.Left = 30;
            btnRooms.Click += BtnRooms_Click;
            this.Controls.Add(btnRooms);

            Button btnReservations = new Button();
            btnReservations.Text = "Manage Reservations";
            btnReservations.Width = 200;
            btnReservations.Height = 40;
            btnReservations.Top = 130;
            btnReservations.Left = 30;
            btnReservations.Click += BtnReservations_Click;
            this.Controls.Add(btnReservations);

            Button btnStaff = new Button();
            btnStaff.Text = "Manage Staff";
            btnStaff.Width = 200;
            btnStaff.Height = 40;
            btnStaff.Top = 180;
            btnStaff.Left = 30;
            btnStaff.Click += BtnStaff_Click;
            this.Controls.Add(btnStaff);

            Button btnServices = new Button();
            btnServices.Text = "Manage Services";
            btnServices.Width = 200;
            btnServices.Height = 40;
            btnServices.Top = 230;
            btnServices.Left = 30;
            btnServices.Click += BtnServices_Click;
            this.Controls.Add(btnServices);

            Button btnPayments = new Button();
            btnPayments.Text = "Manage Payments";
            btnPayments.Width = 200;
            btnPayments.Height = 40;
            btnPayments.Top = 280;
            btnPayments.Left = 30;
            btnPayments.Click += BtnPayments_Click;
            this.Controls.Add(btnPayments);

            // Set form size to accommodate all buttons
            this.Size = new System.Drawing.Size(280, 400);
        }

        private void BtnGuests_Click(object sender, EventArgs e)
        {
            GuestForm guestForm = new GuestForm();
            guestForm.ShowDialog();
        }

        private void BtnRooms_Click(object sender, EventArgs e)
        {
            RoomForm roomForm = new RoomForm();
            roomForm.ShowDialog();
        }

        private void BtnReservations_Click(object sender, EventArgs e)
        {
            ReservationForm reservationForm = new ReservationForm();
            reservationForm.ShowDialog();
        }

        private void BtnStaff_Click(object sender, EventArgs e)
        {
            StaffForm staffForm = new StaffForm();
            staffForm.ShowDialog();
        }

        private void BtnServices_Click(object sender, EventArgs e)
        {
            ServiceForm serviceForm = new ServiceForm();
            serviceForm.ShowDialog();
        }

        private void BtnPayments_Click(object sender, EventArgs e)
        {
            PaymentForm paymentForm = new PaymentForm();
            paymentForm.ShowDialog();
        }
    }
}
