using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace WebLinkBrowserInDesktop.Helpers
{
    internal static class FileHelper
    {
        internal static void SafeSaveConfig(string filePath, object configData)
        {
            FileStream stream = null;
            StreamWriter writer = null;

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new StreamWriter(stream);

                writer.Write(configData);
            }
            catch(IOException ioEx) 
            {
                MessageBox.Show($"File access error: {ioEx.Message}. Please ensure the file is not open in another program and try again.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}");
            }
            finally
            {
                writer?.Close();
                writer?.Dispose();
                stream?.Close();
                stream?.Dispose();
            }
        }

        internal static void SafeWriteConfig(string filePath, object configToSave)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(configToSave, Formatting.Indented);
                SafeSaveConfig(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing configuration: {ex.Message}");
            }
        }
    }
}
