using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Define the IBorrowable interface
interface IBorrowable
{
    void Borrow(Borrower borrower);
    void Return();
}

class Media : IBorrowable
{
    public string ID { get; protected set; }
    public string Title { get; set; }
    public bool IsBorrowed { get; set; } = false;
    public Borrower Borrower { get; set; }


    public Media(string title)
    {
        Title = title;
        ID = GenerateID("BO");
    }

    private string GenerateID(string prefix)
    {
        return prefix + DateTime.Now.ToString("yyyyMMddHHmmss"); ;
    }

    public virtual void Borrow(Borrower borrower)
    {
        IsBorrowed = true;
        Borrower = borrower;
    }

    public virtual void Return()
    {
        IsBorrowed = false;
        Borrower = null;
    }

}

class Book : Media
{
    public string Author { get; set; }
    public string Genre { get; set; }

    public Book(string title, string author, string genre) : base(title)
    {
        Author = author;
        Genre = genre;
    }

    public override void Borrow(Borrower borrower)
    {
        base.Borrow(borrower);
        Console.WriteLine($"Book '{Title}' borrowed by {borrower.Name}.");
    }

    public override void Return()
    {
        Console.WriteLine($"Book '{Title}' returned.");
        base.Return();
    }
}

class DVD : Media
{
    public string Director { get; set; }
    public int Runtime { get; set; }

    public DVD(string title, string director, int runtime) : base(title)
    {
        Director = director;
        Runtime = runtime;
    }

    public override void Borrow(Borrower borrower)
    {
        base.Borrow(borrower);
        Console.WriteLine($"DVD '{Title}' borrowed by {borrower.Name}.");
    }

    public override void Return()
    {
        Console.WriteLine($"DVD '{Title}' returned.");
        base.Return();
    }
}

class Borrower
{
    public string ID { get; private set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }

    public Borrower(string name, string address, string contactNumber, string email)
    {
        ID = GenerateID("BR");
        Name = name;
        Address = address;
        ContactNumber = contactNumber;
        Email = email;
    }

    public static string GenerateID(string prefix)
    {
        return prefix + DateTime.Now.ToString("yyyyMMddHHmmss");
    }
}

class GenericLibrary<T> : IEnumerable<T> where T : Media
{
    private List<T> mediaItems = new List<T>();

    public void AddMedia(T media)
    {
        mediaItems.Add(media);
    }

    public void RemoveMedia(string mediaId)
    {
        T mediaToRemove = mediaItems.Find(media => media.ID == mediaId);
        if (mediaToRemove != null)
        {
            mediaItems.Remove(mediaToRemove);
            Console.WriteLine("Media item removed successfully.");
        }
        else
        {
            Console.WriteLine("Media item not found.");
        }
    }

    public T GetMediaById(string mediaId)
    {
        return mediaItems.Find(media => media.ID == mediaId);
    }

    // Implementation of IEnumerable interface
    public IEnumerator<T> GetEnumerator()
    {
        return mediaItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

class Library
{
    public GenericLibrary<Book> BookLibrary { get; } = new GenericLibrary<Book>();
    public GenericLibrary<DVD> DVDLibrary { get; } = new GenericLibrary<DVD>();
    private List<Borrower> borrowers = new List<Borrower>();

    public void AddBorrower(Borrower borrower)
    {
        borrowers.Add(borrower);
    }

    public void RemoveBorrower(Borrower borrower)
    {
        borrowers.Remove(borrower);
    }

    public void BorrowMedia(Media media, Borrower borrower)
    {
        media.Borrow(borrower);
    }

    public void ReturnMedia(Media media)
    {
        media.Return();
    }

    public void DisplayMediaInventory()
    {
        Console.WriteLine("Books:");
        foreach (var book in BookLibrary)
        {
            string availability = book.IsBorrowed ? "Not Available" : "Available";
            Console.WriteLine($"ID: {book.ID}, Title: {book.Title}, Author: {book.Author}, Genre: {book.Genre}, Availability: {availability}");
        }

        Console.WriteLine("\nDVDs:");
        foreach (var dvd in DVDLibrary)
        {
            string availability = dvd.IsBorrowed ? "Not Available" : "Available";
            Console.WriteLine($"ID: {dvd.ID}, Title: {dvd.Title}, Director: {dvd.Director}, Runtime: {dvd.Runtime} minutes, Availability: {availability}");
        }
    }


    // Method to access borrowers list
    public List<Borrower> GetBorrowers()
    {
        return borrowers;
    }

    public List<Media> SearchMedia(string keyword)
    {
        var results = BookLibrary.Concat<Media>(DVDLibrary)
                    .Where(media => media.Title.Contains(keyword) ||
                                    (media is Book && ((Book)media).Author.Contains(keyword)) ||
                                    (media is Book && ((Book)media).Genre.Contains(keyword)) ||
                                    (media is DVD && ((DVD)media).Director.Contains(keyword)))
                    .ToList();

        return results;
    }

    public List<Media> GetBorrowedMedia()
    {
        var borrowedMedia = BookLibrary.Concat<Media>(DVDLibrary)
                            .Where(media => media.IsBorrowed)
                            .ToList();

        return borrowedMedia;
    }

    public List<Media> GetAvailableMedia()
    {
        var availableMedia = BookLibrary.Concat<Media>(DVDLibrary)
                            .Where(media => !media.IsBorrowed)
                            .ToList();

        return availableMedia;
    }

    public List<Media> GetMediaBorrowedByBorrower(string borrowerID)
    {
        var borrowedByBorrower = BookLibrary.Concat<Media>(DVDLibrary)
                                .Where(media => media.IsBorrowed && media.Borrower.ID == borrowerID)
                                .ToList();

        return borrowedByBorrower;
    }

    public List<Media> GetMediaWithTitleContainingKeyword(string keyword)
    {
        var mediaWithTitleContainingKeyword = BookLibrary.Concat<Media>(DVDLibrary)
                                                .Where(media => media.Title.Contains(keyword))
                                                .ToList();

        return mediaWithTitleContainingKeyword;
    }

    public List<Media> GetMediaBorrowedInLast7Days()
    {
        DateTime startDate = DateTime.Now.AddDays(-7);
        var borrowedInLast7Days = BookLibrary.Concat<Media>(DVDLibrary)
                                    .Where(media => media.IsBorrowed && media.Borrower != null && media.Borrower.ID.Contains("BR") &&
                                            DateTime.ParseExact(media.Borrower.ID.Substring(2), "yyyyMMddHHmmss", null) >= startDate)
                                    .ToList();

        return borrowedInLast7Days;
    }
}

class LibraryApp
{
    static void Main(string[] args)
    {
        Library library = new Library();

        while (true)
        {
            Console.WriteLine("1. Add Book");
            Console.WriteLine("2. Add DVD");
            Console.WriteLine("3. Add Borrower");
            Console.WriteLine("4. Remove Book");
            Console.WriteLine("5. Remove DVD");
            Console.WriteLine("6. Remove Borrower");
            Console.WriteLine("7. Borrow Book/DVD");
            Console.WriteLine("8. Return Book/DVD");
            Console.WriteLine("9. Search Media");
            Console.WriteLine("10. Display Inventory");
            Console.WriteLine("11. Display Borrowers");
            Console.WriteLine("12. Display Borrowed Media");
            Console.WriteLine("13. Display Available Media");
            Console.WriteLine("14. Display Media Borrowed by a Borrower");
            Console.WriteLine("15. Display Media with Title Containing Keyword");
            Console.WriteLine("16. Display Media Borrowed in Last 7 Days");
            Console.WriteLine("17. Exit");

            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook(library);
                    break;
                case "2":
                    AddDVD(library);
                    break;
                case "3":
                    AddBorrower(library);
                    break;
                case "4":
                    RemoveBook(library);
                    break;
                case "5":
                    RemoveDVD(library);
                    break;
                case "6":
                    RemoveBorrower(library);
                    break;
                case "7":
                    BorrowMedia(library);
                    break;
                case "8":
                    ReturnMedia(library);
                    break;
                case "9":
                    SearchMedia(library);
                    break;
                case "10":
                    DisplayInventory(library);
                    break;
                case "11":
                    DisplayBorrowers(library);
                    break;
                case "12":
                    DisplayBorrowedMedia(library);
                    break;
                case "13":
                    DisplayAvailableMedia(library);
                    break;
                case "14":
                    DisplayMediaBorrowedByBorrower(library);
                    break;
                case "15":
                    DisplayMediaWithTitleContainingKeyword(library);
                    break;
                case "16":
                    DisplayMediaBorrowedInLast7Days(library);
                    break;
                case "17":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void AddBook(Library library)
    {
        Console.Write("Enter book title: ");
        string title = Console.ReadLine();
        Console.Write("Enter author: ");
        string author = Console.ReadLine();
        Console.Write("Enter genre: ");
        string genre = Console.ReadLine();

        Book book = new Book(title, author, genre);
        library.BookLibrary.AddMedia(book);

        Console.WriteLine("Book added successfully.");
    }

    static void AddDVD(Library library)
    {
        Console.Write("Enter DVD title: ");
        string title = Console.ReadLine();
        Console.Write("Enter director: ");
        string director = Console.ReadLine();
        Console.Write("Enter runtime (in minutes): ");
        int runtime;
        while (!int.TryParse(Console.ReadLine(), out runtime) || runtime <= 0)
        {
            Console.Write("Invalid input. Enter a valid runtime (in minutes): ");
        }

        DVD dvd = new DVD(title, director, runtime);
        library.DVDLibrary.AddMedia(dvd);

        Console.WriteLine("DVD added successfully.");
    }

    static void AddBorrower(Library library)
    {
        Console.Write("Enter borrower name: ");
        string name = Console.ReadLine();
        Console.Write("Enter address: ");
        string address = Console.ReadLine();
        Console.Write("Enter contact number: ");
        string contactNumber = Console.ReadLine();
        Console.Write("Enter email: ");
        string email = Console.ReadLine();

        Borrower borrower = new Borrower(name, address, contactNumber, email);
        library.AddBorrower(borrower);

        Console.WriteLine($"Borrower added successfully. ID: {borrower.ID}");
    }

    static void RemoveBook(Library library)
    {
        Console.Write("Enter ID of the book to remove: ");
        string id = Console.ReadLine();

        library.BookLibrary.RemoveMedia(id);
    }

    static void RemoveDVD(Library library)
    {
        Console.Write("Enter ID of the DVD to remove: ");
        string id = Console.ReadLine();

        library.DVDLibrary.RemoveMedia(id);
    }

    static void RemoveBorrower(Library library)
    {
        Console.Write("Enter ID of the borrower to remove: ");
        string id = Console.ReadLine();

        Borrower borrowerToRemove = library.GetBorrowers().Find(b => b.ID == id);
        if (borrowerToRemove != null)
        {
            library.RemoveBorrower(borrowerToRemove);
            Console.WriteLine("Borrower removed successfully.");
        }
        else
        {
            Console.WriteLine("Borrower not found.");
        }
    }

    static void BorrowMedia(Library library)
    {
        Console.Write("Enter ID of the media item to borrow: ");
        string id = Console.ReadLine();

        Media media = library.BookLibrary.GetMediaById(id);
        if (media == null)
        {
            media = library.DVDLibrary.GetMediaById(id);
        }

        if (media != null)
        {
            Console.Write("Enter ID of the borrower: ");
            string borrowerID = Console.ReadLine();
            Borrower borrower = library.GetBorrowers().Find(b => b.ID == borrowerID);
            if (borrower != null)
            {
                library.BorrowMedia(media, borrower);
            }
            else
            {
                Console.WriteLine("Borrower not found.");
            }
        }
        else
        {
            Console.WriteLine("Media item not found.");
        }
    }

    static void ReturnMedia(Library library)
    {
        Console.Write("Enter ID of the media item to return: ");
        string id = Console.ReadLine();

        Media media = library.BookLibrary.GetMediaById(id);
        if (media == null)
        {
            media = library.DVDLibrary.GetMediaById(id);
        }

        if (media != null)
        {
            library.ReturnMedia(media);
        }
        else
        {
            Console.WriteLine("Media item not found.");
        }
    }

    static void SearchMedia(Library library)
    {
        Console.Write("Enter search keyword: ");
        string keyword = Console.ReadLine();

        List<Media> results = library.SearchMedia(keyword);
        if (results.Count == 0)
        {
            Console.WriteLine("No media found matching the search criteria.");
        }
        else
        {
            Console.WriteLine("Search results:");
            foreach (var media in results)
            {
                if (media is Book)
                {
                    Book book = (Book)media;
                    Console.WriteLine($"Book: {book.Title} by {book.Author}");
                }
                else if (media is DVD)
                {
                    DVD dvd = (DVD)media;
                    Console.WriteLine($"DVD: {dvd.Title} directed by {dvd.Director}");
                }
            }
        }
    }

    static void DisplayInventory(Library library)
    {
        library.DisplayMediaInventory();
    }

    static void DisplayBorrowers(Library library)
    {
        List<Borrower> borrowers = library.GetBorrowers();
        Console.WriteLine("Borrowers:");
        foreach (var borrower in borrowers)
        {
            Console.WriteLine($"ID: {borrower.ID}, Name: {borrower.Name}, Address: {borrower.Address}, Contact: {borrower.ContactNumber}, Email: {borrower.Email}");
        }
    }

    static void DisplayBorrowedMedia(Library library)
    {
        List<Media> borrowedMedia = library.GetBorrowedMedia();
        Console.WriteLine("Borrowed Media:");
        foreach (var media in borrowedMedia)
        {
            if (media is Book)
            {
                Book book = (Book)media;
                Console.WriteLine($"Book: {book.Title}");
            }
            else if (media is DVD)
            {
                DVD dvd = (DVD)media;
                Console.WriteLine($"DVD: {dvd.Title}");
            }
        }
    }

    static void DisplayAvailableMedia(Library library)
    {
        List<Media> availableMedia = library.GetAvailableMedia();
        Console.WriteLine("Available Media:");
        foreach (var media in availableMedia)
        {
            if (media is Book)
            {
                Book book = (Book)media;
                Console.WriteLine($"Book: {book.Title}");
            }
            else if (media is DVD)
            {
                DVD dvd = (DVD)media;
                Console.WriteLine($"DVD: {dvd.Title}");
            }
        }
    }

    static void DisplayMediaBorrowedByBorrower(Library library)
    {
        Console.Write("Enter ID of the borrower: ");
        string borrowerID = Console.ReadLine();

        List<Media> borrowedByBorrower = library.GetMediaBorrowedByBorrower(borrowerID);
        Console.WriteLine($"Media Borrowed by Borrower (ID: {borrowerID}):");
        foreach (var media in borrowedByBorrower)
        {
            if (media is Book)
            {
                Book book = (Book)media;
                Console.WriteLine($"Book: {book.Title}");
            }
            else if (media is DVD)
            {
                DVD dvd = (DVD)media;
                Console.WriteLine($"DVD: {dvd.Title}");
            }
        }
    }

    static void DisplayMediaWithTitleContainingKeyword(Library library)
    {
        Console.Write("Enter keyword to search titles: ");
        string keyword = Console.ReadLine();

        List<Media> mediaWithTitleContainingKeyword = library.GetMediaWithTitleContainingKeyword(keyword);
        Console.WriteLine($"Media with Titles Containing Keyword \"{keyword}\":");
        foreach (var media in mediaWithTitleContainingKeyword)
        {
            if (media is Book)
            {
                Book book = (Book)media;
                Console.WriteLine($"Book: {book.Title}");
            }
            else if (media is DVD)
            {
                DVD dvd = (DVD)media;
                Console.WriteLine($"DVD: {dvd.Title}");
            }
        }
    }

    static void DisplayMediaBorrowedInLast7Days(Library library)
    {
        List<Media> borrowedInLast7Days = library.GetMediaBorrowedInLast7Days();
        Console.WriteLine("Media Borrowed in Last 7 Days:");
        foreach (var media in borrowedInLast7Days)
        {
            if (media is Book)
            {
                Book book = (Book)media;
                Console.WriteLine($"Book: {book.Title}");
            }
            else if (media is DVD)
            {
                DVD dvd = (DVD)media;
                Console.WriteLine($"DVD: {dvd.Title}");
            }
        }
    }
}
