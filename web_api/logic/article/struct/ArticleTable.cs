using LinqToDB.Mapping;

namespace WebApi.Logic.Article.Struct
{
    [Table(Name = "article")]
    public class ArticleTable
    {
        [Column(Name = "id"), PrimaryKey, NotNull]
        public uint Id { get; set; }

        //in seconds
        [Column(Name = "time"), NotNull]
        public long Time { get; set; }

        [Column(Name = "info"), NotNull]
        public string Info { get; set; }
    }
}