using System;

namespace Gopet.Net.Kiosk
{
    /// <summary>Owner-side kiosk state sent by PET_SERVICE/KIOSK.</summary>
    public sealed class KioskListing
    {
        public sbyte Type;
        public bool HasListing;
        public int ItemId;
        public string FrameImagePath;
        public string Name;
        public string Description;
        public int RemainingSeconds;
        public sbyte FrameCount;
    }

    /// <summary>Parses the custom kiosk packet; item browsing itself remains server-driven menus.</summary>
    public sealed class KioskHandler
    {
        public event Action<KioskListing> ListingReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.KIOSK, OnKiosk);
        }

        private void OnKiosk(Message message)
        {
            var r = message.Reader;
            var listing = new KioskListing
            {
                Type = r.ReadSByte(),
                HasListing = r.ReadInt() == 1
            };
            if (listing.HasListing)
            {
                listing.ItemId = r.ReadInt();
                listing.FrameImagePath = r.ReadUtf();
                listing.Name = r.ReadUtf();
                listing.Description = r.ReadUtf();
                listing.RemainingSeconds = r.ReadInt();
                listing.FrameCount = r.ReadSByte();
            }
            r.ExpectFullyConsumed("KIOSK");
            ListingReceived?.Invoke(listing);
        }
    }

    public static class KioskPackets
    {
        public static Message SelectItem(sbyte kioskType) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SELECT_KIOSK_ITEM).PutSByte(kioskType);

        public static Message RemoveListing(int itemId) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.REMOVE_SELL_ITEM).PutInt(itemId);
    }
}
