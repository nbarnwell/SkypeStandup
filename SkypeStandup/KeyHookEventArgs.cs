using System;
using System.Windows.Input;

namespace SkypeStandup
{
    public class KeyHookEventArgs : EventArgs
    {
        public VirtualKey Key { get; set; }

        public KeyHookEventArgs(VirtualKey key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return string.Format("Key: {0}", Key);
        }
    }
}