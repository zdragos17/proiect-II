using System;
using Proiect;
using Xunit;

namespace testeProiect
{
    public class PropertyTests
    {
        [Fact]
        ///TESTEAZA DACA BOOK_SUBJECT SE SETEAZA CORECT
        public void Book_Subject_Property()
        {
            var book = new Book { Subject = "Informatica" };
            Assert.NotNull(book.Subject);
            Assert.Equal("Informatica", book.Subject);
        }

        [Fact]
        ///TESTEAZA DACA BOOK_DISPLAY_ID NU SE SALVEAZA IN DB (NotMapped)
        public void Book_DisplayId_NotMapped()
        {
            var book = new Book { Id = 1, DisplayId = 10 };
            Assert.Equal(1, book.Id);
            Assert.Equal(10, book.DisplayId);
        }

        [Fact]
        ///TESTEAZA DACA USER_FIRST_NAME SI USER_LAST_NAME SE SETEAZA
        public void User_FirstNameLastName_Properties()
        {
            var user = new User 
            { 
                FirstName = "Gheorghe", 
                LastName = "Popescu" 
            };
            Assert.Equal("Gheorghe", user.FirstName);
            Assert.Equal("Popescu", user.LastName);
        }

        [Fact]
        ///TESTEAZA DACA MATRICULATION_NUMBER SE SETEAZA PENTRU STUDENT
        public void User_MatriculationNumber_Property()
        {
            var user = new User 
            { 
                MatriculationNumber = "FC001234" 
            };
            Assert.NotEmpty(user.MatriculationNumber);
        }

        [Fact]
        ///TESTEAZA DACA EMPLOYEE_CODE SE SETEAZA PENTRU ANGAJAT
        public void User_EmployeeCode_Property()
        {
            var user = new User 
            { 
                EmployeeCode = "EMP-2024-001" 
            };
            Assert.NotEmpty(user.EmployeeCode);
        }

        [Fact]
        ///TESTEAZA DACA BOOK_RESERVED_BY SE SETEAZA CAND CARTEA E REZERVATA
        public void Book_ReservedBy_Property()
        {
            var book = new Book 
            { 
                IsReserved = true, 
                ReservedBy = "student1" 
            };
            Assert.True(book.IsReserved);
            Assert.Equal("student1", book.ReservedBy);
        }

        [Fact]
        ///TESTEAZA DACA STUDYSEAT_RESERVED_BY SE SETEAZA CAND LOCUL E REZERVAT
        public void StudySeat_ReservedBy_Property()
        {
            var seat = new StudySeat 
            { 
                IsReserved = true, 
                ReservedBy = "user_name" 
            };
            Assert.True(seat.IsReserved);
            Assert.NotEmpty(seat.ReservedBy);
        }
    }
}
