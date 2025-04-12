using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBRequestHandler.Context
{
    public class RequestContext
    {
        public NamedPipeServerStream PipeStream { get; set; }
        public Dictionary<string, string> ParsedInstruction { get; set; }
    }
}
