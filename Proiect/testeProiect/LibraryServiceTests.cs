using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Proiect;
using Proiect.Services;
using Proiect.Data;

namespace testeProiect
{
    public class LibraryServiceTests
    {
        private void CleanDatabase()
        {
            using var db = new LibraryContext();
            db.Books.RemoveRange(db.Books);
            db.Users.RemoveRange(db.Users);
            db.BorrowedBooks.RemoveRange(db.BorrowedBooks);
            db.StudySeat.RemoveRange(db.StudySeat);
            db.SaveChanges();
        }

        // BOOKS TESTS 

        [Fact]
        //TESTEAZA DACA SE ADAUGA O CARTE CU STATUS NULL, SA FIE SETAT LA "Disponibil"
        public void SaveBooks_WithNullStatus_ShouldSetDefaultStatus()
        {
            CleanDatabase();
            var service = new LibraryService();
            var book = new List<Book>
            {
                new Book { Title = "C# Guide", Author = "Test Author", Subject = "Programming", Status = null }
            };

            service.SaveBooks(book);
            var books = service.GetAllBooks();

            Assert.Single(books);
            Assert.Equal("Disponibil", books[0].Status);
        }

        [Fact]
        //TESTEAZA DACA SE ADAUGA O CARTE NOUA, SA FIE ADAUGATA IN BAZA DE DATE
        public void SaveBooks_WithNewBook_ShouldAdd()
        {
            CleanDatabase();
            var service = new LibraryService();
            var book = new List<Book>
            {
                new Book { Title = "Java Basics", Author = "John Doe", Subject = "Programming", Status = "Disponibil" }
            };

            service.SaveBooks(book);
            var books = service.GetAllBooks();

            Assert.True(books.Any(b => b.Title == "Java Basics"));
        }

        [Fact]
        //TESTEAZA DACA SE ADAUGA O CARTE CU TITLU EXISTENT, SA FIE ACTUALIZATA IN BAZA DE DATE
        //asigura ca se poate edita o carte existenta fara sa creeze o intrare noua in baza de date, ca sa nu existe duplicate
        public void SaveBooks_WithExistingBook_ShouldUpdate()
        {
            CleanDatabase();
            var service = new LibraryService();
            var book = new List<Book>
            {
                new Book { Title = "Python Guide", Author = "Old Author", Subject = "Programming", Status = "Disponibil" }
            };
            service.SaveBooks(book);

            var updatedBook = new List<Book>
            {
                new Book { Title = "Python Guide", Author = "New Author", Subject = "Programming", Status = "Disponibil" }
            };

            service.SaveBooks(updatedBook);
            var books = service.GetAllBooks();

            Assert.Single(books);
            Assert.Equal("New Author", books[0].Author);
        }

        // USERS TESTS 

        [Fact]
        //TESTEAZA DACA SE ADAUGA UN USER NOU IN BAZA DE DATE
        public void SaveUsers_WithNewUser_ShouldAdd()
        {
            CleanDatabase();
            var service = new LibraryService();
            var user = new List<User>
            {
                new User { Username = "student1", Password = "pass123", Role = "Student" }
            };

            service.SaveUsers(user);
            var users = service.GetAllUsers();

            Assert.True(users.Any(u => u.Username == "student1"));
        }

        [Fact]
        //TESTEAZA DACA SE ADAUGA UN USER CU USERNAME EXISTENT, SA FIE ACTUALIZAT IN BAZA DE DATE
        //asigura faptul ca se pot modifica parolele sau rolurile unui utilizator
        public void SaveUsers_WithExistingUser_ShouldUpdate()
        {
            CleanDatabase();
            var service = new LibraryService();
            var user = new List<User>
            {
                new User { Username = "john", Password = "oldpass", Role = "Student" }
            };
            service.SaveUsers(user);

            var updatedUser = new List<User>
            {
                new User { Username = "john", Password = "newpass", Role = "Angajat" }
            };

            service.SaveUsers(updatedUser);
            var users = service.GetAllUsers();

            var found = users.FirstOrDefault(u => u.Username == "john");
            Assert.NotNull(found);
            Assert.Equal("newpass", found.Password);
            Assert.Equal("Angajat", found.Role);
        }

        //  BORROWED BOOKS TESTS 

        [Fact]
        //TESTEAZA DACA SE ADAUGA UN RECORD NOI DE IMPRUMUT IN BAZA DE DATE
        public void SaveBorrowedBooks_WithNewRecord_ShouldAdd()
        {
            CleanDatabase();
            var service = new LibraryService();
            var borrowed = new List<BorrowedBook>
            {
                new BorrowedBook 
                { 
                    Username = "student1", 
                    Title = "Database Design", 
                    Author = "Expert",
                    Status = "Imprumutata",
                    ReservationDate = DateTime.Now
                }
            };

            service.SaveBorrowedBooks(borrowed);
            var records = service.GetBorrowedBooks();

            Assert.True(records.Any(b => b.Username == "student1" && b.Title == "Database Design"));
        }

        [Fact]
        //TESTEAZA DACA SE ACTUALIZEAZA UN RECORD EXISTENT DE IMPRUMUT (ex: din Rezervata in Imprumutata)
        public void SaveBorrowedBooks_WithExistingRecord_ShouldUpdate()
        {
            CleanDatabase();
            var service = new LibraryService();
            var date = DateTime.Now;
            var borrowed = new List<BorrowedBook>
            {
                new BorrowedBook 
                { 
                    Username = "student1", 
                    Title = "Book1",
                    Author = "Author1",
                    Status = "Rezervata",
                    ReservationDate = date
                }
            };
            service.SaveBorrowedBooks(borrowed);

            var updated = new List<BorrowedBook>
            {
                new BorrowedBook 
                { 
                    Username = "student1", 
                    Title = "Book1",
                    Author = "Author1",
                    Status = "Imprumutata",
                    ReservationDate = date,
                    BorrowDate = DateTime.Now
                }
            };

            service.SaveBorrowedBooks(updated);
            var records = service.GetBorrowedBooks();

            var found = records.FirstOrDefault(b => b.Username == "student1" && b.Title == "Book1");
            Assert.NotNull(found);
            Assert.Equal("Imprumutata", found.Status);
        }

        // STUDY SEATS TESTS 

        [Fact]
        //TESTEAZA DACA GetAllSeats() INITIALIZEAZA 20 LOCURI CAND BAZA DE DATE E GOALA
        public void GetAllSeats_WhenEmpty_ShouldInitialize20Seats()
        {
            CleanDatabase();
            var service = new LibraryService();

            var seats = service.GetAllSeats();

            Assert.Equal(20, seats.Count);
            Assert.True(seats.All(s => !s.IsReserved));
        }

       

        [Fact]
        //TESTEAZA DACA RemoveExpiredSeatReservations() STERGE CORECT REZERVARILE EXPIRATE (>4 ore)
        public void RemoveExpiredSeatReservations_ShouldRemoveExpired()
        {
            CleanDatabase();
            var service = new LibraryService();
            var expiredDate = DateTime.Now.AddHours(-5);
            var seats = new List<StudySeat>
            {
                new StudySeat { SeatNumber = 3, IsReserved = true, ReservedBy = "student1", ReservationDate = expiredDate }
            };
            service.SaveSeats(seats);

            service.RemoveExpiredSeatReservations();
            var retrieved = service.GetAllSeats();

            var seat3 = retrieved.FirstOrDefault(s => s.SeatNumber == 3);
            Assert.NotNull(seat3);
            Assert.False(seat3.IsReserved);
        }
    }
}
