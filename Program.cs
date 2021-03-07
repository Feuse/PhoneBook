using System;
using System.IO;

namespace IOTesting
{
    class Program
    {
        static void Main(string[] args)
        {

            string filename = "C:\\Users\\Feuse135\\source\\repos\\IOTesting\\book.txt";

            PhoneBook book = new PhoneBook(filename);

            Entry e1 = new Entry() { Name = "roman", Phone = "12120", Type = "Home" };
            Entry e2 = new Entry() { Name = "sterlin", Phone = "0", Type = "Home" };
            Entry e3 = new Entry() { Name = "newOne", Phone = "0000000", Type = "Work" };
            Entry e4 = new Entry() { Name = "Yoskeli", Phone = "0", Type = "Work" };
            Entry e5 = new Entry() { Name = "moshiko", Phone = "0", Type = "Home" };


            //book.InsertOrUpdate(e5);
            book.InsertOrUpdate(e5);
            book.InsertOrUpdate(e1);
            book.InsertOrUpdate(e2);
            book.InsertOrUpdate(e3);
            book.InsertOrUpdate(e4);
            //var entries = book.Iterate();

            //  var entry = book.GetByName("newOne");

            //  book.Iterate();
            //add a duplicate 
            // book.InsertOrUpdate(e5);

            //  var entries2 = book.Iterate();

            // var entry2 = book.GetByName("roman");

        }
    }
}
