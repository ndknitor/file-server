using System;
using System.Collections.Generic;

namespace FileServer.Models.Entities;

public partial class NodeSpace
{
    public string Node { get; set; }

    public long AvalibleSpace { get; set; }

    public long TotalSpace { get; set; }
}
