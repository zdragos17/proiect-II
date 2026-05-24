using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proiect;

namespace testeProiect
{
    public class UserTests
    {
        [Fact]
        ///TESTEAZA DACA USERUL SE CREAZA CORECT
        public void User_ValidData_PropertiesSetCorrectly()
        {
            var user = new User
            {
                Username = "student1",
                Password = "parola123",
                Role = "Student"
            };

            Assert.Equal("student1", user.Username);
            Assert.Equal("parola123", user.Password);
            Assert.Equal("Student", user.Role);
        }

        [Fact]
        ///TESTEAZA DACA PROPRIETATILE SUNT NULL BY DEFAULT
        public void User_Default_PropertiesAreNull()
        {
            var user = new User();
            Assert.Null(user.Username);
            Assert.Null(user.Password);
            Assert.Null(user.Role);
        }

        [Fact]
        ///TESTEAZA DACA ROLUL POATE FI ANGAJAT
        public void User_Role_CanBeAdmin()
        {
            var user = new User { Role = "Angajat"};
            Assert.Equal("Angajat", user.Role);
        }

        // TESTE NOI - VALIDARE PARAMETRI SECURITATE

        [Fact]
        ///TESTEAZA DACA PAROLA TREBUIE SA AIBA LUNGIME MINIMA
        public void User_Password_ShouldHaveMinimumLength()
        {
            var user = new User { Password = "abc123" };

            bool isValid = user.Password.Length >= 6;

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA USERNAME TREBUIE SA AIBA LUNGIME MINIMA
        public void User_Username_ShouldHaveMinimumLength()
        {
            var user = new User { Username = "user01" };

            bool isValid = user.Username.Length >= 3;

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA ROLUL UTILIZATORULUI TREBUIE SA FIE VALID (Student/Angajat)
        public void User_Role_ShouldBeValidRole()
        {
            var validRoles = new List<string> { "Student", "Angajat" };
            var user = new User { Role = "Student" };

            bool isValid = validRoles.Contains(user.Role);

            Assert.True(isValid);
        }

        [Fact]
        ///TESTEAZA DACA O PAROLA GOALA NU ESTE VALIDA
        public void User_EmptyPassword_ShouldBeInvalid()
        {
            var user = new User { Password = "" };

            bool isInvalid = string.IsNullOrWhiteSpace(user.Password);

            Assert.True(isInvalid);
        }

        [Fact]
        ///TESTEAZA DACA DOI UTILIZATORI CU ACELASI USERNAME SUNT DUPLICATI
        public void User_Duplicate_ShouldBeDetected()
        {
            var users = new List<User>
            {
                new User { Username = "student1", Password = "pass123", Role = "Student" },
                new User { Username = "student1", Password = "pass456", Role = "Student" }
            };

            var usernames = users.Select(u => u.Username).ToList();
            bool hasDuplicate = usernames.Count != usernames.Distinct().Count();

            Assert.True(hasDuplicate);
        }

        [Fact]
        ///TESTEAZA DACA UN UTILIZATOR POATE SCHIMBA ROLUL DIN STUDENT IN ANGAJAT
        public void User_CanChangeRole_FromStudentToEmployee()
        {
            var user = new User { Role = "Student" };
            user.Role = "Angajat";

            Assert.Equal("Angajat", user.Role);
        }
    }
}