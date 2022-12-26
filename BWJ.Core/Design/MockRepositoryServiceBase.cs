namespace BWJ.Core.Design
{
    public abstract class MockRepositoryServiceBase<T> : IMockRepositoryServiceBase<T>
    {
        protected readonly object @lock = new object();
        private List<T> Data = new List<T>();

        public async Task<IEnumerable<T>> GetRecords()
        {
            List<T> data;
            lock (@lock)
            {
                data = Data;
            }
            return await Task.FromResult(Data);
        }

        public async Task<IEnumerable<T>> GetRecords(Func<T, bool> predicate)
        {
            IEnumerable<T> data;
            lock (@lock)
            {
                data = Data.Where(predicate);
            }

            return await Task.FromResult(data);
        }

        public async Task<T?> GetRecord(Func<T, bool> predicate)
        {
            T? data;
            lock (@lock)
            {
                data = Data.FirstOrDefault(predicate);
            }

            return await Task.FromResult(data);
        }

        public async Task CreateRecord(T model)
        {
            lock (@lock)
            {
                Data.Add(model);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateRecord(Func<T, bool> predicate, T model)
        {
            lock (@lock)
            {
                var old = Data.FirstOrDefault(predicate);
                if (old != null)
                {
                    Data.Remove(old);
                    Data.Add(model);
                }
            }
            await Task.CompletedTask;
        }

        public async Task DeleteRecord(Func<T, bool> predicate)
        {
            lock (@lock)
            {
                var target = Data.FirstOrDefault(predicate);
                if (target != null)
                {
                    Data.Remove(target);
                }
            }
            await Task.CompletedTask;
        }
    }
}
