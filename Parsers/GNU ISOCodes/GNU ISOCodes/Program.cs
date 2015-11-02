using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace GNU_ISOCodes
{
    class Program
    {
        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }

        public class Country
        {
            public string Code { get; set; }
            public string Alpha2 { get; set; }
            public string Alpha3 { get; set; }
            public string Name { get; set; }
            public string CommonName { get; set; }
            public string OfficialName { get; set; }
        }

        static void Main(string[] args)
        {

            string isocodesPath = @"";
            string countryFile = isocodesPath + @"\iso_3166\iso_3166.xml";

            XDocument doc = XDocument.Load(countryFile);

            var entries =
                from x in (
                    from y in doc.Descendants("iso_3166_entries").Descendants("iso_3166_entry")
                    select new Country
                    {
                        Code = y.Attribute("numeric_code").Value,
                        Alpha2 = y.Attribute("alpha_2_code").Value,
                        Alpha3 = y.Attribute("alpha_3_code").Value,
                        Name = y.Attribute("name").Value,
                        CommonName = y.Attribute("common_name") == null ? String.Empty : y.Attribute("common_name").Value,
                        OfficialName = y.Attribute("official_name") == null ? String.Empty : y.Attribute("official_name").Value
                    }
                )
                orderby x.Code
                select x;

            List<Country> Countries = new List<Country>();
            Countries.AddRange(entries.ToList());


        }
    }
}
