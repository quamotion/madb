using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient
{
    public interface IFileListingService
    {
        /// <include file='.\FileListingService.xml' path='/FileListingService/Device/*'/>
        IDevice Device
        {
            get;
        }

        /// <include file='.\FileListingService.xml' path='/FileListingService/Root/*'/>
        FileEntry Root
        {
            get;
        }

        /// <include file='.\FileListingService.xml' path='/FileListingService/GetChildren/*'/>
        FileEntry[] GetChildren(FileEntry entry, bool useCache, IListingReceiver receiver);

        /// <include file='.\FileListingService.xml' path='/FileListingService/FindFileEntry/*'/>
        FileEntry FindFileEntry(String path);

        /// <include file='.\FileListingService.xml' path='/FileListingService/FindFileEntry2/*'/>
        FileEntry FindFileEntry(FileEntry parent, String path);
    }
}