using LinqToDB.Mapping;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/10/2019
 * 
 * ArticleTable
 * 
 */

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