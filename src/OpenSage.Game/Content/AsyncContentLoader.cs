using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OpenSage.Data;
using OpenSage.Utilities;

namespace OpenSage.Content
{
    /*internal class AsyncContentLoader<T>
    {
        private readonly ContentLoader<T> _contentLoader;

        public AsyncContentLoader(ContentLoader<T> contentLoader)
        {
            _contentLoader = contentLoader;
        }

        protected Task<T> LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            return _contentLoader.LoadEntry(entry, contentManager, game, loadOptions);
        }
    }*/

    internal sealed class AsyncLoadingPool
    {
        // type LoadingJob =
        // | ContentLoadingJob of FileSystemEntry * ContentLoader
        // | IniLoadingJob of FileSystemEntry
        internal abstract class LoadingJob
        {
            protected readonly FileSystemEntry Entry;

            protected readonly TaskCompletionSource<object> Completion;
            public Task CompletionTask => Completion.Task;

            protected LoadingJob(FileSystemEntry entry)
            {
                Entry = entry;
                Completion = new TaskCompletionSource<object>();
            }

            public abstract void Run(); 

            internal sealed class ContentLoadingJob : LoadingJob
            {
                private readonly ContentLoader _loader;

                public ContentLoadingJob(FileSystemEntry entry, ContentLoader loader) : base(entry)
                {
                    _loader = loader;
                }

                public override void Run()
                {

                }
            }

            internal sealed class IniLoadingJob : LoadingJob
            {
                public IniLoadingJob(FileSystemEntry entry) : base(entry)
                {

                }

                public override void Run()
                {
                }
            }

            public static LoadingJob LoadContent(FileSystemEntry entry, ContentLoader loader) => new ContentLoadingJob(entry, loader);
            public static LoadingJob LoadIni(FileSystemEntry entry) => new IniLoadingJob(entry);
        }

        private readonly Thread[] _loadingThreads;
        private readonly BlockingCollection<LoadingJob> _loadingJobs;

        public AsyncLoadingPool()
        {
            _loadingJobs = new BlockingCollection<LoadingJob>();
            _loadingThreads = new Thread[4];

            for (var i = 0; i < _loadingThreads.Length; i++)
            {
                var loadingThread = new Thread(HandleLoadingJobs)
                {
                    IsBackground = true,
                    Name = $"Content loading thread #{i + 1}"
                };

                loadingThread.Start();
                _loadingThreads[i] = loadingThread;
            }
        }

        private void HandleLoadingJobs()
        {
            foreach (var job in _loadingJobs.GetConsumingEnumerable())
            {

                switch (job)
                {
                    case LoadingJob.ContentLoadingJob contentJob:
                        break;
                    case LoadingJob.IniLoadingJob iniJob:
                        break;
                }
            }
        }
    }
}
