using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    public sealed class ChatTranscript
    {
        private readonly int _capacity;
        private readonly List<ChatTranscriptEntry> _entries = new List<ChatTranscriptEntry>();

        public ChatTranscript(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        public IReadOnlyList<ChatTranscriptEntry> Entries => _entries;

        public void Add(string sender, string text)
        {
            if (_entries.Count == _capacity) _entries.RemoveAt(0);
            _entries.Add(new ChatTranscriptEntry(sender ?? string.Empty, text ?? string.Empty));
        }
    }

    public sealed class ChatTranscriptEntry
    {
        public ChatTranscriptEntry(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }

        public string Sender { get; }
        public string Text { get; }
    }
}
