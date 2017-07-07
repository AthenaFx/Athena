namespace Athena.Diagnostics
{
    public class LurchTableItem<TKey,TValue>
    {
        public LurchTableItem(TKey k, TValue v)
        {
            Key = k;
            Value = v;
        }
        public TKey Key;
        public TValue Value;
    }
}