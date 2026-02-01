import java.util.*;

class Book {
    String title, author, isbn;
    int copies;

    public Book(String title, String author, String isbn, int copies) {
        this.title = title; this.author = author; this.isbn = isbn; this.copies = copies;
    }

    @Override
    public String toString() {
        return title + " (" + isbn + ") [" + copies + "]";
    }
}

class Reader {
    String name, id;
    List<String> borrowed = new ArrayList<>();

    public Reader(String name, String id) { this.name = name; this.id = id; }

    @Override
    public String toString() { return name + " (" + id + ")"; }
}

class Library {
    Map<String, Book> books = new HashMap<>();
    Map<String, Reader> readers = new HashMap<>();

    public void addBook(Book b) {
        books.merge(b.isbn, b, (oldB, newB) -> { oldB.copies += newB.copies; return oldB; });
        System.out.println("Добавлено: " + b.isbn);
    }

    public void removeBookCopy(String isbn) {
        Book b = books.get(isbn);
        if (b == null) { System.out.println("Не найдено: " + isbn); return; }
        if (b.copies <= 0) { System.out.println("Нет копий для удаления: " + isbn); return; }
        b.copies--;
        if (b.copies == 0) books.remove(isbn);
        System.out.println("Удалена одна копия: " + isbn);
    }

    public void registerReader(Reader r) {
        if (readers.containsKey(r.id)) { System.out.println("Уже зарегистрирован: " + r.id); return; }
        readers.put(r.id, r); System.out.println("Зарегистрирован: " + r.id);
    }

    public void removeReader(String id) {
        Reader r = readers.get(id);
        if (r == null) { System.out.println("Читатель не найден: " + id); return; }
        if (!r.borrowed.isEmpty()) { System.out.println("У читателя есть книги: " + id); return; }
        readers.remove(id); System.out.println("Читатель удалён: " + id);
    }

    public void issueBook(String isbn, String readerId) {
        Book b = books.get(isbn); Reader r = readers.get(readerId);
        if (b == null) { System.out.println("Книга не найдена: " + isbn); return; }
        if (r == null) { System.out.println("Читатель не найден: " + readerId); return; }
        if (b.copies <= 0) { System.out.println("Нет доступных копий: " + isbn); return; }
        b.copies--; r.borrowed.add(isbn);
        System.out.println("Выдана " + isbn + " читателю " + readerId);
    }

    public void returnBook(String isbn, String readerId) {
        Book b = books.get(isbn); Reader r = readers.get(readerId);
        if (b == null || r == null) { System.out.println("Книга или читатель не найдены."); return; }
        if (!r.borrowed.remove(isbn)) { System.out.println("Читатель не брал книгу: " + isbn); return; }
        b.copies++; System.out.println("Возвращена " + isbn + " от " + readerId);
    }

    public void listBooks() {
        System.out.println("Книги:");
        books.values().forEach(b -> System.out.println(" - " + b));
    }

    public void listReaders() {
        System.out.println("Читатели:");
        readers.values().forEach(r -> System.out.println(" - " + r + " Взял(а): " + r.borrowed));
    }
}

public class module1_HomeWork {
    public static void main(String[] args) {
        Library lib = new Library();

        lib.addBook(new Book("Преступление и наказание", "Достоевский", "ISBN001", 2));
        lib.addBook(new Book("Мастер и Маргарита", "Булгаков", "ISBN002", 1));

        lib.registerReader(new Reader("Иван", "R1"));
        lib.registerReader(new Reader("Пётр", "R2"));

        lib.listBooks();
        lib.listReaders();

        lib.issueBook("ISBN001", "R1");
        lib.issueBook("ISBN001", "R2");
        lib.issueBook("ISBN001", "R2");

        lib.returnBook("ISBN001", "R1");
        lib.removeBookCopy("ISBN001");

        lib.listBooks();
        lib.listReaders();
    }
}