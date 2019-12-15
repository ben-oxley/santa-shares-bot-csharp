

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
            int time = (int)(DateTime.Now.Second / 60.0);
            while (true)
            {
                try
                {
                    User userStatus = await GetUserStatus();
                    foreach (var item in userStatus.items)
                    {
                        await Sell(item, item.amount);
                    }
                    while (time == DateTime.Now.Minute && DateTime.Now.Second<30)
                    {
                        Thread.Sleep(100);
                    }
                    time = DateTime.Now.Minute;
                    //Buy an item
                    Item[] items = await GetItemList();
                    
                    
                    List<Task> taskList = new List<Task>();

                    foreach (var itemToBuy in items.Where(i=>i.amount>0))
                    {
                        for (int i = 0; i < 100; i++) taskList.Add(Buy(itemToBuy, itemToBuy.amount));
                        
                    }
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();

                    while (time == DateTime.Now.Minute && DateTime.Now.Second<30)
                    {
                        Thread.Sleep(100);
                    }
                    time = DateTime.Now.Minute;
                    
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