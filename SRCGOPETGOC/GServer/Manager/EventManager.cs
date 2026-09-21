using Gopet.Data.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gopet.Data.Event.Year2024;
using Gopet.Data.Collections;
using Gopet.Data.Event.Year2025;
using Gopet.Data.Event.DailyCheckin;
namespace Gopet.Manager
{
    public class EventManager
    {
        public static readonly CopyOnWriteArrayList<EventBase> _events = new CopyOnWriteArrayList<EventBase>();
        static Thread thread = new Thread(Run);
        public static AutoResetEvent ResetEvent = new AutoResetEvent(false);
        public static bool IsRunning { get; private set; } = false;

        static EventManager()
        {
            _events.Add(ArenaEvent.Instance);
            //_events.Add(Summer2024Event.Instance);
            _events.Add(BannerEvent.Instance);
            //_events.Add(Winter2024Event.Instance);
            //_events.Add(TeacherDay2024.Instance);
            _events.Add(GameBirthdayEvent.Instance);
            _events.Add(DailyCheckinEvent.Instance);
        }

        /// <summary>
        /// Thêm sự kiện KHI MÁY CHỦ ĐANG CHẠY — tức là sự kiện vừa mở thật.
        ///
        /// <para>Các sự kiện cố định đăng ký thẳng bằng <c>_events.Add</c> trong static
        /// constructor, KHÔNG đi qua đây. Nhờ vậy thư báo dưới đây không bị bắn lại mỗi
        /// lần khởi động lại máy chủ.</para>
        ///
        /// <para>Cũng vì thế mà KHÔNG báo ở <c>EventBase.Init</c> hay khi
        /// <c>Condition</c> bật: <c>Init</c> chạy lại mỗi lần khởi động, còn
        /// <c>Condition</c> là cửa sổ lặp theo giờ (đấu trường mở 7 lần/ngày) — cả hai đều
        /// biến hộp thư thành chỗ spam. Khung giờ lặp là việc của băng chữ chạy
        /// (<c>BannerEvent</c>), không phải của thư.</para>
        /// </summary>
        public static void AddEvent(EventBase eventBase)
        {
            _events.Add(eventBase);
            AnnounceEventOpened(eventBase);
        }

        /// <summary>Báo cho toàn server biết sự kiện vừa mở. Sự kiện không đặt tên thì bỏ qua.</summary>
        private static void AnnounceEventOpened(EventBase eventBase)
        {
            var name = eventBase?.Name;
            if (string.IsNullOrWhiteSpace(name)) return;

            SystemLetterService.SendToAll(
                Gopet.Data.User.Letter.EVENT,
                "Sự kiện",
                $"Sự kiện {name} đã mở.",
                $"Sự kiện {name} vừa bắt đầu. Vào game tham gia để nhận thưởng nhé!");
        }

        public static void Start()
        {
            IsRunning = true;
            thread.IsBackground = true;
            thread.Name = "Luồng Event";
            thread.Start();
        }

        public static void FindAndUseItemEvent(int itemId, Player player)
        {
            var findEvent = _events.Where(p => p.ItemsOfEvent.Contains(itemId));
            if (findEvent.Any())
            {
                findEvent.First().UseItem(itemId, player);
            }
            else
            {
                player.redDialog(player.Language.CannotFindEvents);
            }
        }

        public static void FindAndUseItemEvent(int itemId, Player player, int Count)
        {
            var findEvent = _events.Where(p => p.ItemsOfEvent.Contains(itemId));
            if (findEvent.Any())
            {
                findEvent.First().UseItemCount(itemId, player, Count);
            }
            else
            {
                player.redDialog(player.Language.CannotFindEvents);
            }
        }

        private static void Run()
        {
            while (IsRunning)
            {
                foreach (var item in _events.ToArray())
                {
                    if (!item.IsInitOK)
                    {
                        item.Init();
                        item.IsInitOK = true;
                        continue;
                    }
                    if (item.Condition)
                    {
                        try
                        {
                            item.Update();
                        }
                        catch (Exception ex)
                        {
                            GopetManager.ServerMonitor.LogError(ex.ToString());
                            ResetEvent.WaitOne(1000);
                        }
                    }

                    if (item.NeedRemove)
                    {
                        _events.Remove(item);
                    }
                }
                ResetEvent.WaitOne(1000);
            }
        }
    }
}
