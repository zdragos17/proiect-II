using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proiect;

namespace testeProiect
{
    public class StudySeatTests
    {
        [Fact]
        ///TESTEAZA DACA LOCUL SE CREAZA CORECT
        public void StudySeats_ValidData_PropertiesSetCorrectly()
        {
            var seat = new StudySeat
            {
                SeatNumber = 5,
                IsReserved = true,
                ReservedBy = "Student",
                ReservationDate = DateTime.Now
            };

            Assert.Equal(5, seat.SeatNumber);
            Assert.True(seat.IsReserved);
            Assert.Equal("Student", seat.ReservedBy);
            Assert.NotNull(seat.ReservationDate);
        }

        [Fact]
        ///TESTEAZA DACA LOCUL E REZERVAT BY DEFAULT
        public void StudySeat_Default_IsNotReserved()
        {
            var seat = new StudySeat();
            Assert.False(seat.IsReserved);
        }

        [Fact]
        ///TESTEAZA DACA DATA REZERVARII E NULL BY DEFAULT
        public void StudySeat_Default_ReservationDateIsNull()
        {
            var seat = new StudySeat();
            Assert.Null(seat.ReservationDate);
        }
    }
}
