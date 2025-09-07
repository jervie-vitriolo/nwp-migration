using NWP_DB_Migration.Article;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

List<string[]> articleList = new List<string[]>();
articleList.Add(new[] { "28,197" });



var fileName = @"C:\temp\stories.nwp.2025.9.yaml";

//ProcessAuthors(fileName, "author:");


foreach (var range in articleList)
{
    // Split the string by comma and convert each part to an integer
    int[] intArray = range.FirstOrDefault().Split(',')
                    .Select(s => int.Parse(s.Trim())) // Trim to handle potential whitespace
                    .ToArray();

    int startLine = intArray[0];
    int endLine = intArray[1];

    List<string> articles = GetArticles(fileName, startLine, endLine);
    string yml = string.Join("\n", articles);

    //start parsing
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
        .Build();

    
    //yml contains a string containing your YAML
    var p = deserializer.Deserialize<Post>(yml);
    if (p != null)
    {
        CreatetSqlInsert(p);
    }

    
    //Author
    //Get the Id of the author from user table
    //author 38, category 36

}


void CreatetSqlInsert(Post post){

    string InsertString = $"INSERT INTO `wp_posts` ( `post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) " +
                          $"VALUES( '{getPostAuthorID(post.author)}', '{post.created}', '{post.created}', '{getPostContent(post)}', '{post.title}', '{post.caption}', '{getPostStatus(post)}', 'open', 'open', '', '{post.title.Replace(" ","-")}', '', '', '{post.lastmodified}', '{post.lastmodified}', '', 0, 'https://newswatchplus-staging.azurewebsites.net/?p=', 0, 'post', '', 0);";

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
    AuthorsList AuthorsList = new AuthorsList();
    var ID = AuthorsList.GetAuthors().FirstOrDefault(s => s.Author == name).ID;
    return ID;
}

string getPostContent(Post post)
{
    return "";
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
static void ProcessAuthors(string filePath, string searchText)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Error: File not found at {filePath}");
        return;
    }

    try
    {
        // Use File.ReadLines to read lines lazily and efficiently
        var matchingLines = File.ReadLines(filePath)
                                .Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                .Where(item => item.LineText.Contains(searchText));

        string[] authors = matchingLines.Select(x => x.LineText.Replace("author:","").Replace("\'","").Trim()).Distinct().ToArray();
        List<string> wp_users = new List<string>();
        List<string> wp_usermeta = new List<string>();

        if (matchingLines.Any())
        {
            int i = 0;
            int user_id = 6;
            foreach (var author in authors)
            {
                user_id++;
                i++;
                wp_users.Add($"INSERT INTO `wp_users` ( `ID`,`user_login`, `user_pass`, `user_nicename`, `user_email`, `user_url`, `user_registered`, `user_activation_key`, `user_status`, `display_name`) VALUES({user_id},'migrateduser-{i}', '$wp$2y$10$GveTsPNj/qlcYltZ7sctouaGAgwxGsCs83u.HXbm.XabHUDa0w/jy', 'migrateduser-{i}', 'migrated.user.{i}@newswatchplus.ph', 'https://newswatchplus-staging.azurewebsites.net/author/migrateduser-{i}', '2025-09-05 07:22:19', '', 0, '{author}');");

                wp_usermeta.Add($"INSERT INTO `wp_usermeta` (`user_id`, `meta_key`, `meta_value`) VALUES" +
                $"( {user_id}, 'nickname', 'migrateduser-{i}')," +
                $"( {user_id}, 'first_name', '{author}'),"+
                $"( {user_id}, 'last_name', ''),"+
                $"( {user_id}, 'rich_editing', 'true'),"+
                $"( {user_id}, 'syntax_highlighting', 'true'),"+
                $"( {user_id}, 'comment_shortcuts', 'false'),"+
                $"( {user_id}, 'admin_color', 'fresh'),"+
                $"( {user_id}, 'show_admin_bar_front', 'true'),"+
                "(" + user_id + ", 'wp_capabilities', 'a:1:{s:6:\"author\";b:1;}' ),"+
                $"( {user_id}, 'wp_user_level', '2');");

            }

        }
        else
        {
            Console.WriteLine($"'{searchText}' not found in the file.");
        }
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Error: File not found at '{filePath}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}