using System.IO.Pipes;

namespace DBRequestHandler.Context
{
    public class RequestContext
    {
        public NamedPipeServerStream PipeStream { get; set; }
        public Dictionary<string, string> ParsedInstruction { get; set; }
    }
}
