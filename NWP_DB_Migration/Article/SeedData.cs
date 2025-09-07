using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWP_DB_Migration.Article
{
    
    public class AuthorsList
    {
        public int ID { get; set; }
        public string Author { get; set; }

        public List<AuthorsList> GetAuthors()
        {
            List<AuthorsList> authors = new List<AuthorsList>();

            authors.Add(new AuthorsList { ID = 7, Author = "Tristan Nodalo, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 8, Author = "Pavel Polityuk, Reuters" });
            authors.Add(new AuthorsList { ID = 9, Author = "CJ Marquez, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 10, Author = "Daniza Fernandez, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 11, Author = "Mohammad Yunus Yawar and Charlotte Greenfield, Reu..." });
            authors.Add(new AuthorsList { ID = 12, Author = "Reuters" });
            authors.Add(new AuthorsList { ID = 13, Author = "Laurie Chen and Mei Mei Chu, Reuters" });
            authors.Add(new AuthorsList { ID = 14, Author = "Panu Wongcha-um and Panarat Thepgumpanat, Reuters" });
            authors.Add(new AuthorsList { ID = 15, Author = "Jio de Leon" });
            authors.Add(new AuthorsList { ID = 16, Author = "Joshua McElwee, Reuters" });
            authors.Add(new AuthorsList { ID = 17, Author = "Lance Mejico, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 18, Author = "Lois Calderon, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 19, Author = "Eimor Santos, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 20, Author = "Michi Ancheta" });
            authors.Add(new AuthorsList { ID = 21, Author = "Joe Cash, Reuters" });
            authors.Add(new AuthorsList { ID = 22, Author = "Vitalii Hnidyi, Reuters" });
            authors.Add(new AuthorsList { ID = 23, Author = "Laurie Chen, Reuters" });
            authors.Add(new AuthorsList { ID = 24, Author = "Simon Jennings and Shrivathsa Sridhar, Reuters" });
            authors.Add(new AuthorsList { ID = 25, Author = "Dave Graham, Reuters" });
            authors.Add(new AuthorsList { ID = 26, Author = "Jelo Ritzhie Mantaring, NewsWatch Plus" });
            authors.Add(new AuthorsList { ID = 27, Author = "Rishika Sadam, Reuters" });
            authors.Add(new AuthorsList { ID = 28, Author = "Rollo Ross and Danielle Broadway, Reuters" });
            authors.Add(new AuthorsList { ID = 29, Author = "Jelo Mantaring, Daniza Fernandez, and Lance Mejico..." });
            authors.Add(new AuthorsList { ID = 30, Author = "Ben Blanchard, Reuters" });

            return authors;
        }
    }

    public class CategoryList
    {
        public int ID { get; set; }
        public string Category { get; set; }

        public List<CategoryList> GetCategory()
        {
            List<CategoryList> categories = new List<CategoryList>();

            return categories;
        }
    }

}
