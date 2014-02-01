using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Net.DDP.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Net.DDP.SignalR;
namespace Net.DDP.Client.Test
{
    internal class Program
    {
        private static List<Message> _messages = new List<Message>(); 

        private static void Main(string[] args)
        {
            new Thread(() =>
            {
                var subscriber = new MeteorSubscriber();
                var client = new DDPClient(subscriber);

                // TODO; hack
                subscriber.Client = client;

                client.Connect("localhost:3000");

                subscriber.Bind(_messages, "Items", "all-items");

                while (true)
                {
                    Thread.Sleep(100);
                }
            }).Start();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }

    public class MeteorSubscriber : IDataSubscriber
    {
        public DDPClient Client { get; set; }

        private readonly Dictionary<string, List<IBinding<object>>> _bindings = new Dictionary<string, List<IBinding<object>>>(); 

        public MeteorSubscriber()
        {
            
        }

        public void DataReceived(string data)
        {
            Console.WriteLine(data);

            var message = JsonConvert.DeserializeObject<Message>(data);

            switch (message.Type)
            {
                case "added":
                    var added = JsonConvert.DeserializeObject<AddedMessage>(data);

                    var bindings = _bindings[added.Collection];

                    foreach (var binding in bindings)
                    {
                        binding

                        Console.WriteLine();
                    }
                    //subscription.onEvent += (s, e) =>
                    //{
                    //    Console.WriteLine(e.Data);

                    //    var message = JsonConvert.DeserializeObject<Message>(e.Data);

                    //    switch (message.Type)
                    //    {
                    //        case "added":

                    //            var fields = e.Data.GetFields();

                    //            var newObject = JsonConvert.DeserializeObject<T>(fields);

                    //            list.Add(newObject);
                    //            break;
                    //    }

                    //    Console.WriteLine();
                    //};

                    Console.WriteLine();
                    break;
            }

            Console.WriteLine();

            //try
            //{
            //    if (data.type == "added")
            //    {
            //        Console.WriteLine(data.prodCode + ": " + data.prodName + ": collection: " + data.collection);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }

        public void Bind<T>(List<T> list, string collectionName, string subscribeTo)
            where T : new()
        {
            if (!_bindings.ContainsKey(collectionName))
                _bindings.Add(collectionName, new List<IBinding<object>>());

            _bindings[collectionName].Add(new Binding<List<T>>(list));

            Client.Subscribe(subscribeTo);
        }

        private interface IBinding<T>
            where T : new()
        {
            T Target { get; }
        }

        private class Binding<T> : IBinding<T>
            where T : new()
        {
            public T Target {get; private set; }

            public Binding(T target)
            {
                Target = target;
            }
        }

        //private class CollectionBinding<L, T> : IBinding
        //    where L : IList<T>
        //{
        //    private L _target;
        //    private T _generic;

        //    public CollectionBinding(L target, T generic)
        //    {
        //        _target = target;
        //        _generic = generic;
        //    }
        //}
    }

    public class Message
    {
        [JsonProperty("msg")]
        public string Type { get; set; }
    }

    public abstract class Collection : Message
    {
        [JsonProperty("collection")]
        public string CollectionName { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    public class AddedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }

    public class ChangedMessage : Message
    {
        public string Collection { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> Fields { get; set; }
    }
}
