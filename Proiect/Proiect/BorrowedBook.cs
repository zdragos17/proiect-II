using System;

namespace Proiect
{
    public class BorrowedBook
    {
        public string Username { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Status { get; set; } // Rezervata / Imprumutata / Expirata
        public DateTime? ReservationDate { get; set; }
        public DateTime? BorrowDate { get; set; }
    }
}