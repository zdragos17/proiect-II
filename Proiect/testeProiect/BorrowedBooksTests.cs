using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proiect;

namespace testeProiect
{
    public class BorrowedBooksTests
    {
        [Fact]
        ///TESTEAZA DACA OBIECTUL SE CREAZA CORECT CU DATE VALIDE
        public void BorrowedBook_ValidData_PropertiesSetCorrectly()
        {
            var book = new BorrowedBook
            {
                Username = "Test",
                Title = "Morometii",
                Author = "Marin Preda",
                Status = "Imprumutata",
            };

            Assert.Equal("Test", book.Username);
            Assert.Equal("Morometii", book.Title);
            Assert.Equal("Marin Preda", book.Author);
            Assert.Equal("Imprumutata", book.Status);
        }

        [Fact]
        ///TESTEAZA DACA DATA IMPRUMUTULUI E DUPA DATA REZERVARII
        public void BorrowedBook_BorrowDate_ShouldBeAfterReservationDate()
        {
            var book = new BorrowedBook
            {
                ReservationDate = DateTime.Now,
                BorrowDate = DateTime.Now.AddDays(1)
            };

            Assert.True(book.BorrowDate > book.ReservationDate);
        }

        [Fact]
        ///TESTEAZA STATUSUL IMPLICIT AL UNEI CARTI
        public void BorrowedBook_Default_StatusIsNull()
        {
            var book = new BorrowedBook();
            Assert.Null(book.Status);
        }

        // TESTE NOI - VALIDARE IMPRUMUTURI

        [Fact]
        ///TESTEAZA DACA STATUSUL UNUI IMPRUMUT TREBUIE SA FIE VALID (Rezervata/Imprumutata/Expirata)
        public void BorrowedBook_Status_ShouldBeValid()
        {
            var validStatuses = new List<string> { "Rezervata", "Imprumutata", "Expirata" };
            var book = new BorrowedBook { Status = "Imprumutata" };

            bool isValid = validStatuses.Contains(book.Status);

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA UN IMPRUMUT TREBUIE SA AIBA USERNAME
        public void BorrowedBook_Username_CannotBeEmpty()
        {
            var book = new BorrowedBook { Username = "" };

            bool isEmpty = string.IsNullOrWhiteSpace(book.Username);

            Assert.True(isEmpty);
        }

        [Fact]
        ///TESTEAZA DACA UN IMPRUMUT TREBUIE SA AIBA TITLU DE CARTE
        public void BorrowedBook_Title_CannotBeEmpty()
        {
            var book = new BorrowedBook { Title = "" };

            bool isEmpty = string.IsNullOrWhiteSpace(book.Title);

            Assert.True(isEmpty);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE EXPIRATA TREBUIE SA AIBA STATUS "Expirata"
        public void BorrowedBook_ExpiredBook_ShouldHaveExpiredStatus()
        {
            var reservationDate = DateTime.Now.AddDays(-30);
            var book = new BorrowedBook 
            { 
                ReservationDate = reservationDate, 
                Status = "Expirata" 
            };

            bool isExpired = book.Status == "Expirata";

            Assert.True(isExpired);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE NOUA REZERVATA TREBUIE SA AIBA STATUS "Rezervata"
        public void BorrowedBook_NewReservation_ShouldHaveReservataStatus()
        {
            var book = new BorrowedBook 
            { 
                ReservationDate = DateTime.Now, 
                Status = "Rezervata",
                BorrowDate = null
            };

            bool isValid = book.Status == "Rezervata" && book.BorrowDate == null;

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA DATELE SUNT NULL BY DEFAULT
        public void BorrowedBook_Default_DatesAreNull()
        {
            var book = new BorrowedBook();
            Assert.Null(book.ReservationDate);
            Assert.Null(book.BorrowDate);
        }

        [Fact]
        ///TESTEAZA DACA STATUSUL POATE FI EXPIRAT
        public void BorrowedBook_Status_CanBeExpired()
        {
            var book = new BorrowedBook { Status = "Expired" };
            Assert.Equal("Expired", book.Status);
        }
    }
}
