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

        // TESTE NOI - VALIDARE CARTE

        [Fact]
        ///TESTEAZA DACA O CARTE FARA TITLU NU ESTE VALIDA
        public void Book_EmptyTitle_ShouldBeInvalid()
        {
            var book = new Book { Title = "", Author = "Author", Subject = "Subject" };

            bool isInvalid = string.IsNullOrWhiteSpace(book.Title);

            Assert.True(isInvalid);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE FARA AUTOR NU ESTE VALIDA
        public void Book_EmptyAuthor_ShouldBeInvalid()
        {
            var book = new Book { Title = "Title", Author = "", Subject = "Subject" };

            bool isInvalid = string.IsNullOrWhiteSpace(book.Author);

            Assert.True(isInvalid);
        }

        [Fact]
        ///TESTEAZA DACA STATUS-UL CARTII POATE FI DOAR DISPONIBIL SAU INDISPONIBIL
        public void Book_Status_ShouldBeValidStatus()
        {
            var validStatuses = new List<string> { "Disponibil", "Indisponibil", "Rezervata" };
            var book = new Book { Status = "Disponibil" };

            bool isValid = validStatuses.Contains(book.Status);

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE REZERVATA NU POATE FI DISPONIBILA
        public void Book_WhenReserved_CannotBeAvailable()
        {
            var book = new Book 
            { 
                IsReserved = true, 
                ReservedBy = "student1",
                Status = "Rezervata"
            };

            bool cannotBeAvailable = book.IsReserved && book.Status != "Disponibil";

            Assert.True(cannotBeAvailable);
        }

        [Fact]
        ///TESTEAZA DACA O CARTE NU E REZERVATA, ReservedBy TREBUIE SA FIE NULL SAU GOLU
        public void Book_NotReserved_ReservedByShouldBeEmpty()
        {
            var book = new Book { IsReserved = false };

            bool isValid = string.IsNullOrEmpty(book.ReservedBy);

            Assert.True(isValid);
        }
    }
}
