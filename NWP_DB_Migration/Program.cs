using MySql.Data.MySqlClient;
using NWP_DB_Migration.Article;
using System;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static void Main(string[] args)
    {
        List<string> WP_Post_Article_InsertSql_list;
        List<string> WP_PostMeta_list;
        List<string> WP_term_relationships_list;
        int ErrorCount = 0; 
        string[] directories = { @"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\8",
                                
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\6",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\7",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\8",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\9",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\10",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\11",
                                //@"C:\Users\jervi\Documents\nwp-data\Articles\nwp\2024\12"
                                 };

        //PREP: clean up space after period in multiline tags,
        //AddStartHereAndCleanTags();

        //Clean up article
        //CleanUpArticle();

        // SQL
        GenerateInsertSql();


        //Generate classes - ALL DONE
        //GeneratePostClass();

        //Authors - ALL DONE
        //ProcessAuthors();

        //Extract fetured image

        //ExtractFeaturedImage();


        void AddStartHereAndCleanTags()
        {

            // Loop through each directory
            foreach (string directory in directories)
            {
                Console.WriteLine(directory);
                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    Console.WriteLine($"Found file: {filePath}");
                    AppendSingleQoute(filePath);

                    string[] matchingLines = File.ReadAllLines(filePath);

                    //add START HERE----->
                    for (int i = 0; i < matchingLines.Count(); i++)
                    {
                        if (!matchingLines[i].Contains("'  'mgnl:")
                            && !matchingLines[i].Contains("    'author")
                            && !matchingLines[i].Contains("    'categories")
                            && !matchingLines[i].Contains("    'created")
                            && !matchingLines[i].Contains("    'imagesource")
                            && !matchingLines[i].Contains("    'jcr")
                            && !matchingLines[i].Contains("    'lead")
                            && !matchingLines[i].Contains("    'stories")
                            && !matchingLines[i].Contains("    'template")
                            && !matchingLines[i].Contains("    'title")
                            && !matchingLines[i].Contains("    'visualType")
                            && !matchingLines[i].Contains("'jcr:")
                            && !matchingLines[i].Contains("  'mgnl")
                            )
                        {
                            matchingLines[i] = matchingLines[i].Replace("'  '", "'  'START HERE----->");
                        }

                        //Tags
                        if (matchingLines[i].Contains("'mgnl:tags': ["))
                        {
                            matchingLines[i] = "    'mgnl:tags': []";
                        }

                    }

                    File.WriteAllLines(filePath, matchingLines);
                    RemoveSingleQoute(filePath);
                }
            }
            Console.WriteLine($"Completed");
        }

        void AppendSingleQoute(string filePath)
        {
            string[] matchingLines = File.ReadAllLines(filePath);

            for (int i = 0; i < matchingLines.Count(); i++)
            {
                //if (matchingLines[i].Equals(""))
                //{
                //    continue;
                //}
                matchingLines[i] = $"'{matchingLines[i]}";
            }

            File.WriteAllLines(filePath, matchingLines);
        }

        void RemoveSingleQoute(string filePath)
        {
            string matchingLines = File.ReadAllText(filePath);

            matchingLines = matchingLines.Replace("''", "'");
            matchingLines = matchingLines.Replace("'  '", "  '");
            matchingLines = matchingLines.Replace("'    '", "    '");
            matchingLines = matchingLines.Replace("'      '", "      '");
            matchingLines = matchingLines.Replace("'        ", "        ");
            matchingLines = matchingLines.Replace("'      ", "      ");

            File.WriteAllText(filePath, matchingLines);
        }

        void GenerateInsertSql()
        {
            WP_Post_Article_InsertSql_list = new List<string>();
            WP_PostMeta_list = new List<string>();
            WP_term_relationships_list = new List<string>();

            // Loop through each directory
            foreach (string directory in directories)
            {
                Console.WriteLine(directory);
                int PostID = 4955;
                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    Console.WriteLine($"Found file: {filePath}");

                    var matchingLines = File.ReadLines(filePath)
                                        .Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                        .Where(item => item.LineText.Contains("START HERE----->"));

                    int[] lineNumbers = matchingLines.Select(x => x.LineNumber).ToArray();


                    for (int i = 0; i < lineNumbers.Length; i++)
                    {
                        try
                        {
                            int startLine = lineNumbers[i] + 1;
                            int endLine = 0;

                            if (i == lineNumbers.Length - 1)
                            {
                                endLine = File.ReadAllLines(filePath).Length;
                            }
                            else
                            {
                                endLine = lineNumbers[i + 1] - 1;
                            }

                            List<string> articles = GetArticles(filePath, startLine, endLine);
                            string yml = string.Join("\n", articles);

                            //start parsing
                            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                                .Build();


                            //yml contains a string containing your YAML
                            //Replace aphostrophe with ‘
                            string CleanYml = CleanApostrophe(yml);
                             CleanYml = CleanStories(CleanYml);

                            var p = deserializer.Deserialize<Post>(CleanYml);
                            if (p != null)
                            {
                                CreatePostInsertSql(p, PostID);
                                PostID++;
                            }
                        }
                        catch (Exception ex) 
                        {
                            ErrorCount++;
                            Console.WriteLine($"Error at line {lineNumbers[i]} - {ex}");
                            continue;
                        }

                    }

                    
                }
                Console.WriteLine($"Total Error {ErrorCount}");
                //LogCreatedPostInsertSql(directory);
            }

            Console.WriteLine($"Completed");
        }

        int GetPostMetaValue(string imagesource)
        {
            return 0;

            try
            {
                string connStr = "server=nwpstaging-0dea0b440a-wpdbserver.mysql.database.azure.com;user=jdchodieso;database=nwpstaging_0dea0b440a_database;password=gJPcCa2O6yB$jfTm;";
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();

                var sql = $"select ID from wp_posts where post_title='{imagesource}';";
                int id = 0;
                var wp_post = new MySqlCommand(sql, conn);
                MySqlDataReader reader = wp_post.ExecuteReader();

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

        void GeneratePostClass()
        {
            string section = string.Empty;
            string ClassInit = string.Empty;
            string XMas = string.Empty;


            for (int i = 0; i < 600; i++)
            {
                section = section + $"internal class section{i} {{ \n" +
               "public string primarytype { get; set; } \n" +
               "public string uuid { get; set; } \n" +
               "public string activationstatus { get; set; } \n" +
               "public DateTime created { get; set; } \n" +
               "public string createdby { get; set; } \n" +
               "public DateTime lastactivated { get; set; } \n" +
               "public string lastactivatedby { get; set; } \n" +
               "public DateTime lastmodified { get; set; } \n" +
               "public string lastmodifiedby { get; set; } \n" +
               "public string type { get; set; } \n" +
               "public string text { get; set; } \n" +
               "public string image { get; set; } \n" +
               "public string contentwidth { get; set; } \n" +
               "public string imagecaption { get; set; } \n" +
               "public string embed { get; set; } \n" +
               "public string scale { get; set; } \n" +
               "public string imagecredit { get; set; } \n" +
               "public string imagealttext { get; set; } \n" +
               "public string url { get; set; } \n" +
               "public string videocredit { get; set; } \n" +
               "public string imagelist { get; set; } \n" +
               "public string authorimage { get; set; } \n" +
               "public string embedcode { get; set; } }\n\n\n";

                
                ClassInit = ClassInit + $"public section{i} section{i} = new section{i}();";

                XMas = XMas + $"finalString = finalString + CleanChecks(getBlock(post.section{i}.type, post.section{i}.embedcode,post.section{i}.text,post.section{i}.url));\n";
            }

            string ClassDeclaration = $"namespace NWP_DB_Migration.Article\r\n {{ {section}  }}";

        }

        void CleanUpArticle()
        {
            // Loop through each directory
            foreach (string directory in directories)
            {
                Console.WriteLine(directory);

                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    Console.WriteLine($"Clean up Section");
                    string matchingLines = File.ReadAllText(filePath);

                    //section
                    for (int i = 0; i < 582; i++)
                    {
                        matchingLines = matchingLines.Replace($"'{i}':", $"section{i}:");
                    }

                    //small caps
                    matchingLines = matchingLines.Replace("mixinTypes", "mixintypes");
                    matchingLines = matchingLines.Replace("primaryType", "primarytype");
                    matchingLines = matchingLines.Replace("createdBy", "createdby");
                    matchingLines = matchingLines.Replace("lastActivated", "lastactivated");
                    matchingLines = matchingLines.Replace("lastActivatedBy", "lastactivatedby");
                    matchingLines = matchingLines.Replace("lastModified", "lastmodified':");
                    matchingLines = matchingLines.Replace("lastModifiedBy", "lastmodifiedby");
                    matchingLines = matchingLines.Replace("visualType", "visualtype");
                    matchingLines = matchingLines.Replace("activationStatus", "activationstatus");
                    matchingLines = matchingLines.Replace("contentWidth", "contentwidth");
                    matchingLines = matchingLines.Replace("embedCode", "embedcode");
                    matchingLines = matchingLines.Replace("imageCaption", "imagecaption");
                    matchingLines = matchingLines.Replace("authorImage", "authorimage");
                    matchingLines = matchingLines.Replace("imageCaption", "imagecaption");
                    matchingLines = matchingLines.Replace("imageCredit", "imagecredit");
                    matchingLines = matchingLines.Replace("imageList", "imagelist");
                    
                    //clean
                    matchingLines = matchingLines.Replace("'jcr:", "'");
                    matchingLines = matchingLines.Replace("'mgnl:", "'");
                    matchingLines = matchingLines.Replace("[]", "''");
                    matchingLines = matchingLines.Replace(": ['", ": '");
                    matchingLines = matchingLines.Replace("']", "'");
                    matchingLines = matchingLines.Replace("  '", "  ");
                    matchingLines = matchingLines.Replace("': '", ": '");
                    matchingLines = matchingLines.Replace("': ", ": ");
                    matchingLines = matchingLines.Replace("START HERE----->", "'START HERE----->");

                    matchingLines = matchingLines.Replace("lastactivatedBy", "lastactivatedby");
                    matchingLines = matchingLines.Replace("lastactivatedVersionCreated", "lastactivatedversioncreated");
                    matchingLines = matchingLines.Replace("lastactivatedVersion", "lastactivatedversion");
                    

                    File.WriteAllText(filePath, matchingLines);
                }
            }
        }

        void CreatePostInsertSql(Post post, int PostID)
        {
            string WP_Post_Article_InsertSql = $"INSERT INTO `wp_posts` ( `ID`,`post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) " +
                                  $"VALUES({PostID} ,'{getPostAuthorID(post.author)}', '{formatDateTime(post.created)}', '{formatDateTime(post.created)}', '{getPostContent(post)}', '{mysqlStringFormat(post.title)}', '{mysqlStringFormat(post.caption)}', '{getPostStatus(post)}', 'open', 'open', '', '{mysqlStringFormat(post.title).Replace(" ", "-")}', '', '', '{formatDateTime(post.lastmodified)}', '{formatDateTime(post.lastmodified)}', '', 0, 'https://newswatchplus-staging.azurewebsites.net/?p=', 0, 'post', '', 0);";

            string WP_PostMeta = $"INSERT INTO `wp_postmeta` ( `post_id`, `meta_key`, `meta_value`) VALUES( {PostID}, '_thumbnail_id', '{GetPostMetaValue(post.imagesource)}');";

            string WP_term_relationships = $"INSERT INTO wp_term_relationships(OBJECT_ID,TERM_TAXONOMY_ID,TERM_ORDER) VALUES({PostID},{getCategoryId(post.categories)},0);";

            WP_Post_Article_InsertSql_list.Add(WP_Post_Article_InsertSql);
            WP_PostMeta_list.Add(WP_PostMeta);
            WP_term_relationships_list.Add(WP_term_relationships);

            SaveDataToDatabase(WP_Post_Article_InsertSql, WP_PostMeta, WP_term_relationships);
        }

        void LogCreatedPostInsertSql(string directory)
        {
            var tempArry = directory.Split("\\");
            var subpath = $@"{tempArry[tempArry.Length - 2]}\{tempArry[tempArry.Length - 1]}";
            var dirDestination = $@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Articles\nwp - in progress\{subpath}";

            if (!Directory.Exists(dirDestination))
            {
                Directory.CreateDirectory(dirDestination);
            }

            if (WP_Post_Article_InsertSql_list.Count > 0)
            {
                string filePath = $"{dirDestination}\\WP_Post_Article_InsertSql_list.sql";
                using (StreamWriter sw = File.AppendText(filePath)) // Opens the file in append mode
                {

                    foreach (var sql in WP_Post_Article_InsertSql_list)
                    {
                        sw.WriteLine(sql);
                    }
                }
            }

            if (WP_PostMeta_list.Count > 0)
            {
                string filePath = $"{dirDestination}\\WP_PostMeta_list.sql";
                using (StreamWriter sw = File.AppendText(filePath)) // Opens the file in append mode
                {

                    foreach (var sql in WP_PostMeta_list)
                    {
                        sw.WriteLine(sql);
                    }
                }
            }
            if (WP_term_relationships_list.Count > 0)
            {
                string filePath = $"{dirDestination}\\WP_term_relationships_list.sql";
                using (StreamWriter sw = File.AppendText(filePath)) // Opens the file in append mode
                {

                    foreach (var sql in WP_term_relationships_list)
                    {
                        sw.WriteLine(sql);
                    }
                }
            }
        }


        int getCategoryId(string categoryId)
        {
            if (categoryId == null)
            {
                return 1;
            }
            else
            {
                NWPCategoryList NWPCategoryList = new NWPCategoryList();
                var nwpCategory = NWPCategoryList.GetCategory().FirstOrDefault(s => s.ID.ToUpper().Trim().Equals(categoryId.ToUpper().Trim()));

                if (nwpCategory != null)
                {
                    CategoryList CategoryList = new CategoryList();
                    int ID = CategoryList.GetCategoryList().FirstOrDefault(s => s.Category.ToUpper().Trim().Equals(nwpCategory.Category.ToUpper().Trim())).ID;

                    if (ID != 0)
                    {
                        return ID;
                    }
                }

                return 36;
            }
        }

        string mysqlStringFormat(string text)
        {
            if (text != null)
            {
                return Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");
            }

            return string.Empty;
        }



        string formatDateTime(DateTime created)
        {
            return created.ToString("yyyy-MM-dd HH:mm:ss");
        }

        string getPostStatus(Post post)
        {
            if (post.activationstatus)
            {
                return "publish";
            }

            return "draft";
        }

        int getPostAuthorID(string name)
        {
            if (name == null || name==string.Empty)
            {
                return 1332; //Newswatch plus
            }
            else
            {
                AuthorsList AuthorsList = new AuthorsList();
                var ID = AuthorsList.GetAuthors(name);
                return ID;
            }

        }

        string getBlock(string type, string embedcode,string text, string url)
        {
            //PBA Season 49 Philippine Cup: San Miguel vs Meralco
            //imageGallery,embedCode, text
            //post.section0.type == "embedCode"

            switch (type)
            {
                case "text":
                    return text;
                case "embedcode":
                    return embedcode;
                case "externalLink":
                    return url;
                default:
                   
                    return string.Empty;
            }

        }

        #region getPostContent


        string getPostContent(Post post)
        {
            string finalString = string.Empty;

            try
            {
                if (post.visualtype == "video" && post.embedsource.Contains("https://www.youtube.com/embed"))
                {
                    finalString = post.embedsource.Replace("width=\"560\"", "width=\"800\"").Replace("height=\"315\"", "height=\"500\"");
                }



                finalString = finalString + CleanChecks(getBlock(post.section0.type, post.section0.embedcode, post.section0.text, post.section0.url));
                finalString = finalString + CleanChecks(getBlock(post.section1.type, post.section1.embedcode, post.section1.text, post.section1.url));
                finalString = finalString + CleanChecks(getBlock(post.section2.type, post.section2.embedcode, post.section2.text, post.section2.url));
                finalString = finalString + CleanChecks(getBlock(post.section3.type, post.section3.embedcode, post.section3.text, post.section3.url));
                finalString = finalString + CleanChecks(getBlock(post.section4.type, post.section4.embedcode, post.section4.text, post.section4.url));
                finalString = finalString + CleanChecks(getBlock(post.section5.type, post.section5.embedcode, post.section5.text, post.section5.url));
                finalString = finalString + CleanChecks(getBlock(post.section6.type, post.section6.embedcode, post.section6.text, post.section6.url));
                finalString = finalString + CleanChecks(getBlock(post.section7.type, post.section7.embedcode, post.section7.text, post.section7.url));
                finalString = finalString + CleanChecks(getBlock(post.section8.type, post.section8.embedcode, post.section8.text, post.section8.url));
                finalString = finalString + CleanChecks(getBlock(post.section9.type, post.section9.embedcode, post.section9.text, post.section9.url));
                finalString = finalString + CleanChecks(getBlock(post.section10.type, post.section10.embedcode, post.section10.text, post.section10.url));
                finalString = finalString + CleanChecks(getBlock(post.section11.type, post.section11.embedcode, post.section11.text, post.section11.url));
                finalString = finalString + CleanChecks(getBlock(post.section12.type, post.section12.embedcode, post.section12.text, post.section12.url));
                finalString = finalString + CleanChecks(getBlock(post.section13.type, post.section13.embedcode, post.section13.text, post.section13.url));
                finalString = finalString + CleanChecks(getBlock(post.section14.type, post.section14.embedcode, post.section14.text, post.section14.url));
                finalString = finalString + CleanChecks(getBlock(post.section15.type, post.section15.embedcode, post.section15.text, post.section15.url));
                finalString = finalString + CleanChecks(getBlock(post.section16.type, post.section16.embedcode, post.section16.text, post.section16.url));
                finalString = finalString + CleanChecks(getBlock(post.section17.type, post.section17.embedcode, post.section17.text, post.section17.url));
                finalString = finalString + CleanChecks(getBlock(post.section18.type, post.section18.embedcode, post.section18.text, post.section18.url));
                finalString = finalString + CleanChecks(getBlock(post.section19.type, post.section19.embedcode, post.section19.text, post.section19.url));
                finalString = finalString + CleanChecks(getBlock(post.section20.type, post.section20.embedcode, post.section20.text, post.section20.url));
                finalString = finalString + CleanChecks(getBlock(post.section21.type, post.section21.embedcode, post.section21.text, post.section21.url));
                finalString = finalString + CleanChecks(getBlock(post.section22.type, post.section22.embedcode, post.section22.text, post.section22.url));
                finalString = finalString + CleanChecks(getBlock(post.section23.type, post.section23.embedcode, post.section23.text, post.section23.url));
                finalString = finalString + CleanChecks(getBlock(post.section24.type, post.section24.embedcode, post.section24.text, post.section24.url));
                finalString = finalString + CleanChecks(getBlock(post.section25.type, post.section25.embedcode, post.section25.text, post.section25.url));
                finalString = finalString + CleanChecks(getBlock(post.section26.type, post.section26.embedcode, post.section26.text, post.section26.url));
                finalString = finalString + CleanChecks(getBlock(post.section27.type, post.section27.embedcode, post.section27.text, post.section27.url));
                finalString = finalString + CleanChecks(getBlock(post.section28.type, post.section28.embedcode, post.section28.text, post.section28.url));
                finalString = finalString + CleanChecks(getBlock(post.section29.type, post.section29.embedcode, post.section29.text, post.section29.url));
                finalString = finalString + CleanChecks(getBlock(post.section30.type, post.section30.embedcode, post.section30.text, post.section30.url));
                finalString = finalString + CleanChecks(getBlock(post.section31.type, post.section31.embedcode, post.section31.text, post.section31.url));
                finalString = finalString + CleanChecks(getBlock(post.section32.type, post.section32.embedcode, post.section32.text, post.section32.url));
                finalString = finalString + CleanChecks(getBlock(post.section33.type, post.section33.embedcode, post.section33.text, post.section33.url));
                finalString = finalString + CleanChecks(getBlock(post.section34.type, post.section34.embedcode, post.section34.text, post.section34.url));
                finalString = finalString + CleanChecks(getBlock(post.section35.type, post.section35.embedcode, post.section35.text, post.section35.url));
                finalString = finalString + CleanChecks(getBlock(post.section36.type, post.section36.embedcode, post.section36.text, post.section36.url));
                finalString = finalString + CleanChecks(getBlock(post.section37.type, post.section37.embedcode, post.section37.text, post.section37.url));
                finalString = finalString + CleanChecks(getBlock(post.section38.type, post.section38.embedcode, post.section38.text, post.section38.url));
                finalString = finalString + CleanChecks(getBlock(post.section39.type, post.section39.embedcode, post.section39.text, post.section39.url));
                finalString = finalString + CleanChecks(getBlock(post.section40.type, post.section40.embedcode, post.section40.text, post.section40.url));
                finalString = finalString + CleanChecks(getBlock(post.section41.type, post.section41.embedcode, post.section41.text, post.section41.url));
                finalString = finalString + CleanChecks(getBlock(post.section42.type, post.section42.embedcode, post.section42.text, post.section42.url));
                finalString = finalString + CleanChecks(getBlock(post.section43.type, post.section43.embedcode, post.section43.text, post.section43.url));
                finalString = finalString + CleanChecks(getBlock(post.section44.type, post.section44.embedcode, post.section44.text, post.section44.url));
                finalString = finalString + CleanChecks(getBlock(post.section45.type, post.section45.embedcode, post.section45.text, post.section45.url));
                finalString = finalString + CleanChecks(getBlock(post.section46.type, post.section46.embedcode, post.section46.text, post.section46.url));
                finalString = finalString + CleanChecks(getBlock(post.section47.type, post.section47.embedcode, post.section47.text, post.section47.url));
                finalString = finalString + CleanChecks(getBlock(post.section48.type, post.section48.embedcode, post.section48.text, post.section48.url));
                finalString = finalString + CleanChecks(getBlock(post.section49.type, post.section49.embedcode, post.section49.text, post.section49.url));
                finalString = finalString + CleanChecks(getBlock(post.section50.type, post.section50.embedcode, post.section50.text, post.section50.url));
                finalString = finalString + CleanChecks(getBlock(post.section51.type, post.section51.embedcode, post.section51.text, post.section51.url));
                finalString = finalString + CleanChecks(getBlock(post.section52.type, post.section52.embedcode, post.section52.text, post.section52.url));
                finalString = finalString + CleanChecks(getBlock(post.section53.type, post.section53.embedcode, post.section53.text, post.section53.url));
                finalString = finalString + CleanChecks(getBlock(post.section54.type, post.section54.embedcode, post.section54.text, post.section54.url));
                finalString = finalString + CleanChecks(getBlock(post.section55.type, post.section55.embedcode, post.section55.text, post.section55.url));
                finalString = finalString + CleanChecks(getBlock(post.section56.type, post.section56.embedcode, post.section56.text, post.section56.url));
                finalString = finalString + CleanChecks(getBlock(post.section57.type, post.section57.embedcode, post.section57.text, post.section57.url));
                finalString = finalString + CleanChecks(getBlock(post.section58.type, post.section58.embedcode, post.section58.text, post.section58.url));
                finalString = finalString + CleanChecks(getBlock(post.section59.type, post.section59.embedcode, post.section59.text, post.section59.url));
                finalString = finalString + CleanChecks(getBlock(post.section60.type, post.section60.embedcode, post.section60.text, post.section60.url));
                finalString = finalString + CleanChecks(getBlock(post.section61.type, post.section61.embedcode, post.section61.text, post.section61.url));
                finalString = finalString + CleanChecks(getBlock(post.section62.type, post.section62.embedcode, post.section62.text, post.section62.url));
                finalString = finalString + CleanChecks(getBlock(post.section63.type, post.section63.embedcode, post.section63.text, post.section63.url));
                finalString = finalString + CleanChecks(getBlock(post.section64.type, post.section64.embedcode, post.section64.text, post.section64.url));
                finalString = finalString + CleanChecks(getBlock(post.section65.type, post.section65.embedcode, post.section65.text, post.section65.url));
                finalString = finalString + CleanChecks(getBlock(post.section66.type, post.section66.embedcode, post.section66.text, post.section66.url));
                finalString = finalString + CleanChecks(getBlock(post.section67.type, post.section67.embedcode, post.section67.text, post.section67.url));
                finalString = finalString + CleanChecks(getBlock(post.section68.type, post.section68.embedcode, post.section68.text, post.section68.url));
                finalString = finalString + CleanChecks(getBlock(post.section69.type, post.section69.embedcode, post.section69.text, post.section69.url));
                finalString = finalString + CleanChecks(getBlock(post.section70.type, post.section70.embedcode, post.section70.text, post.section70.url));
                finalString = finalString + CleanChecks(getBlock(post.section71.type, post.section71.embedcode, post.section71.text, post.section71.url));
                finalString = finalString + CleanChecks(getBlock(post.section72.type, post.section72.embedcode, post.section72.text, post.section72.url));
                finalString = finalString + CleanChecks(getBlock(post.section73.type, post.section73.embedcode, post.section73.text, post.section73.url));
                finalString = finalString + CleanChecks(getBlock(post.section74.type, post.section74.embedcode, post.section74.text, post.section74.url));
                finalString = finalString + CleanChecks(getBlock(post.section75.type, post.section75.embedcode, post.section75.text, post.section75.url));
                finalString = finalString + CleanChecks(getBlock(post.section76.type, post.section76.embedcode, post.section76.text, post.section76.url));
                finalString = finalString + CleanChecks(getBlock(post.section77.type, post.section77.embedcode, post.section77.text, post.section77.url));
                finalString = finalString + CleanChecks(getBlock(post.section78.type, post.section78.embedcode, post.section78.text, post.section78.url));
                finalString = finalString + CleanChecks(getBlock(post.section79.type, post.section79.embedcode, post.section79.text, post.section79.url));
                finalString = finalString + CleanChecks(getBlock(post.section80.type, post.section80.embedcode, post.section80.text, post.section80.url));
                finalString = finalString + CleanChecks(getBlock(post.section81.type, post.section81.embedcode, post.section81.text, post.section81.url));
                finalString = finalString + CleanChecks(getBlock(post.section82.type, post.section82.embedcode, post.section82.text, post.section82.url));
                finalString = finalString + CleanChecks(getBlock(post.section83.type, post.section83.embedcode, post.section83.text, post.section83.url));
                finalString = finalString + CleanChecks(getBlock(post.section84.type, post.section84.embedcode, post.section84.text, post.section84.url));
                finalString = finalString + CleanChecks(getBlock(post.section85.type, post.section85.embedcode, post.section85.text, post.section85.url));
                finalString = finalString + CleanChecks(getBlock(post.section86.type, post.section86.embedcode, post.section86.text, post.section86.url));
                finalString = finalString + CleanChecks(getBlock(post.section87.type, post.section87.embedcode, post.section87.text, post.section87.url));
                finalString = finalString + CleanChecks(getBlock(post.section88.type, post.section88.embedcode, post.section88.text, post.section88.url));
                finalString = finalString + CleanChecks(getBlock(post.section89.type, post.section89.embedcode, post.section89.text, post.section89.url));
                finalString = finalString + CleanChecks(getBlock(post.section90.type, post.section90.embedcode, post.section90.text, post.section90.url));
                finalString = finalString + CleanChecks(getBlock(post.section91.type, post.section91.embedcode, post.section91.text, post.section91.url));
                finalString = finalString + CleanChecks(getBlock(post.section92.type, post.section92.embedcode, post.section92.text, post.section92.url));
                finalString = finalString + CleanChecks(getBlock(post.section93.type, post.section93.embedcode, post.section93.text, post.section93.url));
                finalString = finalString + CleanChecks(getBlock(post.section94.type, post.section94.embedcode, post.section94.text, post.section94.url));
                finalString = finalString + CleanChecks(getBlock(post.section95.type, post.section95.embedcode, post.section95.text, post.section95.url));
                finalString = finalString + CleanChecks(getBlock(post.section96.type, post.section96.embedcode, post.section96.text, post.section96.url));
                finalString = finalString + CleanChecks(getBlock(post.section97.type, post.section97.embedcode, post.section97.text, post.section97.url));
                finalString = finalString + CleanChecks(getBlock(post.section98.type, post.section98.embedcode, post.section98.text, post.section98.url));
                finalString = finalString + CleanChecks(getBlock(post.section99.type, post.section99.embedcode, post.section99.text, post.section99.url));
                finalString = finalString + CleanChecks(getBlock(post.section100.type, post.section100.embedcode, post.section100.text, post.section100.url));
                finalString = finalString + CleanChecks(getBlock(post.section101.type, post.section101.embedcode, post.section101.text, post.section101.url));
                finalString = finalString + CleanChecks(getBlock(post.section102.type, post.section102.embedcode, post.section102.text, post.section102.url));
                finalString = finalString + CleanChecks(getBlock(post.section103.type, post.section103.embedcode, post.section103.text, post.section103.url));
                finalString = finalString + CleanChecks(getBlock(post.section104.type, post.section104.embedcode, post.section104.text, post.section104.url));
                finalString = finalString + CleanChecks(getBlock(post.section105.type, post.section105.embedcode, post.section105.text, post.section105.url));
                finalString = finalString + CleanChecks(getBlock(post.section106.type, post.section106.embedcode, post.section106.text, post.section106.url));
                finalString = finalString + CleanChecks(getBlock(post.section107.type, post.section107.embedcode, post.section107.text, post.section107.url));
                finalString = finalString + CleanChecks(getBlock(post.section108.type, post.section108.embedcode, post.section108.text, post.section108.url));
                finalString = finalString + CleanChecks(getBlock(post.section109.type, post.section109.embedcode, post.section109.text, post.section109.url));
                finalString = finalString + CleanChecks(getBlock(post.section110.type, post.section110.embedcode, post.section110.text, post.section110.url));
                finalString = finalString + CleanChecks(getBlock(post.section111.type, post.section111.embedcode, post.section111.text, post.section111.url));
                finalString = finalString + CleanChecks(getBlock(post.section112.type, post.section112.embedcode, post.section112.text, post.section112.url));
                finalString = finalString + CleanChecks(getBlock(post.section113.type, post.section113.embedcode, post.section113.text, post.section113.url));
                finalString = finalString + CleanChecks(getBlock(post.section114.type, post.section114.embedcode, post.section114.text, post.section114.url));
                finalString = finalString + CleanChecks(getBlock(post.section115.type, post.section115.embedcode, post.section115.text, post.section115.url));
                finalString = finalString + CleanChecks(getBlock(post.section116.type, post.section116.embedcode, post.section116.text, post.section116.url));
                finalString = finalString + CleanChecks(getBlock(post.section117.type, post.section117.embedcode, post.section117.text, post.section117.url));
                finalString = finalString + CleanChecks(getBlock(post.section118.type, post.section118.embedcode, post.section118.text, post.section118.url));
                finalString = finalString + CleanChecks(getBlock(post.section119.type, post.section119.embedcode, post.section119.text, post.section119.url));
                finalString = finalString + CleanChecks(getBlock(post.section120.type, post.section120.embedcode, post.section120.text, post.section120.url));
                finalString = finalString + CleanChecks(getBlock(post.section121.type, post.section121.embedcode, post.section121.text, post.section121.url));
                finalString = finalString + CleanChecks(getBlock(post.section122.type, post.section122.embedcode, post.section122.text, post.section122.url));
                finalString = finalString + CleanChecks(getBlock(post.section123.type, post.section123.embedcode, post.section123.text, post.section123.url));
                finalString = finalString + CleanChecks(getBlock(post.section124.type, post.section124.embedcode, post.section124.text, post.section124.url));
                finalString = finalString + CleanChecks(getBlock(post.section125.type, post.section125.embedcode, post.section125.text, post.section125.url));
                finalString = finalString + CleanChecks(getBlock(post.section126.type, post.section126.embedcode, post.section126.text, post.section126.url));
                finalString = finalString + CleanChecks(getBlock(post.section127.type, post.section127.embedcode, post.section127.text, post.section127.url));
                finalString = finalString + CleanChecks(getBlock(post.section128.type, post.section128.embedcode, post.section128.text, post.section128.url));
                finalString = finalString + CleanChecks(getBlock(post.section129.type, post.section129.embedcode, post.section129.text, post.section129.url));
                finalString = finalString + CleanChecks(getBlock(post.section130.type, post.section130.embedcode, post.section130.text, post.section130.url));
                finalString = finalString + CleanChecks(getBlock(post.section131.type, post.section131.embedcode, post.section131.text, post.section131.url));
                finalString = finalString + CleanChecks(getBlock(post.section132.type, post.section132.embedcode, post.section132.text, post.section132.url));
                finalString = finalString + CleanChecks(getBlock(post.section133.type, post.section133.embedcode, post.section133.text, post.section133.url));
                finalString = finalString + CleanChecks(getBlock(post.section134.type, post.section134.embedcode, post.section134.text, post.section134.url));
                finalString = finalString + CleanChecks(getBlock(post.section135.type, post.section135.embedcode, post.section135.text, post.section135.url));
                finalString = finalString + CleanChecks(getBlock(post.section136.type, post.section136.embedcode, post.section136.text, post.section136.url));
                finalString = finalString + CleanChecks(getBlock(post.section137.type, post.section137.embedcode, post.section137.text, post.section137.url));
                finalString = finalString + CleanChecks(getBlock(post.section138.type, post.section138.embedcode, post.section138.text, post.section138.url));
                finalString = finalString + CleanChecks(getBlock(post.section139.type, post.section139.embedcode, post.section139.text, post.section139.url));
                finalString = finalString + CleanChecks(getBlock(post.section140.type, post.section140.embedcode, post.section140.text, post.section140.url));
                finalString = finalString + CleanChecks(getBlock(post.section141.type, post.section141.embedcode, post.section141.text, post.section141.url));
                finalString = finalString + CleanChecks(getBlock(post.section142.type, post.section142.embedcode, post.section142.text, post.section142.url));
                finalString = finalString + CleanChecks(getBlock(post.section143.type, post.section143.embedcode, post.section143.text, post.section143.url));
                finalString = finalString + CleanChecks(getBlock(post.section144.type, post.section144.embedcode, post.section144.text, post.section144.url));
                finalString = finalString + CleanChecks(getBlock(post.section145.type, post.section145.embedcode, post.section145.text, post.section145.url));
                finalString = finalString + CleanChecks(getBlock(post.section146.type, post.section146.embedcode, post.section146.text, post.section146.url));
                finalString = finalString + CleanChecks(getBlock(post.section147.type, post.section147.embedcode, post.section147.text, post.section147.url));
                finalString = finalString + CleanChecks(getBlock(post.section148.type, post.section148.embedcode, post.section148.text, post.section148.url));
                finalString = finalString + CleanChecks(getBlock(post.section149.type, post.section149.embedcode, post.section149.text, post.section149.url));
                finalString = finalString + CleanChecks(getBlock(post.section150.type, post.section150.embedcode, post.section150.text, post.section150.url));
                finalString = finalString + CleanChecks(getBlock(post.section151.type, post.section151.embedcode, post.section151.text, post.section151.url));
                finalString = finalString + CleanChecks(getBlock(post.section152.type, post.section152.embedcode, post.section152.text, post.section152.url));
                finalString = finalString + CleanChecks(getBlock(post.section153.type, post.section153.embedcode, post.section153.text, post.section153.url));
                finalString = finalString + CleanChecks(getBlock(post.section154.type, post.section154.embedcode, post.section154.text, post.section154.url));
                finalString = finalString + CleanChecks(getBlock(post.section155.type, post.section155.embedcode, post.section155.text, post.section155.url));
                finalString = finalString + CleanChecks(getBlock(post.section156.type, post.section156.embedcode, post.section156.text, post.section156.url));
                finalString = finalString + CleanChecks(getBlock(post.section157.type, post.section157.embedcode, post.section157.text, post.section157.url));
                finalString = finalString + CleanChecks(getBlock(post.section158.type, post.section158.embedcode, post.section158.text, post.section158.url));
                finalString = finalString + CleanChecks(getBlock(post.section159.type, post.section159.embedcode, post.section159.text, post.section159.url));
                finalString = finalString + CleanChecks(getBlock(post.section160.type, post.section160.embedcode, post.section160.text, post.section160.url));
                finalString = finalString + CleanChecks(getBlock(post.section161.type, post.section161.embedcode, post.section161.text, post.section161.url));
                finalString = finalString + CleanChecks(getBlock(post.section162.type, post.section162.embedcode, post.section162.text, post.section162.url));
                finalString = finalString + CleanChecks(getBlock(post.section163.type, post.section163.embedcode, post.section163.text, post.section163.url));
                finalString = finalString + CleanChecks(getBlock(post.section164.type, post.section164.embedcode, post.section164.text, post.section164.url));
                finalString = finalString + CleanChecks(getBlock(post.section165.type, post.section165.embedcode, post.section165.text, post.section165.url));
                finalString = finalString + CleanChecks(getBlock(post.section166.type, post.section166.embedcode, post.section166.text, post.section166.url));
                finalString = finalString + CleanChecks(getBlock(post.section167.type, post.section167.embedcode, post.section167.text, post.section167.url));
                finalString = finalString + CleanChecks(getBlock(post.section168.type, post.section168.embedcode, post.section168.text, post.section168.url));
                finalString = finalString + CleanChecks(getBlock(post.section169.type, post.section169.embedcode, post.section169.text, post.section169.url));
                finalString = finalString + CleanChecks(getBlock(post.section170.type, post.section170.embedcode, post.section170.text, post.section170.url));
                finalString = finalString + CleanChecks(getBlock(post.section171.type, post.section171.embedcode, post.section171.text, post.section171.url));
                finalString = finalString + CleanChecks(getBlock(post.section172.type, post.section172.embedcode, post.section172.text, post.section172.url));
                finalString = finalString + CleanChecks(getBlock(post.section173.type, post.section173.embedcode, post.section173.text, post.section173.url));
                finalString = finalString + CleanChecks(getBlock(post.section174.type, post.section174.embedcode, post.section174.text, post.section174.url));
                finalString = finalString + CleanChecks(getBlock(post.section175.type, post.section175.embedcode, post.section175.text, post.section175.url));
                finalString = finalString + CleanChecks(getBlock(post.section176.type, post.section176.embedcode, post.section176.text, post.section176.url));
                finalString = finalString + CleanChecks(getBlock(post.section177.type, post.section177.embedcode, post.section177.text, post.section177.url));
                finalString = finalString + CleanChecks(getBlock(post.section178.type, post.section178.embedcode, post.section178.text, post.section178.url));
                finalString = finalString + CleanChecks(getBlock(post.section179.type, post.section179.embedcode, post.section179.text, post.section179.url));
                finalString = finalString + CleanChecks(getBlock(post.section180.type, post.section180.embedcode, post.section180.text, post.section180.url));
                finalString = finalString + CleanChecks(getBlock(post.section181.type, post.section181.embedcode, post.section181.text, post.section181.url));
                finalString = finalString + CleanChecks(getBlock(post.section182.type, post.section182.embedcode, post.section182.text, post.section182.url));
                finalString = finalString + CleanChecks(getBlock(post.section183.type, post.section183.embedcode, post.section183.text, post.section183.url));
                finalString = finalString + CleanChecks(getBlock(post.section184.type, post.section184.embedcode, post.section184.text, post.section184.url));
                finalString = finalString + CleanChecks(getBlock(post.section185.type, post.section185.embedcode, post.section185.text, post.section185.url));
                finalString = finalString + CleanChecks(getBlock(post.section186.type, post.section186.embedcode, post.section186.text, post.section186.url));
                finalString = finalString + CleanChecks(getBlock(post.section187.type, post.section187.embedcode, post.section187.text, post.section187.url));
                finalString = finalString + CleanChecks(getBlock(post.section188.type, post.section188.embedcode, post.section188.text, post.section188.url));
                finalString = finalString + CleanChecks(getBlock(post.section189.type, post.section189.embedcode, post.section189.text, post.section189.url));
                finalString = finalString + CleanChecks(getBlock(post.section190.type, post.section190.embedcode, post.section190.text, post.section190.url));
                finalString = finalString + CleanChecks(getBlock(post.section191.type, post.section191.embedcode, post.section191.text, post.section191.url));
                finalString = finalString + CleanChecks(getBlock(post.section192.type, post.section192.embedcode, post.section192.text, post.section192.url));
                finalString = finalString + CleanChecks(getBlock(post.section193.type, post.section193.embedcode, post.section193.text, post.section193.url));
                finalString = finalString + CleanChecks(getBlock(post.section194.type, post.section194.embedcode, post.section194.text, post.section194.url));
                finalString = finalString + CleanChecks(getBlock(post.section195.type, post.section195.embedcode, post.section195.text, post.section195.url));
                finalString = finalString + CleanChecks(getBlock(post.section196.type, post.section196.embedcode, post.section196.text, post.section196.url));
                finalString = finalString + CleanChecks(getBlock(post.section197.type, post.section197.embedcode, post.section197.text, post.section197.url));
                finalString = finalString + CleanChecks(getBlock(post.section198.type, post.section198.embedcode, post.section198.text, post.section198.url));
                finalString = finalString + CleanChecks(getBlock(post.section199.type, post.section199.embedcode, post.section199.text, post.section199.url));
                finalString = finalString + CleanChecks(getBlock(post.section200.type, post.section200.embedcode, post.section200.text, post.section200.url));
                finalString = finalString + CleanChecks(getBlock(post.section201.type, post.section201.embedcode, post.section201.text, post.section201.url));
                finalString = finalString + CleanChecks(getBlock(post.section202.type, post.section202.embedcode, post.section202.text, post.section202.url));
                finalString = finalString + CleanChecks(getBlock(post.section203.type, post.section203.embedcode, post.section203.text, post.section203.url));
                finalString = finalString + CleanChecks(getBlock(post.section204.type, post.section204.embedcode, post.section204.text, post.section204.url));
                finalString = finalString + CleanChecks(getBlock(post.section205.type, post.section205.embedcode, post.section205.text, post.section205.url));
                finalString = finalString + CleanChecks(getBlock(post.section206.type, post.section206.embedcode, post.section206.text, post.section206.url));
                finalString = finalString + CleanChecks(getBlock(post.section207.type, post.section207.embedcode, post.section207.text, post.section207.url));
                finalString = finalString + CleanChecks(getBlock(post.section208.type, post.section208.embedcode, post.section208.text, post.section208.url));
                finalString = finalString + CleanChecks(getBlock(post.section209.type, post.section209.embedcode, post.section209.text, post.section209.url));
                finalString = finalString + CleanChecks(getBlock(post.section210.type, post.section210.embedcode, post.section210.text, post.section210.url));
                finalString = finalString + CleanChecks(getBlock(post.section211.type, post.section211.embedcode, post.section211.text, post.section211.url));
                finalString = finalString + CleanChecks(getBlock(post.section212.type, post.section212.embedcode, post.section212.text, post.section212.url));
                finalString = finalString + CleanChecks(getBlock(post.section213.type, post.section213.embedcode, post.section213.text, post.section213.url));
                finalString = finalString + CleanChecks(getBlock(post.section214.type, post.section214.embedcode, post.section214.text, post.section214.url));
                finalString = finalString + CleanChecks(getBlock(post.section215.type, post.section215.embedcode, post.section215.text, post.section215.url));
                finalString = finalString + CleanChecks(getBlock(post.section216.type, post.section216.embedcode, post.section216.text, post.section216.url));
                finalString = finalString + CleanChecks(getBlock(post.section217.type, post.section217.embedcode, post.section217.text, post.section217.url));
                finalString = finalString + CleanChecks(getBlock(post.section218.type, post.section218.embedcode, post.section218.text, post.section218.url));
                finalString = finalString + CleanChecks(getBlock(post.section219.type, post.section219.embedcode, post.section219.text, post.section219.url));
                finalString = finalString + CleanChecks(getBlock(post.section220.type, post.section220.embedcode, post.section220.text, post.section220.url));
                finalString = finalString + CleanChecks(getBlock(post.section221.type, post.section221.embedcode, post.section221.text, post.section221.url));
                finalString = finalString + CleanChecks(getBlock(post.section222.type, post.section222.embedcode, post.section222.text, post.section222.url));
                finalString = finalString + CleanChecks(getBlock(post.section223.type, post.section223.embedcode, post.section223.text, post.section223.url));
                finalString = finalString + CleanChecks(getBlock(post.section224.type, post.section224.embedcode, post.section224.text, post.section224.url));
                finalString = finalString + CleanChecks(getBlock(post.section225.type, post.section225.embedcode, post.section225.text, post.section225.url));
                finalString = finalString + CleanChecks(getBlock(post.section226.type, post.section226.embedcode, post.section226.text, post.section226.url));
                finalString = finalString + CleanChecks(getBlock(post.section227.type, post.section227.embedcode, post.section227.text, post.section227.url));
                finalString = finalString + CleanChecks(getBlock(post.section228.type, post.section228.embedcode, post.section228.text, post.section228.url));
                finalString = finalString + CleanChecks(getBlock(post.section229.type, post.section229.embedcode, post.section229.text, post.section229.url));
                finalString = finalString + CleanChecks(getBlock(post.section230.type, post.section230.embedcode, post.section230.text, post.section230.url));
                finalString = finalString + CleanChecks(getBlock(post.section231.type, post.section231.embedcode, post.section231.text, post.section231.url));
                finalString = finalString + CleanChecks(getBlock(post.section232.type, post.section232.embedcode, post.section232.text, post.section232.url));
                finalString = finalString + CleanChecks(getBlock(post.section233.type, post.section233.embedcode, post.section233.text, post.section233.url));
                finalString = finalString + CleanChecks(getBlock(post.section234.type, post.section234.embedcode, post.section234.text, post.section234.url));
                finalString = finalString + CleanChecks(getBlock(post.section235.type, post.section235.embedcode, post.section235.text, post.section235.url));
                finalString = finalString + CleanChecks(getBlock(post.section236.type, post.section236.embedcode, post.section236.text, post.section236.url));
                finalString = finalString + CleanChecks(getBlock(post.section237.type, post.section237.embedcode, post.section237.text, post.section237.url));
                finalString = finalString + CleanChecks(getBlock(post.section238.type, post.section238.embedcode, post.section238.text, post.section238.url));
                finalString = finalString + CleanChecks(getBlock(post.section239.type, post.section239.embedcode, post.section239.text, post.section239.url));
                finalString = finalString + CleanChecks(getBlock(post.section240.type, post.section240.embedcode, post.section240.text, post.section240.url));
                finalString = finalString + CleanChecks(getBlock(post.section241.type, post.section241.embedcode, post.section241.text, post.section241.url));
                finalString = finalString + CleanChecks(getBlock(post.section242.type, post.section242.embedcode, post.section242.text, post.section242.url));
                finalString = finalString + CleanChecks(getBlock(post.section243.type, post.section243.embedcode, post.section243.text, post.section243.url));
                finalString = finalString + CleanChecks(getBlock(post.section244.type, post.section244.embedcode, post.section244.text, post.section244.url));
                finalString = finalString + CleanChecks(getBlock(post.section245.type, post.section245.embedcode, post.section245.text, post.section245.url));
                finalString = finalString + CleanChecks(getBlock(post.section246.type, post.section246.embedcode, post.section246.text, post.section246.url));
                finalString = finalString + CleanChecks(getBlock(post.section247.type, post.section247.embedcode, post.section247.text, post.section247.url));
                finalString = finalString + CleanChecks(getBlock(post.section248.type, post.section248.embedcode, post.section248.text, post.section248.url));
                finalString = finalString + CleanChecks(getBlock(post.section249.type, post.section249.embedcode, post.section249.text, post.section249.url));
                finalString = finalString + CleanChecks(getBlock(post.section250.type, post.section250.embedcode, post.section250.text, post.section250.url));
                finalString = finalString + CleanChecks(getBlock(post.section251.type, post.section251.embedcode, post.section251.text, post.section251.url));
                finalString = finalString + CleanChecks(getBlock(post.section252.type, post.section252.embedcode, post.section252.text, post.section252.url));
                finalString = finalString + CleanChecks(getBlock(post.section253.type, post.section253.embedcode, post.section253.text, post.section253.url));
                finalString = finalString + CleanChecks(getBlock(post.section254.type, post.section254.embedcode, post.section254.text, post.section254.url));
                finalString = finalString + CleanChecks(getBlock(post.section255.type, post.section255.embedcode, post.section255.text, post.section255.url));
                finalString = finalString + CleanChecks(getBlock(post.section256.type, post.section256.embedcode, post.section256.text, post.section256.url));
                finalString = finalString + CleanChecks(getBlock(post.section257.type, post.section257.embedcode, post.section257.text, post.section257.url));
                finalString = finalString + CleanChecks(getBlock(post.section258.type, post.section258.embedcode, post.section258.text, post.section258.url));
                finalString = finalString + CleanChecks(getBlock(post.section259.type, post.section259.embedcode, post.section259.text, post.section259.url));
                finalString = finalString + CleanChecks(getBlock(post.section260.type, post.section260.embedcode, post.section260.text, post.section260.url));
                finalString = finalString + CleanChecks(getBlock(post.section261.type, post.section261.embedcode, post.section261.text, post.section261.url));
                finalString = finalString + CleanChecks(getBlock(post.section262.type, post.section262.embedcode, post.section262.text, post.section262.url));
                finalString = finalString + CleanChecks(getBlock(post.section263.type, post.section263.embedcode, post.section263.text, post.section263.url));
                finalString = finalString + CleanChecks(getBlock(post.section264.type, post.section264.embedcode, post.section264.text, post.section264.url));
                finalString = finalString + CleanChecks(getBlock(post.section265.type, post.section265.embedcode, post.section265.text, post.section265.url));
                finalString = finalString + CleanChecks(getBlock(post.section266.type, post.section266.embedcode, post.section266.text, post.section266.url));
                finalString = finalString + CleanChecks(getBlock(post.section267.type, post.section267.embedcode, post.section267.text, post.section267.url));
                finalString = finalString + CleanChecks(getBlock(post.section268.type, post.section268.embedcode, post.section268.text, post.section268.url));
                finalString = finalString + CleanChecks(getBlock(post.section269.type, post.section269.embedcode, post.section269.text, post.section269.url));
                finalString = finalString + CleanChecks(getBlock(post.section270.type, post.section270.embedcode, post.section270.text, post.section270.url));
                finalString = finalString + CleanChecks(getBlock(post.section271.type, post.section271.embedcode, post.section271.text, post.section271.url));
                finalString = finalString + CleanChecks(getBlock(post.section272.type, post.section272.embedcode, post.section272.text, post.section272.url));
                finalString = finalString + CleanChecks(getBlock(post.section273.type, post.section273.embedcode, post.section273.text, post.section273.url));
                finalString = finalString + CleanChecks(getBlock(post.section274.type, post.section274.embedcode, post.section274.text, post.section274.url));
                finalString = finalString + CleanChecks(getBlock(post.section275.type, post.section275.embedcode, post.section275.text, post.section275.url));
                finalString = finalString + CleanChecks(getBlock(post.section276.type, post.section276.embedcode, post.section276.text, post.section276.url));
                finalString = finalString + CleanChecks(getBlock(post.section277.type, post.section277.embedcode, post.section277.text, post.section277.url));
                finalString = finalString + CleanChecks(getBlock(post.section278.type, post.section278.embedcode, post.section278.text, post.section278.url));
                finalString = finalString + CleanChecks(getBlock(post.section279.type, post.section279.embedcode, post.section279.text, post.section279.url));
                finalString = finalString + CleanChecks(getBlock(post.section280.type, post.section280.embedcode, post.section280.text, post.section280.url));
                finalString = finalString + CleanChecks(getBlock(post.section281.type, post.section281.embedcode, post.section281.text, post.section281.url));
                finalString = finalString + CleanChecks(getBlock(post.section282.type, post.section282.embedcode, post.section282.text, post.section282.url));
                finalString = finalString + CleanChecks(getBlock(post.section283.type, post.section283.embedcode, post.section283.text, post.section283.url));
                finalString = finalString + CleanChecks(getBlock(post.section284.type, post.section284.embedcode, post.section284.text, post.section284.url));
                finalString = finalString + CleanChecks(getBlock(post.section285.type, post.section285.embedcode, post.section285.text, post.section285.url));
                finalString = finalString + CleanChecks(getBlock(post.section286.type, post.section286.embedcode, post.section286.text, post.section286.url));
                finalString = finalString + CleanChecks(getBlock(post.section287.type, post.section287.embedcode, post.section287.text, post.section287.url));
                finalString = finalString + CleanChecks(getBlock(post.section288.type, post.section288.embedcode, post.section288.text, post.section288.url));
                finalString = finalString + CleanChecks(getBlock(post.section289.type, post.section289.embedcode, post.section289.text, post.section289.url));
                finalString = finalString + CleanChecks(getBlock(post.section290.type, post.section290.embedcode, post.section290.text, post.section290.url));
                finalString = finalString + CleanChecks(getBlock(post.section291.type, post.section291.embedcode, post.section291.text, post.section291.url));
                finalString = finalString + CleanChecks(getBlock(post.section292.type, post.section292.embedcode, post.section292.text, post.section292.url));
                finalString = finalString + CleanChecks(getBlock(post.section293.type, post.section293.embedcode, post.section293.text, post.section293.url));
                finalString = finalString + CleanChecks(getBlock(post.section294.type, post.section294.embedcode, post.section294.text, post.section294.url));
                finalString = finalString + CleanChecks(getBlock(post.section295.type, post.section295.embedcode, post.section295.text, post.section295.url));
                finalString = finalString + CleanChecks(getBlock(post.section296.type, post.section296.embedcode, post.section296.text, post.section296.url));
                finalString = finalString + CleanChecks(getBlock(post.section297.type, post.section297.embedcode, post.section297.text, post.section297.url));
                finalString = finalString + CleanChecks(getBlock(post.section298.type, post.section298.embedcode, post.section298.text, post.section298.url));
                finalString = finalString + CleanChecks(getBlock(post.section299.type, post.section299.embedcode, post.section299.text, post.section299.url));
                finalString = finalString + CleanChecks(getBlock(post.section300.type, post.section300.embedcode, post.section300.text, post.section300.url));
                finalString = finalString + CleanChecks(getBlock(post.section301.type, post.section301.embedcode, post.section301.text, post.section301.url));
                finalString = finalString + CleanChecks(getBlock(post.section302.type, post.section302.embedcode, post.section302.text, post.section302.url));
                finalString = finalString + CleanChecks(getBlock(post.section303.type, post.section303.embedcode, post.section303.text, post.section303.url));
                finalString = finalString + CleanChecks(getBlock(post.section304.type, post.section304.embedcode, post.section304.text, post.section304.url));
                finalString = finalString + CleanChecks(getBlock(post.section305.type, post.section305.embedcode, post.section305.text, post.section305.url));
                finalString = finalString + CleanChecks(getBlock(post.section306.type, post.section306.embedcode, post.section306.text, post.section306.url));
                finalString = finalString + CleanChecks(getBlock(post.section307.type, post.section307.embedcode, post.section307.text, post.section307.url));
                finalString = finalString + CleanChecks(getBlock(post.section308.type, post.section308.embedcode, post.section308.text, post.section308.url));
                finalString = finalString + CleanChecks(getBlock(post.section309.type, post.section309.embedcode, post.section309.text, post.section309.url));
                finalString = finalString + CleanChecks(getBlock(post.section310.type, post.section310.embedcode, post.section310.text, post.section310.url));
                finalString = finalString + CleanChecks(getBlock(post.section311.type, post.section311.embedcode, post.section311.text, post.section311.url));
                finalString = finalString + CleanChecks(getBlock(post.section312.type, post.section312.embedcode, post.section312.text, post.section312.url));
                finalString = finalString + CleanChecks(getBlock(post.section313.type, post.section313.embedcode, post.section313.text, post.section313.url));
                finalString = finalString + CleanChecks(getBlock(post.section314.type, post.section314.embedcode, post.section314.text, post.section314.url));
                finalString = finalString + CleanChecks(getBlock(post.section315.type, post.section315.embedcode, post.section315.text, post.section315.url));
                finalString = finalString + CleanChecks(getBlock(post.section316.type, post.section316.embedcode, post.section316.text, post.section316.url));
                finalString = finalString + CleanChecks(getBlock(post.section317.type, post.section317.embedcode, post.section317.text, post.section317.url));
                finalString = finalString + CleanChecks(getBlock(post.section318.type, post.section318.embedcode, post.section318.text, post.section318.url));
                finalString = finalString + CleanChecks(getBlock(post.section319.type, post.section319.embedcode, post.section319.text, post.section319.url));
                finalString = finalString + CleanChecks(getBlock(post.section320.type, post.section320.embedcode, post.section320.text, post.section320.url));
                finalString = finalString + CleanChecks(getBlock(post.section321.type, post.section321.embedcode, post.section321.text, post.section321.url));
                finalString = finalString + CleanChecks(getBlock(post.section322.type, post.section322.embedcode, post.section322.text, post.section322.url));
                finalString = finalString + CleanChecks(getBlock(post.section323.type, post.section323.embedcode, post.section323.text, post.section323.url));
                finalString = finalString + CleanChecks(getBlock(post.section324.type, post.section324.embedcode, post.section324.text, post.section324.url));
                finalString = finalString + CleanChecks(getBlock(post.section325.type, post.section325.embedcode, post.section325.text, post.section325.url));
                finalString = finalString + CleanChecks(getBlock(post.section326.type, post.section326.embedcode, post.section326.text, post.section326.url));
                finalString = finalString + CleanChecks(getBlock(post.section327.type, post.section327.embedcode, post.section327.text, post.section327.url));
                finalString = finalString + CleanChecks(getBlock(post.section328.type, post.section328.embedcode, post.section328.text, post.section328.url));
                finalString = finalString + CleanChecks(getBlock(post.section329.type, post.section329.embedcode, post.section329.text, post.section329.url));
                finalString = finalString + CleanChecks(getBlock(post.section330.type, post.section330.embedcode, post.section330.text, post.section330.url));
                finalString = finalString + CleanChecks(getBlock(post.section331.type, post.section331.embedcode, post.section331.text, post.section331.url));
                finalString = finalString + CleanChecks(getBlock(post.section332.type, post.section332.embedcode, post.section332.text, post.section332.url));
                finalString = finalString + CleanChecks(getBlock(post.section333.type, post.section333.embedcode, post.section333.text, post.section333.url));
                finalString = finalString + CleanChecks(getBlock(post.section334.type, post.section334.embedcode, post.section334.text, post.section334.url));
                finalString = finalString + CleanChecks(getBlock(post.section335.type, post.section335.embedcode, post.section335.text, post.section335.url));
                finalString = finalString + CleanChecks(getBlock(post.section336.type, post.section336.embedcode, post.section336.text, post.section336.url));
                finalString = finalString + CleanChecks(getBlock(post.section337.type, post.section337.embedcode, post.section337.text, post.section337.url));
                finalString = finalString + CleanChecks(getBlock(post.section338.type, post.section338.embedcode, post.section338.text, post.section338.url));
                finalString = finalString + CleanChecks(getBlock(post.section339.type, post.section339.embedcode, post.section339.text, post.section339.url));
                finalString = finalString + CleanChecks(getBlock(post.section340.type, post.section340.embedcode, post.section340.text, post.section340.url));
                finalString = finalString + CleanChecks(getBlock(post.section341.type, post.section341.embedcode, post.section341.text, post.section341.url));
                finalString = finalString + CleanChecks(getBlock(post.section342.type, post.section342.embedcode, post.section342.text, post.section342.url));
                finalString = finalString + CleanChecks(getBlock(post.section343.type, post.section343.embedcode, post.section343.text, post.section343.url));
                finalString = finalString + CleanChecks(getBlock(post.section344.type, post.section344.embedcode, post.section344.text, post.section344.url));
                finalString = finalString + CleanChecks(getBlock(post.section345.type, post.section345.embedcode, post.section345.text, post.section345.url));
                finalString = finalString + CleanChecks(getBlock(post.section346.type, post.section346.embedcode, post.section346.text, post.section346.url));
                finalString = finalString + CleanChecks(getBlock(post.section347.type, post.section347.embedcode, post.section347.text, post.section347.url));
                finalString = finalString + CleanChecks(getBlock(post.section348.type, post.section348.embedcode, post.section348.text, post.section348.url));
                finalString = finalString + CleanChecks(getBlock(post.section349.type, post.section349.embedcode, post.section349.text, post.section349.url));
                finalString = finalString + CleanChecks(getBlock(post.section350.type, post.section350.embedcode, post.section350.text, post.section350.url));
                finalString = finalString + CleanChecks(getBlock(post.section351.type, post.section351.embedcode, post.section351.text, post.section351.url));
                finalString = finalString + CleanChecks(getBlock(post.section352.type, post.section352.embedcode, post.section352.text, post.section352.url));
                finalString = finalString + CleanChecks(getBlock(post.section353.type, post.section353.embedcode, post.section353.text, post.section353.url));
                finalString = finalString + CleanChecks(getBlock(post.section354.type, post.section354.embedcode, post.section354.text, post.section354.url));
                finalString = finalString + CleanChecks(getBlock(post.section355.type, post.section355.embedcode, post.section355.text, post.section355.url));
                finalString = finalString + CleanChecks(getBlock(post.section356.type, post.section356.embedcode, post.section356.text, post.section356.url));
                finalString = finalString + CleanChecks(getBlock(post.section357.type, post.section357.embedcode, post.section357.text, post.section357.url));
                finalString = finalString + CleanChecks(getBlock(post.section358.type, post.section358.embedcode, post.section358.text, post.section358.url));
                finalString = finalString + CleanChecks(getBlock(post.section359.type, post.section359.embedcode, post.section359.text, post.section359.url));
                finalString = finalString + CleanChecks(getBlock(post.section360.type, post.section360.embedcode, post.section360.text, post.section360.url));
                finalString = finalString + CleanChecks(getBlock(post.section361.type, post.section361.embedcode, post.section361.text, post.section361.url));
                finalString = finalString + CleanChecks(getBlock(post.section362.type, post.section362.embedcode, post.section362.text, post.section362.url));
                finalString = finalString + CleanChecks(getBlock(post.section363.type, post.section363.embedcode, post.section363.text, post.section363.url));
                finalString = finalString + CleanChecks(getBlock(post.section364.type, post.section364.embedcode, post.section364.text, post.section364.url));
                finalString = finalString + CleanChecks(getBlock(post.section365.type, post.section365.embedcode, post.section365.text, post.section365.url));
                finalString = finalString + CleanChecks(getBlock(post.section366.type, post.section366.embedcode, post.section366.text, post.section366.url));
                finalString = finalString + CleanChecks(getBlock(post.section367.type, post.section367.embedcode, post.section367.text, post.section367.url));
                finalString = finalString + CleanChecks(getBlock(post.section368.type, post.section368.embedcode, post.section368.text, post.section368.url));
                finalString = finalString + CleanChecks(getBlock(post.section369.type, post.section369.embedcode, post.section369.text, post.section369.url));
                finalString = finalString + CleanChecks(getBlock(post.section370.type, post.section370.embedcode, post.section370.text, post.section370.url));
                finalString = finalString + CleanChecks(getBlock(post.section371.type, post.section371.embedcode, post.section371.text, post.section371.url));
                finalString = finalString + CleanChecks(getBlock(post.section372.type, post.section372.embedcode, post.section372.text, post.section372.url));
                finalString = finalString + CleanChecks(getBlock(post.section373.type, post.section373.embedcode, post.section373.text, post.section373.url));
                finalString = finalString + CleanChecks(getBlock(post.section374.type, post.section374.embedcode, post.section374.text, post.section374.url));
                finalString = finalString + CleanChecks(getBlock(post.section375.type, post.section375.embedcode, post.section375.text, post.section375.url));
                finalString = finalString + CleanChecks(getBlock(post.section376.type, post.section376.embedcode, post.section376.text, post.section376.url));
                finalString = finalString + CleanChecks(getBlock(post.section377.type, post.section377.embedcode, post.section377.text, post.section377.url));
                finalString = finalString + CleanChecks(getBlock(post.section378.type, post.section378.embedcode, post.section378.text, post.section378.url));
                finalString = finalString + CleanChecks(getBlock(post.section379.type, post.section379.embedcode, post.section379.text, post.section379.url));
                finalString = finalString + CleanChecks(getBlock(post.section380.type, post.section380.embedcode, post.section380.text, post.section380.url));
                finalString = finalString + CleanChecks(getBlock(post.section381.type, post.section381.embedcode, post.section381.text, post.section381.url));
                finalString = finalString + CleanChecks(getBlock(post.section382.type, post.section382.embedcode, post.section382.text, post.section382.url));
                finalString = finalString + CleanChecks(getBlock(post.section383.type, post.section383.embedcode, post.section383.text, post.section383.url));
                finalString = finalString + CleanChecks(getBlock(post.section384.type, post.section384.embedcode, post.section384.text, post.section384.url));
                finalString = finalString + CleanChecks(getBlock(post.section385.type, post.section385.embedcode, post.section385.text, post.section385.url));
                finalString = finalString + CleanChecks(getBlock(post.section386.type, post.section386.embedcode, post.section386.text, post.section386.url));
                finalString = finalString + CleanChecks(getBlock(post.section387.type, post.section387.embedcode, post.section387.text, post.section387.url));
                finalString = finalString + CleanChecks(getBlock(post.section388.type, post.section388.embedcode, post.section388.text, post.section388.url));
                finalString = finalString + CleanChecks(getBlock(post.section389.type, post.section389.embedcode, post.section389.text, post.section389.url));
                finalString = finalString + CleanChecks(getBlock(post.section390.type, post.section390.embedcode, post.section390.text, post.section390.url));
                finalString = finalString + CleanChecks(getBlock(post.section391.type, post.section391.embedcode, post.section391.text, post.section391.url));
                finalString = finalString + CleanChecks(getBlock(post.section392.type, post.section392.embedcode, post.section392.text, post.section392.url));
                finalString = finalString + CleanChecks(getBlock(post.section393.type, post.section393.embedcode, post.section393.text, post.section393.url));
                finalString = finalString + CleanChecks(getBlock(post.section394.type, post.section394.embedcode, post.section394.text, post.section394.url));
                finalString = finalString + CleanChecks(getBlock(post.section395.type, post.section395.embedcode, post.section395.text, post.section395.url));
                finalString = finalString + CleanChecks(getBlock(post.section396.type, post.section396.embedcode, post.section396.text, post.section396.url));
                finalString = finalString + CleanChecks(getBlock(post.section397.type, post.section397.embedcode, post.section397.text, post.section397.url));
                finalString = finalString + CleanChecks(getBlock(post.section398.type, post.section398.embedcode, post.section398.text, post.section398.url));
                finalString = finalString + CleanChecks(getBlock(post.section399.type, post.section399.embedcode, post.section399.text, post.section399.url));
                finalString = finalString + CleanChecks(getBlock(post.section400.type, post.section400.embedcode, post.section400.text, post.section400.url));
                finalString = finalString + CleanChecks(getBlock(post.section401.type, post.section401.embedcode, post.section401.text, post.section401.url));
                finalString = finalString + CleanChecks(getBlock(post.section402.type, post.section402.embedcode, post.section402.text, post.section402.url));
                finalString = finalString + CleanChecks(getBlock(post.section403.type, post.section403.embedcode, post.section403.text, post.section403.url));
                finalString = finalString + CleanChecks(getBlock(post.section404.type, post.section404.embedcode, post.section404.text, post.section404.url));
                finalString = finalString + CleanChecks(getBlock(post.section405.type, post.section405.embedcode, post.section405.text, post.section405.url));
                finalString = finalString + CleanChecks(getBlock(post.section406.type, post.section406.embedcode, post.section406.text, post.section406.url));
                finalString = finalString + CleanChecks(getBlock(post.section407.type, post.section407.embedcode, post.section407.text, post.section407.url));
                finalString = finalString + CleanChecks(getBlock(post.section408.type, post.section408.embedcode, post.section408.text, post.section408.url));
                finalString = finalString + CleanChecks(getBlock(post.section409.type, post.section409.embedcode, post.section409.text, post.section409.url));
                finalString = finalString + CleanChecks(getBlock(post.section410.type, post.section410.embedcode, post.section410.text, post.section410.url));
                finalString = finalString + CleanChecks(getBlock(post.section411.type, post.section411.embedcode, post.section411.text, post.section411.url));
                finalString = finalString + CleanChecks(getBlock(post.section412.type, post.section412.embedcode, post.section412.text, post.section412.url));
                finalString = finalString + CleanChecks(getBlock(post.section413.type, post.section413.embedcode, post.section413.text, post.section413.url));
                finalString = finalString + CleanChecks(getBlock(post.section414.type, post.section414.embedcode, post.section414.text, post.section414.url));
                finalString = finalString + CleanChecks(getBlock(post.section415.type, post.section415.embedcode, post.section415.text, post.section415.url));
                finalString = finalString + CleanChecks(getBlock(post.section416.type, post.section416.embedcode, post.section416.text, post.section416.url));
                finalString = finalString + CleanChecks(getBlock(post.section417.type, post.section417.embedcode, post.section417.text, post.section417.url));
                finalString = finalString + CleanChecks(getBlock(post.section418.type, post.section418.embedcode, post.section418.text, post.section418.url));
                finalString = finalString + CleanChecks(getBlock(post.section419.type, post.section419.embedcode, post.section419.text, post.section419.url));
                finalString = finalString + CleanChecks(getBlock(post.section420.type, post.section420.embedcode, post.section420.text, post.section420.url));
                finalString = finalString + CleanChecks(getBlock(post.section421.type, post.section421.embedcode, post.section421.text, post.section421.url));
                finalString = finalString + CleanChecks(getBlock(post.section422.type, post.section422.embedcode, post.section422.text, post.section422.url));
                finalString = finalString + CleanChecks(getBlock(post.section423.type, post.section423.embedcode, post.section423.text, post.section423.url));
                finalString = finalString + CleanChecks(getBlock(post.section424.type, post.section424.embedcode, post.section424.text, post.section424.url));
                finalString = finalString + CleanChecks(getBlock(post.section425.type, post.section425.embedcode, post.section425.text, post.section425.url));
                finalString = finalString + CleanChecks(getBlock(post.section426.type, post.section426.embedcode, post.section426.text, post.section426.url));
                finalString = finalString + CleanChecks(getBlock(post.section427.type, post.section427.embedcode, post.section427.text, post.section427.url));
                finalString = finalString + CleanChecks(getBlock(post.section428.type, post.section428.embedcode, post.section428.text, post.section428.url));
                finalString = finalString + CleanChecks(getBlock(post.section429.type, post.section429.embedcode, post.section429.text, post.section429.url));
                finalString = finalString + CleanChecks(getBlock(post.section430.type, post.section430.embedcode, post.section430.text, post.section430.url));
                finalString = finalString + CleanChecks(getBlock(post.section431.type, post.section431.embedcode, post.section431.text, post.section431.url));
                finalString = finalString + CleanChecks(getBlock(post.section432.type, post.section432.embedcode, post.section432.text, post.section432.url));
                finalString = finalString + CleanChecks(getBlock(post.section433.type, post.section433.embedcode, post.section433.text, post.section433.url));
                finalString = finalString + CleanChecks(getBlock(post.section434.type, post.section434.embedcode, post.section434.text, post.section434.url));
                finalString = finalString + CleanChecks(getBlock(post.section435.type, post.section435.embedcode, post.section435.text, post.section435.url));
                finalString = finalString + CleanChecks(getBlock(post.section436.type, post.section436.embedcode, post.section436.text, post.section436.url));
                finalString = finalString + CleanChecks(getBlock(post.section437.type, post.section437.embedcode, post.section437.text, post.section437.url));
                finalString = finalString + CleanChecks(getBlock(post.section438.type, post.section438.embedcode, post.section438.text, post.section438.url));
                finalString = finalString + CleanChecks(getBlock(post.section439.type, post.section439.embedcode, post.section439.text, post.section439.url));
                finalString = finalString + CleanChecks(getBlock(post.section440.type, post.section440.embedcode, post.section440.text, post.section440.url));
                finalString = finalString + CleanChecks(getBlock(post.section441.type, post.section441.embedcode, post.section441.text, post.section441.url));
                finalString = finalString + CleanChecks(getBlock(post.section442.type, post.section442.embedcode, post.section442.text, post.section442.url));
                finalString = finalString + CleanChecks(getBlock(post.section443.type, post.section443.embedcode, post.section443.text, post.section443.url));
                finalString = finalString + CleanChecks(getBlock(post.section444.type, post.section444.embedcode, post.section444.text, post.section444.url));
                finalString = finalString + CleanChecks(getBlock(post.section445.type, post.section445.embedcode, post.section445.text, post.section445.url));
                finalString = finalString + CleanChecks(getBlock(post.section446.type, post.section446.embedcode, post.section446.text, post.section446.url));
                finalString = finalString + CleanChecks(getBlock(post.section447.type, post.section447.embedcode, post.section447.text, post.section447.url));
                finalString = finalString + CleanChecks(getBlock(post.section448.type, post.section448.embedcode, post.section448.text, post.section448.url));
                finalString = finalString + CleanChecks(getBlock(post.section449.type, post.section449.embedcode, post.section449.text, post.section449.url));
                finalString = finalString + CleanChecks(getBlock(post.section450.type, post.section450.embedcode, post.section450.text, post.section450.url));
                finalString = finalString + CleanChecks(getBlock(post.section451.type, post.section451.embedcode, post.section451.text, post.section451.url));
                finalString = finalString + CleanChecks(getBlock(post.section452.type, post.section452.embedcode, post.section452.text, post.section452.url));
                finalString = finalString + CleanChecks(getBlock(post.section453.type, post.section453.embedcode, post.section453.text, post.section453.url));
                finalString = finalString + CleanChecks(getBlock(post.section454.type, post.section454.embedcode, post.section454.text, post.section454.url));
                finalString = finalString + CleanChecks(getBlock(post.section455.type, post.section455.embedcode, post.section455.text, post.section455.url));
                finalString = finalString + CleanChecks(getBlock(post.section456.type, post.section456.embedcode, post.section456.text, post.section456.url));
                finalString = finalString + CleanChecks(getBlock(post.section457.type, post.section457.embedcode, post.section457.text, post.section457.url));
                finalString = finalString + CleanChecks(getBlock(post.section458.type, post.section458.embedcode, post.section458.text, post.section458.url));
                finalString = finalString + CleanChecks(getBlock(post.section459.type, post.section459.embedcode, post.section459.text, post.section459.url));
                finalString = finalString + CleanChecks(getBlock(post.section460.type, post.section460.embedcode, post.section460.text, post.section460.url));
                finalString = finalString + CleanChecks(getBlock(post.section461.type, post.section461.embedcode, post.section461.text, post.section461.url));
                finalString = finalString + CleanChecks(getBlock(post.section462.type, post.section462.embedcode, post.section462.text, post.section462.url));
                finalString = finalString + CleanChecks(getBlock(post.section463.type, post.section463.embedcode, post.section463.text, post.section463.url));
                finalString = finalString + CleanChecks(getBlock(post.section464.type, post.section464.embedcode, post.section464.text, post.section464.url));
                finalString = finalString + CleanChecks(getBlock(post.section465.type, post.section465.embedcode, post.section465.text, post.section465.url));
                finalString = finalString + CleanChecks(getBlock(post.section466.type, post.section466.embedcode, post.section466.text, post.section466.url));
                finalString = finalString + CleanChecks(getBlock(post.section467.type, post.section467.embedcode, post.section467.text, post.section467.url));
                finalString = finalString + CleanChecks(getBlock(post.section468.type, post.section468.embedcode, post.section468.text, post.section468.url));
                finalString = finalString + CleanChecks(getBlock(post.section469.type, post.section469.embedcode, post.section469.text, post.section469.url));
                finalString = finalString + CleanChecks(getBlock(post.section470.type, post.section470.embedcode, post.section470.text, post.section470.url));
                finalString = finalString + CleanChecks(getBlock(post.section471.type, post.section471.embedcode, post.section471.text, post.section471.url));
                finalString = finalString + CleanChecks(getBlock(post.section472.type, post.section472.embedcode, post.section472.text, post.section472.url));
                finalString = finalString + CleanChecks(getBlock(post.section473.type, post.section473.embedcode, post.section473.text, post.section473.url));
                finalString = finalString + CleanChecks(getBlock(post.section474.type, post.section474.embedcode, post.section474.text, post.section474.url));
                finalString = finalString + CleanChecks(getBlock(post.section475.type, post.section475.embedcode, post.section475.text, post.section475.url));
                finalString = finalString + CleanChecks(getBlock(post.section476.type, post.section476.embedcode, post.section476.text, post.section476.url));
                finalString = finalString + CleanChecks(getBlock(post.section477.type, post.section477.embedcode, post.section477.text, post.section477.url));
                finalString = finalString + CleanChecks(getBlock(post.section478.type, post.section478.embedcode, post.section478.text, post.section478.url));
                finalString = finalString + CleanChecks(getBlock(post.section479.type, post.section479.embedcode, post.section479.text, post.section479.url));
                finalString = finalString + CleanChecks(getBlock(post.section480.type, post.section480.embedcode, post.section480.text, post.section480.url));
                finalString = finalString + CleanChecks(getBlock(post.section481.type, post.section481.embedcode, post.section481.text, post.section481.url));
                finalString = finalString + CleanChecks(getBlock(post.section482.type, post.section482.embedcode, post.section482.text, post.section482.url));
                finalString = finalString + CleanChecks(getBlock(post.section483.type, post.section483.embedcode, post.section483.text, post.section483.url));
                finalString = finalString + CleanChecks(getBlock(post.section484.type, post.section484.embedcode, post.section484.text, post.section484.url));
                finalString = finalString + CleanChecks(getBlock(post.section485.type, post.section485.embedcode, post.section485.text, post.section485.url));
                finalString = finalString + CleanChecks(getBlock(post.section486.type, post.section486.embedcode, post.section486.text, post.section486.url));
                finalString = finalString + CleanChecks(getBlock(post.section487.type, post.section487.embedcode, post.section487.text, post.section487.url));
                finalString = finalString + CleanChecks(getBlock(post.section488.type, post.section488.embedcode, post.section488.text, post.section488.url));
                finalString = finalString + CleanChecks(getBlock(post.section489.type, post.section489.embedcode, post.section489.text, post.section489.url));
                finalString = finalString + CleanChecks(getBlock(post.section490.type, post.section490.embedcode, post.section490.text, post.section490.url));
                finalString = finalString + CleanChecks(getBlock(post.section491.type, post.section491.embedcode, post.section491.text, post.section491.url));
                finalString = finalString + CleanChecks(getBlock(post.section492.type, post.section492.embedcode, post.section492.text, post.section492.url));
                finalString = finalString + CleanChecks(getBlock(post.section493.type, post.section493.embedcode, post.section493.text, post.section493.url));
                finalString = finalString + CleanChecks(getBlock(post.section494.type, post.section494.embedcode, post.section494.text, post.section494.url));
                finalString = finalString + CleanChecks(getBlock(post.section495.type, post.section495.embedcode, post.section495.text, post.section495.url));
                finalString = finalString + CleanChecks(getBlock(post.section496.type, post.section496.embedcode, post.section496.text, post.section496.url));
                finalString = finalString + CleanChecks(getBlock(post.section497.type, post.section497.embedcode, post.section497.text, post.section497.url));
                finalString = finalString + CleanChecks(getBlock(post.section498.type, post.section498.embedcode, post.section498.text, post.section498.url));
                finalString = finalString + CleanChecks(getBlock(post.section499.type, post.section499.embedcode, post.section499.text, post.section499.url));
                finalString = finalString + CleanChecks(getBlock(post.section500.type, post.section500.embedcode, post.section500.text, post.section500.url));
                finalString = finalString + CleanChecks(getBlock(post.section501.type, post.section501.embedcode, post.section501.text, post.section501.url));
                finalString = finalString + CleanChecks(getBlock(post.section502.type, post.section502.embedcode, post.section502.text, post.section502.url));
                finalString = finalString + CleanChecks(getBlock(post.section503.type, post.section503.embedcode, post.section503.text, post.section503.url));
                finalString = finalString + CleanChecks(getBlock(post.section504.type, post.section504.embedcode, post.section504.text, post.section504.url));
                finalString = finalString + CleanChecks(getBlock(post.section505.type, post.section505.embedcode, post.section505.text, post.section505.url));
                finalString = finalString + CleanChecks(getBlock(post.section506.type, post.section506.embedcode, post.section506.text, post.section506.url));
                finalString = finalString + CleanChecks(getBlock(post.section507.type, post.section507.embedcode, post.section507.text, post.section507.url));
                finalString = finalString + CleanChecks(getBlock(post.section508.type, post.section508.embedcode, post.section508.text, post.section508.url));
                finalString = finalString + CleanChecks(getBlock(post.section509.type, post.section509.embedcode, post.section509.text, post.section509.url));
                finalString = finalString + CleanChecks(getBlock(post.section510.type, post.section510.embedcode, post.section510.text, post.section510.url));
                finalString = finalString + CleanChecks(getBlock(post.section511.type, post.section511.embedcode, post.section511.text, post.section511.url));
                finalString = finalString + CleanChecks(getBlock(post.section512.type, post.section512.embedcode, post.section512.text, post.section512.url));
                finalString = finalString + CleanChecks(getBlock(post.section513.type, post.section513.embedcode, post.section513.text, post.section513.url));
                finalString = finalString + CleanChecks(getBlock(post.section514.type, post.section514.embedcode, post.section514.text, post.section514.url));
                finalString = finalString + CleanChecks(getBlock(post.section515.type, post.section515.embedcode, post.section515.text, post.section515.url));
                finalString = finalString + CleanChecks(getBlock(post.section516.type, post.section516.embedcode, post.section516.text, post.section516.url));
                finalString = finalString + CleanChecks(getBlock(post.section517.type, post.section517.embedcode, post.section517.text, post.section517.url));
                finalString = finalString + CleanChecks(getBlock(post.section518.type, post.section518.embedcode, post.section518.text, post.section518.url));
                finalString = finalString + CleanChecks(getBlock(post.section519.type, post.section519.embedcode, post.section519.text, post.section519.url));
                finalString = finalString + CleanChecks(getBlock(post.section520.type, post.section520.embedcode, post.section520.text, post.section520.url));
                finalString = finalString + CleanChecks(getBlock(post.section521.type, post.section521.embedcode, post.section521.text, post.section521.url));
                finalString = finalString + CleanChecks(getBlock(post.section522.type, post.section522.embedcode, post.section522.text, post.section522.url));
                finalString = finalString + CleanChecks(getBlock(post.section523.type, post.section523.embedcode, post.section523.text, post.section523.url));
                finalString = finalString + CleanChecks(getBlock(post.section524.type, post.section524.embedcode, post.section524.text, post.section524.url));
                finalString = finalString + CleanChecks(getBlock(post.section525.type, post.section525.embedcode, post.section525.text, post.section525.url));
                finalString = finalString + CleanChecks(getBlock(post.section526.type, post.section526.embedcode, post.section526.text, post.section526.url));
                finalString = finalString + CleanChecks(getBlock(post.section527.type, post.section527.embedcode, post.section527.text, post.section527.url));
                finalString = finalString + CleanChecks(getBlock(post.section528.type, post.section528.embedcode, post.section528.text, post.section528.url));
                finalString = finalString + CleanChecks(getBlock(post.section529.type, post.section529.embedcode, post.section529.text, post.section529.url));
                finalString = finalString + CleanChecks(getBlock(post.section530.type, post.section530.embedcode, post.section530.text, post.section530.url));
                finalString = finalString + CleanChecks(getBlock(post.section531.type, post.section531.embedcode, post.section531.text, post.section531.url));
                finalString = finalString + CleanChecks(getBlock(post.section532.type, post.section532.embedcode, post.section532.text, post.section532.url));
                finalString = finalString + CleanChecks(getBlock(post.section533.type, post.section533.embedcode, post.section533.text, post.section533.url));
                finalString = finalString + CleanChecks(getBlock(post.section534.type, post.section534.embedcode, post.section534.text, post.section534.url));
                finalString = finalString + CleanChecks(getBlock(post.section535.type, post.section535.embedcode, post.section535.text, post.section535.url));
                finalString = finalString + CleanChecks(getBlock(post.section536.type, post.section536.embedcode, post.section536.text, post.section536.url));
                finalString = finalString + CleanChecks(getBlock(post.section537.type, post.section537.embedcode, post.section537.text, post.section537.url));
                finalString = finalString + CleanChecks(getBlock(post.section538.type, post.section538.embedcode, post.section538.text, post.section538.url));
                finalString = finalString + CleanChecks(getBlock(post.section539.type, post.section539.embedcode, post.section539.text, post.section539.url));
                finalString = finalString + CleanChecks(getBlock(post.section540.type, post.section540.embedcode, post.section540.text, post.section540.url));
                finalString = finalString + CleanChecks(getBlock(post.section541.type, post.section541.embedcode, post.section541.text, post.section541.url));
                finalString = finalString + CleanChecks(getBlock(post.section542.type, post.section542.embedcode, post.section542.text, post.section542.url));
                finalString = finalString + CleanChecks(getBlock(post.section543.type, post.section543.embedcode, post.section543.text, post.section543.url));
                finalString = finalString + CleanChecks(getBlock(post.section544.type, post.section544.embedcode, post.section544.text, post.section544.url));
                finalString = finalString + CleanChecks(getBlock(post.section545.type, post.section545.embedcode, post.section545.text, post.section545.url));
                finalString = finalString + CleanChecks(getBlock(post.section546.type, post.section546.embedcode, post.section546.text, post.section546.url));
                finalString = finalString + CleanChecks(getBlock(post.section547.type, post.section547.embedcode, post.section547.text, post.section547.url));
                finalString = finalString + CleanChecks(getBlock(post.section548.type, post.section548.embedcode, post.section548.text, post.section548.url));
                finalString = finalString + CleanChecks(getBlock(post.section549.type, post.section549.embedcode, post.section549.text, post.section549.url));
                finalString = finalString + CleanChecks(getBlock(post.section550.type, post.section550.embedcode, post.section550.text, post.section550.url));
                finalString = finalString + CleanChecks(getBlock(post.section551.type, post.section551.embedcode, post.section551.text, post.section551.url));
                finalString = finalString + CleanChecks(getBlock(post.section552.type, post.section552.embedcode, post.section552.text, post.section552.url));
                finalString = finalString + CleanChecks(getBlock(post.section553.type, post.section553.embedcode, post.section553.text, post.section553.url));
                finalString = finalString + CleanChecks(getBlock(post.section554.type, post.section554.embedcode, post.section554.text, post.section554.url));
                finalString = finalString + CleanChecks(getBlock(post.section555.type, post.section555.embedcode, post.section555.text, post.section555.url));
                finalString = finalString + CleanChecks(getBlock(post.section556.type, post.section556.embedcode, post.section556.text, post.section556.url));
                finalString = finalString + CleanChecks(getBlock(post.section557.type, post.section557.embedcode, post.section557.text, post.section557.url));
                finalString = finalString + CleanChecks(getBlock(post.section558.type, post.section558.embedcode, post.section558.text, post.section558.url));
                finalString = finalString + CleanChecks(getBlock(post.section559.type, post.section559.embedcode, post.section559.text, post.section559.url));
                finalString = finalString + CleanChecks(getBlock(post.section560.type, post.section560.embedcode, post.section560.text, post.section560.url));
                finalString = finalString + CleanChecks(getBlock(post.section561.type, post.section561.embedcode, post.section561.text, post.section561.url));
                finalString = finalString + CleanChecks(getBlock(post.section562.type, post.section562.embedcode, post.section562.text, post.section562.url));
                finalString = finalString + CleanChecks(getBlock(post.section563.type, post.section563.embedcode, post.section563.text, post.section563.url));
                finalString = finalString + CleanChecks(getBlock(post.section564.type, post.section564.embedcode, post.section564.text, post.section564.url));
                finalString = finalString + CleanChecks(getBlock(post.section565.type, post.section565.embedcode, post.section565.text, post.section565.url));
                finalString = finalString + CleanChecks(getBlock(post.section566.type, post.section566.embedcode, post.section566.text, post.section566.url));
                finalString = finalString + CleanChecks(getBlock(post.section567.type, post.section567.embedcode, post.section567.text, post.section567.url));
                finalString = finalString + CleanChecks(getBlock(post.section568.type, post.section568.embedcode, post.section568.text, post.section568.url));
                finalString = finalString + CleanChecks(getBlock(post.section569.type, post.section569.embedcode, post.section569.text, post.section569.url));
                finalString = finalString + CleanChecks(getBlock(post.section570.type, post.section570.embedcode, post.section570.text, post.section570.url));
                finalString = finalString + CleanChecks(getBlock(post.section571.type, post.section571.embedcode, post.section571.text, post.section571.url));
                finalString = finalString + CleanChecks(getBlock(post.section572.type, post.section572.embedcode, post.section572.text, post.section572.url));
                finalString = finalString + CleanChecks(getBlock(post.section573.type, post.section573.embedcode, post.section573.text, post.section573.url));
                finalString = finalString + CleanChecks(getBlock(post.section574.type, post.section574.embedcode, post.section574.text, post.section574.url));
                finalString = finalString + CleanChecks(getBlock(post.section575.type, post.section575.embedcode, post.section575.text, post.section575.url));
                finalString = finalString + CleanChecks(getBlock(post.section576.type, post.section576.embedcode, post.section576.text, post.section576.url));
                finalString = finalString + CleanChecks(getBlock(post.section577.type, post.section577.embedcode, post.section577.text, post.section577.url));
                finalString = finalString + CleanChecks(getBlock(post.section578.type, post.section578.embedcode, post.section578.text, post.section578.url));
                finalString = finalString + CleanChecks(getBlock(post.section579.type, post.section579.embedcode, post.section579.text, post.section579.url));
                finalString = finalString + CleanChecks(getBlock(post.section580.type, post.section580.embedcode, post.section580.text, post.section580.url));
                finalString = finalString + CleanChecks(getBlock(post.section581.type, post.section581.embedcode, post.section581.text, post.section581.url));
                finalString = finalString + CleanChecks(getBlock(post.section582.type, post.section582.embedcode, post.section582.text, post.section582.url));
                finalString = finalString + CleanChecks(getBlock(post.section583.type, post.section583.embedcode, post.section583.text, post.section583.url));
                finalString = finalString + CleanChecks(getBlock(post.section584.type, post.section584.embedcode, post.section584.text, post.section584.url));
                finalString = finalString + CleanChecks(getBlock(post.section585.type, post.section585.embedcode, post.section585.text, post.section585.url));
                finalString = finalString + CleanChecks(getBlock(post.section586.type, post.section586.embedcode, post.section586.text, post.section586.url));
                finalString = finalString + CleanChecks(getBlock(post.section587.type, post.section587.embedcode, post.section587.text, post.section587.url));
                finalString = finalString + CleanChecks(getBlock(post.section588.type, post.section588.embedcode, post.section588.text, post.section588.url));
                finalString = finalString + CleanChecks(getBlock(post.section589.type, post.section589.embedcode, post.section589.text, post.section589.url));
                finalString = finalString + CleanChecks(getBlock(post.section590.type, post.section590.embedcode, post.section590.text, post.section590.url));
                finalString = finalString + CleanChecks(getBlock(post.section591.type, post.section591.embedcode, post.section591.text, post.section591.url));
                finalString = finalString + CleanChecks(getBlock(post.section592.type, post.section592.embedcode, post.section592.text, post.section592.url));
                finalString = finalString + CleanChecks(getBlock(post.section593.type, post.section593.embedcode, post.section593.text, post.section593.url));
                finalString = finalString + CleanChecks(getBlock(post.section594.type, post.section594.embedcode, post.section594.text, post.section594.url));
                finalString = finalString + CleanChecks(getBlock(post.section595.type, post.section595.embedcode, post.section595.text, post.section595.url));
                finalString = finalString + CleanChecks(getBlock(post.section596.type, post.section596.embedcode, post.section596.text, post.section596.url));
                finalString = finalString + CleanChecks(getBlock(post.section597.type, post.section597.embedcode, post.section597.text, post.section597.url));
                finalString = finalString + CleanChecks(getBlock(post.section598.type, post.section598.embedcode, post.section598.text, post.section598.url));
                finalString = finalString + CleanChecks(getBlock(post.section599.type, post.section599.embedcode, post.section599.text, post.section599.url));




            }
            catch (Exception ex)
            {
                return finalString;
            }

            return finalString;
        }


        #endregion


        string CleanChecks(string text)
        {
            try
            {
                if (text == string.Empty) return string.Empty;

                text = Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");

                if (!text.Contains("<p>") && !text.Contains("</p>"))
                {
                    return $"<p>{text}</p> {Environment.NewLine}";

                }

                return text + Environment.NewLine;
            }
            catch (Exception)
            {
                throw;
            }

        }

        List<string> GetArticles(string filePath, int startLine, int endLine)
        {
            // Adjust for 0-based indexing if your line numbers are 1-based.
            // If startLine and endLine are already 0-based, remove the -1.
            int skipCount = startLine - 1;
            int takeCount = endLine - startLine + 1;

            if (skipCount < 0 || takeCount < 0)
            {
                throw new ArgumentException("Invalid line range. Start line and end line must be positive, and endLine must be greater than or equal to startLine.");
            }

            try
            {
                // Use File.ReadLines for memory efficiency with large files.
                // Skip the lines before the start of the range, then take the specified number of lines.
                return File.ReadLines(filePath)
                           .Skip(skipCount)
                           .Take(takeCount)
                           .ToList();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found at {filePath}");
                return new List<string>(); // Return empty list on error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<string>(); // Return empty list on error
            }
        }


        void ExtractFeaturedImage()
        {
            // Get all subdirectories in the specified path
            var DIR2024 = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024";
            string[] directories = { @$"{DIR2024}\6",
                             @$"{DIR2024}\5",
                             @$"{DIR2024}\7"};


            string subpath = "";
            bool Is2ndLevel = false;
            bool IsImageNext = false;
            // Loop through each directory
            foreach (string directory in directories)
            {
                var tempArry = directory.Split("\\");
                subpath = $@"{tempArry[tempArry.Length - 2]}\{tempArry[tempArry.Length - 1]}";

                Console.WriteLine(directory);
                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    Console.WriteLine($"Found file: {filePath}");
                    
                    BeautifyXML(filePath);
                    DeleteFolderTypeNode(filePath);
                    AppendSingleQoute(filePath);
                    
                    string[] lines = File.ReadAllLines(filePath);
                    string imageSource = "";

                    for (int i = 0; i < lines.Length; i++)
                    {

                        //check for filename
                        if (!IsImageNext && lines[i].Equals("'        <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                        {
                            imageSource = lines[i + 1].Replace("<sv:value>", "").Replace("</sv:value>", "").Replace("'","").Trim();
                            Is2ndLevel = false;
                            IsImageNext=true;
                        }

                        if (!IsImageNext && lines[i].Equals("'            <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                        {
                            imageSource = lines[i + 1].Replace("<sv:value>", "").Replace("</sv:value>", "").Replace("'", "").Trim();
                            Is2ndLevel = true;
                            IsImageNext = true;
                        }

                        //check for actual base64 string images for parsing
                        if (Is2ndLevel)
                        {
                            if (lines[i].Contains("<sv:property sv:name=\"jcr:data\" sv:type=\"Binary\">"))
                            {
                                Base64StringToJpeg(lines[i + 1].Replace("'                    <sv:value>", "").Replace("</sv:value>", ""), imageSource.Trim(), subpath);
                                IsImageNext = false;
                            }
                        }
                        else
                        {
                            if (lines[i].Contains("<sv:property sv:name=\"jcr:data\" sv:type=\"Binary\">"))
                            {
                                Base64StringToJpeg(lines[i + 1].Replace("'                <sv:value>", "").Replace("</sv:value>", ""), imageSource.Trim(), subpath);
                                IsImageNext = false;
                            }
                        }
                    }

                    RemoveSingleQoute(filePath);
                }

            }

        }

        void Base64StringToJpeg(string base64String, string fileName, string subPath)
        {
            try
            {
                string dirDestination = $@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp-extracted\{subPath}";

                if (!Directory.Exists(dirDestination))
                {
                    Directory.CreateDirectory(dirDestination);
                }

                string filePath = $@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp-extracted\{subPath}\{fileName}.jpg";
                File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: Invalid Base64 string. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void ProcessAuthors()
        {

            // Get all subdirectories in the specified path
            var nwp_dir = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\articles\nwp\";
            string[] directories = {
                             @$"{nwp_dir}\2024\5",
                             @$"{nwp_dir}\2024\6",
                             @$"{nwp_dir}\2024\7",
                             @$"{nwp_dir}\2024\8",
                             @$"{nwp_dir}\2024\9",
                             @$"{nwp_dir}\2024\10",
                             @$"{nwp_dir}\2024\11",
                             @$"{nwp_dir}\2024\12",
                             @$"{nwp_dir}\2025\1",
                             @$"{nwp_dir}\2025\2",
                             @$"{nwp_dir}\2025\3",
                             @$"{nwp_dir}\2025\4",
                             @$"{nwp_dir}\2025\5",
                             @$"{nwp_dir}\2025\6",
                             @$"{nwp_dir}\2025\7",
                             @$"{nwp_dir}\2025\8",

    };

            var searchText = "'author':";
            // Loop through each directory
            List<string> authorsList = new List<string>();
            foreach (string directory in directories)
            {

                Console.WriteLine(directory);

                foreach (string filePath in Directory.EnumerateFiles(directory))
                {
                    Console.WriteLine($"Found file: {filePath}");

                    // Use File.ReadLines to read lines lazily and efficiently
                    var matchingLines = File.ReadLines(filePath)
                                            .Where(item => item.Contains(searchText));

                    string[] authors = matchingLines.Select(x => x.Replace($"{searchText}", "").Replace("\'", "").Trim()).Distinct().ToArray();
                    if (authors.Length > 0)
                    {
                        authorsList.AddRange(authors);
                    }

                }

            }


            int i = 0;
            int user_id = 7;
            List<string> wp_users = new List<string>();
            List<string> wp_usermeta = new List<string>();

            authorsList = authorsList.OrderBy(authorsList => authorsList).ToList();

            foreach (var author in authorsList.Distinct())
            {

                user_id++;
                i++;
                wp_users.Add($"INSERT INTO `wp_users` ( `ID`,`user_login`, `user_pass`, `user_nicename`, `user_email`, `user_url`, `user_registered`, `user_activation_key`, `user_status`, `display_name`) VALUES({user_id},'migrateduser-{i}', '$wp$2y$10$GveTsPNj/qlcYltZ7sctouaGAgwxGsCs83u.HXbm.XabHUDa0w/jy', 'migrateduser-{i}', 'migrated.user.{i}@newswatchplus.ph', 'https://newswatchplus-staging.azurewebsites.net/author/migrateduser-{i}', '2025-09-05 07:22:19', '', 0, '{author}');");

                wp_usermeta.Add($"INSERT INTO `wp_usermeta` (`user_id`, `meta_key`, `meta_value`) VALUES" +
                $"( {user_id}, 'nickname', 'migrateduser-{i}')," +
                $"( {user_id}, 'first_name', '{author}')," +
                $"( {user_id}, 'last_name', '')," +
                $"( {user_id}, 'rich_editing', 'true')," +
                $"( {user_id}, 'syntax_highlighting', 'true')," +
                $"( {user_id}, 'comment_shortcuts', 'false')," +
                $"( {user_id}, 'admin_color', 'fresh')," +
                $"( {user_id}, 'show_admin_bar_front', 'true')," +
                "(" + user_id + ", 'wp_capabilities', 'a:1:{s:6:\"author\";b:1;}' )," +
                $"( {user_id}, 'wp_user_level', '2');");

            }

            var _user = wp_users;
            var _usermeta = wp_usermeta;
        }
    }

    private static string CleanApostrophe(string yml)
    {
        
        string[] lines = yml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);





        // Iterate through each line using a foreach loop
        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"(\w+'\w+)|(\w+' \w+)|(\w+''\w+)|(\w+'' \w+)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "’");
                lines[i] = lines[i].Replace(match.Value, newValue);
            }

        }

        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"(\w+ '\w+)|(\w+ ''\w+)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "‘");
                lines[i] = lines[i].Replace(match.Value, newValue);
            }


        }


        var revertFormat = string.Join(Environment.NewLine, lines);
        return revertFormat;
    }

    private static string CleanStories(string yml)
    {

        string[] lines = yml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Iterate through each line using a foreach loop
        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"stories: '\w+";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "’");
                lines[i] = "    stories: ''";
            }

        }


        var revertFormat = string.Join(Environment.NewLine, lines);
        return revertFormat;
    }
    private static void SaveDataToDatabase(string wP_Post_Article_InsertSql, string wP_PostMeta, string wP_term_relationships)
    {
        //string connStr = "server=nwpstaging-0dea0b440a-wpdbserver.mysql.database.azure.com;user=jdchodieso;database=nwpstaging_0dea0b440a_database;password=gJPcCa2O6yB$jfTm;";
        //MySqlConnection conn = new MySqlConnection(connStr);
        //conn.Open();

        
        //var wp_post = new MySqlCommand(wP_Post_Article_InsertSql, conn);
        //wp_post.ExecuteNonQuery();

        //var wp_postMeta = new MySqlCommand(wP_PostMeta, conn);
        //wp_postMeta.ExecuteNonQuery();


        //var wP_term = new MySqlCommand(wP_term_relationships, conn);
        //wP_term.ExecuteNonQuery();


        //conn.Close();

        Console.WriteLine("Saved");
    }

    /// <summary>
    /// Todo
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void BeautifyXML(string filePath)
    {
        throw new NotImplementedException();
    }

    private static void DeleteFolderTypeNode(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        try
        {
            for (int i = 0; i < lines.Length; i++)
            {
                //check for filename
                if (lines[i].Contains("<sv:value>mgnl:folder</sv:value>"))
                {
                    for (int j = i; j <= i + 10; j++)
                    {
                        lines[j] = "";
                    }

                    File.WriteAllLines(filePath, lines);
                }


            }
        }
        catch (Exception)
        {

            
        }

        
    }
}

