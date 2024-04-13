using System.Collections;

namespace ReadVideo.Server.Services.BotStateManagement
{
    public class BotStateCollection : IEnumerable<BotState>
    {
        private ICollection<BotState> _collection { get; set; } = new List<BotState>();

        public void AddState(BotState state) => _collection.Add(state);


        // This is the generic version required by IEnumerable<T>
        public IEnumerator<BotState> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        // This is the non-generic version required by IEnumerable
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
