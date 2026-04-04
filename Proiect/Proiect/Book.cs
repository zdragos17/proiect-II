using System;

namespace Proiect
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public bool IsReserved { get; set; }
        public string ReservedBy { get; set; }
        public DateTime? ReservationDate { get; set; }
        public string Status { get; set; }
    }
}