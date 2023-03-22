using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseSeller.Core.Generators
{
    public static class CodeGenerators
    {
        public static string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
