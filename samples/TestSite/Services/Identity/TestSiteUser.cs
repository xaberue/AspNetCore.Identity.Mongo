using AspNetCore.Identity.MongoDb.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace SampleSite.Identity
{
    [BsonIgnoreExtraElements]
    public class TestSiteUser : MongoUser
    {
    }
}
