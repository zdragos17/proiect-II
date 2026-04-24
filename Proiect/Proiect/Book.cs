using System;
using System.ComponentModel.DataAnnotations;

namespace Proiect
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }

        public bool IsReserved { get; set; }
        public string ReservedBy { get; set; }

        public DateTime? ReservationDate { get; set; }
        public DateTime? BorrowDate { get; set; }

        public string Status { get; set; }
    }
}