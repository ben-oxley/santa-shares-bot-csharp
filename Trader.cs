

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
                    //Buy an item
                    Item[] items = await GetItemList();
                    Item[] userItems = await GetUserInventory();
                    foreach (var userItem in userItems)
                    {
                        await Sell(userItem, userItem.amount);
                    }
                    User userStatus = await GetUserStatus();
                    itemHistory.Add(items);
                    if (itemHistory.Count > 100) itemHistory.RemoveAt(0);
                    Item[] oldestHistory = itemHistory[0];
                    if (itemHistory.Count > 1)
                    {
                        IEnumerable<IGrouping<double, Item>> enumerable = items.Where(i => i.amount > 0).GroupBy(i =>
                        {
                            Item item2 = oldestHistory.Where(i => i.item_id == i.item_id).FirstOrDefault();
                            double gradient = (i.price - item2.price) / (double)itemHistory.Count;
                            return gradient;
                        });
                        List<IGrouping<double, Item>> list = enumerable.OrderByDescending(i => i.Key).ToList();
                        int funds = userStatus.balance;
                        bool finished = false;
                        foreach (var profitableItemGroup in list)
                        {
                            foreach (var profitableItem in profitableItemGroup)
                            {
                                if (profitableItem.price * profitableItem.amount > funds)
                                {
                                    finished = true;
                                    break;
                                }

                                await Buy(profitableItem, profitableItem.amount);
                                funds -= profitableItem.price * profitableItem.amount;
                            }
                            if (finished) break;
                        }

                    }
                    Thread.Sleep(60000);
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