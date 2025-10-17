using System;
using System.Collections.Generic;
using System.Linq;

//entitati
record Book
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Author { get; init; }
    public int Year { get; init; }
}
//exceptii custom
class BookNotFoundException : Exception
{
    public BookNotFoundException(int id)
        : base($"Cartea cu id {id} nu a fost gasita.") { }
}

class InvalidSortPropertyException : Exception
{
    public InvalidSortPropertyException(string property)
        : base($"Proprietatea '{property}' nu este permisa!") { }
}

class InvalidQueryParameterException : Exception
{
    public InvalidQueryParameterException(string parameter, string reason)
        : base($"Parametrul de interogare '{parameter}' este invalid: {reason}") { }
}

//middleware    

static class Middleware
{
    public static void Handle(Action action)
    {
        try
        {
            action();
        }
        catch (BookNotFoundException ex)
        {
            Console.WriteLine($"[eroare]: {ex.Message}");
        }
        catch (InvalidSortPropertyException ex)
        {
            Console.WriteLine($"[eroare Sortare]: {ex.Message}");
        }
        catch (InvalidQueryParameterException ex)
        {
            Console.WriteLine($"[eroare Query]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[eroare necunoscuta]: {ex.Message}");
        }
        Console.WriteLine();
    }
}

//cqrs
//comenzi
interface ICommand { }

record CreateBookCommand(Book Book) : ICommand;
record UpdateBookCommand(int Id, string Title, string Author, int Year) : ICommand;
record DeleteBookCommand(int Id) : ICommand;

//interogari
interface IQuery<TResult> { }

record GetAllBooksQuery(string FilterAuthor = null, string SortBy = null) : IQuery<List<Book>>;
record GetBookByIdQuery(int Id) : IQuery<Book>;

// repository books cu 3 carti initializate
class BookRepository
{
    private readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Harry Potter", Author = "Marius Andrei", Year = 2008 },
        new Book { Id = 2, Title = "Carte de mancare", Author = "Marian Andrei", Year = 1999 },
        new Book { Id = 3, Title = "C++", Author = "Daniel Alex", Year = 1990 }
    };

    public List<Book> Books => _books;
}
// handlere
class CommandHandler
{
    private readonly BookRepository _repo;

    public CommandHandler(BookRepository repo)
    {
        _repo = repo;
    }

    public void Handle(ICommand command)
    {
        switch (command)
        {
            case CreateBookCommand c:
                _repo.Books.Add(c.Book);
                Console.WriteLine($"Cartea '{c.Book.Title}' a fost adaugata!");
                break;

            case UpdateBookCommand u:
                var bookToUpdate = _repo.Books.FirstOrDefault(b => b.Id == u.Id);
                if (bookToUpdate == null)
                    throw new BookNotFoundException(u.Id);

                _repo.Books.Remove(bookToUpdate);
                _repo.Books.Add(new Book { Id = u.Id, Title = u.Title, Author = u.Author, Year = u.Year });
                Console.WriteLine($"cartea cu id {u.Id} a fost actualizata!");
                break;

            case DeleteBookCommand d:
                var bookToDelete = _repo.Books.FirstOrDefault(b => b.Id == d.Id);
                if (bookToDelete == null)
                    throw new BookNotFoundException(d.Id);

                _repo.Books.Remove(bookToDelete);
                Console.WriteLine($"cartea '{bookToDelete.Title}' a fost stearsa!");
                break;
        }
    }
}
//handlere pentru query-uri
class QueryHandler
{
    private readonly BookRepository _repo;

    public QueryHandler(BookRepository repo)
    {
        _repo = repo;
    }

    public TResult Handle<TResult>(IQuery<TResult> query)
    {
        switch (query)
        {
            case GetAllBooksQuery q:
                IEnumerable<Book> result = _repo.Books;

                if (!string.IsNullOrEmpty(q.FilterAuthor))
                {
                    result = result.Where(b => b.Author.Contains(q.FilterAuthor, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(q.SortBy))
                {
                    if (q.SortBy.ToLower() == "title")
                        result = result.OrderBy(b => b.Title);
                    else if (q.SortBy.ToLower() == "year")
                        result = result.OrderBy(b => b.Year);
                    else
                        throw new InvalidQueryParameterException("sortBy", $"sortarea dupa: '{q.SortBy}' nu exista sau nu este posibila");
                }

                return (TResult)(object)result.ToList();

            case GetBookByIdQuery q2:
                var found = _repo.Books.FirstOrDefault(b => b.Id == q2.Id);
                if (found == null)
                    throw new BookNotFoundException(q2.Id);
                return (TResult)(object)found;
        }

        throw new InvalidOperationException("Query necunoscut");
    }
}

//functia de afisare a cartilor
static class Utils
{
    public static void Display(Book book)
    {
        Console.WriteLine($"Book: '{book.Title}' - {book.Author} ({book.Year})");
    }
}


class Program
{
    static void Main()
    {
        BookRepository repo = new();
        CommandHandler commandHandler = new(repo);
        QueryHandler queryHandler = new(repo);

        //afisam toate cartile din repository 
        Console.WriteLine("Carti initiale:");
        Middleware.Handle(() =>
        {
            var books = queryHandler.Handle(new GetAllBooksQuery());
            foreach (var b in books)
            {
                Utils.Display(b);
            }
        });

        Console.WriteLine();

        //adaugam o carte noua
        Middleware.Handle(() =>
        {
            var newBook = new Book { Id = 4, Title = "C#", Author = "Daniel S.", Year = 2019 };
            commandHandler.Handle(new CreateBookCommand(newBook));
        });

        Middleware.Handle(() =>
        {
            Console.WriteLine("Filtrare dupa autor si sortare dupa titlu");
            var filtered = queryHandler.Handle(new GetAllBooksQuery("Andrei", "year"));
            foreach (var b in filtered)
            {
                Utils.Display(b);
            }
        });

        //incercam exceptia de InvalidQueryParameterException
        Middleware.Handle(() =>
        {
            Console.WriteLine("Sortam dupa o propietate invalida");
            queryHandler.Handle(new GetAllBooksQuery(null, "price"));
        });

        Middleware.Handle(() =>
        {
            commandHandler.Handle(new UpdateBookCommand(1, "Henry Porter", "Dan Dan", 2025));
        });

        //stergem cartea 2
        Middleware.Handle(() =>
        {
            commandHandler.Handle(new DeleteBookCommand(2));
        });
        
        //incercap exceptia de delete
        Middleware.Handle(() =>
        {
            commandHandler.Handle(new DeleteBookCommand(32));
        });

        Console.WriteLine("Lista noua a cartilor dupa actualizari:");
        Middleware.Handle(() =>
        {
            var all = queryHandler.Handle(new GetAllBooksQuery(null, "year"));
            foreach (var b in all)
            {
                Utils.Display(b);
            }
        });
    }
}
