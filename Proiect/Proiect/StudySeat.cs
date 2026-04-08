using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect
{
    public class StudySeat
    {
        public int SeatNumber { get; set; }
        public bool IsReserved { get; set; }
        public string ReservedBy { get; set; }
        public DateTime? ReservationDate { get; set; }
    }
}
