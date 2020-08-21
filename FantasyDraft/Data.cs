using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FantasyDraft
{
    class Data
    {
        public static List<string> LoadCSVFile(string filePath)
        {
            var reader = new StreamReader(File.OpenRead(filePath));
            List<string> searchList = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                searchList.Add(line);
            }

            return searchList;
        }
    }
}
