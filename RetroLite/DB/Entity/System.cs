using LiteDB;

namespace RetroLite.DB.Entity
{
    public class System
    {
        [BsonId]
        public string Name { get; set; }
        public string[] ValidExtensions { get; set; }
    }
}