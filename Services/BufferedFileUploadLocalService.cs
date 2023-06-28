using CookingRecipes.Interfaces;

namespace CookingRecipes.Services
{
    public class BufferedFileUploadLocalService : IBufferedFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BufferedFileUploadLocalService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> UploadFile(IFormFile file)
        {
            string path = "";
            try
            {
                if (file?.Length > 0)
                {
                    //path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "UploadedFiles"));
                    path = Path.Combine(_webHostEnvironment.WebRootPath, "uploadedfiles");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(path, fileName);
                    using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    //return true;
                    var fileUrl = "/uploadedfiles/" + fileName;
                    return fileUrl;

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File Copy Failed", ex);
            }
        }
    }
}
