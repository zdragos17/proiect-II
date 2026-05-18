using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Proiect.Data; // Asigură-te că ai namespace-ul corect

namespace Proiect
{
    public partial class AssistantChatWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string currentUsername;

        // MEMORIA GLOBALĂ (STATICĂ) - Rămâne vie cât timp aplicația e deschisă
        private static ObservableCollection<ChatMessage> _sharedMessages;
        private static string _lastUsername;
        private static int _sharedRecommendationCount = 0;
        private static Random _rnd = new Random();

        public AssistantChatWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;

            // Dacă e prima dată când deschidem aplicația SAU dacă s-a logat alt utilizator
            if (_sharedMessages == null || _lastUsername != username)
            {
                _sharedMessages = new ObservableCollection<ChatMessage>();
                _lastUsername = username;
                _sharedRecommendationCount = 0;

                // Adăugăm mesajul de bun venit doar o singură dată
                _sharedMessages.Add(new ChatMessage
                {
                    Message = $"Salut {username}! 👋\n\nSunt asistentul bibliotecii tale. Îmi poți cere:\n" +
                              "📚 Să caut cărți (ex: 'ai cărți de informatică?' sau 'dar de filozofie?')\n" +
                              "📖 Detalii despre contul tău (ex: 'ce cărți am?')\n" +
                              "💡 Recomandări (ex: 'recomandă-mi o carte' sau 'alta carte')\n" +
                              "📊 Statistici (ex: 'câte cărți avem în total?')\n\n" +
                              "Cum te pot ajuta azi?",
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }

            // Conectăm interfața la memoria globală
            ChatMessages.ItemsSource = _sharedMessages;

            // Dăm scroll jos în caz că există deja o conversație lungă
            Dispatcher.InvokeAsync(ScrollToBottom, System.Windows.Threading.DispatcherPriority.Background);
            InputTextBox.Focus();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void SendMessage()
        {
            string userMessage = InputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            AddUserMessage(userMessage);
            InputTextBox.Clear();

            try
            {
                string response = ProcessUserMessage(userMessage);
                AddAssistantMessage(response);
            }
            catch (Exception)
            {
                AddAssistantMessage("❌ Scuze, am întâmpinat o problemă la generarea răspunsului. Te rog încearcă din nou.");
            }
        }

        private string ProcessUserMessage(string userMessage)
        {
            string lower = userMessage.ToLower();

            // 1. Salutări
            if (lower.Contains("salut") || lower.Contains("bună") || lower.Contains("buna") || lower.Contains("hello"))
            {
                _sharedRecommendationCount = 0;
                return $"👋 Salut, {currentUsername}! Cu ce te pot ajuta astăzi?";
            }

            // 2. Recomandări și conversație continuă ("alta carte")
            if (lower.Contains("recomand") || lower.Contains("sugerează") || lower.Contains("sugereaza") ||
                lower.Contains("ce să citesc") || lower.Contains("ce sa citesc") || lower.Contains("sugestie") ||
                lower.Contains("alta carte") || lower.Contains("altă carte") || lower.Contains("altceva") ||
                lower.Contains("mai vreau") || lower.Contains("mai dă-mi") || lower.Contains("mai da-mi") ||
                lower.Contains("alta") || lower.Contains("altul"))
            {
                _sharedRecommendationCount++;

                var allBooks = _libraryService.GetAllBooks();
                var availableBooks = allBooks.Where(b => b.Status != null && b.Status.Contains("Disponibil")).ToList();
                var borrowedBooks = _libraryService.GetBorrowedBooks();
                var userHistory = borrowedBooks.Where(b => b.Username == currentUsername).ToList();

                if (availableBooks.Count == 0)
                    return "❌ Din păcate, biblioteca este complet goală (sau toate cărțile sunt rezervate). Revino mai târziu!";

                // Intrăm pe "Surpriză" la a 3-a și a 4-a încercare
                if (_sharedRecommendationCount == 3 || _sharedRecommendationCount == 4)
                {
                    bool isSecondSurprise = (_sharedRecommendationCount == 4);

                    if (isSecondSurprise)
                    {
                        _sharedRecommendationCount = 0; // Resetăm abia DUPĂ a doua surpriză
                    }

                    var readTitles = userHistory.Select(h => h.Title).ToList();
                    var userReadDomains = allBooks
                        .Where(b => readTitles.Contains(b.Title) && !string.IsNullOrEmpty(b.Subject))
                        .Select(b => b.Subject)
                        .Distinct()
                        .ToList();

                    var surpriseBooks = availableBooks.Where(b => !userReadDomains.Contains(b.Subject)).ToList();

                    Book randomBook;
                    if (surpriseBooks.Any())
                        randomBook = surpriseBooks[_rnd.Next(surpriseBooks.Count)];
                    else
                        randomBook = availableBooks[_rnd.Next(availableBooks.Count)];

                    string introMessage = isSecondSurprise
                        ? "🎲 Rămânem în afara zonei de confort pentru încă o recomandare! Încearcă ceva din domeniul"
                        : "🎲 Ai cerut destule recomandări similare, așa că e timpul să ieși din zona de confort!\n\nÎncearcă ceva complet diferit din domeniul";

                    return $"{introMessage} '{randomBook.Subject}':\n\n📖 {randomBook.Title}\n✍️ de {randomBook.Author}";
                }
                else
                {
                    // Recomandare Normală
                    if (userHistory.Any())
                    {
                        var lastBorrowed = userHistory.OrderByDescending(b => b.ReservationDate).FirstOrDefault();
                        var lastBookDetails = allBooks.FirstOrDefault(b => b.Title == lastBorrowed?.Title);

                        if (lastBookDetails != null)
                        {
                            var readTitles = userHistory.Select(h => h.Title).ToList();

                            var similarBooks = availableBooks
                                .Where(b => b.Subject == lastBookDetails.Subject && !readTitles.Contains(b.Title))
                                .ToList();

                            if (similarBooks.Any())
                            {
                                var rec = similarBooks[_rnd.Next(similarBooks.Count)];
                                return $"📚 Știu că ți-a plăcut '{lastBookDetails.Title}'. Din același domeniu ({rec.Subject}), îți recomand cu căldură:\n\n📖 {rec.Title}\n✍️ de {rec.Author}";
                            }

                            var authorBooks = availableBooks
                                .Where(b => b.Author == lastBookDetails.Author && !readTitles.Contains(b.Title))
                                .ToList();

                            if (authorBooks.Any())
                            {
                                var rec = authorBooks[_rnd.Next(authorBooks.Count)];
                                return $"✍️ Văd că apreciezi stilul lui {lastBookDetails.Author}. Ce zici de o altă operă de-a sa?\n\n📖 {rec.Title}";
                            }
                        }
                    }

                    var fallbackBook = availableBooks[_rnd.Next(availableBooks.Count)];
                    return $"💡 Iată o carte excelentă pe care merită să o citești:\n\n📖 {fallbackBook.Title}\n✍️ de {fallbackBook.Author}";
                }
            }

            _sharedRecommendationCount = 0;

            // 3. Cărți disponibile
            if (lower.Contains("disponibil") || lower.Contains("libere") || lower.Contains("ce cărți ai") || lower.Contains("ce carti ai") || lower.Contains("carti disponibile"))
            {
                var allBooks = _libraryService.GetAllBooks();
                var availableBooks = allBooks.Where(b => b.Status != null && b.Status.Contains("Disponibil")).ToList();
                if (availableBooks.Count == 0) return "❌ Din păcate, nu avem cărți disponibile în acest moment.";

                StringBuilder sb = new StringBuilder("📚 Iată câteva cărți disponibile acum:\n\n");
                foreach (var book in availableBooks.Take(5))
                {
                    sb.AppendLine($"📖 {book.Title}\n   ✍️ {book.Author}");
                }
                if (availableBooks.Count > 5)
                    sb.AppendLine($"\n... și încă {availableBooks.Count - 5} cărți în catalog.");

                return sb.ToString().TrimEnd();
            }

            // 4. Cărțile mele
            if (lower.Contains("cărțile mele") || lower.Contains("cartile mele") || lower.Contains("am împrumutat") ||
                lower.Contains("am rezervat") || lower.Contains("cartile") || lower.Contains("cărțile") ||
                lower.Contains("ce carti am") || lower.Contains("ce cărți am") || lower.Contains("istoric") ||
                lower.Contains("istoricul"))
            {
                var borrowedBooks = _libraryService.GetBorrowedBooks();
                var userBooks = borrowedBooks.Where(b => b.Username == currentUsername && b.Status != "Expirata").ToList();

                if (userBooks.Count == 0)
                    return "✅ Nu ai nicio carte împrumutată sau rezervată în acest moment. E timpul pentru o lectură nouă!";

                StringBuilder sb = new StringBuilder("📖 Aici sunt cărțile tale active:\n\n");
                foreach (var book in userBooks)
                {
                    string icon = (book.Status != null && book.Status.Contains("Rezervat")) ? "🔔" : "📕";
                    sb.AppendLine($"{icon} {book.Title} ({book.Status})");
                }
                return sb.ToString().TrimEnd();
            }

            // 5. Căutare generală
            var allBooksDomain = _libraryService.GetAllBooks();

            if (lower.Contains("informatic") || lower.Contains("programare") || lower.Contains("c#") || lower.Contains("algoritmi"))
                return GenerateDomainResponse("informatic", allBooksDomain);

            if (lower.Contains("matematic"))
                return GenerateDomainResponse("matematic", allBooksDomain);

            if (lower.Contains("literatur") || lower.Contains("roman"))
                return GenerateDomainResponse("literatur", allBooksDomain);

            if (lower.Contains("filosof") || lower.Contains("filozof"))
                return GenerateDomainResponse("filosof", allBooksDomain);

            if (lower.Contains("istori"))
                return GenerateDomainResponse("istori", allBooksDomain);

            var existingSubjects = allBooksDomain
                .Where(b => !string.IsNullOrWhiteSpace(b.Subject))
                .Select(b => b.Subject.ToLower().Trim())
                .Distinct();

            foreach (var subject in existingSubjects)
            {
                if (subject.Length > 3 && lower.Contains(subject))
                {
                    return GenerateDomainResponse(subject, allBooksDomain);
                }
            }

            // 6. Statistici
            if (lower.Contains("statistic") || lower.Contains("câte") || lower.Contains("cate") || lower.Contains("total"))
            {
                var allBooksStats = _libraryService.GetAllBooks();
                int available = allBooksStats.Count(b => b.Status != null && b.Status.Contains("Disponibil"));
                int borrowed = allBooksStats.Count(b => b.Status != null && b.Status.Contains("Imprumutat"));
                int reserved = allBooksStats.Count(b => b.Status != null && b.Status.Contains("Rezervat"));

                return $"📊 Pe scurt, situația bibliotecii arată cam așa:\n\n" +
                       $"📦 Cărți înregistrate: {allBooksStats.Count}\n" +
                       $"✅ Gata de citit: {available}\n" +
                       $"📕 Acasă la studenți: {borrowed}\n" +
                       $"🔔 Rezervate recent: {reserved}";
            }

            return "🤔 Nu sunt sigur că am înțeles perfect.\n\n" +
                   "Poți să mă întrebi lucruri precum:\n" +
                   "• 'Ai ceva cărți de matematică/istorie/filozofie?'\n" +
                   "• 'Ce cărți mai am rezervate?'\n" +
                   "• 'Recomandă-mi o carte' sau 'Alta carte'\n" +
                   "• 'Câte cărți sunt disponibile?'\n\n" +
                   "Sau spune-mi altfel ce cauți!";
        }

        private string GenerateDomainResponse(string domainKeyword, System.Collections.Generic.List<Book> allBooks)
        {
            var domainBooks = allBooks.Where(b => b.Subject != null && b.Subject.ToLower().Contains(domainKeyword)).ToList();

            if (domainBooks.Count == 0)
                return $"❌ Nu am găsit nicio carte care să aibă legătură cu acest domeniu.";

            string realDomainName = domainBooks.First().Subject;

            StringBuilder sb = new StringBuilder($"📚 Uite ce am găsit pentru domeniul '{realDomainName}':\n\n");
            foreach (var book in domainBooks.Take(5))
            {
                string avail = (book.Status != null && book.Status.Contains("Disponibil")) ? "✅" : "❌";
                sb.AppendLine($"{avail} {book.Title} - {book.Author}");
            }
            return sb.ToString().TrimEnd();
        }

        private void AddUserMessage(string message)
        {
            _sharedMessages.Add(new ChatMessage { Message = message, IsUser = true, Timestamp = DateTime.Now });
            Dispatcher.InvokeAsync(ScrollToBottom, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void AddAssistantMessage(string message)
        {
            _sharedMessages.Add(new ChatMessage { Message = message, IsUser = false, Timestamp = DateTime.Now });
            Dispatcher.InvokeAsync(ScrollToBottom, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ScrollToBottom()
        {
            var scrollViewer = GetScrollViewer();
            scrollViewer?.ScrollToEnd();
        }

        private ScrollViewer GetScrollViewer()
        {
            return FindVisualChild<ScrollViewer>(ChatMessages);
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T variable) return variable;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
    }

    public class ChatMessage
    {
        public string Message { get; set; }
        public bool IsUser { get; set; }
        public bool IsAssistant => !IsUser;
        public DateTime Timestamp { get; set; }
    }
}