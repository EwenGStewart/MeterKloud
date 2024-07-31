namespace MeterDataLib.Parsers
{
    public class StreamConverter
    {
        public static MemoryStream ConvertToMemoryStream(Stream forwardOnlyStream)
        {
            if (forwardOnlyStream == null)
            {
                throw new ArgumentNullException(nameof(forwardOnlyStream));
            }

            var memoryStream = new MemoryStream();
            forwardOnlyStream.CopyTo(memoryStream);
            memoryStream.Position = 0; // Reset the position to the beginning

            return memoryStream;
        }
    }
}
