using Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IFileImport
    {
        void StartImport(UploadingProgress progress, DocumentType type, string filePath, string userId);
        Task<UploadingProgress> CreateProgressAsync(string fileId, string userId);
        Task<UploadingProgress> GetProgressByIdAsync(int ID, string userId);
        Task<UploadingProgress> GetProgressByFileNameAsync(string fileId, string userId);
        Task<UploadingProgress> GetCurrentUserProgressAsync(string userId);
    }
}
