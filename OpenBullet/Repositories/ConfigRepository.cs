using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuriLib;
using RuriLib.Interfaces;
using RuriLib.ViewModels;

namespace OpenBullet.Repositories
{
    public class ConfigRepository : IRepository<ConfigViewModel, string>
    {
        public string BaseFolder { get; set; }

        public ConfigRepository(string baseFolder)
        {
            BaseFolder = baseFolder;
        }

        public void Add(ConfigViewModel entity)
        {
            if (!File.Exists(entity.Path))
            {
                IOManager.SaveConfig(entity.Config, entity.Path);
            }
            else
            {
                throw new IOException("A file with the same name already exists");
            }
        }

        public IEnumerable<ConfigViewModel> Get()
        {
            throw new NotImplementedException();
        }

        public ConfigViewModel Get(string id)
        {
            throw new NotImplementedException();
        }

        public void Remove(ConfigViewModel entity)
        {
            if (File.Exists(entity.Path))
            {
                File.Delete(entity.Path);
            }
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public void Update(ConfigViewModel entity)
        {
            if (File.Exists(entity.Path))
            {
                IOManager.SaveConfig(entity.Config, entity.Path);
            }
            else
            {
                throw new IOException("File not found");
            }
        }

        public void Add(IEnumerable<ConfigViewModel> entities)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<ConfigViewModel> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<ConfigViewModel> entities)
        {
            throw new NotImplementedException();
        }
    }
}
