using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace IOTesting
{
    public class PhoneBook
    {
        private string filename;
        public PhoneBook(string _filename)
        {
            filename = _filename;
        }
        public Entry GetByName(string name)
        {
            var indexFile = ConfigurationManager.AppSettings["indexFile"];
            var index = Encoding.ASCII.GetBytes(name);
            return FindInIndexStorage(indexFile, index, filename);
        }
        // will overwrite the entry if there is already
        // one with the same Name
        public void InsertOrUpdate(Entry e)
        {
            var indexFile = ConfigurationManager.AppSettings["indexFile"];
            var temp = ConfigurationManager.AppSettings["temp"];
            var indexInBytes = Encoding.ASCII.GetBytes(e.Name);
            var found = FindInIndexStorage(indexFile, indexInBytes, filename);
            if (found != null)
            {
                var index = FindIndex(indexFile, IndexToBytes(e));
                var book = LoadBook();
                CreateNewArray(book, index, temp, e, indexFile);
            }
            else
            {
                SaveIndex(IndexToBytes(e), indexFile, e);
                InsertEntity(e);
            }
        }

        //Get a list of all the entries, in Name order
        public IEnumerable<Entry> Iterate()
        {
            List<Entry> entries = new List<Entry>();
            var indexFile = ConfigurationManager.AppSettings["indexFile"];
            var ordered = IterateIndexes(indexFile);
            foreach (var item in ordered)
            {
                entries.Add(GetByName(item.Key));
            }
            return entries.AsEnumerable();
        }

        private SortedList<string, string> IterateIndexes(string indexFile)
        {
            SortedList<string, string> list = new SortedList<string, string>();
            FileStream fs = new FileStream(indexFile, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            var line = "";
            while ((line = sr.ReadLine()) != null)
            {
                var obj = JsonConvert.DeserializeObject<IndexedEntry>(line);

                list.Add(System.Text.Encoding.UTF8.GetString(obj.ByteName), "");
            }

            sr.Close();
            fs.Close();
            return list;
        }

        public byte[] IndexToBytes(Entry e)
        {
            return Encoding.ASCII.GetBytes(e.Name);
        }
        public byte[] EntryToBytes(Entry e)
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(e));
        }

        public int GetSize(Entry e)
        {
            return Encoding.ASCII.GetBytes(e.ToString()).Length;
        }
        public int GetFileEnd()
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            var file = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return Encoding.ASCII.GetBytes(file).Length;

        }

        public void SaveIndex(byte[] index, string indexFile, Entry e)
        {
            FileStream fs = new FileStream(indexFile, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);

            var start = GetFileEnd();
            var end = (long)start + GetSize(e);

            IndexedEntry es = new IndexedEntry() { ByteName = index, Block = new[] { start, end } };

            var obj = JsonConvert.SerializeObject(es);

            sw.WriteLine(obj);
            sw.Flush();
            sw.Close();
            fs.Close();

        }
        public IndexedEntry FindIndex(string indexFile, byte[] index)
        {
            using (var fs = new FileStream(indexFile, FileMode.Open))
            {
                using (var sw = new StreamReader(fs))
                {
                    var line = "";
                    while ((line = sw.ReadLine()) != null)
                    {
                        var obj = JsonConvert.DeserializeObject<IndexedEntry>(line);

                        if (ByteArrayCompare(index, obj.ByteName))
                        {
                            sw.Close();
                            fs.Close();
                            return obj;
                        }
                    }
                    sw.Close();
                    fs.Close();
                }
            }
            return null;
        }
        public Entry FindInIndexStorage(string indexFile, byte[] index, string bookFile)
        {
            var indexObj = FindIndex(indexFile, index);  //find index

            if (indexObj != null)
            {
                FileStream fs = new FileStream(bookFile, FileMode.Open);
                StreamReader sw = new StreamReader(fs);
                var blockLen = indexObj.Block[1] - indexObj.Block[0];
                char[] buffer = new char[blockLen];

                fs.Seek(indexObj.Block[0], SeekOrigin.Current);
                sw.ReadBlock(buffer, 0, (int)blockLen);
                string found = new string(buffer);

                string cleaned = found.Replace("\n", "").Replace("\r", "");

                fs.Close();
                sw.Close();
                return JsonConvert.DeserializeObject<Entry>(cleaned);
            }
            return null;
        }

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        public void CreateNewArray(byte[] oldArray, IndexedEntry e, string temp, Entry entry, string indexFile)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamWriter sw = new StreamWriter(fs);

            var entryBytes = EntryToBytes(entry);
            var indexBytes = IndexToBytes(entry);
            char[] entryChar = System.Text.Encoding.ASCII.GetString(entryBytes).ToCharArray();
            int sizeDiff;
            byte[] end;
            char[] cEnd=null;

            var oldBlockSize = (int)e.Block[1] - (int)e.Block[0];

            var entrySize = entryChar.Length;

            if (entrySize < oldBlockSize)
            {
                sizeDiff = oldBlockSize - entrySize;
                end = oldArray.SubArray((int)e.Block[1], oldArray.Length - (int)e.Block[1]);
                cEnd = System.Text.Encoding.ASCII.GetString(end).ToCharArray();
            }
            else if (entrySize > oldBlockSize)
            {
                sizeDiff = entrySize - oldBlockSize;

                end = oldArray.SubArray((int)e.Block[1], oldArray.Length - (int)e.Block[1]);
                cEnd = System.Text.Encoding.ASCII.GetString(end).ToCharArray();
            }
            else
            {
                sizeDiff = entrySize - oldBlockSize;
            }
            if (sizeDiff == 0)
            {

                end = oldArray.SubArray((int)e.Block[1], oldArray.Length - (int)e.Block[1]);
                cEnd = System.Text.Encoding.ASCII.GetString(end).ToCharArray();
            }

            fs.Seek((int)e.Block[0], SeekOrigin.Current);
            sw.Write(entryChar, 0, entryChar.Length);
            sw.Write(cEnd, 0, cEnd.Length);
            fs.SetLength(fs.Length - sizeDiff);
            var entryStart = (int)e.Block[0];
            var entryEnd = (int)e.Block[0] + entrySize;
            var l = fs.Length;

            //var sizeDiff = 0;
            bool sub = false;
            IndexedEntry indexToReplace = new IndexedEntry() { ByteName = indexBytes, Block = new long[] { entryStart, entryEnd } };


            if (entryEnd > oldBlockSize)
            {
                sizeDiff = entryEnd - (int)e.Block[1];
            }
            else
            {
                sizeDiff = oldBlockSize - entrySize;
                sub = true;
            }

            sw.Flush();
            sw.Flush();

            sw.Close();
            fs.Close();

            RemoveIndexAndSave(e, temp, indexFile, sizeDiff, indexToReplace, sub);
        }

        private void RemoveIndexAndSave(IndexedEntry indexToRemove, string temp, string indexFile, int sizeDiff, IndexedEntry indexToReplace, bool sub)
        {
            string result = System.Text.Encoding.UTF8.GetString(indexToRemove.ByteName);

            using (var sr1 = new StreamReader(indexFile))
            using (var sw1 = new StreamWriter(temp))
            {
                var line = "";
                bool flag = false;
                while ((line = sr1.ReadLine()) != null)
                {
                    var obj = JsonConvert.DeserializeObject<IndexedEntry>(line);
                    string resultobj = System.Text.Encoding.UTF8.GetString(obj.ByteName);
                    if (flag && sub)
                    {
                        obj.Block[0] = obj.Block[0] - sizeDiff;
                        obj.Block[1] = obj.Block[1] - sizeDiff;
                    }
                    else if (flag)
                    {
                        obj.Block[0] = obj.Block[0] + sizeDiff;
                        obj.Block[1] = obj.Block[1] + sizeDiff;
                    }
                    if (resultobj != result)
                    {
                        var reWrittenObj = JsonConvert.SerializeObject(obj);
                        sw1.WriteLine(reWrittenObj);
                    }
                    else
                    {
                        sw1.WriteLine(JsonConvert.SerializeObject(indexToReplace));
                        flag = true;
                    }
                }
            }

            File.Delete(indexFile);
            File.Move(temp, indexFile);
        }

        public void InsertEntity(Entry e)
        {
            FileStream fs = new FileStream(filename, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(e);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public byte[] LoadBook()
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            byte[] arr = new byte[fs.Length];
            MemoryStream ms = new MemoryStream(arr);

            var all = sr.ReadToEnd();

            ms.Close();
            sr.Close();
            fs.Close();

            return Encoding.ASCII.GetBytes(all);
        }
    }
}
