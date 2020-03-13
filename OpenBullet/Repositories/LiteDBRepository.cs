using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using RuriLib.Interfaces;
using RuriLib.Models;

namespace OpenBullet.Repositories
{
    public class LiteDBRepository<T> : IRepository<T, Guid> where T : Persistable<Guid>
    { 
        private LiteDatabase _db;
        private LiteCollection<T> _coll;

        public string ConnectionString { get; set; }
        public string Collection { get; set; }

        public LiteDBRepository(string connectionString, string collection)
        {
            ConnectionString = connectionString;
            Collection = collection;
        }

        private void Connect()
        {
            _db = new LiteDatabase(ConnectionString);
            _coll = _db.GetCollection<T>(Collection);
        }

        private void Disconnect()
        {
            _db.Dispose();
            _coll = null;
        }

        public void Add(T entity)
        {
            Connect();
            _coll.Insert(entity);
            Disconnect();
        }

        public void Add(IEnumerable<T> entities)
        {
            Connect();
            _coll.InsertBulk(entities);
            Disconnect();
        }

        public IEnumerable<T> Get()
        {
            Connect();
            var result = _coll.FindAll();
            Disconnect();
            return result;
        }

        public T Get(Guid id)
        {
            Connect();
            var result = _coll.FindById(id);
            Disconnect();
            return result;
        }

        public void Remove(T entity)
        {
            Connect();
            _coll.Delete(entity.Id);
            Disconnect();
        }

        public void Remove(IEnumerable<T> entities)
        {
            Connect();
            
            foreach (var entity in entities)
            {
                _coll.Delete(entity.Id);
            }

            Disconnect();
        }

        public void RemoveAll()
        {
            Connect();
            _db.DropCollection(Collection);
            Disconnect();
        }

        public void Update(T entity)
        {
            Connect();
            _coll.Update(entity);
            Disconnect();
        }

        public void Update(IEnumerable<T> entities)
        {
            Connect();
            _coll.Update(entities);
            Disconnect();
        }
    }
}
