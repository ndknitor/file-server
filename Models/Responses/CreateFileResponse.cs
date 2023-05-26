namespace FileServer.Models.Responses
{
    public class CreateFileResponse : StandardResponse
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
}