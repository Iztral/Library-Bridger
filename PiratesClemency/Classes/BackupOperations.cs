using SpotifyAPI.Web.Models;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PiratesClemency.Classes
{
    public class BackupOperations
    {
        public void WriteToXML(List<LocalTrack> listLocal_, List<FullTrack> listSpotify_)
        {
            if (Directory.Exists("Backup"))
            {
                Directory.Delete("Backup", true);
            }
            Directory.CreateDirectory("Backup");

            XmlSerializer serialiser = new XmlSerializer(typeof(List<LocalTrack>));
            TextWriter FileStream = new StreamWriter("Backup\\list_local.xml");
            serialiser.Serialize(FileStream, listLocal_);
            FileStream.Close();

            serialiser = new XmlSerializer(typeof(List<string>));
            FileStream = new StreamWriter("Backup\\list_spotify.xml");
            serialiser.Serialize(FileStream, WriteFullTrack(listSpotify_));
            FileStream.Close();
        }

        private List<string> WriteFullTrack(List<FullTrack> listSpotify_)
        {
            List<string> trackIds = new List<string>();
            foreach (FullTrack fullTrack in listSpotify_)
            {
                trackIds.Add(fullTrack.Id);
            }
            return trackIds;
        }

        public void ReadFromXML(ref List<LocalTrack> listLocal_, ref List<string> listSpotify_)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<LocalTrack>));
            FileStream fs = new FileStream(@"Backup\\list_local.xml", FileMode.Open);
            listLocal_ = (List<LocalTrack>)serializer.Deserialize(fs);
            fs.Close();

            serializer = new XmlSerializer(typeof(List<string>));
            fs = new FileStream(@"Backup\\list_spotify.xml", FileMode.Open);
            listSpotify_ = (List<string>)serializer.Deserialize(fs);
            fs.Close();
        }
    }
}

