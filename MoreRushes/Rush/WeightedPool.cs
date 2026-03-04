namespace MoreRushes.Rush
{
    internal class WeightedPool<T>
    {
        private readonly List<T> _items = [];
        private readonly List<uint> _weights = [];
        private uint _totalWeight;

        public void Add(T item, uint weight)
        {
            if (weight <= 0)
                return;

            _items.Add(item);
            _weights.Add(weight);
            _totalWeight += weight;
        }

        public void Clear()
        {
            _items.Clear();
            _weights.Clear();
            _totalWeight = 0;
        }

        public T GetDeterministic(uint seed)
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("WeightedPool is empty.");
            
            if (_items.Count == 1) 
                return _items[0];

            uint rand = RushSeedUtility.NextUInt(ref seed);

            uint roll = rand % _totalWeight;

            uint cumulative = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                cumulative += _weights[i];
                if (roll < cumulative)
                    return _items[i];
            }

            return _items[_items.Count - 1];
        }
    }
}
