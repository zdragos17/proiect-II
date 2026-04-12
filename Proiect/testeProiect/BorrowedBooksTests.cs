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
