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

        private const int OpeningHour = 9;
        private const int ClosingHour = 20;
        private const int ReservationHours = 4;

        public StudyRoomsWindow(string username)
        {
            InitializeComponent();
            _currentUsername = username;

            _libraryService.RemoveExpiredSeatReservations();
            LoadSeats();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _libraryService.RemoveExpiredSeatReservations();
            LoadSeats();
        }

        private DateTime GetReservationEndTime(DateTime reservationStart)
        {
            DateTime normalEndTime = reservationStart.AddHours(ReservationHours);
            DateTime libraryClosingTime = reservationStart.Date.AddHours(ClosingHour);

            return normalEndTime < libraryClosingTime
                ? normalEndTime
                : libraryClosingTime;
        }

        private string FormatRemainingTime(TimeSpan remaining)
        {
            int hours = (int)remaining.TotalHours;
            int minutes = remaining.Minutes;

            if (hours > 0)
                return $"{hours}h {minutes}min";

            return $"{minutes}min";
        }

        private void LoadSeats()
        {
            _seats = _libraryService.GetAllSeats();

            var displaySeats = _seats.Select(s =>
            {
                string tooltip;

                if (s.IsReserved && s.ReservationDate.HasValue)
                {
                    DateTime freeAt = GetReservationEndTime(s.ReservationDate.Value);
                    TimeSpan remaining = freeAt - DateTime.Now;

                    if (remaining.TotalMinutes > 0)
                    {
                        tooltip = $"Loc ocupat. Liber în {FormatRemainingTime(remaining)}. Disponibil de la {freeAt:HH:mm}.";
                    }
                    else
                    {
                        tooltip = "Rezervarea a expirat. Apasă Refresh pentru actualizare.";
                    }
                }
                else if (s.IsReserved)
                {
                    tooltip = "Loc ocupat.";
                }
                else
                {
                    tooltip = "Loc disponibil.";
                }

                return new
                {
                    s.SeatNumber,
                    ToolTip = tooltip,
                    Color = s.IsReserved
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A8192E"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"))
                };
            }).ToList();

            SeatsItemsControl.ItemsSource = displaySeats;
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button clickedButton = sender as System.Windows.Controls.Button;

            if (clickedButton == null)
                return;

            int seatNumber = (int)clickedButton.Tag;
            StudySeat selectedSeat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);

            if (selectedSeat == null)
                return;

            if (selectedSeat.IsReserved)
            {
                if (selectedSeat.ReservedBy == _currentUsername)
                {
                    string message = $"Ai rezervat deja acest loc la ora {selectedSeat.ReservationDate?.ToString("HH:mm")}.";

                    if (selectedSeat.ReservationDate.HasValue)
                    {
                        DateTime freeAt = GetReservationEndTime(selectedSeat.ReservationDate.Value);
                        TimeSpan remaining = freeAt - DateTime.Now;

                        if (remaining.TotalMinutes > 0)
                        {
                            message += $"\nMai ai {FormatRemainingTime(remaining)}.";
                            message += $"\nRezervarea expiră la ora {freeAt:HH:mm}.";
                        }
                        else
                        {
                            message += "\nRezervarea a expirat. Apasă Refresh pentru actualizare.";
                        }
                    }

                    MessageBox.Show(message);
                }
                else
                {
                    string message = "Acest loc este deja rezervat de altcineva.";

                    if (selectedSeat.ReservationDate.HasValue)
                    {
                        DateTime freeAt = GetReservationEndTime(selectedSeat.ReservationDate.Value);
                        TimeSpan remaining = freeAt - DateTime.Now;

                        if (remaining.TotalMinutes > 0)
                        {
                            message += $"\nLocul va fi liber în {FormatRemainingTime(remaining)}.";
                            message += $"\nDisponibil de la ora {freeAt:HH:mm}.";
                        }
                        else
                        {
                            message += "\nRezervarea a expirat. Apasă Refresh pentru actualizare.";
                        }
                    }

                    MessageBox.Show(message);
                }

                return;
            }

            DateTime now = DateTime.Now;
            DateTime openingTime = now.Date.AddHours(OpeningHour);
            DateTime closingTime = now.Date.AddHours(ClosingHour);

            if (now < openingTime || now >= closingTime)
            {
                MessageBox.Show("Rezervările se pot face doar între orele 09:00 și 19:59.");
                return;
            }

            if (_seats.Any(s => s.IsReserved && s.ReservedBy == _currentUsername))
            {
                MessageBox.Show("Ai deja un loc de studiu rezervat. Nu poți rezerva mai multe în același timp.");
                return;
            }

            DateTime reservationEndTime = GetReservationEndTime(now);
            TimeSpan reservationDuration = reservationEndTime - now;

            string confirmMessage;

            if (now.Hour >= 16)
            {
                confirmMessage =
                    $"Vrei să rezervi locul {seatNumber} până la ora {reservationEndTime:HH:mm}?\n" +
                    "Biblioteca se închide la ora 20:00.";
            }
            else
            {
                confirmMessage =
                    $"Vrei să rezervi locul {seatNumber} pentru următoarele {FormatRemainingTime(reservationDuration)}?\n" +
                    $"Rezervarea expiră la ora {reservationEndTime:HH:mm}.";
            }

            MessageBoxResult result = MessageBox.Show(
                confirmMessage,
                "Confirmare rezervare",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            selectedSeat.IsReserved = true;
            selectedSeat.ReservedBy = _currentUsername;
            selectedSeat.ReservationDate = now;

            _libraryService.SaveSeats(_seats);

            LoadSeats();

            MessageBox.Show(
                $"Locul {seatNumber} a fost rezervat cu succes.\nRezervarea expiră la ora {reservationEndTime:HH:mm}.",
                "Rezervare reușită",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StudentWindow studentWindow = new StudentWindow(_currentUsername);
            studentWindow.Show();
            this.Close();
        }
        private void ReleaseSeatButton_Click(object sender, RoutedEventArgs e)
        {
            // Căutăm locul rezervat de studentul conectat în acest moment
            StudySeat mySeat = _seats.FirstOrDefault(s => s.IsReserved && s.ReservedBy == _currentUsername);

            if (mySeat == null)
            {
                MessageBox.Show("Nu ai niciun loc rezervat în acest moment pe care să îl poți elibera.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmare opțională, dar utilă
            MessageBoxResult result = MessageBox.Show(
                $"Ești sigur că vrei să eliberezi locul {mySeat.SeatNumber}?",
                "Confirmare eliberare",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                mySeat.IsReserved = false;
                mySeat.ReservedBy = ""; 
                mySeat.ReservationDate = null;

                try
                {
                    _libraryService.SaveSeats(_seats);

                    LoadSeats();

                    MessageBox.Show($"Locul {mySeat.SeatNumber} a fost eliberat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Eroare la eliberarea locului: {innerError}", "Eroare DB", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}