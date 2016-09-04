using Akka.Configuration;
using MongoDB.Bson.Serialization;

namespace AtLeastOnceDelivery.Console
{
    using Akka.Actor;

    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                persistence {
                    journal {
                            # Path to the journal plugin to be used
                            plugin = ""akka.persistence.journal.mongodb""

                            # In-memory journal plugin.
                            mongodb {
                                # Class name of the plugin.
                                class = ""Akka.Persistence.MongoDb.Journal.MongoDbJournal, Akka.Persistence.MongoDb""
                            
                                connection-string = ""mongodb://localhost:27017/SampleSystem""
                                collection = ""Journal""
                                # Dispatcher for the plugin actor.
                                plugin-dispatcher = ""akka.actor.default-dispatcher""
                        }
                    }
                snapshot-store {

                        # Path to the snapshot store plugin to be used
                        plugin = ""akka.persistence.snapshot-store.mongodb""

                        # Local filesystem snapshot store plugin.
                        mongodb {
                            # Class name of the plugin.
                            class = ""Akka.Persistence.MongoDb.Snapshot.MongoDbSnapshotStore, Akka.Persistence.MongoDb""
                            
                            connection-string = ""mongodb://localhost:27017/SampleSystem""
                            collection = ""Snapshot""
                            # Dispatcher for the plugin actor.
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                        }
                    }
                }
                }
            ");

            BsonClassMap.RegisterClassMap<Write>();

            using (var actorSystem = ActorSystem.Create("AtLeastOnceDeliveryDemo", config))
            {
                var recipientActor = actorSystem.ActorOf(Props.Create(() => new MyRecipientActor()), "receiver");
                var atLeastOnceDeliveryActor =
                    actorSystem.ActorOf(Props.Create(() => new MyAtLeastOnceDeliveryActor(recipientActor)), "delivery");

                atLeastOnceDeliveryActor.Tell(new Write("Hello"));

                actorSystem.WhenTerminated.Wait();
            }
        }
    }
}
