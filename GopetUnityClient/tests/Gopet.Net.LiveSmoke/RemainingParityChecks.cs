using System;
using Gopet.Net.Pet;
using Gopet.Net.Social;

namespace Gopet.Net.LiveSmoke
{
    internal static class RemainingParityChecks
    {
        public static void Run(GopetSocket socket, MessageRouter router)
        {
            var pet = new PetProfileHandler();
            PetProfile profile = null;
            PetGymState gym = null;
            pet.ProfileReceived += value => profile = value;
            pet.GymReceived += value => gym = value;
            pet.RegisterOn(router);

            var letters = new LetterHandler();
            Mailbox mailbox = null;
            letters.MailboxReceived += value => mailbox = value;
            letters.RegisterOn(router);

            socket.Send(PetProfilePackets.RequestProfile());
            MessagePump.Until(socket, router, () => profile != null, TimeSpan.FromSeconds(5));
            Report.Check("JJ. MAGIC trả profile pet thật và parser đọc sạch",
                profile != null && profile.TemplateId > 0,
                profile == null ? "không nhận MAGIC" : $"template={profile.TemplateId}");

            socket.Send(PetProfilePackets.RequestGym());
            MessagePump.Until(socket, router, () => gym != null, TimeSpan.FromSeconds(5));
            Report.Check("KK. GYM trả ba lựa chọn tiềm năng",
                gym != null && gym.Options.Length == 3,
                gym == null ? "không nhận GYM" : $"options={gym.Options.Length}");

            socket.Send(LetterPackets.RequestMailbox());
            MessagePump.Until(socket, router, () => mailbox != null, TimeSpan.FromSeconds(5));
            Report.Check("LL. LETTER_BOX trả mailbox thật và parser đọc sạch",
                mailbox != null,
                "không nhận LETTER_BOX");
        }
    }
}
