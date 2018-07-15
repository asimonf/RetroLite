using LiteDB;

namespace RetroLite.DB.Entity
{
    public class Core
    {
        [BsonId]
        public string Name { get; set; }
        public string Path { get; set; }
        public string System { get; set; }
    }
}