

public abstract class Book
{
    private string Title;
    private bool isBorrowed;
    
    public Book()
}

public class Library
{
    private readonly List<Book> books = new();
    private readonly List<User> users = new();
    
    public void AddBook(Book book) => books.Add(book);
    public void AddUser(User user) => users.Add(user);
    
    public IEnumerable<Book> ListBooks() => books;
    public IEnumerable<User> ListUsers() => users;

    public bool BorrowBook(string title, string userName)
    {
        var book = books.FirstOrDefault(b => b.Title == title);
        var user = users.FirstOrDefault(u.Name )
    }
}