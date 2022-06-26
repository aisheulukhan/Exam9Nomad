using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NomadBack.Utilities.Extention
{
    public static class FileExtention
    {
        public static bool CheckSize(this IFormFile file, int kb)
        {
            if (file.Length / 1024 > kb) return true;
            return false;
        }
        public static bool CheckType(this IFormFile file, string type)
        {
            if (file.ContentType.Contains(type)) return true;
            return false;
        }
        public async static Task<string> SaveFileAsync(this IFormFile file, string savePath)
        {
            string fileName = Guid.NewGuid().ToString() + file.FileName;
            string path = Path.Combine(savePath, fileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
