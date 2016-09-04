namespace AtLeastOnceDelivery.Console
{
    using System;
    using Akka.Actor;

    public class MyRecipientActor : ReceiveActor
    {
        public MyRecipientActor()
        {
            Receive<ReliableDeliveryEnvelope<Write>>(write =>
            {
                Sender.Tell(new ReliableDeliveryAck(write.MessageId));
                Console.WriteLine($"Message: {write.MessageId}");
            });
        }
    }
}