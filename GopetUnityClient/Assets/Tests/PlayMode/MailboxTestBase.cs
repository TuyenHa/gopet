using System.Collections.Generic;
using System.Linq;
using Gopet.Net.Social;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Phần dựng cảnh dùng chung của các bộ test hộp thư: canvas HUD, dữ liệu thư mẫu và
    /// hàm đo hình chữ nhật. Gom vào đây để hai bộ test không chép lại cùng một đoạn.
    /// </summary>
    public abstract class MailboxTestBase
    {
        protected GameObject Root;

        [SetUp]
        public void SetUpRoot()
        {
            Root = new GameObject("Mailbox Root", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler));
            ((RectTransform)Root.transform).sizeDelta = new Vector2(720f, 405f);
        }

        [TearDown]
        public void TearDownRoot()
        {
            if (Root != null) Object.DestroyImmediate(Root);
        }

        /// <summary>Một thư mỗi loại: admin chưa đọc, sự kiện đã đọc, bạn bè chưa đọc.</summary>
        protected static Mailbox ThreeLetters() => new Mailbox
        {
            Letters = new[]
            {
                new Letter { LetterId = 1, Type = 2, Title = "Thư admin", ShortContent = "Xin chào", IsMark = false },
                new Letter { LetterId = 2, Type = 3, Title = "Sự kiện", ShortContent = "Quà tháng", IsMark = true },
                new Letter { LetterId = 3, Type = 1, Title = "Bạn bè", ShortContent = "Đi săn không", IsMark = false },
            }
        };

        /// <summary>Một thư đủ dài để chắc chắn vượt chiều cao vùng đọc (176 đơn vị).</summary>
        protected static Mailbox OneLongLetter() => new Mailbox
        {
            Letters = new[]
            {
                new Letter
                {
                    LetterId = 9, Type = 2, Title = "Thư rất dài", ShortContent = "Dài lắm",
                    Content = string.Concat(Enumerable.Repeat(
                        "Đây là một đoạn nội dung dài dùng để kiểm tra việc cuộn trong ô đọc thư. ", 40)),
                    IsMark = false,
                }
            }
        };

        protected static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y,
                corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }
    }
}
