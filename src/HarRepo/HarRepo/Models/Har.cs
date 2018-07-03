using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HarRepo.Models
{
    public class Har
    {
        [Required]
        public Log Log { get; set; }
    }

    public class Log
    {
        [Required]
        public Entry[] Entries { get; set; }
    }

    public class Entry
    {

        public string PageRef { get; set; }

        [Required]
        public Timing Timings { get; set; }

        [Required]
        public Request Request { get; set; }

        [Required]
        public double Time { get; set; }

    }

    public class Timing
    {
        [Required]
        public double Blocked { get; set; }
        [Required]
        public double Dns { get; set; }
        [Required]
        public double Connect { get; set; }
        [Required]
        public double Send { get; set; }
        [Required]
        public double Wait { get; set; }
        [Required]
        public double Receive { get; set; }
        [Required]
        public double Ssl { get; set; }

        public string Comment { get; set; }
    }

    public class Request
    {
        [Required]
        public string Method { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string HttpVersion { get; set; }

        [Required]
        public int HeadersSize { get; set; }

        [Required]
        public int BodySize { get; set; }
    }
}
