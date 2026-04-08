using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Proiect
{
    public partial class StudyRoomsWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string _currentUsername;
        private List<StudySeat> _seats;

        public StudyRoomsWindow(string username)
        {
            InitializeComponent();
            _currentUsername = username;

            
            _libraryService.RemoveExpiredSeatReservations();
            LoadSeats();
        }

        private void LoadSeats()
        {
            _seats = _libraryService.GetAllSeats();

            var displaySeats = _seats.Select(s => new
            {
                s.SeatNumber,
                Color = s.IsReserved ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A8192E")) : 
                                       new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"))   
            }).ToList();

            SeatsItemsControl.ItemsSource = displaySeats;
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
         
            System.Windows.Controls.Button clickedButton = sender as System.Windows.Controls.Button;
            int seatNumber = (int)clickedButton.Tag;

            StudySeat selectedSeat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);

            if (selectedSeat.IsReserved)
            {
                if (selectedSeat.ReservedBy == _currentUsername)
                {
                    MessageBox.Show($"Ai rezervat deja acest loc la ora {selectedSeat.ReservationDate?.ToString("HH:mm")}.");
                }
                else
                {
                    MessageBox.Show("Acest loc este deja rezervat de altcineva!");
                }
                return;
            }

            
            if (_seats.Any(s => s.IsReserved && s.ReservedBy == _currentUsername))
            {
                MessageBox.Show("Ai deja un loc de studiu rezervat. Nu poți rezerva mai multe în același timp.");
                return;
            }

            
            selectedSeat.IsReserved = true;
            selectedSeat.ReservedBy = _currentUsername;
            selectedSeat.ReservationDate = DateTime.Now;

            _libraryService.SaveSeats(_seats);

            MessageBox.Show($"Ai rezervat locul {seatNumber} cu succes pentru următoarele 4 ore!");
            LoadSeats(); 
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StudentWindow studentWindow = new StudentWindow(_currentUsername);
            studentWindow.Show();
            this.Close();
        }
    }
}