using System.Runtime.Serialization;

namespace PrivateGalleryCreator
{
    [DataContract]
    public class ExtensionList
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "extensions")]
        public Extension[] Extensions { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract]
    public class Extension
    {
        [DataMember(Name = "vsixId")]
        public string VsixId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
