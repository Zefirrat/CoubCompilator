using System;
using CoubCompilator.Http;
using Newtonsoft.Json;

namespace CoubCompilator.CoubClasses
{
    public class CoubAPI
    {
        /// <summary>
        /// Loading pages of result CoubAPI
        /// </summary>
        /// <param name="page">Page to load</param>
        /// <param name="per_page">Results per page (MAX 25)</param>
        /// <param name="section">Category load from</param>
        /// <returns></returns>
        public Welcome GetPage(int page, int per_page, Order order = Order.newest_popular, string section = "hot", string url = "https://coub.com/api/v2/timeline/")
        {
            Postman postman = new Postman();
            Console.WriteLine("Postman is here.");
            string uri = url + section + "?page=" + page + "&per_page=" + per_page + "&order_by=" + nameof(order);
            Console.WriteLine($"Get uri: {uri}");
            string resultGet = postman.Get(uri);
            Console.WriteLine($"Result is not empty: {string.IsNullOrEmpty(resultGet)}");
            Console.WriteLine("Parsing json.");
            Welcome coubResult = Welcome.FromJson(resultGet);
            Console.WriteLine("Json parsed successfully.");
            return coubResult;
        }

        public enum Order
        {
            likes_count, views_count, newest_popular, oldest

        }
    }
}
