

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
    public class Monitor{
        private static readonly string LocalFile = Environment.ExpandEnvironmentVariables("%APPDATA%/santa/pricehistory.csv");
        private Dictionary<string,List<int>> history{get;set;} = new Dictionary<string, List<int>>();


        public Monitor(){
        }

        public async Task Run(){
            
            
            while(true){
                try{
                    //Buy an item
                    Item[] items = await GetItemList();
                    if (!Directory.Exists(Environment.ExpandEnvironmentVariables("%APPDATA%/santa/"))) Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%APPDATA%/santa/"));
                    if (!File.Exists(LocalFile))  await File.AppendAllTextAsync(LocalFile, String.Join(',',items.OrderBy(i=>i.item_id).Select(i=>i.item_name+i.item_id))+"\r\n");
                    Thread.Sleep(60000);
                    foreach(var item in items){
                        if (!history.ContainsKey(item.item_name)) history[item.item_name] = new List<int>();
                        history[item.item_name].Append(item.price);
                    }
                    await File.AppendAllTextAsync(LocalFile, String.Join(',',items.OrderBy(i=>i.item_id).Select(i=>i.price))+"\r\n");
                } catch (Exception e){
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async Task<Item[]> GetItemList()
        {
            HttpClient client = new HttpClient();
            Item[] items = await client.GetTypeAsJsonAsync<Item[]>(Program.APIUrl + "items");
            return items;
        }

    
    }
}