using System;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>Đếm và in kết quả từng check. Thoát khác 0 nếu có check nào fail.</summary>
    internal static class Report
    {
        private static int _failures;

        public static bool AnyFailure => _failures > 0;
        public static int FailureCount => _failures;

        public static void Check(string name, bool passed, string reason)
        {
            if (passed) { Console.WriteLine($"[PASS] {name}"); return; }
            Fail(name, reason);
        }

        /// <summary>
        /// Bọc một nhóm check trong try riêng. Nếu nhóm này ném thì các nhóm sau
        /// vẫn chạy — gộp chung một try thì một ngoại lệ ở nhóm đầu sẽ âm thầm
        /// nuốt mất mọi check còn lại.
        /// </summary>
        public static void Group(string name, Action body)
        {
            try { body(); }
            catch (Exception ex) { Fail($"{name} (ngoại lệ)", $"{ex.GetType().Name}: {ex.Message}"); }
        }

        public static void Fail(string name, string reason)
        {
            _failures++;
            Console.WriteLine($"[FAIL] {name} — {reason}");
        }
    }
}
