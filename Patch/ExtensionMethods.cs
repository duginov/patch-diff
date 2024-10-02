using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch
{
    public static class ExtensionMethods
    {
        public static StringBuilder AppendFormattedTitle(this StringBuilder sb, string text)
        {
            sb.AppendLine();
            sb.AppendLine($"{text}");
            sb.AppendLine("-----------------------------------------------------------");
            return sb;
        }
    }
}
