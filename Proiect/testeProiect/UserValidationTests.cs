// UserValidationTests.cs
using Xunit;
using Proiect;
using System.Collections.Generic;
using System.Linq;

namespace Proiect.Tests
{
    public class UserValidationTests
    {
        
        [Fact]
        ///TESTEAZA DACA USERNAME-UL DUPLICAT EXISTA IN LISTA
        public void Register_DuplicateUsername_ShouldExistInList()
        {
            var users = new List<User>
            {
                new User { Username = "student1", Password = "parola", Role = "student" }
            };

            bool exists = users.Any(u => u.Username == "student1");

            Assert.True(exists);
        }

        
        [Fact]
        ///TESTEAZA DACA UN USERNAME NOU NU EXISTA IN LISTA
        public void Register_NewUsername_ShouldNotExistInList()
        {
            var users = new List<User>
            {
                new User { Username = "student1", Password = "parola", Role = "student" }
            };

            bool exists = users.Any(u => u.Username == "student2");

            Assert.False(exists);
        }

        
        [Fact]
        ///TESTEAZA DACA USERNAME-UL GOL NU E VALID
        public void Register_EmptyUsername_ShouldBeInvalid()
        {
            string username = "";

            bool isInvalid = string.IsNullOrWhiteSpace(username);

            Assert.True(isInvalid);
        }

        
        [Fact]
        ///TESTEAZA DACA PAROLA GOALA NU E VALIDA
        public void Register_EmptyPassword_ShouldBeInvalid()
        {
            string password = "   ";

            bool isInvalid = string.IsNullOrWhiteSpace(password);

            Assert.True(isInvalid);
        }

        
        [Fact]
        ///TESTEAZA DACA CREDENTIALELE SUNT CORECTE
        public void Login_CorrectCredentials_ShouldFindUser()
        {
            var users = new List<User>
            {
                new User { Username = "student1", Password = "parola123", Role = "student" }
            };

            var found = users.FirstOrDefault(u => u.Username == "student1" && u.Password == "parola123");

            Assert.NotNull(found);
        }

        
        [Fact]
        ///TESTEAZA DACA CREDENTIALELE SUNT GRESITE
        public void Login_WrongPassword_ShouldNotFindUser()
        {
            var users = new List<User>
            {
                new User { Username = "student1", Password = "parola123", Role = "student" }
            };

            var found = users.FirstOrDefault(u => u.Username == "student1" && u.Password == "gresit");

            Assert.Null(found);
        }

        
        [Fact]
        ///TESTEAZA DACA ROLUL DE STUDENT E CORECT
        public void Login_StudentRole_ShouldBeStudent()
        {
            var user = new User { Username = "student1", Password = "parola", Role = "student" };

            Assert.Equal("student", user.Role);
        }
    }
}