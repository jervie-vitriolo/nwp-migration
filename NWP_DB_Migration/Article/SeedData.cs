using MySql.Data.MySqlClient;
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

        public int GetAuthors(string username)
        {
            try
            {
                string connStr = "server=nwpstaging-0dea0b440a-wpdbserver.mysql.database.azure.com;user=jdchodieso;database=nwpstaging_0dea0b440a_database;password=gJPcCa2O6yB$jfTm;";
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();

                var sql = $"select ID from wp_users where display_name COLLATE utf8mb4_bin ='{username}';";
                int id = 1332; //Newswatchpluss Staff
                var mycommand = new MySqlCommand(sql, conn);
                MySqlDataReader reader = mycommand.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32("ID");
                }

                conn.Close();

                return id;
            }
            catch (Exception)
            {
                //log the imagesource id if can't be found
                return 0;
            }

        }
    }

    public class CategoryList
    {
        public int ID { get; set; }
        public string Category { get; set; }

        public List<CategoryList> GetCategoryList()
        {
            List<CategoryList> categories = new List<CategoryList>();
            categories.Add(new CategoryList { ID = 1, Category = "Uncategorized" });
            categories.Add(new CategoryList { ID = 2, Category = "news-magazine-x" });
            categories.Add(new CategoryList { ID = 13, Category = "Breaking News" });
            categories.Add(new CategoryList { ID = 14, Category = "Business Insights" });
            categories.Add(new CategoryList { ID = 15, Category = "Entertainment Buzz" });
            categories.Add(new CategoryList { ID = 16, Category = "Fashion Forward" });
            categories.Add(new CategoryList { ID = 17, Category = "Health &amp; Wellness" });
            categories.Add(new CategoryList { ID = 18, Category = "Science Discoveries" });
            categories.Add(new CategoryList { ID = 19, Category = "Sports Highlights" });
            categories.Add(new CategoryList { ID = 20, Category = "Technology Trends" });
            categories.Add(new CategoryList { ID = 21, Category = "Travel Diaries" });
            categories.Add(new CategoryList { ID = 22, Category = "World Politics" });
            categories.Add(new CategoryList { ID = 25, Category = "Audio" });
            categories.Add(new CategoryList { ID = 26, Category = "Gallery" });
            categories.Add(new CategoryList { ID = 27, Category = "Video" });
            categories.Add(new CategoryList { ID = 28, Category = "Global Watch" });
            categories.Add(new CategoryList { ID = 30, Category = "Plus Picks" });
            categories.Add(new CategoryList { ID = 31, Category = "SONA 2025" });
            categories.Add(new CategoryList { ID = 32, Category = "Top Stories" });
            categories.Add(new CategoryList { ID = 33, Category = "Trending Now" });
            categories.Add(new CategoryList { ID = 34, Category = "Featured Stories" });
            categories.Add(new CategoryList { ID = 35, Category = "Sponsored" });
            categories.Add(new CategoryList { ID = 36, Category = "News" });
            categories.Add(new CategoryList { ID = 37, Category = "Petcare" });
            categories.Add(new CategoryList { ID = 38, Category = "Lifestyle" });
            categories.Add(new CategoryList { ID = 39, Category = "Sponsored" });
            categories.Add(new CategoryList { ID = 40, Category = "Entertainment" });
            categories.Add(new CategoryList { ID = 41, Category = "Food" });
            categories.Add(new CategoryList { ID = 46, Category = "Secondary" });
            categories.Add(new CategoryList { ID = 47, Category = "Posts" });
            categories.Add(new CategoryList { ID = 48, Category = "header" });
            categories.Add(new CategoryList { ID = 49, Category = "single-post" });
            categories.Add(new CategoryList { ID = 50, Category = "text" });
            categories.Add(new CategoryList { ID = 51, Category = "after_paragraph" });
            categories.Add(new CategoryList { ID = 52, Category = "sample" });
            categories.Add(new CategoryList { ID = 53, Category = "message" });
            categories.Add(new CategoryList { ID = 54, Category = "php" });
            categories.Add(new CategoryList { ID = 55, Category = "everywhere" });
            categories.Add(new CategoryList { ID = 56, Category = "disable" });
            categories.Add(new CategoryList { ID = 57, Category = "comments" });
            categories.Add(new CategoryList { ID = 58, Category = "html" });
            categories.Add(new CategoryList { ID = 59, Category = "site_wide_header" });
            categories.Add(new CategoryList { ID = 60, Category = "before_post" });
            categories.Add(new CategoryList { ID = 61, Category = "before_paragraph" });
            categories.Add(new CategoryList { ID = 62, Category = "before_excerpt" });
            categories.Add(new CategoryList { ID = 63, Category = "after_post" });
            categories.Add(new CategoryList { ID = 64, Category = "after_content" });
            categories.Add(new CategoryList { ID = 65, Category = "between_posts" });
            categories.Add(new CategoryList { ID = 66, Category = "before_content" });
            categories.Add(new CategoryList { ID = 67, Category = "archive_before_post" });
            categories.Add(new CategoryList { ID = 68, Category = "js" });
            categories.Add(new CategoryList { ID = 69, Category = "css" });
            categories.Add(new CategoryList { ID = 70, Category = "Videos" });
            categories.Add(new CategoryList { ID = 71, Category = "Community" });
            categories.Add(new CategoryList { ID = 72, Category = "Batas et Al" });
            categories.Add(new CategoryList { ID = 73, Category = "The Story of the Filipino" });
            categories.Add(new CategoryList { ID = 74, Category = "Building Bridges" });
            categories.Add(new CategoryList { ID = 75, Category = "One Small Act" });
            categories.Add(new CategoryList { ID = 77, Category = "Culinary Chronicles" });
            categories.Add(new CategoryList { ID = 78, Category = "world" });
            categories.Add(new CategoryList { ID = 79, Category = "business" });
            categories.Add(new CategoryList { ID = 80, Category = "entertainments" });
            categories.Add(new CategoryList { ID = 81, Category = "sports" });
            categories.Add(new CategoryList { ID = 82, Category = "life" });
            categories.Add(new CategoryList { ID = 83, Category = "polistic" });
            categories.Add(new CategoryList { ID = 84, Category = "regional" });
            categories.Add(new CategoryList { ID = 85, Category = "transportation" });
            categories.Add(new CategoryList { ID = 86, Category = "digitalseries" });
            categories.Add(new CategoryList { ID = 87, Category = "thesource" });
            categories.Add(new CategoryList { ID = 88, Category = "metro" });
            categories.Add(new CategoryList { ID = 89, Category = "opinion" });
            categories.Add(new CategoryList { ID = 90, Category = "presidentduterte" });
            categories.Add(new CategoryList { ID = 91, Category = "topstories" });
            categories.Add(new CategoryList { ID = 92, Category = "seagames" });
            categories.Add(new CategoryList { ID = 93, Category = "incoming" });
            categories.Add(new CategoryList { ID = 94, Category = "womens-month" });
            categories.Add(new CategoryList { ID = 95, Category = "theexchange" });
            categories.Add(new CategoryList { ID = 96, Category = "thefilipinovotes2022" });
            categories.Add(new CategoryList { ID = 97, Category = "Pulse" });
            categories.Add(new CategoryList { ID = 98, Category = "Culture" });
            categories.Add(new CategoryList { ID = 99, Category = "Politics" });
            categories.Add(new CategoryList { ID = 100, Category = "Literature" });
            categories.Add(new CategoryList { ID = 101, Category = "Arts" });
            categories.Add(new CategoryList { ID = 102, Category = "Tech" });
            categories.Add(new CategoryList { ID = 103, Category = "business life" });
            categories.Add(new CategoryList { ID = 104, Category = "Rituals" });
            categories.Add(new CategoryList { ID = 105, Category = "Creatives Questionnaire" });
            categories.Add(new CategoryList { ID = 106, Category = "webcomics" });
            categories.Add(new CategoryList { ID = 107, Category = "Health" });
            categories.Add(new CategoryList { ID = 108, Category = "Current Events" });
            categories.Add(new CategoryList { ID = 109, Category = "Education" });
            categories.Add(new CategoryList { ID = 110, Category = "Pets" });
            categories.Add(new CategoryList { ID = 111, Category = "labor" });
            categories.Add(new CategoryList { ID = 112, Category = "Infrastructure" });
            categories.Add(new CategoryList { ID = 113, Category = "Workplace" });
            categories.Add(new CategoryList { ID = 114, Category = "Essay" });
            categories.Add(new CategoryList { ID = 115, Category = "Elections" });
            categories.Add(new CategoryList { ID = 116, Category = "environment" });
            categories.Add(new CategoryList { ID = 117, Category = "Style" });
            categories.Add(new CategoryList { ID = 118, Category = "Fashion" });
            categories.Add(new CategoryList { ID = 119, Category = "Design" });
            categories.Add(new CategoryList { ID = 120, Category = "Retail" });
            categories.Add(new CategoryList { ID = 121, Category = "Beauty" });
            categories.Add(new CategoryList { ID = 122, Category = "Cover" });
            categories.Add(new CategoryList { ID = 123, Category = "Leisure" });
            categories.Add(new CategoryList { ID = 124, Category = "THE GUIDE" });
            categories.Add(new CategoryList { ID = 125, Category = "TRAVEL" });
            categories.Add(new CategoryList { ID = 126, Category = "FITNESS" });
            categories.Add(new CategoryList { ID = 127, Category = "Motoring" });
            categories.Add(new CategoryList { ID = 128, Category = "Life Choices" });
            categories.Add(new CategoryList { ID = 129, Category = "FILM" });
            categories.Add(new CategoryList { ID = 130, Category = "SportsDesk" });
            categories.Add(new CategoryList { ID = 131, Category = "Basketball" });
            categories.Add(new CategoryList { ID = 132, Category = "Volleyball" });
            categories.Add(new CategoryList { ID = 133, Category = "Boxing" });
            categories.Add(new CategoryList { ID = 134, Category = "Music" });
            categories.Add(new CategoryList { ID = 135, Category = "television" });
            categories.Add(new CategoryList { ID = 136, Category = "Podcasts" });
            categories.Add(new CategoryList { ID = 137, Category = "Theater" });
            categories.Add(new CategoryList { ID = 138, Category = "Daily Solutions" });
            
            return categories;
        }
    }
    
    public class NWPCategoryList
    {
        public string ID { get; set; }
        public string Category { get; set; }

        public List<NWPCategoryList> GetCategory()
        {
            List<NWPCategoryList> categories = new List<NWPCategoryList>();
            categories.Add(new NWPCategoryList { ID = "47875775-1e19-4807-a703-c6d00e541860", Category = "News" });
            categories.Add(new NWPCategoryList { ID = "b5c7fd13-b072-434b-a2fc-51f36711d5f1", Category = "Pulse" });
            categories.Add(new NWPCategoryList { ID = "290033c0-cbb5-4422-80c5-83c54c89968a", Category = "Culture" });
            categories.Add(new NWPCategoryList { ID = "41546778-693f-468f-8455-faeabe60685a", Category = "Politics" });
            categories.Add(new NWPCategoryList { ID = "9b1b5d13-8e24-44af-a6a6-6869a10831b5", Category = "Literature" });
            categories.Add(new NWPCategoryList { ID = "185503d8-b7df-4484-8128-8c848922ac55", Category = "Arts" });
            categories.Add(new NWPCategoryList { ID = "7429cdca-48f3-4aa3-bd8e-9ef21f1a41f3", Category = "Tech" });
            categories.Add(new NWPCategoryList { ID = "c4a01294-78eb-45df-8fba-81282986c8ab", Category = "transportation" });
            categories.Add(new NWPCategoryList { ID = "2ecc840b-d56b-4834-ac4c-78896a3b511d", Category = "business-life" });
            categories.Add(new NWPCategoryList { ID = "8bd181df-dd86-4dd1-89f9-63bd9d5ae25f", Category = "business life" });
            categories.Add(new NWPCategoryList { ID = "986786ae-6d97-49f0-a786-0ed36e29134c", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "7a73ec5f-3c1f-49ac-8d1d-1df957c4ae88", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "3b57fb4c-cbb4-42a3-913a-b4560e922b26", Category = "Creatives Questionnaire" });
            categories.Add(new NWPCategoryList { ID = "e8fefcba-4cf8-4d1a-bb34-1d1036e97c38", Category = "webcomics" });
            categories.Add(new NWPCategoryList { ID = "735d724c-6174-4aba-b899-8b979f1f0b3e", Category = "health" });
            categories.Add(new NWPCategoryList { ID = "1f30b305-5844-41f6-8a17-f7412e057357", Category = "Current-Events" });
            categories.Add(new NWPCategoryList { ID = "84dec93d-cf65-4138-9f38-77e58e3c02e2", Category = "Relationships" });
            categories.Add(new NWPCategoryList { ID = "a4a88f6e-271a-449f-98bb-a4b202985ab8", Category = "sports" });
            categories.Add(new NWPCategoryList { ID = "1c569840-3f1f-4603-9967-02b09f6e4813", Category = "Education" });
            categories.Add(new NWPCategoryList { ID = "ddadc30f-89a1-4cef-831b-9d52d1ea0cd1", Category = "Pets" });
            categories.Add(new NWPCategoryList { ID = "fd2db645-e6d0-49a9-93de-21b97cd1af1a", Category = "labor" });
            categories.Add(new NWPCategoryList { ID = "62328bfa-ec97-40c2-ba77-93f8b8be5aac", Category = "Metro" });
            categories.Add(new NWPCategoryList { ID = "b7c497f7-a0f7-4b04-982a-fc1c6ecc7ccb", Category = "Infrastructure" });
            categories.Add(new NWPCategoryList { ID = "636621c4-0e5c-496d-b2e8-5004e1c4aa55", Category = "Workplace" });
            categories.Add(new NWPCategoryList { ID = "3e7f7344-2f35-4c82-b6e7-82a5b52aabd4", Category = "Essay" });
            categories.Add(new NWPCategoryList { ID = "173396f6-101a-4dc5-924e-02fcbaef9cf7", Category = "Elections" });
            categories.Add(new NWPCategoryList { ID = "e3a1ee27-2698-4592-94bc-bdd10d57bc3b", Category = "environment" });
            categories.Add(new NWPCategoryList { ID = "bd8b42bc-e7a1-4b70-9b17-05ffdcafc797", Category = "Style" });
            categories.Add(new NWPCategoryList { ID = "766f5732-6357-4601-a756-32d7e4983557", Category = "fashion" });
            categories.Add(new NWPCategoryList { ID = "9356d6a0-7cc7-4898-af62-46494f60aa48", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "cb51ef7d-1c7c-4770-8f0a-eb2eca05fb9f", Category = "Design" });
            categories.Add(new NWPCategoryList { ID = "eec41a1e-77c2-433b-9435-e580d4245525", Category = "retail" });
            categories.Add(new NWPCategoryList { ID = "ff381269-34c1-419f-a958-9477017d303c", Category = "Retail" });
            categories.Add(new NWPCategoryList { ID = "e1e68e26-39ca-4e76-8d60-4e2e7f04a174", Category = "beauty" });
            categories.Add(new NWPCategoryList { ID = "36504758-2a49-4aa7-9b46-2710aa858ac3", Category = "Cover" });
            categories.Add(new NWPCategoryList { ID = "11b1cfe7-5939-4eaf-9881-e779f11bcfb1", Category = "magnolia-ben" });
            categories.Add(new NWPCategoryList { ID = "650381bc-dc6d-49d3-a5bc-fe8571b56143", Category = "Leisure" });
            categories.Add(new NWPCategoryList { ID = "1e353b02-5707-46f9-9062-4e0c2faf09c4", Category = "THE GUIDE" });
            categories.Add(new NWPCategoryList { ID = "8784d89d-97c1-47ac-accf-883926207c11", Category = "the guide" });
            categories.Add(new NWPCategoryList { ID = "33c3a6ba-9c5d-4432-a222-b48ef88c0d94", Category = "FOOD" });
            categories.Add(new NWPCategoryList { ID = "2e21852d-15a9-4c86-99ef-48455f5737c4", Category = "TRAVEL" });
            categories.Add(new NWPCategoryList { ID = "9684baf3-8449-4657-8a91-579a14f3c194", Category = "FITNESS" });
            categories.Add(new NWPCategoryList { ID = "79ef301a-fa7e-4c95-9876-85787f12a96c", Category = "Motoring" });
            categories.Add(new NWPCategoryList { ID = "d16ac9a0-f718-489b-958f-51947c551e11", Category = "Life Choices" });
            categories.Add(new NWPCategoryList { ID = "fa6915f8-3baf-47f1-9d6e-9ad370ce6ef3", Category = "tech" });
            categories.Add(new NWPCategoryList { ID = "671da441-b480-49a3-a758-74a76ef5e1fe", Category = "Tech" });
            categories.Add(new NWPCategoryList { ID = "525bc4b0-91c9-4fd9-96ae-81874c2ed577", Category = "FILM" });
            categories.Add(new NWPCategoryList { ID = "877ad2d4-e6bd-4452-a5d8-8b30546e1e2e", Category = "ENTERTAINMENT" });
            categories.Add(new NWPCategoryList { ID = "ca346a78-4ae7-4d2b-b15f-82f46485631e", Category = "entertainment" });
            categories.Add(new NWPCategoryList { ID = "e25fbecb-611d-4690-90d6-6b7ff9863a48", Category = "Videos" });
            categories.Add(new NWPCategoryList { ID = "5b845259-ef50-460b-b676-3b56755e1461", Category = "Daily Solutions" });
            categories.Add(new NWPCategoryList { ID = "c75f1201-ff0c-4893-9822-ee81c2328096", Category = "thefilipinovotes2022" });
            categories.Add(new NWPCategoryList { ID = "620f8ff8-89ca-4fc0-87f8-c8467426c354", Category = "Community" });
            categories.Add(new NWPCategoryList { ID = "a36fb4a9-c1d5-4546-8ec9-7a2092e57004", Category = "magnolia-ben" });
            categories.Add(new NWPCategoryList { ID = "b3b3f789-2fc8-49bd-8c51-bc446e3087d9", Category = "News" });
            categories.Add(new NWPCategoryList { ID = "90febd36-3bb8-453a-86aa-322eeb5841f5", Category = "World" });
            categories.Add(new NWPCategoryList { ID = "e44ef96f-d476-4488-a4bd-b475f1a649f1", Category = "Business" });
            categories.Add(new NWPCategoryList { ID = "62aa87a7-964d-4dbe-9ee7-1438ef387b29", Category = "business" });
            categories.Add(new NWPCategoryList { ID = "111cf2e1-ee3e-406d-8e6d-d80924595c9c", Category = "Entertainments" });
            categories.Add(new NWPCategoryList { ID = "c004ecd2-64d5-4c1d-8d0e-49ea9f96758f", Category = "entertainment" });
            categories.Add(new NWPCategoryList { ID = "26cd2aca-e96d-4813-956c-11c0f3a1281f", Category = "SportsDesk" });
            categories.Add(new NWPCategoryList { ID = "a7a876fa-5afc-4057-8139-948712923fe2", Category = "sports" });
            categories.Add(new NWPCategoryList { ID = "c3f12b54-fee8-44a1-83f9-4f23b8f66e35", Category = "Basketball" });
            categories.Add(new NWPCategoryList { ID = "746f2071-3de4-42a9-b5df-1ec643285ccf", Category = "Volleyball" });
            categories.Add(new NWPCategoryList { ID = "90596b89-9dc4-42ba-8cb0-3bcc8ec00a8f", Category = "Boxing" });
            categories.Add(new NWPCategoryList { ID = "999e7475-6bc2-416f-a710-4ca19030ab96", Category = "Life" });
            categories.Add(new NWPCategoryList { ID = "767a923f-14a8-4e1c-8399-65cbe6e7fff1", Category = "Culture" });
            categories.Add(new NWPCategoryList { ID = "bc57ce13-ecf3-424b-9656-5ebd6db13f4d", Category = "Politics" });
            categories.Add(new NWPCategoryList { ID = "f282e085-eb71-478d-8913-26e41e97ff58", Category = "Literature" });
            categories.Add(new NWPCategoryList { ID = "670430cb-4458-4236-9e56-1a973a4097bb", Category = "Arts" });
            categories.Add(new NWPCategoryList { ID = "0d23a08c-5900-4397-82dc-45ef83a7e997", Category = "Tech" });
            categories.Add(new NWPCategoryList { ID = "ee495b03-2ce1-49d5-a05d-01a874fd9860", Category = "transportation" });
            categories.Add(new NWPCategoryList { ID = "908572d3-cb9f-47cd-a8c7-42aa2d5eebf0", Category = "business-life" });
            categories.Add(new NWPCategoryList { ID = "6662cb7b-1f0b-45ed-932f-09fe8629d706", Category = "business life" });
            categories.Add(new NWPCategoryList { ID = "702a1831-ec02-4d9c-90f7-dafd2a58dfe4", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "54fdd4f0-c589-40ec-8690-555b73b795ba", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "23e71a11-b820-4955-b187-b025abf2e012", Category = "Creatives Questionnaire" });
            categories.Add(new NWPCategoryList { ID = "eb60916c-a02f-4ddf-a766-bf099e77d1a0", Category = "Webcomics" });
            categories.Add(new NWPCategoryList { ID = "b9906d93-48af-4314-b060-44c84c842389", Category = "Health" });
            categories.Add(new NWPCategoryList { ID = "fb402e97-bbae-4a4e-8dc1-07a5a21665ce", Category = "Current-Events" });
            categories.Add(new NWPCategoryList { ID = "e17b9514-df49-4627-9b87-1529b192b3f0", Category = "Relationships" });
            categories.Add(new NWPCategoryList { ID = "55ce8f22-cfe7-44bb-aeae-9b69099c13b8", Category = "Sports" });
            categories.Add(new NWPCategoryList { ID = "a1472b70-4862-469f-8422-6459da9ba733", Category = "Education" });
            categories.Add(new NWPCategoryList { ID = "1d7858ce-7763-4c8d-b53c-c9cee4512864", Category = "Pets" });
            categories.Add(new NWPCategoryList { ID = "6a43f267-74be-4517-9854-a87732f2b3c9", Category = "labor" });
            categories.Add(new NWPCategoryList { ID = "2570f730-dc36-4df8-93df-9f3159fc0ccb", Category = "Metro" });
            categories.Add(new NWPCategoryList { ID = "c1f540fc-30ae-44ac-893e-430d9feab853", Category = "Infrastructure" });
            categories.Add(new NWPCategoryList { ID = "140c3b35-ac25-4605-a06b-d7139c073b51", Category = "Workplace" });
            categories.Add(new NWPCategoryList { ID = "7e456e81-6d10-4e32-92e0-69966592b66b", Category = "Essay" });
            categories.Add(new NWPCategoryList { ID = "ff074974-510b-4c9c-a18e-8cdd3fc5bcc3", Category = "Elections" });
            categories.Add(new NWPCategoryList { ID = "6458e142-26b0-4fba-9a64-ad138012e0c9", Category = "environment" });
            categories.Add(new NWPCategoryList { ID = "b295f662-da43-4186-9947-462ee7cc92d1", Category = "Style" });
            categories.Add(new NWPCategoryList { ID = "8f576cb5-dd86-4f5c-a712-46d6a9d5ab95", Category = "fashion" });
            categories.Add(new NWPCategoryList { ID = "589e521b-4363-46c7-9f01-1b91c9a7d16e", Category = "Rituals" });
            categories.Add(new NWPCategoryList { ID = "bf00e680-8020-4f85-954e-e242450f836b", Category = "Design" });
            categories.Add(new NWPCategoryList { ID = "931b80cc-030d-42b8-a489-5f9ce98601b5", Category = "Retail" });
            categories.Add(new NWPCategoryList { ID = "8f0b0169-0114-4232-8661-7ec8167b74dd", Category = "Retail" });
            categories.Add(new NWPCategoryList { ID = "9aa5acf0-6236-43c7-b647-31c92dae8f42", Category = "Beauty" });
            categories.Add(new NWPCategoryList { ID = "73c6293f-1e98-4778-a8f4-ec49f5987896", Category = "Leisure" });
            categories.Add(new NWPCategoryList { ID = "3955dbb4-db2c-4a0c-af65-beeeab1a54b0", Category = "THE GUIDE" });
            categories.Add(new NWPCategoryList { ID = "d68949f9-708d-4907-aa01-49da0ff43d13", Category = "the guide" });
            categories.Add(new NWPCategoryList { ID = "97f8b757-c828-4d93-bb30-40d38e238fb5", Category = "FOOD" });
            categories.Add(new NWPCategoryList { ID = "6a5ff05c-9832-4001-b84c-5130bf6e5691", Category = "TRAVEL" });
            categories.Add(new NWPCategoryList { ID = "ba48e5fb-1101-4be1-b08a-797c5a3bde81", Category = "FITNESS" });
            categories.Add(new NWPCategoryList { ID = "27ea7883-00c3-4a65-98ba-6edba636d2af", Category = "Motoring" });
            categories.Add(new NWPCategoryList { ID = "1761994c-aaa7-4fa6-ab05-64062f9c56ab", Category = "Life Choices" });
            categories.Add(new NWPCategoryList { ID = "16163b1b-7fb4-4a92-bfe3-fffac206030c", Category = "Tech" });
            categories.Add(new NWPCategoryList { ID = "ecfda233-3517-4749-968c-c742aa72383f", Category = "Entertainment" });
            categories.Add(new NWPCategoryList { ID = "dd8a9fff-dce1-406e-ba00-e08f2c961a57", Category = "MUSIC" });
            categories.Add(new NWPCategoryList { ID = "e227df04-80ed-48e4-bcf3-45e13532960e", Category = "FILM" });
            categories.Add(new NWPCategoryList { ID = "6bea8794-3217-41e7-8ff3-8446e3583f3e", Category = "Film" });
            categories.Add(new NWPCategoryList { ID = "153741d1-3eb4-4f2a-963c-82d142477e63", Category = "television" });
            categories.Add(new NWPCategoryList { ID = "bba5720d-74c9-4700-b967-1c9a103984f7", Category = "Podcasts" });
            categories.Add(new NWPCategoryList { ID = "315f78d0-63cc-4d10-a905-30be27315622", Category = "Theater" });
            categories.Add(new NWPCategoryList { ID = "ef06beb1-3dea-4b28-a051-32c60315f8f1", Category = "polistic" });
            categories.Add(new NWPCategoryList { ID = "d9d3a5d0-bad7-4ec1-a2ac-62415dfefa64", Category = "Regional" });
            categories.Add(new NWPCategoryList { ID = "d79cec3f-3d3d-43b4-ba13-9570763c6111", Category = "Videos" });
            categories.Add(new NWPCategoryList { ID = "9f881b11-f250-41ce-9ad5-334f860f07c0", Category = "transportation" });
            categories.Add(new NWPCategoryList { ID = "d3c185ff-d11a-4813-b2bc-ca8392c1c427", Category = "Digital Series" });
            categories.Add(new NWPCategoryList { ID = "1033ef4e-3828-4911-abeb-0c44153b925b", Category = "The Source" });
            categories.Add(new NWPCategoryList { ID = "713c7564-edbc-472e-b026-9dea8bded8cc", Category = "Metro" });
            categories.Add(new NWPCategoryList { ID = "9c97e21f-a76f-46f3-bc77-d7074b7893bf", Category = "Lifestyle" });
            categories.Add(new NWPCategoryList { ID = "9a63ac44-4e24-4e3b-b2d8-1e9ec61398ae", Category = "Opinion" });
            categories.Add(new NWPCategoryList { ID = "b8a94b95-5970-434f-91e3-bcc814f844e0", Category = "opinion" });
            categories.Add(new NWPCategoryList { ID = "3b628815-8eb3-4b30-8f42-37bb64533b71", Category = "President Duterte" });
            categories.Add(new NWPCategoryList { ID = "ed9efd6a-3cd9-43cf-b259-28a22237b7e8", Category = "topstories" });
            categories.Add(new NWPCategoryList { ID = "61057f2a-d0fc-4d40-a84a-1a9ed2a87e04", Category = "Sea Games" });
            categories.Add(new NWPCategoryList { ID = "b4e2c86c-aa62-48e1-b5f8-5247c19d9897", Category = "Incoming Videos" });
            categories.Add(new NWPCategoryList { ID = "f01c6cd4-f3df-4bcb-afef-d4703eafa7f5", Category = "incoming" });
            categories.Add(new NWPCategoryList { ID = "64caaab1-730f-4abd-a067-c8d3210275f8", Category = "womens-month" });
            categories.Add(new NWPCategoryList { ID = "c004be9d-0bf7-475d-a9c6-313eb4c6e4a3", Category = "The Exchange" });
            categories.Add(new NWPCategoryList { ID = "fad1bf5d-01c2-40b9-9113-5e48d86cb278", Category = "The Exchange Videos" });
            categories.Add(new NWPCategoryList { ID = "fa7bd7ec-33f8-4679-ad62-bea5fb9dfacb", Category = "The Filipino Votes" });
            categories.Add(new NWPCategoryList { ID = "c0b7daad-dd62-4955-adee-420ee08cf155", Category = "thefilipinovotes2022" });


            return categories;
        }
    }

    
}
