using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace KonyvtariNyilvantarto
{
    public class Book
    {
        uint _ID;
        public uint ID { get => _ID; }

        string _author;
        public string Szerző { get => _author; }

        string _title;
        public string Cím { get => _title; }

        string _releaseYear;
        public string KiadásÉve { get => _releaseYear; }

        string _publisher;
        public string Kiadó { get => _publisher; }

        public bool Kölcsönözhető { get => _isBorrowable; }
        bool _isBorrowable;

        public Book(string line)
        {
            string[] separatedLine = line.Split(';');

            _ID = Convert.ToUInt32(separatedLine[0]);
            _author = separatedLine[1];
            _title = separatedLine[2];
            _releaseYear = separatedLine[3];
            _publisher = separatedLine[4];
            _isBorrowable = Convert.ToBoolean(separatedLine[5]);
        }
    }

    public class Member
    {
        uint _ID;
        public uint ID { get => _ID; }
        string _name;
        public string Név { get => _name; }
        string _address;
        public string Lakcím { get => _address; }

        public Member(string line)
        {
            string[] separatedLine = line.Split(';');

            _ID = Convert.ToUInt32(separatedLine[0]);
            _name = separatedLine[1];
            _address = string.Join(", ",separatedLine.Skip(2));
        }
    }

    public class Borrow
    {
        uint _ID;
        public uint ID { get => _ID; }
        uint _borrowerID;
        public uint TagID { get => _borrowerID; }
        uint _bookID;
        public uint KönyvID { get => _bookID; }
        DateTime _borrowDate;
        public DateTime KölcsönzésDátuma { get => _borrowDate; }
        DateTime? _returnDate;
        public DateTime? KölcsönzésVisszavétele { get => _returnDate; }

        public Borrow(string line)
        {
            string[] separatedLine = line.Split(';');

            _ID = Convert.ToUInt32(separatedLine[0]);
            _borrowerID = Convert.ToUInt32(separatedLine[1]);
            _bookID = Convert.ToUInt32(separatedLine[2]);
            _borrowDate = DateTime.ParseExact(separatedLine[3], "yyyy.MM.dd.", null);
            if (separatedLine[4] != "")
                _returnDate = DateTime.ParseExact(separatedLine[4], "yyyy.MM.dd.", null);
            else
                _returnDate = null;
        }
    }

    public partial class MainWindow : Window
    {

        public string[] PathsToData = new string[3];

        public BindingList<Book> Books = new BindingList<Book>();
        public BindingList<Member> Members = new BindingList<Member>();
        public List<Borrow> Borrows = new List<Borrow>();
        public BindingList<Borrow> DisplayedBorrows = new BindingList<Borrow>();

        public MainWindow()
        {
            InitializeComponent();

            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "Könyvtár Állományok (*.txt)|*.txt",
                RestoreDirectory = true
            };

            fileDialog.Title = "Válaszd ki a könyv állomány helyét";
            fileDialog.ShowDialog();
            PathsToData[0] = fileDialog.FileName;

            fileDialog.Title = "Válaszd ki a tag állomány helyét";
            fileDialog.ShowDialog();
            PathsToData[1] = fileDialog.FileName;

            fileDialog.Title = "Válaszd ki a kölcsönzés állomány helyét";
            fileDialog.ShowDialog();
            PathsToData[2] = fileDialog.FileName;

            string[] input = File.ReadAllLines(PathsToData[0]);
            foreach (string i in input)
            {
                if (i.Trim() == "") continue;
                Books.Add(new Book(i));
            }


            input = File.ReadAllLines(PathsToData[1]);
            foreach(string i in input)
            {
                if (i.Trim() == "") continue;
                Members.Add(new Member(i));
            }


            input = File.ReadAllLines(PathsToData[2]);
            foreach(string i in input)
            {
                if (i.Trim() == "") continue;
                Borrows.Add(new Borrow(i));
            }

            BookDataGrid.ItemsSource = Books;
            MemberDataGrid.ItemsSource = Members;
            BorrowDataGrid.ItemsSource = DisplayedBorrows;

            Borrows.ForEach(x => DisplayedBorrows.Add(x));
        }

 
        private void BookDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BookFillFields(BookDataGrid.SelectedIndex);
        }

        void BookFillFields(int index)
        {
            if (index == -1) return;

            if(!AuthorField.IsEnabled)
            {
                AuthorField.IsEnabled = true;
                TitleField.IsEnabled = true;
                ReleaseYearField.IsEnabled = true;
                PublisherField.IsEnabled = true;
                BorrowableCheck.IsEnabled = true;
            }


            IDField.Text = Books[index].ID.ToString();
            AuthorField.Text = Books[index].Szerző;
            TitleField.Text = Books[index].Cím;
            ReleaseYearField.Text = Books[index].KiadásÉve;
            PublisherField.Text = Books[index].Kiadó;
            BorrowableCheck.IsChecked = Books[index].Kölcsönözhető;
        }

        private void NewBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthorField.IsEnabled)
            {
                AuthorField.IsEnabled = true;
                TitleField.IsEnabled = true;
                ReleaseYearField.IsEnabled = true;
                PublisherField.IsEnabled = true;
                BorrowableCheck.IsEnabled = true;
            }

            IDField.Text = (Books[Books.Count - 1].ID + 1).ToString();
            AuthorField.Text = "";
            TitleField.Text = "";
            ReleaseYearField.Text = "";
            PublisherField.Text = "";
            BorrowableCheck.IsChecked = false;
        }

        private void BookSaveButton_Click(object sender, RoutedEventArgs e)
        {
            int bookEntryID = Books.ToList().FindIndex(x => x.ID == int.Parse(IDField.Text));

            string borrowableString = BorrowableCheck.IsChecked.ToString().ToUpper()[0] + BorrowableCheck.IsChecked.ToString().Substring(1);
            string newLine = $"\n{IDField.Text};{AuthorField.Text};{TitleField.Text};{ReleaseYearField.Text};{PublisherField.Text};{borrowableString}";
            if (bookEntryID == -1)
            {
                File.AppendAllText(PathsToData[0], newLine);
                Books.Add(new Book(newLine));
            }
            else
            {
                Books[bookEntryID] = new Book(newLine);
            }

        }

        private void BookDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int bookEntryID = Books.ToList().FindIndex(x => x.ID == int.Parse(IDField.Text));
            if(bookEntryID != -1)
            {
                Books.RemoveAt(bookEntryID);
                List<string> currentFile = File.ReadAllLines(PathsToData[0]).ToList();
                currentFile.RemoveAt(bookEntryID);
                File.WriteAllLines(PathsToData[0], currentFile);
            }
        }


        void MemberFillFields(int index)
        {
            if (index == -1) return;

            if (!MemberNameField.IsEnabled)
            {
                MemberNameField.IsEnabled = true;
                MemberAddressField.IsEnabled = true;
            }

            MemberIDField.Text = Members[index].ID.ToString();
            MemberNameField.Text = Members[index].Név;
            MemberAddressField.Text = Members[index].Lakcím;

            MemberBorrowedBooksGrid.ItemsSource = Borrows.Where(x => x.TagID == Members[index].ID);
        }

        private void MemberDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MemberFillFields(MemberDataGrid.SelectedIndex);
        }

        private void NewMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MemberNameField.IsEnabled)
            {
                MemberNameField.IsEnabled = true;
                MemberAddressField.IsEnabled = true;
            }

            MemberIDField.Text = (Members[Members.Count - 1].ID + 1).ToString();
            MemberNameField.Text = "";
            MemberAddressField.Text = "";
        }

        private void MemberDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int memberEntryID = Members.ToList().FindIndex(x => x.ID == int.Parse(MemberIDField.Text));
            if (memberEntryID != -1)
            {
                Members.RemoveAt(memberEntryID);
                List<string> currentFile = File.ReadAllLines(PathsToData[1]).ToList();
                currentFile.RemoveAt(memberEntryID);
                File.WriteAllLines(PathsToData[1], currentFile);
            }
        }

        private void MemberSaveButton_Click(object sender, RoutedEventArgs e)
        {
            int memberEntryID = Members.ToList().FindIndex(x => x.ID == int.Parse(MemberIDField.Text));

            string newLine = $"\n{MemberIDField.Text};{MemberNameField.Text};{MemberAddressField.Text.Replace(", ", ";")}";
            if (memberEntryID == -1)
            {
                File.AppendAllText(PathsToData[1], newLine);
                Members.Add(new Member(newLine));
            }
            else
            {
                Members[memberEntryID] = new Member(newLine);
            }
        }



        private void BorrowSearchButton_Click(object sender, RoutedEventArgs e)
        {
            List<Borrow> foundResults = new List<Borrow>();
            List<Book> searchedBooks = Books.Where(x => x.Szerző.ToLower().StartsWith(BorrowSearchAuthorField.Text.ToLower())).ToList();
            searchedBooks = searchedBooks.Intersect(Books.Where(x => x.Cím.ToLower().StartsWith(BorrowSearchTitleField.Text.ToLower())), new BorrowEquality()).ToList();
            List<Member> searchedMembers = Members.Where(x => x.Név.ToLower().StartsWith(BorrowSearchMemberField.Text.ToLower())).ToList();

            foundResults = Borrows.Where(x => searchedBooks.Exists(y => y.ID == x.KönyvID)).ToList();
            foundResults = foundResults.Where(x => searchedMembers.Exists(y => y.ID == x.TagID)).ToList();
            
            if((bool)OnlyExpiredCheck.IsChecked)
            {
                foundResults = foundResults.Where(x => x.KölcsönzésVisszavétele == null && (DateTime.Now - x.KölcsönzésDátuma).TotalDays > 30).ToList();
            }

            DisplayedBorrows.Clear();
            foundResults.ForEach(x => DisplayedBorrows.Add(x));
        }

        class BorrowEquality : IEqualityComparer<Book>
        {
            public bool Equals(Book b1, Book b2) => b1.ID == b2.ID;

            public int GetHashCode(Book book) => book.GetHashCode();
            
        }

        private void OnlyExpiredCheck_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
