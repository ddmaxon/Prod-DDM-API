namespace Prod_DDM_API.Data
{
    public class FileWatcher
    {

        public FileWatcher()
        {
            
        }

        public bool CheckFiles(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            string[] files = Directory.GetFiles(directory);
            if (files.Length == 0)
            {
                return false;
            }

            foreach (string file in files)
            {
                try
                {
                    // Überprüfen, ob die Datei geschrieben wird
                    using (StreamReader sr = new StreamReader(file))
                    {
                        if (sr.ReadLine() != null)
                        {
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new EndOfStreamException(ex.Message);
                }
            }
            return false;
        }
    }
}
