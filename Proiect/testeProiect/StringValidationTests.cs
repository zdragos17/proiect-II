using System;
using Proiect;
using Xunit;

namespace testeProiect
{
    public class StringValidationTests
    {
        [Fact]
        ///TESTEAZA DACA TITLU CU CARACTERE SPECIALE (C#, ™) SE ACCEPTA
        public void Book_Title_WithSpecialCharacters()
        {
            var book = new Book { Title = "C# Programming™" };
            Assert.NotEmpty(book.Title);
            Assert.Contains("C#", book.Title);
        }

        [Fact]
        ///TESTEAZA DACA USERNAME CU UNDERSCORE SI PUNCTE SE ACCEPTA
        public void User_Username_WithUnderscoreAndDots()
        {
            var user = new User { Username = "user_2024.test" };
            Assert.Contains("_", user.Username);
        }

        [Fact]
        ///TESTEAZA DACA PAROLA CU CARACTERE SPECIALE (@, !) SE ACCEPTA
        public void User_Password_WithSpecialCharacters()
        {
            var user = new User { Password = "P@ss123!" };
            Assert.Contains("@", user.Password);
        }

        [Fact]
        ///TESTEAZA DACA SE POT STOCA MULTI AUTORI SEPARATI PRIN VIRGULA
        public void Book_Author_MultipleAuthors()
        {
            var book = new Book { Author = "Author1, Author2, Author3" };
            var count = book.Author.Split(", ").Length;
            Assert.Equal(3, count);
        }

        [Fact]
        ///TESTEAZA DACA SE COMBINA CORECT FIRST_NAME SI LAST_NAME PENTRU NUME COMPLET
        public void User_FullName_Combined()
        {
            var user = new User { FirstName = "Ion", LastName = "Popescu" };
            var fullName = $"{user.FirstName} {user.LastName}";
            Assert.Equal("Ion Popescu", fullName);
        }

        [Fact]
        ///TESTEAZA DACA SUBJECT CU CATEGORII MULTIPLE SE STOCHEAZA CORECT
        public void Book_Subject_MultipleCategories()
        {
            var book = new Book { Subject = "Fiction, Adventure, Mystery" };
            var categories = book.Subject.Split(", ");
            Assert.True(categories.Length >= 2);
        }

        [Fact]
        ///TESTEAZA DACA FACULTY SE STOCHEAZA CU NUMERE SI CARACTERE SPECIALE
        public void User_Faculty_WithNumbers()
        {
            var user = new User { Faculty = "Facultatea de Informatica - 2024" };
            Assert.NotEmpty(user.Faculty);
            Assert.Contains("2024", user.Faculty);
        }
    }
}
