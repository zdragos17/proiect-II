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


    }
}
