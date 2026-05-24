using System;
using Proiect;
using Xunit;

namespace testeProiect
{
    public class DateValidationTests
    {
        [Fact]
        ///TESTEAZA CALCULUL CORECT AL DIFERENTEI DE ZILE INTRE DOUA DATE
        public void BorrowedBook_DaysSpan_Calculation()
        {
            var book = new BorrowedBook
            {
                ReservationDate = new DateTime(2024, 1, 1),
                BorrowDate = new DateTime(2024, 1, 5)
            };
            var days = (book.BorrowDate.Value - book.ReservationDate.Value).TotalDays;
            Assert.Equal(4, days);
        }

        [Fact]
        ///TESTEAZA DACA O REZERVARE FACUTA IN ULTIMELE 24 ORE SE CONSIDERA RECENTA
        public void StudySeat_ReservationDate_Recent()
        {
            var seat = new StudySeat { ReservationDate = DateTime.Now.AddHours(-2) };
            var recent = (DateTime.Now - seat.ReservationDate.Value).TotalHours < 24;
            Assert.True(recent);
        }

        [Fact]
        ///TESTEAZA DACA O DATA VIITOARE SE POATE SETA LA RESERVATION_DATE
        public void StudySeat_ReservationDate_Future()
        {
            var futureDate = DateTime.Now.AddDays(7);
            var seat = new StudySeat { ReservationDate = futureDate };
            Assert.True(seat.ReservationDate > DateTime.Now);
        }

        [Fact]
        ///TESTEAZA DACA SE EXTRAGE CORECT LUNA DINTR-O DATA
        public void BorrowedBook_ExtractMonth_FromDate()
        {
            var book = new BorrowedBook 
            { 
                ReservationDate = new DateTime(2024, 6, 15) 
            };
            Assert.Equal(6, book.ReservationDate.Value.Month);
        }
    }
}
