using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SqlToOracle.Core.Interface
{
    public interface ICommand
    {

        List<string> Excute();
    }


}
