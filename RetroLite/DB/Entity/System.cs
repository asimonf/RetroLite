using LiteDB;

namespace RetroLite.DB.Entity
{
    public class System
    {
        [BsonId]
        public string Name { get; set; }
    }
}