using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ActionStreetMap.Osm.Index.Search.Storage;

namespace ActionStreetMap.Osm.Index.Search
{
    public class SearchEngine
    {
        private readonly SafeDictionary<string, int> _words = new SafeDictionary<string, int>();
        private readonly BitmapIndex _bitmaps;
        private readonly ILog _log = LogManager.GetLogger(typeof(SearchEngine));
        private int _lastDocNum;
        private readonly string _fileName = "words";
        private readonly string _path = "";
        private readonly KeyStoreLong _docs;

        public SearchEngine(string IndexPath, string FileName)
        {
            _path = IndexPath;
            _fileName = FileName;
            if (_path.EndsWith(Path.DirectorySeparatorChar.ToString()) == false) _path += Path.DirectorySeparatorChar;
            Directory.CreateDirectory(IndexPath);
            LogManager.Configure(_path + "log.txt", 200, false);
            _log.Debug("\r\n\r\n");
            _log.Debug("Starting hOOt....");
            _log.Debug("Storage Folder = " + _path);

            _docs = new KeyStoreLong(_path + "files.docs", false);
            _bitmaps = new BitmapIndex(_path, _fileName + ".mgbmp");
            _lastDocNum = _docs.Count();
            LoadWords();
        }

        public int WordCount
        {
            get { return _words.Count; }
        }

        public int DocumentCount
        {
            get { return _lastDocNum; }
        }

        public void Save()
        {
            InternalSave();
        }

        public void Index(int recordnumber, string text)
        {
            AddtoIndex(recordnumber, text);
        }

        public WahBitArray Query(string filter, int maxsize)
        {
            return ExecutionPlan(filter, maxsize);
        }

        public int Index(Document document, bool deleteold)
        {
            _log.Debug("indexing doc : " + document.Element.Id);
            DateTime dt = FastDateTime.Now;

            if (deleteold || document.DocNumber == -1)
                document.DocNumber = _lastDocNum++;

            _docs.Set(document.Element.Id, DocumentSerializer.Serialize(document));

            _log.Debug("writing doc to disk (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = FastDateTime.Now;

            var builder = new StringBuilder(document.Element.Tags.Count*16);
            foreach (KeyValuePair<string, string> pair in document.Element.Tags)
                builder.AppendFormat("{0} {1}", pair.Key, pair.Value);

            // index doc
            AddtoIndex(document.DocNumber, builder.ToString());
            _log.Debug("indexing time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);

            return _lastDocNum;
        }

        public IEnumerable<int> FindRows(string filter)
        {
            WahBitArray bits = ExecutionPlan(filter, _docs.RecordCount());
            // enumerate records
            return bits.GetBitIndexes();
        }

        public IEnumerable<Document> FindDocuments(string filter)
        {
            WahBitArray bits = ExecutionPlan(filter, _docs.RecordCount());
            // enumerate documents
            foreach (int i in bits.GetBitIndexes())
            {
                if (i > _lastDocNum - 1)
                    break;
                var bytes = _docs.ReadData(i);
                Document d = DocumentSerializer.Deserialize(bytes);

                yield return d;
            }
        }

        public Document FindById(long id)
        {
            byte[] data;
            _docs.Get(id, out data);
            return DocumentSerializer.Deserialize(data);
        }

        /* public IEnumerable<string> FindDocumentFileNames(string filter)
        {
            WahBitArray bits = ExecutionPlan(filter, _docs.RecordCount());
            // enumerate documents
            foreach (int i in bits.GetBitIndexes())
            {
                if (i > _lastDocNum - 1)
                    break;
                string b = _docs.ReadData(i);
                var d = (Dictionary<string, object>)fastJSON.JSON.Instance.Parse(b);

                yield return d["FileName"].ToString();
            }
        }*/

        public void RemoveDocument(int number)
        {
            // add number to deleted bitmap
            //_deleted.Set(true, number);
        }

        /* public bool RemoveDocument(string filename)
        {
            // remove doc based on filename
            byte[] b;
            if (_docs.Get(filename.ToLower(), out b))
            {
                Document d = fastJSON.JSON.Instance.ToObject<Document>(Encoding.Unicode.GetString(b));
                RemoveDocument(d.DocNumber);
                return true;
            }
            return false;
        }*/

        /*public bool IsIndexed(string filename)
        {
            byte[] b;
            return _docs.Get(filename.ToLower(), out b);
        }*/

        public void OptimizeIndex()
        {
            _bitmaps.Commit(false);
            _bitmaps.Optimize();
        }

        #region [  P R I V A T E   M E T H O D S  ]

        private WahBitArray ExecutionPlan(string filter, int maxsize)
        {
            _log.Debug("query : " + filter);
            DateTime dt = FastDateTime.Now;
            // query indexes
            string[] words = filter.Split(' ');
            bool defaultToAnd = !(filter.IndexOfAny(new[] {'+', '-'}, 0) > 0);

            WahBitArray bits = null;

            foreach (string s in words)
            {
                int c;
                string word = s;
                if (s == "") continue;

                Operation op = Operation.Or;
                if (defaultToAnd)
                    op = Operation.And;

                if (s.StartsWith("+"))
                {
                    op = Operation.And;
                    word = s.Replace("+", "");
                }

                if (s.StartsWith("-"))
                {
                    op = Operation.AndNot;
                    word = s.Replace("-", "");
                }

                if (s.Contains("*") || s.Contains("?"))
                {
                    WahBitArray wildbits = null;
                    // do wildcard search
                    Regex reg = new Regex("^" + s.Replace("*", ".*").Replace("?", "."), RegexOptions.IgnoreCase);
                    foreach (string key in _words.Keys())
                    {
                        if (reg.IsMatch(key))
                        {
                            _words.TryGetValue(key, out c);
                            WahBitArray ba = _bitmaps.GetBitmap(c);

                            wildbits = DoBitOperation(wildbits, ba, Operation.Or, maxsize);
                        }
                    }
                    if (bits == null)
                        bits = wildbits;
                    else
                    {
                        if (op == Operation.And)
                            bits = bits.And(wildbits);
                        else
                            bits = bits.Or(wildbits);
                    }
                }
                else if (_words.TryGetValue(word.ToLowerInvariant(), out c))
                {
                    // bits logic
                    WahBitArray ba = _bitmaps.GetBitmap(c);
                    bits = DoBitOperation(bits, ba, op, maxsize);
                }
            }
            if (bits == null)
                return new WahBitArray();

            // remove deleted docs
            WahBitArray ret = bits;
            // if (_docMode)
            //    ret = bits.AndNot(_deleted.GetBits());
            // else
            //     ret = bits;
            _log.Debug("query time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);
            return ret;
        }

        private static WahBitArray DoBitOperation(WahBitArray bits, WahBitArray c, Operation op, int maxsize)
        {
            if (bits != null)
            {
                switch (op)
                {
                    case Operation.And:
                        bits = c.And(bits);
                        break;
                    case Operation.Or:
                        bits = c.Or(bits);
                        break;
                    case Operation.AndNot:
                        bits = c.And(bits.Not(maxsize));
                        break;
                }
            }
            else
                bits = c;
            return bits;
        }

        private readonly object _lock = new object();

        private void InternalSave()
        {
            lock (_lock)
            {
                _log.Debug("saving index...");
                DateTime dt = FastDateTime.Now;
                // save deleted
                //_deleted.SaveIndex();

                // save docs 
                _docs.SaveIndex();
                _bitmaps.Commit(false);

                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8);

                // save words and bitmaps
                using (FileStream words = new FileStream(_path + _fileName + ".words", FileMode.Create))
                {
                    foreach (string key in _words.Keys())
                    {
                        bw.Write(key);
                        bw.Write(_words[key]);
                    }
                    byte[] b = ms.ToArray();
                    words.Write(b, 0, b.Length);
                    words.Flush();
                    words.Close();
                }
                _log.Debug("save time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);
            }
        }

        private void LoadWords()
        {
            if (File.Exists(_path + _fileName + ".words") == false)
                return;
            // load words
            byte[] b = File.ReadAllBytes(_path + _fileName + ".words");
            if (b.Length == 0)
                return;
            MemoryStream ms = new MemoryStream(b);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8);
            string s = br.ReadString();
            while (s != "")
            {
                int off = br.ReadInt32();
                _words.Add(s, off);
                try
                {
                    s = br.ReadString();
                }
                catch
                {
                    s = "";
                }
            }
            _log.Debug("Word Count = " + _words.Count);
        }

        private void AddtoIndex(int recnum, string text)
        {
            if (text == "" || text == null)
                return;
            string[] keys;
            //if (_docMode)
            {
                _log.Debug("text size = " + text.Length);
                Dictionary<string, int> wordfreq = GenerateWordFreq(text);
                _log.Debug("word count = " + wordfreq.Count);
                var kk = wordfreq.Keys;
                keys = new string[kk.Count];
                kk.CopyTo(keys, 0);
            }


            foreach (string key in keys)
            {
                if (key == "")
                    continue;

                int bmp;
                if (_words.TryGetValue(key, out bmp))
                {
                    _bitmaps.GetBitmap(bmp).Set(recnum, true);
                }
                else
                {
                    bmp = _bitmaps.GetFreeRecordNumber();
                    _bitmaps.SetDuplicate(bmp, recnum);
                    _words.Add(key, bmp);
                }
            }
        }

        private Dictionary<string, int> GenerateWordFreq(string text)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>(500);

            char[] chars = text.ToCharArray();
            int index = 0;
            int run = -1;
            int count = chars.Length;
            while (index < count)
            {
                char c = chars[index++];
                if (!char.IsLetter(c))
                {
                    if (run != -1)
                    {
                        ParseString(dic, chars, index, run);
                        run = -1;
                    }
                }
                else if (run == -1)
                    run = index - 1;
            }

            if (run != -1)
            {
                ParseString(dic, chars, index, run);
                run = -1;
            }

            return dic;
        }

        private void ParseString(Dictionary<string, int> dic, char[] chars, int end, int start)
        {
            // check if upper lower case mix -> extract words
            int uppers = 0;
            bool found = false;
            for (int i = start; i < end; i++)
            {
                if (char.IsUpper(chars[i]))
                    uppers++;
            }
            // not all uppercase
            if (uppers != end - start - 1)
            {
                int lastUpper = start;

                string word = "";
                for (int i = start + 1; i < end; i++)
                {
                    char c = chars[i];
                    if (char.IsUpper(c))
                    {
                        found = true;
                        word = new string(chars, lastUpper, i - lastUpper).ToLowerInvariant().Trim();
                        AddDictionary(dic, word);
                        lastUpper = i;
                    }
                }
                if (lastUpper > start)
                {
                    string last = new string(chars, lastUpper, end - lastUpper).ToLowerInvariant().Trim();
                    if (word != last)
                        AddDictionary(dic, last);
                }
            }
            if (found == false)
            {
                string s = new string(chars, start, end - start).ToLowerInvariant().Trim();
                AddDictionary(dic, s);
            }
        }

        private void AddDictionary(Dictionary<string, int> dic, string word)
        {
            int l = word.Length;
            if (l > Global.DefaultStringKeySize)
                return;
            if (l < 2)
                return;
            if (char.IsLetterOrDigit(word[l - 1]) == false)
                word = new string(word.ToCharArray(), 0, l - 1);
            if (word.Length < 2)
                return;
            int cc = 0;
            if (dic.TryGetValue(word, out cc))
                dic[word] = ++cc;
            else
                dic.Add(word, 1);
        }

        #endregion

        public void Shutdown()
        {
            Save();
            _docs.Shutdown();
        }

        public void FreeMemory()
        {
            if (_bitmaps != null)
                _bitmaps.FreeMemory();
            if (_docs != null)
                _docs.FreeMemory();
        }
    }
}