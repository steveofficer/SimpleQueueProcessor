using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BackgroundQueue
{
    public class DbQueueSource : IQueueSource
    {
        private readonly DatabaseContext _context;

        public DbQueueSource(DatabaseContext context) {
            _context = context;
        }
        
        public async Task<MessageBatch> GetMessagesAsync(DateTime currentProcessingTime, int batchSize = 20)
        {
           // The table hints tell SQL to give us eXclusive access to the record so that no other task schedulers can work on the same records,
            // and to also READPAST any records that are already locked.
            // This means that the tasks we return are guaranteed to not be running anywhere else.

            // The benefit to using XLOCK over adding an "InProgress" field to the table, is that if the process fails for whatever reason,
            // XLOCK will automatically release the lock because the transaction is automatically rolled back. Whereas, the InProgress field would
            // need us to create a compensating action in order to unlock records. This would probably involve needing to know instance ids and having a
            // "RollbackFailedTasks" Background task, nightmare.
            var transaction = _context.Database.BeginTransaction();

            var messages = await _context
                .MessageQueue
                .FromSqlInterpolated($"SELECT TOP ({batchSize}) * FROM MessageQueue WITH (XLOCK, READPAST) WHERE NextRunTime < {currentProcessingTime}")
                .ToListAsync();

            return new MessageBatch(messages, () => { 
                _context.SaveChanges();
                transaction.Commit(); 
            });
        }

        public void RemoveMessage(Message message) => _context.MessageQueue.Remove(message);

        public void UpdateMessage(Message message) => _context.MessageQueue.Update(message);
    }
}