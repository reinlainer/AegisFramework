using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Aegis.Threading
{
    public interface ILockObject : IDisposable
    {
        ILockObject Enter();
        void Leave();
    }





    public sealed class ReaderLock : ILockObject
    {
        private ReaderWriterLockSlim _lock;


        public ReaderLock(ReaderWriterLockSlim lockObj)
        {
            _lock = lockObj;
        }


        public ILockObject Enter()
        {
            _lock.EnterReadLock();
            return this;
        }


        public void Leave()
        {
            _lock.ExitReadLock();
        }


        public void Dispose()
        {
            _lock.ExitReadLock();
        }
    }





    public sealed class WriterLock : ILockObject
    {
        private ReaderWriterLockSlim _lock;


        public WriterLock(ReaderWriterLockSlim lockObj)
        {
            _lock = lockObj;
        }


        public ILockObject Enter()
        {
            _lock.EnterWriteLock();
            return this;
        }


        public void Leave()
        {
            _lock.ExitWriteLock();
        }


        public void Dispose()
        {
            _lock.ExitWriteLock();
        }
    }





    public sealed class RWLock
    {
        private ReaderWriterLockSlim _lock;
        private ILockObject _lockRead, _lockWrite;


        public ReaderLock ReaderLock { get { return (ReaderLock)_lockRead.Enter(); } }
        public WriterLock WriterLock { get { return (WriterLock)_lockWrite.Enter(); } }


        public RWLock()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _lockRead = new ReaderLock(_lock);
            _lockWrite = new WriterLock(_lock);
        }
    }
}
