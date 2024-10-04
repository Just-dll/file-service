using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Data
{
    public class StorageProviderChangeTracker : IChangeTracking
    {
        public bool IsChanged => throw new NotImplementedException();

        public void AcceptChanges()
        {
            throw new NotImplementedException();
        }
    }
}
