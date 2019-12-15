

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace santa_shares
{
    public class Trader
    {

        private User user { get; set; }
        private Items inventory { get; set; }
        private AuthenticationHeaderValue auth { get; set; }
        private List<Item[]> itemHistory { get; set; } = new List<Item[]>();

        private Random randomSource = new Random();

        public Trader(User user)
        {
            this.user = user;
            auth = new AuthenticationHeaderValue("token", user.token);
        }

        public async Task Run()
        {
            while (true)
            {
                try
                {
                    User userStatus = await GetUserStatus();
                    if (userStatus.balance<133700000){
                        int diff = 133700000-userStatus.balance;
                        int minDiff = userStatus.items.Select(i=>Math.Abs(i.price-diff)).Min();
                        Console.WriteLine(minDiff);
                        Item item = userStatus.items.Where(i=>Math.Abs(i.price-diff)==minDiff).FirstOrDefault();
                        await Sell(item, 1);
                    } else if (userStatus.balance == 133700000) {
                        Thread.Sleep(1000);
                        Console.WriteLine("Done.");
                    } else {
                        Item[] items = await GetItemList();
                        int diff = userStatus.balance-133700000;
                        int minDiff = items.Where(i=>i.amount>0).Select(i=>Math.Abs(i.price-diff)).Min();
                        Console.WriteLine(minDiff);
                        Item item = items.Where(i=>i.amount>0).Where(i=>Math.Abs(i.price-diff)==minDiff).FirstOrDefault();
                        await Buy(item, 1);
                    }
                   
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async Task<Item[]> GetItemList()
        {
            HttpClient client = new HttpClient();
            Item[] items = await client.GetTypeAsJsonAsync<Item[]>(Program.APIUrl + "items", auth);
            return items;
        }

        private async Task<Item[]> GetUserInventory()
        {
            HttpClient client = new HttpClient();
            User userStatus = await client.GetTypeAsJsonAsync<User>(Program.APIUrl + "users/" + user.user_id, auth);
            return userStatus.items;
        }

        private async Task<User> GetUserStatus()
        {
            HttpClient client = new HttpClient();
            User userStatus = await client.GetTypeAsJsonAsync<User>(Program.APIUrl + "users/" + user.user_id, auth);
            return userStatus;
        }

        private async Task<bool> Buy(Item item, int qty)
        {
            Item itemRequest = new Item()
            {
                item_name = item.item_name,
                price = item.price,
                item_id = item.item_id,
                amount = qty
            };
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(Program.APIUrl + "buy", itemRequest, auth);
            return httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Created;
        }

        private async Task<bool> Sell(Item item, int qty)
        {
            Item itemRequest = new Item()
            {
                item_name = item.item_name,
                price = item.price,
                item_id = item.item_id,
                amount = qty
            };
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(Program.APIUrl + "sell", itemRequest, auth);
            return httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Created;
        }
    }
}