using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwsStandardAspApplication.Models
{
    public class DataContext
    {
        public Task MockQuery() => 
            Task.Delay(TimeSpan.FromMilliseconds(100));

    }
}
