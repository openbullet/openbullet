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

        public string Name { get; set; }

        public LiteDBRepository(string name)
        {
            Name = name;
            _db = new LiteDatabase(Globals.dataBaseFile);
            _coll = _db.GetCollection<T>(name);
        }

        ~LiteDBRepository()
        {
            _db.Dispose();
        }

        public void Add(T entity)
        {
            _coll.Insert(entity);
        }

        public IEnumerable<T> Get()
        {
            return _coll.FindAll();
        }

        public T Get(Guid id)
        {
            return _coll.FindById(id);
        }

        public void Remove(T entity)
        {
            _coll.Delete(entity.Id);
        }

        public void RemoveAll()
        {
            _db.DropCollection(Name);
        }

        public void Update(T entity)
        {
            _coll.Update(entity);
        }
    }
}
