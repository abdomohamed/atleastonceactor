namespace AtLeastOnceDelivery.Console
{
    using System;
    using Akka.Actor;
    using Akka.Persistence;

    public class MyAtLeastOnceDeliveryActor : AtLeastOnceDeliveryReceiveActor
    {
        private readonly IActorRef _targetActor;
        public override string PersistenceId => "AtleastOnceActor";
        
        public MyAtLeastOnceDeliveryActor(IActorRef targetActor)
        {
            _targetActor = targetActor;

            Recover<Write>((m) =>
            {
                Console.WriteLine($"recover write message: {m}");
            });

            Recover<SnapshotOffer>(offer =>
            {
                var snapshot = offer.Snapshot as Akka.Persistence.AtLeastOnceDeliverySnapshot;
                SetDeliverySnapshot(snapshot);
            });

            Command<Write>(write =>
            {
                Persist(write, write1 =>
                {
                    Deliver(_targetActor.Path, messageId => new ReliableDeliveryEnvelope<Write>(write, messageId));

                    SaveSnapshot(GetDeliverySnapshot());
                });
            });

            Command<ReliableDeliveryAck>(ack =>
            {
                ConfirmDelivery(ack.MessageId);
            });
        }
    }
}