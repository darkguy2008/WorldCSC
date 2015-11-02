using Qube.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GNU_ISOCodes
{
    #region XML Output
    [XmlRoot("Countries")]
    public class Countries
    {
        [XmlElement("Country")]
        public List<Country> Items { get; set; }
        public Countries()
        {
            Items = new List<Country>();
        }
    }

    public class Country
    {
        [XmlAttribute]
        public string Code { get; set; }
        [XmlAttribute]
        public string Alpha2 { get; set; }
        [XmlAttribute]
        public string Alpha3 { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string CommonName { get; set; }
        [XmlAttribute]
        public string OfficialName { get; set; }
    }

    [XmlRoot("States")]
    public class States
    {
        [XmlElement("State")]
        public List<State> Items { get; set; }
        public States()
        {
            Items = new List<State>();
        }
    }

    public class State
    {
        [XmlAttribute]
        public string CountryCode { get; set; }
        [XmlAttribute]
        public string Code { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Parent { get; set; }
    }
    #endregion

    public class Application
    {

        public void Main(string[] args)
        {
            // PUT HERE YOUR iso-codes-3.xx FOLDER PATH
            string path = @"";
            ProcessCountries(path);
        }

        public void ProcessCountries(string isocodesPath)
        {
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

            Countries c = new Countries();
            c.Items.AddRange(entries.ToList());

            string xmlCountries = Xml.Serialize(c);
            using (StreamWriter sw = new StreamWriter(Program.AppPath + "Countries.xml"))
                sw.Write(xmlCountries);

            FileInfo[] files = new DirectoryInfo(isocodesPath + @"\iso_3166").GetFiles("*.po", SearchOption.TopDirectoryOnly);
            foreach (FileInfo poFile in files)
            {
                string poName = poFile.Name.Substring(0, poFile.Name.IndexOf(poFile.Extension));

                Console.Write("Translating country list into " + poName.ToUpper() + "... ");
                Countries tc = new Countries();

                string[] lines = File.ReadAllLines(poFile.FullName);
                Dictionary<string, string> trans = new Dictionary<string, string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.StartsWith("msgid"))
                    {
                        string str = lines[i + 1];
                        string oldStr = line.Substring("msgid \"".Length);
                        oldStr = oldStr.Substring(0, oldStr.LastIndexOf("\""));
                        string newStr = str.Substring("msgstr \"".Length);
                        newStr = newStr.Substring(0, newStr.LastIndexOf("\""));
                        trans[oldStr] = newStr;
                    }
                }

                foreach (Country cnt in c.Items)
                {
                    bool add = false;
                    Country c2 = new Country();
                    cnt.CopyObject(c2);
                    foreach (var kvp in trans)
                    {
                        if (c2.Name == kvp.Key) { c2.Name = kvp.Value; add = true; }
                        if (c2.CommonName == kvp.Key) { c2.CommonName = kvp.Value; add = true; }
                        if (c2.OfficialName == kvp.Key) { c2.OfficialName = kvp.Value; add = true; }
                    }
                    if (add)
                        tc.Items.Add(c2);
                }

                string translation = Xml.Serialize(tc);

                using (StreamWriter sw = new StreamWriter(Program.AppPath + "Countries." + poName + ".xml"))
                    sw.Write(translation);

                Console.WriteLine("Translated.");
            }

            ProcessStates(c, isocodesPath);
        }

        public void ProcessStates(Countries countryList, string isocodesPath)
        {
            string stateFile = isocodesPath + @"\iso_3166_2\iso_3166_2.xml";

            XDocument doc = XDocument.Load(stateFile);

            var countries = from x in doc.Descendants("iso_3166_2_entries").Descendants("iso_3166_country")
                            select x;

            States st = new States();

            foreach(var xmlCountry in countries)
            {
                Country c = countryList.Items.Where(x => x.Alpha2 == xmlCountry.Attribute("code").Value).SingleOrDefault();
                if(c != null)
                {
                    foreach(var xmlSubset in xmlCountry.Descendants("iso_3166_subset"))
                    {
                        string type = xmlSubset.Attribute("type").Value;
                        foreach (var xmlEntry in xmlSubset.Descendants("iso_3166_2_entry"))
                            st.Items.Add(new State()
                            {
                                Type = type,
                                CountryCode = c.Code,
                                Code = xmlEntry.Attribute("code").Value,
                                Name = xmlEntry.Attribute("name").Value,
                                Parent = xmlEntry.Attribute("parent") == null ? String.Empty : xmlEntry.Attribute("parent").Value
                            });
                    }
                }
            }

            st.Items = st.Items.OrderBy(x => x.CountryCode).ToList();

            string xmlStates = Xml.Serialize(st);
            using (StreamWriter sw = new StreamWriter(Program.AppPath + "States.xml"))
                sw.Write(xmlStates);

            // TODO: Somehow, the translations are WRONG for the Spanish language,
            // sometimes, some state strings are set as "Phone:" for instance, and there's
            // obviously no state named "Phone:". I've left the States.xml file with
            // its default values for all other translations until this is fixed or
            // there is another alternative for the States.xml translated values.

            FileInfo[] files = new DirectoryInfo(isocodesPath + @"\iso_3166_2").GetFiles("*.po", SearchOption.TopDirectoryOnly);
            foreach (FileInfo poFile in files)
            {
                string poName = poFile.Name.Substring(0, poFile.Name.IndexOf(poFile.Extension));

                Console.Write("Translating state list into " + poName.ToUpper() + "... ");
                States ts = new States();

                string[] lines = File.ReadAllLines(poFile.FullName);
                Dictionary<string, string> trans = new Dictionary<string, string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.StartsWith("#. name for"))
                    {
                        string[] scodes = line.Split(',');
                        for (int x = 0; x < scodes.Length; x++)
                        {
                            scodes[x] = scodes[x].Substring(scodes[x].IndexOf("for ") + "for ".Length).Trim();
                            State s = st.Items.Where(z => z.Code == scodes[x]).SingleOrDefault();
                            if (s != null)
                            {
                                string str = lines[i + 1];
                                if (str.StartsWith("#"))
                                    str = lines[i + 2];
                                string newStr = str.Substring("msgid \"".Length);
                                newStr = newStr.Substring(0, newStr.LastIndexOf("\""));
                                trans[s.Code] = newStr;
                            }
                        }
                    }
                }

                foreach (State s in st.Items)
                {
                    bool add = false;
                    State s2 = new State();
                    s.CopyObject(s2);
                    foreach (var kvp in trans)
                        if (s2.Code == kvp.Key) { s2.Name = kvp.Value; add = true; }
                    if (add)
                        ts.Items.Add(s2);
                }

                string translation = Xml.Serialize(ts);

                using (StreamWriter sw = new StreamWriter(Program.AppPath + "States." + poName + ".xml"))
                    sw.Write(translation);

                Console.WriteLine("Translated.");
            }
        }
    }

}
