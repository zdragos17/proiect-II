using Proiect.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Proiect
{
    public partial class AssistantChatWindow : Window
    {
        private string _username;

        public AssistantChatWindow(string username)
        {
            InitializeComponent();
            _username = username;

            AddChatMessage($"Salut, {_username}! Analizez istoricul tău de lectură...", true);

            GenerateRecommendation();
        }

        private void GenerateRecommendation()
        {
            using (var db = new LibraryContext())
            {
                var history = db.BorrowedBooks.Where(b => b.Username == _username).ToList();
                var availableBooks = db.Books.Where(b => b.Status == "Disponibil").ToList();

                string responseMessage = "";

                if (history.Any())
                {
                    string lastReadTitle = history.Last().Title;
                    var lastBookDetails = db.Books.FirstOrDefault(b => b.Title == lastReadTitle);

                    if (lastBookDetails != null)
                    {
                        var domainMatch = availableBooks.FirstOrDefault(b =>
                            b.Subject == lastBookDetails.Subject &&
                            !history.Any(h => h.Title == b.Title));

                        if (domainMatch != null && !string.IsNullOrEmpty(domainMatch.Subject) && domainMatch.Subject != "General")
                        {
                            responseMessage = $"Am observat că îți plac cărțile din domeniul '{domainMatch.Subject}'. Îți recomand cu căldură:\n\n📖 {domainMatch.Title}\n✍️ de {domainMatch.Author}.";
                        }
                        else
                        {
                            var authorMatch = availableBooks.FirstOrDefault(b =>
                                b.Author == lastBookDetails.Author &&
                                !history.Any(h => h.Title == b.Title));

                            if (authorMatch != null && authorMatch.Author != "Autor Necunoscut")
                            {
                                responseMessage = $"Dacă ți-a plăcut cum scrie {authorMatch.Author}, ar trebui să încerci și:\n\n📖 {authorMatch.Title}.";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(responseMessage))
                {
                    if (availableBooks.Any())
                    {
                        Random rnd = new Random();
                        var randomBook = availableBooks[rnd.Next(availableBooks.Count)];
                        responseMessage = $"Ieși din zona de confort! Recomandarea mea surpriză pentru azi este:\n\n📖 {randomBook.Title}\n🧠 Domeniu: {randomBook.Subject}.";
                    }
                    else
                    {
                        responseMessage = "Din păcate, biblioteca este goală momentan. Revino mai târziu!";
                    }
                }

                AddChatMessage(responseMessage, false);
            }
        }

        private void AddChatMessage(string text, bool isSystemWelcome)
        {
            Border bubble = new Border
            {
                CornerRadius = new CornerRadius(15, 15, 15, 0),
                Padding = new Thickness(12),
                Margin = new Thickness(10, 5, 40, 10),
                Background = isSystemWelcome ? new SolidColorBrush(Color.FromRgb(230, 230, 230)) : new SolidColorBrush(Color.FromRgb(168, 25, 46))
            };

            TextBlock messageText = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = isSystemWelcome ? Brushes.Black : Brushes.White,
                FontSize = 13
            };

            bubble.Child = messageText;
            ChatStackPanel.Children.Add(bubble);
        }

        private void GenerateNewRecommendation_Click(object sender, RoutedEventArgs e)
        {
            GenerateRecommendation();
        }
    }
}