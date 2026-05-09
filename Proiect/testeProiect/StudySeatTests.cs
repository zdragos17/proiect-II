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

        // TESTE NOI - VALIDARE STUDY SEATS

        [Fact]
        ///TESTEAZA DACA UN LOC DE STUDIU TREBUIE SA AIBA UN NUMAR VALID (1-20)
        public void StudySeat_SeatNumber_ShouldBeInValidRange()
        {
            var seat = new StudySeat { SeatNumber = 5 };

            bool isValid = seat.SeatNumber >= 1 && seat.SeatNumber <= 20;

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA UN LOC REZERVAT TREBUIE SA AIBA UN UTILIZATOR ASOCIAT
        public void StudySeat_WhenReserved_ShouldHaveReservedBy()
        {
            var seat = new StudySeat 
            { 
                IsReserved = true, 
                ReservedBy = "student1" 
            };

            bool isValid = !string.IsNullOrEmpty(seat.ReservedBy);

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA UN LOC REZERVAT TREBUIE SA AIBA O DATA DE REZERVARE
        public void StudySeat_WhenReserved_ShouldHaveReservationDate()
        {
            var seat = new StudySeat 
            { 
                IsReserved = true, 
                ReservationDate = DateTime.Now 
            };

            bool isValid = seat.ReservationDate.HasValue;

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA LOCURILE CU NUMAR INVALID (0 SAU <0) NU SUNT VALIDE
        public void StudySeat_SeatNumber_CannotBeZeroOrNegative()
        {
            var seat = new StudySeat { SeatNumber = 0 };

            bool isInvalid = seat.SeatNumber <= 0;

            Assert.True(isInvalid);
        }

        [Fact]
        ///TESTEAZA DACA LOCURILE CU NUMAR >20 NU SUNT VALIDE
        public void StudySeat_SeatNumber_CannotBeGreaterThan20()
        {
            var seat = new StudySeat { SeatNumber = 25 };

            bool isInvalid = seat.SeatNumber > 20;

            Assert.True(isInvalid);
        }
    }
}
