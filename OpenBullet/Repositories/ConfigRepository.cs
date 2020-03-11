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
    // The ID is a tuple containing the category and the filename without extension (they are enough to uniquely identify a config in the repo)
    public class ConfigRepository : IRepository<ConfigViewModel, (string, string)>
    {
        public static string defaultCategory = "Default";
        public string BaseFolder { get; set; }

        public ConfigRepository(string baseFolder)
        {
            BaseFolder = baseFolder;
        }

        public void Add(ConfigViewModel entity)
        {
            var path = GetPath(entity);

            // Create the category folder if it doesn't exist
            if (entity.Category != defaultCategory && !Directory.Exists(entity.Category))
            {
                Directory.CreateDirectory(Path.Combine(BaseFolder, entity.Category));
            }

            if (!File.Exists(path))
            {
                IOManager.SaveConfig(entity.Config, path);
            }
            else
            {
                throw new IOException("A config with the same name and category already exists");
            }
        }

        public IEnumerable<ConfigViewModel> Get()
        {
            List<ConfigViewModel> configs = new List<ConfigViewModel>();

            // Load the configs in the root folder
            foreach (var file in Directory.EnumerateFiles(OB.configFolder).Where(file => file.EndsWith(".loli")))
            {
                try 
                {
                    configs.Add(new ConfigViewModel(
                        Path.GetFileNameWithoutExtension(file),
                        defaultCategory,
                        IOManager.LoadConfig(file)));
                }
                catch { }
            }

            // Load the configs in the subfolders
            foreach (var categoryFolder in Directory.EnumerateDirectories(OB.configFolder))
            {
                foreach (var file in Directory.EnumerateFiles(categoryFolder).Where(file => file.EndsWith(".loli")))
                {
                    try 
                    {
                        configs.Add(new ConfigViewModel(
                            Path.GetFileNameWithoutExtension(file),
                            Path.GetFileName(categoryFolder),
                            IOManager.LoadConfig(file))); 
                    }
                    catch { }
                }
            }

            return configs;
        }

        public ConfigViewModel Get((string, string) id)
        {
            var category = id.Item1;
            var fileName = id.Item2;
            
            var path = GetPath(category, fileName);

            return new ConfigViewModel(fileName, category, IOManager.LoadConfig(path));
        }

        public void Remove(ConfigViewModel entity)
        {
            var path = GetPath(entity);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new IOException("File not found");
            }
        }

        public void RemoveAll()
        {
            Directory.Delete(BaseFolder, true);
            Directory.CreateDirectory(BaseFolder);
        }

        public void Update(ConfigViewModel entity)
        {
            var path = GetPath(entity);

            if (File.Exists(path))
            {
                IOManager.SaveConfig(entity.Config, path);
            }
            else
            {
                throw new IOException("File not found");
            }
        }

        public void Add(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public void Remove(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public void Update(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        private string GetPath(ConfigViewModel config)
        {
            return GetPath(config.Category, config.FileName);
        }

        private string GetPath(string category, string fileName)
        {
            var file = $"{fileName}.loli";

            if (category != defaultCategory)
            {
                return Path.Combine(BaseFolder, category, file);
            }
            else
            {
                return Path.Combine(BaseFolder, file);
            }
        }
    }
}
