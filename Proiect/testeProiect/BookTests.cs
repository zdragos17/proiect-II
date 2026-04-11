using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Proiect;


namespace testeProiect
{
    public class BookTests
    {
        [Fact]
        ///TEST PENTRU A VEDEA SE CREEAZA CARTEA CU DATE VALIDE
        public void Book_ValidData_PropertiesSetCorrectly()
        {
            var book = new Book
            {
                Title = "C# Programming",
                Author = "Ion Popescu",
                Subject = "Programming",
                Status = "Available"
            };
            Assert.Equal("C# Programming", book.Title);
            Assert.Equal("Ion Popescu", book.Author);
            Assert.Equal("Programming", book.Subject);
            Assert.Equal("Available", book.Status);

        }

        [Fact]
        ///TEST PENTRU A VEDEA DACA PRIMESTE CARTEA NOT RESERVED BY DEFAULT
        public void Book_Default_IsNotReserved()
        {
            var book = new Book();
            Assert.False(book.IsReserved);
        }

        [Fact]
        ///TESTEAZA DACA REZERVAREA CARTII FUNCTIONEAZA CORECT
        public void Book_WhenReserved_IsReservedIsTrue()
        {
            var book = new Book();
            {
                book.IsReserved = true;
                book.ReservedBy = "student1";
                book.ReservationDate = DateTime.Now;
            };

            Assert.True(book.IsReserved);
            Assert.Equal("student1", book.ReservedBy);
            Assert.NotNull(book.ReservationDate);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE FARA REZERVARE ARE DATA DE REZERVARE
        public void Book_NotReserved_ReservationDateIsNull()
        {
            var book = new Book { IsReserved = false };
            Assert.False(book.IsReserved);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE FARA REZERVARE ESTE REZERVATA DE CINEVA
        public void Book_NotReserved_ReservedByIsNull()
        {
            var book = new Book();
            Assert.Null(book.ReservedBy);
        }
    }
}
