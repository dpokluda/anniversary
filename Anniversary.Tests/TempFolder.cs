namespace Anniversary.Tests;

/// <summary>
/// A unique temporary folder that test files are written to; deleted on dispose.
/// </summary>
internal sealed class TempFolder : IDisposable
{
    /// <summary>
    /// Creates a new, empty temporary folder.
    /// </summary>
    public TempFolder()
    {
        FolderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "AnniversaryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FolderPath);
    }

    /// <summary>
    /// Gets the full path of the folder.
    /// </summary>
    public string FolderPath { get; }

    /// <summary>
    /// Writes a UTF-8 text file into the folder.
    /// </summary>
    /// <param name="name">The file name (may include sub-folders).</param>
    /// <param name="content">The file content.</param>
    /// <returns>The full path of the written file.</returns>
    public string Write(string name, string content)
    {
        string path = System.IO.Path.Combine(FolderPath, name);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Returns the full path of a file in the folder.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <returns>The full path.</returns>
    public string PathOf(string name)
    {
        return System.IO.Path.Combine(FolderPath, name);
    }

    /// <summary>
    /// Deletes the folder and everything in it.
    /// </summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(FolderPath, recursive: true);
        }
        catch (IOException)
        {
            // Best effort; the OS cleans the temp folder eventually.
        }
    }
}
