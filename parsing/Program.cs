using System;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.Linq;
using System.Collections.Generic;
using Npgsql;

namespace parsing
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                ApplicationId = 7479662,
                Login = "79196861086",
                #region
                Password = "Lublyucs16",
                #endregion
                Settings = Settings.All,
            });
            var get = api.Wall.Get(new WallGetParams
            {
                OwnerId = -35488145,
                Count = 200
            });
            Console.WriteLine(get.WallPosts.Count);
            var List = new List<string>();
            var Dictionary = new Dictionary<string, int>();
            foreach (var post in get.WallPosts)
            {
                var list = post.Text.Split(' ').ToList();
                foreach (var word in list)
                {
                    if (Dictionary.ContainsKey(word))
                    {
                        Dictionary[word]++;
                    }
                    else
                        Dictionary.Add(word, 1);
                }
            }
            foreach(var dic in Dictionary)
            {
                if (dic.Key == string.Empty)
                    Dictionary.Remove(dic.Key);
            }
            var count = 0;
            var temp = 1;
            var dict = Dictionary.OrderByDescending(x => x.Value).ToList();
            Dictionary<string, int> top100 = new Dictionary<string, int>();
            foreach(var dic in dict)
            {
                if (dic.Value == 1)
                    count++;
                if (temp <= 100)
                {
                    top100.Add(dic.Key, dic.Value);
                    temp++;
                }
            }
            Console.WriteLine(count  + " - кол-во уникальных слов");
            foreach(var word in top100)
            {
                Console.WriteLine(word.Key + " - " + word.Value);
            }

            //Заполняем таблицу


            var cs = "Host=localhost;Username=postgres;Password=Ruzesygan1;Database=Metabase;";
            using (var con = new NpgsqlConnection(cs))
            {
                try
                {
                    con.Open();
                    using var cmd = new NpgsqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "delete from words";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
            foreach (var word in top100)
            {
                using (var con = new NpgsqlConnection(cs))
                {
                    try
                    {
                        con.Open();
                        using var cmd = new NpgsqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "INSERT INTO words(key, Value) VALUES(@key, @Value)";
                        cmd.Parameters.AddWithValue("key", word.Key);
                        cmd.Parameters.AddWithValue("Value", word.Value);
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }

        }
    }
}
