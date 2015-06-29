using Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IFileImport
    {
        void StartImport(UploadingProgress progress, DocumentType type, string filePath, string userID);
        Task<UploadingProgress> CreateProgressAsync(string fileName, string userID);
        Task<UploadingProgress> GetProgressByIdAsync(int ID, string userID);
        Task<UploadingProgress> GetProgressByFileNameAsync(string fileName, string userID);
        Task<UploadingProgress> GetCurrentUserProgressAsync(string userID);
    }
}
